﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Xml;
using MyCardGameCommon;

public class CardDeck
{
    /// <summary>
    /// 本类中封装卡组操作的基本功能
    /// </summary>
    private CardDeckInfo M_CardDeckInfo;

    private List<CardInfo_Base> Cards = new List<CardInfo_Base>();
    private List<CardInfo_Base> AbandonCards = new List<CardInfo_Base>();

    public bool IsEmpty = false;
    public bool IsAbandonCardsEmpty = false;

    public delegate void OnCardDeckCountChange(int count);

    public OnCardDeckCountChange CardDeckCountChangeHandler;

    private void checkEmpty()
    {
        IsEmpty = Cards.Count == 0;
        IsAbandonCardsEmpty = AbandonCards.Count == 0;
    }

    public CardDeck(CardDeckInfo cdi, OnCardDeckCountChange handler)
    {
        M_CardDeckInfo = cdi;
        CardDeckCountChangeHandler = handler;
        AddCards(AllCards.GetCards(M_CardDeckInfo.CardIDs));
        checkEmpty();
        if (GamePlaySettings.SuffleCardDeck) SuffleSelf();
    }

    private void AddCard(CardInfo_Base cardInfo)
    {
        Cards.Add(cardInfo);
        CardDeckCountChangeHandler(Cards.Count);
    }

    private void AddCard(CardInfo_Base cardInfo, int index)
    {
        Cards.Insert(index, cardInfo);
        CardDeckCountChangeHandler(Cards.Count);
    }

    private void AddCards(List<CardInfo_Base> cardInfos)
    {
        Cards.AddRange(cardInfos);
        CardDeckCountChangeHandler(Cards.Count);
    }

    private void RemoveCard(CardInfo_Base cardInfo)
    {
        Cards.Remove(cardInfo);
        CardDeckCountChangeHandler(Cards.Count);
    }

    private void RemoveCards(List<CardInfo_Base> cardInfos)
    {
        foreach (CardInfo_Base cardInfoBase in cardInfos)
        {
            Cards.Remove(cardInfoBase);
        }

        CardDeckCountChangeHandler(Cards.Count);
    }

    public CardInfo_Type FindATypeOfCard<CardInfo_Type>() where CardInfo_Type : CardInfo_Base
    {
        foreach (CardInfo_Base cb in Cards)
        {
            if (cb is CardInfo_Type)
            {
                return (CardInfo_Type) cb;
            }
        }

        return null;
    }

    public List<CardInfo_Type> FindATypeOfCards<CardInfo_Type>(int cardNumber) where CardInfo_Type : CardInfo_Base
    {
        List<CardInfo_Type> resList = new List<CardInfo_Type>();
        int count = 0;
        foreach (CardInfo_Base cb in Cards)
        {
            if (cb is CardInfo_Type)
            {
                count++;
                resList.Add((CardInfo_Type) cb);
                if (count >= cardNumber)
                {
                    break;
                }
            }
        }

        return resList;
    }

    public CardInfo_Base DrawCardOnTop()
    {
        if (Cards.Count > 0)
        {
            CardInfo_Base res = Cards[0];
            RemoveCard(res);
            AbandonCards.Add(res);
            checkEmpty();
            return res;
        }
        else
        {
            return null;
        }
    }

    public List<CardInfo_Base> DrawCardsOnTop(int cardNumber)
    {
        List<CardInfo_Base> resList = new List<CardInfo_Base>();
        for (int i = 0; i < Math.Min(Cards.Count, cardNumber); i++)
        {
            resList.Add(Cards[i]);
        }

        foreach (CardInfo_Base cb in resList)
        {
            RemoveCard(cb);
            AbandonCards.Add(cb);
            checkEmpty();
        }

        return resList;
    }

    public void AddCardToButtom(CardInfo_Base newCard)
    {
        AddCard(newCard);
        checkEmpty();
    }

    public CardInfo_Base GetFirstCardInfo()
    {
        if (Cards.Count > 0)
        {
            return Cards[0];
        }
        else
        {
            return null;
        }
    }

    public List<CardInfo_Base> GetTopCardsInfo(int cardNumber)
    {
        List<CardInfo_Base> resList = new List<CardInfo_Base>();
        for (int i = 0; i < Math.Min(Cards.Count, cardNumber); i++)
        {
            resList.Add(Cards[i]);
        }

        return resList;
    }

    public void SuffleSelf()
    {
        Suffle(Cards);
    }

    public static void Suffle(List<CardInfo_Base> targetCardList)
    {
        for (int i = 0; i < targetCardList.Count * 1; i++)
        {
            int cardNum1 = new Random().Next(0, targetCardList.Count);
            int cardNum2 = new Random().Next(0, targetCardList.Count);
            if (cardNum1 != cardNum2)
            {
                CardInfo_Base tmp = targetCardList[cardNum1];
                targetCardList[cardNum1] = targetCardList[cardNum2];
                targetCardList[cardNum2] = tmp;
            }
            else
            {
                i--;
            }
        }
    }

    public bool GetARetinueCardToTheTop()
    {
        CardInfo_Base target_cb = null;
        foreach (CardInfo_Base cb in Cards)
        {
            if (cb.BaseInfo.CardType == CardTypes.Retinue && !cb.BattleInfo.IsSodier)
            {
                target_cb = cb;
                break;
            }
        }

        if (target_cb != null)
        {
            RemoveCard(target_cb);
            AddCard(target_cb, 0);
            return true;
        }

        return false;
    }

    public void AbandonCardRecycle()
    {
        foreach (CardInfo_Base ac in AbandonCards)
        {
            AddCard(ac);
        }

        checkEmpty();
        AbandonCards.Clear();
        Suffle(Cards);
    }
}

public struct CardDeckInfo
{
    public int CardNumber;

    public int[] CardIDs;

    public CardDeckInfo(int[] cardIDs)
    {
        CardIDs = cardIDs;
        CardNumber = cardIDs.Length;
    }
}