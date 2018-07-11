using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public class ProtoManager
{
    private Dictionary<int, Func<DataStream, Request>> mProtocolMapping;

    public delegate void requestDelegate(Socket socket, Request request);

    private Dictionary<int, List<requestDelegate>> mDelegateMapping;

    public ProtoManager()
    {
        mProtocolMapping = new Dictionary<int, Func<DataStream, Request>>();
        mDelegateMapping = new Dictionary<int, List<requestDelegate>>();

        AddProtocol<TestConnectRequest>(NetProtocols.TEST_CONNECT);
        AddProtocol<ClientIdRequest>(NetProtocols.SEND_CLIENT_ID);
        AddProtocol<ServerInfoRequest>(NetProtocols.INFO_NUMBER);
        AddProtocol<ServerWarningRequest>(NetProtocols.WARNING_NUMBER);
        AddProtocol<MatchRequest>(NetProtocols.Match);
        AddProtocol<PlayerRequest>(NetProtocols.PLAYER);
        AddProtocol<PlayerCostRequest>(NetProtocols.PLAYER_COST_CHANGE);
        AddProtocol<DrawCardRequest>(NetProtocols.DRAW_CARD);
        AddProtocol<SummonRetinueRequest>(NetProtocols.SUMMON_RETINUE);
        AddProtocol<PlayerTurnRequest>(NetProtocols.PLAYER_TURN);
        AddProtocol<ClientEndRoundRequest>(NetProtocols.CLIENT_END_ROUND);
        AddProtocol<CardDeckRequest>(NetProtocols.CARD_DECK_INFO);
    }


    public void AddProtocol<T>(int protocol) where T : Request, new()
    {
        if (mProtocolMapping.ContainsKey(protocol))
        {
            mProtocolMapping.Remove(protocol);
        }

        mProtocolMapping.Add(protocol,
            (stream) =>
            {
                T data = new T();
                data.Deserialize(stream);
                return data;
            });
    }

    /// <summary>
    /// 添加代理，在接受到数据时会下发数据
    /// </summary>
    /// <param name="protocol">Protocol.</param>
    /// <param name="d">D.</param>
    public void AddRequestDelegate(int protocol, requestDelegate d)
    {
        List<requestDelegate> dels;
        if (mDelegateMapping.ContainsKey(protocol))
        {
            dels = mDelegateMapping[protocol];
            for (int i = 0; i < dels.Count; i++)
            {
                if (dels[i] == d)
                {
                    return;
                }
            }
        }
        else
        {
            dels = new List<requestDelegate>();
            mDelegateMapping.Add(protocol, dels);
        }

        dels.Add(d);
    }

    public void DelRespDelegate(int protocol, requestDelegate d)
    {
        if (mDelegateMapping.ContainsKey(protocol))
        {
            List<requestDelegate> dels = mDelegateMapping[protocol];
            dels.Remove(d);
        }
    }

    public Request TryDeserialize(byte[] data, Socket socket)
    {
        DataStream stream = new DataStream(data, true);

        int protocol = stream.ReadSInt32();
        Request request = null;
        if (mProtocolMapping.ContainsKey(protocol))
        {
            request = mProtocolMapping[protocol](stream);
            if (request != null)
            {
                if (mDelegateMapping.ContainsKey(protocol))
                {
                    List<requestDelegate> dels = mDelegateMapping[protocol];
                    foreach (requestDelegate del in dels)
                    {
                        del(socket, request);
                    }
                }
            }
        }
        else
        {
            throw new Exception("no register protocol : " + protocol + "!please reg to RegisterResp.");
        }

        return request;
    }
}