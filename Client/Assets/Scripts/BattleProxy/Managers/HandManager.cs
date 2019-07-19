﻿using System;
using System.Collections.Generic;

internal class Battle_HandManager
{
    public BattlePlayer BattlePlayer;
    public List<CardBase> Cards = new List<CardBase>();
    public HashSet<int> UsableCards = new HashSet<int>();

    public Battle_HandManager(BattlePlayer battlePlayer)
    {
        BattlePlayer = battlePlayer;
    }

    #region DrawCards

    private void HandAddCard(CardBase newCard)
    {
        Cards.Add(newCard);
        BattlePlayer.BattleStatistics.Draws++;
    }

    internal void DrawCards(int cardNumber)
    {
        int maxDrawCardNumber = Math.Min(Math.Min(cardNumber, GamePlaySettings.MaxHandCard - Cards.Count), BattlePlayer.CardDeckManager.CardDeck.CardCount());
        if (maxDrawCardNumber != cardNumber)
        {
            DrawCards(maxDrawCardNumber);
            return;
        }

        List<DrawCardRequest.CardIdAndInstanceId> cardInfos = new List<DrawCardRequest.CardIdAndInstanceId>();

        foreach (CardInfo_Base cb in BattlePlayer.CardDeckManager.DrawCardsOnTop(maxDrawCardNumber))
        {
            CardBase newCard = CardBase.InstantiateCardByCardInfo(cb, BattlePlayer, BattlePlayer.GameManager.GenerateNewCardInstanceId());
            HandAddCard(newCard);
            BattlePlayer.CardDeckManager.CardDeck.AddCardInstanceId(newCard.CardInfo.CardID, newCard.M_CardInstanceId);
            cardInfos.Add(new DrawCardRequest.CardIdAndInstanceId(newCard.CardInfo.CardID, newCard.M_CardInstanceId));
        }

        OnPlayerGetCards(cardInfos);
    }

    internal void DrawCardsByType(CardTypes cardType, int number)
    {
        int newCardCount = BattlePlayer.CardDeckManager.PutCardsOnTopByType(cardType, number);
        DrawCards(newCardCount);
    }

    internal void DrawHeroCards(int number)
    {
        int newCardCount = BattlePlayer.CardDeckManager.PutHeroCardToTop(number);
        DrawCards(newCardCount);
    }

    internal void PutCardsOnTopByID(List<int> cardIDs)
    {
        BattlePlayer.CardDeckManager.CardDeck.PutCardsToTopByID(cardIDs);
    }

    #endregion

    internal int GetRandomHandCardId(HashSet<int> exceptionInstanceIDList = null)
    {
        List<int> validCardIDs = new List<int>();
        foreach (CardBase cb in Cards)
        {
            if (!exceptionInstanceIDList.Contains(cb.M_CardInstanceId))
            {
                validCardIDs.Add(cb.CardInfo.CardID);
            }
        }

        List<int> res = Utils.GetRandomFromList(validCardIDs, 1);
        if (res.Count > 0)
        {
            return res[0];
        }
        else
        {
            return -1;
        }
    }

    internal List<int> GetRandomSpellCardInstanceIds(int count, int exceptCardInstanceID)
    {
        List<CardBase> spellCards = new List<CardBase>();
        Cards.ForEach(card =>
        {
            if (card is CardSpell spellCard)
            {
                if (spellCard.M_CardInstanceId != exceptCardInstanceID && spellCard.CardInfo.TargetInfo.HasNoTarget)
                {
                    spellCards.Add(spellCard);
                }
            }
        });
        if (count > spellCards.Count) count = spellCards.Count;
        List<CardBase> res = Utils.GetRandomFromList(spellCards, count);
        List<int> resIDs = new List<int>();
        res.ForEach(card => { resIDs.Add(card.M_CardInstanceId); });
        return resIDs;
    }

    internal void GetATempCardByID(int cardID)
    {
        CardInfo_Base cardInfo = AllCards.GetCard(cardID);
        CardBase newCard = CardBase.InstantiateCardByCardInfo(cardInfo, BattlePlayer, BattlePlayer.GameManager.GenerateNewTempCardInstanceId());
        OnPlayerGetCard(cardID, newCard.M_CardInstanceId);
        HandAddCard(newCard);
    }

    internal void GetACardByID(int cardID, int overrideCardInstanceID = -1)
    {
        CardInfo_Base cardInfo = AllCards.GetCard(cardID);
        CardBase newCard;
        if (overrideCardInstanceID == -1)
        {
            newCard = CardBase.InstantiateCardByCardInfo(cardInfo, BattlePlayer, BattlePlayer.GameManager.GenerateNewTempCardInstanceId());
        }
        else
        {
            newCard = CardBase.InstantiateCardByCardInfo(cardInfo, BattlePlayer, overrideCardInstanceID);
        }

        OnPlayerGetCard(cardID, newCard.M_CardInstanceId);
        HandAddCard(newCard);
    }

    public void OnPlayerGetCard(int cardId, int cardInstanceId)
    {
        DrawCardRequest request1 = new DrawCardRequest(BattlePlayer.ClientId, new DrawCardRequest.CardIdAndInstanceId(cardId, cardInstanceId), true);
        DrawCardRequest request2 = new DrawCardRequest(BattlePlayer.ClientId, new DrawCardRequest.CardIdAndInstanceId(cardId, cardInstanceId), false);
        BattlePlayer?.MyClientProxy?.CurrentClientRequestResponseBundle.AttachedRequests.Add(request1);
        BattlePlayer?.MyEnemyPlayer?.MyClientProxy?.CurrentClientRequestResponseBundle.AttachedRequests.Add(request2);
    }

