﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 本类中封装卡组操作的游戏逻辑高级功能
/// </summary>
public class ServerCardDeckManager
{
    public ServerPlayer ServerPlayer;

    public CardDeckInfo M_UnlockCards;
    public CardDeckInfo M_LockCards;
    public List<CardDeck> M_CardDecks;
    private CardDeck m_CurrentCardDeck;

    public CardDeck M_CurrentCardDeck
    {
        get { return m_CurrentCardDeck; }
        set { m_CurrentCardDeck = value; }
    }

    public CardInfo_Base DrawRetinueCard()
    {
        if (M_CurrentCardDeck.IsEmpty)
        {
            M_CurrentCardDeck.AbandonCardRecycle();
            M_CurrentCardDeck.SuffleSelf();
        }

        M_CurrentCardDeck.GetARetinueCardToTheTop();
        CardInfo_Base newCardInfoBase = DrawTop();
        return newCardInfoBase;
    }

    public CardInfo_Base DrawTop()
    {
        if (M_CurrentCardDeck.IsEmpty)
        {
            M_CurrentCardDeck.AbandonCardRecycle();
            M_CurrentCardDeck.SuffleSelf();
        }

        CardInfo_Base newCardInfoBase = M_CurrentCardDeck.DrawCardOnTop();
        OnPlayerGetCard(newCardInfoBase.CardID);
        return newCardInfoBase;
    }

    public void OnPlayerGetCard(int cardId)
    {
        DrawCardRequest request1 = new DrawCardRequest(ServerPlayer.ClientId, cardId, true);
        DrawCardRequest request2 = new DrawCardRequest(ServerPlayer.ClientId, cardId, false);
        Server.SV.SendMessage(request1, ServerPlayer.ClientId);
        Server.SV.SendMessage(request2, ServerPlayer.EnemyClientId);
    }
}