﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

internal class Proxy : ProxyBase
{
    private Queue<ClientRequestBase> SendRequestsQueue = new Queue<ClientRequestBase>();

    protected override void Response()
    {
    }

    public override ClientStates ClientState
    {
        get { return clientState; }
        set
        {
            if (clientState != value) OnClientStateChange(value);
            clientState = value;
            ClientLog.Instance.PrintClientStates("Client states: " + ClientState);
        }
    }

    public delegate void ClientStateEventHandler(ClientStates clientStates);

    public static event ClientStateEventHandler OnClientStateChange;

    public Proxy(Socket socket, int clientId, int clientMoney, bool isStopReceive) : base(socket, clientId, clientMoney, isStopReceive)
    {
    }

    public void CancelMatch()
    {
        CancelMatchRequest request = new CancelMatchRequest(ClientId);
        SendMessage(request);
        ClientState = ClientStates.SubmitCardDeck;
    }

    public void LeaveGame()
    {
        LeaveGameRequest request = new LeaveGameRequest();
        SendMessage(request);
        ClientState = ClientStates.SubmitCardDeck;
    }

    public void OnSendCardDeck(CardDeckInfo cardDeckInfo)
    {
        CardDeckRequest req = new CardDeckRequest(ClientId, cardDeckInfo);
        SendMessage(req);
        ClientState = ClientStates.SubmitCardDeck;
    }

    public void OnBeginMatch()
    {
        MatchRequest req = new MatchRequest(ClientId);
        SendMessage(req);
        ClientState = ClientStates.Matching;
    }

    #region 收发基础组件

    public void Send() //每帧调用
    {
        if (SendRequestsQueue.Count > 0)
        {
            ClientRequestBase request = SendRequestsQueue.Dequeue();
            Thread thread = new Thread(Client.Instance.Send);
            thread.IsBackground = true;
            thread.Start(request);
        }
    }

    public void SendMessage(ClientRequestBase request)
    {
        SendRequestsQueue.Enqueue(request);
    }

    public void Response(Socket socket, RequestBase r)
    {
        ClientLog.Instance.PrintReceive("Server: " + r.DeserializeLog());
        if (!(r is ResponseBundleBase))
        {
            switch (r.GetProtocol())
            {
                case NetProtocols.REGISTER_RESULT_REQUEST:
                {
                    RegisterResultRequest request = (RegisterResultRequest) r;
                    if (request.isSuccess)
                    {
                        NoticeManager.Instance.ShowInfoPanel("注册成功", 0, 0.5f);
                    }
                    else
                    {
                        NoticeManager.Instance.ShowInfoPanel("该用户名已被注册", 0, 0.5f);
                    }

                    ClientId = request.clientId;
                    ClientState = ClientStates.Nothing;
                    break;
                }
                case NetProtocols.LOGIN_RESULT_REQUEST:
                {
                    LoginResultRequest request = (LoginResultRequest) r;
                    ClientId = request.givenClientId;
                    if (request.isSuccess)
                    {
                        ClientState = ClientStates.Login;
                        NoticeManager.Instance.ShowInfoPanel("登录成功", 0, 0.5f);
                        LoginManager.Instance.HideCanvas();
                    }
                    else
                    {
                        NoticeManager.Instance.ShowInfoPanel("登录失败，请检查用户名或密码", 0, 0.5f);
                        ClientState = ClientStates.Nothing;
                    }

                    break;
                }
                case NetProtocols.CLIENT_MONEY_REQUEST:
                {
                    ClientMoneyRequest request = (ClientMoneyRequest) r;
                    ClientMoney = request.clientMoney;
                    SelectCardDeckManager.Instance.SetLeftMoneyText(ClientMoney);
                    break;
                }
                case NetProtocols.GAME_STOP_BY_LEAVE_REQUEST:
                {
                    GameStopByLeaveRequest request = (GameStopByLeaveRequest) r;
                    RoundManager.Instance.OnGameStopByLeave(request);
                    break;
                }
                case NetProtocols.RANDOM_NUMBER_SEED_REQUEST:
                {
                    RoundManager.Instance.OnRandomNumberSeed((RandomNumberSeedRequest) r);
                    break;
                }
            }
        }
        else
        {
            ResponseBundleBase request = (ResponseBundleBase) r;
            foreach (ServerRequestBase attachedRequest in request.AttachedRequests) //请求预处理，提取关键信息，如随从死亡、弃牌等会影响客户端交互的信息
            {
                RoundManager.Instance.ResponseToSideEffects_PrePass(attachedRequest);
            }

            foreach (ServerRequestBase attachedRequest in request.AttachedRequests)
            {
                RoundManager.Instance.ResponseToSideEffects(attachedRequest);
            }
        }
    }

    #endregion
}