    public void OnPlayerGetCards(List<DrawCardRequest.CardIdAndInstanceId> cardInfos)
    {
        if (cardInfos.Count == 0) return;
        DrawCardRequest request1 = new DrawCardRequest(BattlePlayer.ClientId, cardInfos, true);
        DrawCardRequest request2 = new DrawCardRequest(BattlePlayer.ClientId, cardInfos, false);
        BattlePlayer?.MyClientProxy?.CurrentClientRequestResponseBundle.AttachedRequests.Add(request1);
        BattlePlayer?.MyEnemyPlayer?.MyClientProxy?.CurrentClientRequestResponseBundle.AttachedRequests.Add(request2);
    }

    internal void DropCard(int cardInstanceId)
    {
        DropCard(GetCardByCardInstanceId(cardInstanceId));
    }

    internal void DropCard(CardBase dropCard)
    {
        DropCardRequest request = new DropCardRequest(BattlePlayer.ClientId, Cards.IndexOf(dropCard));
        BattlePlayer.MyClientProxy.BattleGameManager.Broadcast_AddRequestToOperationResponse(request);
        Cards.Remove(dropCard);
        UsableCards.Remove(dropCard.M_CardInstanceId);
        if (!dropCard.CardInfo.BaseInfo.IsTemp) BattlePlayer.CardDeckManager.CardDeck.RecycleCardInstanceID(dropCard.M_CardInstanceId);
    }

    internal void UseCard(int cardInstanceId, List<int> targetMechIds = null, List<int> targetEquipIds = null, List<int> targetClientIds = null, bool onlyTriggerNotUse = false)
    {
        CardBase useCard = GetCardByCardInstanceId(cardInstanceId);
        Utils.DebugLog("AI UseCard:" + useCard.CardInfo.BaseInfo.CardNames["zh"]);

        if (onlyTriggerNotUse)
        {
            CardBase copyCard = CardBase.InstantiateCardByCardInfo(useCard.CardInfo.Clone(), useCard.BattlePlayer, BattlePlayer.GameManager.GenerateNewTempCardInstanceId());
            BattlePlayer.GameManager.EventManager.Invoke(SideEffectExecute.TriggerTime.OnPlayCard,
                new ExecutorInfo(
                    clientId: BattlePlayer.ClientId,
                    targetClientIds: targetClientIds,
                    targetMechIds: targetMechIds,
                    cardId: copyCard.CardInfo.CardID,
                    cardInstanceId: copyCard.M_CardInstanceId,
                    targetEquipIds: targetEquipIds));
            copyCard.UnRegisterSideEffect();
        }
        else
        {
            BattlePlayer.BattleStatistics.UseCard(useCard.CardInfo);
            UseCardRequest request = new UseCardRequest(BattlePlayer.ClientId, useCard.M_CardInstanceId, useCard.CardInfo.Clone());
            BattlePlayer.MyClientProxy.BattleGameManager.Broadcast_AddRequestToOperationResponse(request);
            BattlePlayer.UseMetal(useCard.CardInfo.BaseInfo.Metal);
            BattlePlayer.UseEnergy(useCard.CardInfo.BaseInfo.Energy);
            BattlePlayer.GameManager.EventManager.Invoke(SideEffectExecute.TriggerTime.OnPlayCard,
                new ExecutorInfo(
                    clientId: BattlePlayer.ClientId,
                    targetClientIds: targetClientIds,
                    targetMechIds: targetMechIds,
                    cardId: useCard.CardInfo.CardID,
                    cardInstanceId: cardInstanceId,
                    targetEquipIds: targetEquipIds));
            if (!useCard.CardInfo.BaseInfo.IsTemp)
            {
                if (useCard.CardInfo.BaseInfo.CardType == CardTypes.Spell || useCard.CardInfo.BaseInfo.CardType == CardTypes.Energy)
                {
                    BattlePlayer.CardDeckManager.CardDeck.RecycleCardInstanceID(cardInstanceId);
                }
            }

            useCard.UnRegisterSideEffect();
            Cards.Remove(useCard);
            UsableCards.Remove(useCard.M_CardInstanceId);
        }
    }

    public void BeginRound()
    {
        RefreshAllCardUsable();
    }

    public void EndRound()
    {
        SetAllCardUnusable();
    }

    public void RefreshAllCardUsable() //刷新所有卡牌是否可用
    {
        foreach (CardBase card in Cards) card.Usable = (BattlePlayer == BattlePlayer.GameManager.CurrentPlayer) && card.M_Metal <= BattlePlayer.MetalLeft && card.M_Energy <= BattlePlayer.EnergyLeft;
    }

    public void SetAllCardUnusable() //禁用所有手牌
    {
        foreach (CardBase card in Cards) card.Usable = false;
    }

    #region Utils

    internal CardBase GetCardByCardInstanceId(int cardInstanceId)
    {
        foreach (CardBase serverCardBase in Cards)
        {
            if (serverCardBase.M_CardInstanceId == cardInstanceId)
            {
                return serverCardBase;
            }
        }

        return null;
    }

    internal CardInfo_Base GetHandCardInfo(int cardInstanceId)
    {
        foreach (CardBase serverCardBase in Cards)
        {
            if (serverCardBase.M_CardInstanceId == cardInstanceId)
            {
                return serverCardBase.CardInfo;
            }
        }

        return null;
    }

    #endregion
}