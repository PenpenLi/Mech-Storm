﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

internal class BigBonusItem : BonusItem_Base
{
    public override void PoolRecycle()
    {
        if (CurrentCard != null) CurrentCard.PoolRecycle();
        CurrentCard = null;
        base.PoolRecycle();
    }

    [SerializeField] private Transform CardContainer;
    [SerializeField] private Transform CardRotationSample;
    [SerializeField] private Text UnlockText;
    [SerializeField] private Image UnlockImage;
    [SerializeField] private Image CardMask;

    public CardBase CurrentCard;

    void Awake()
    {
        UnlockText.text = GameManager.Instance.IsEnglish ? "To unlock this card" : "解锁该卡片";
    }

    public override void Initialize(Bonus bonus)
    {
        base.Initialize(bonus);
        UnlockText.enabled = false;
        UnlockImage.enabled = false;
        CardMask.enabled = false;
        switch (bonus.M_BonusType)
        {
            case Bonus.BonusType.UnlockCardByID:
            {
                CurrentCard = CardBase.InstantiateCardByCardInfo(AllCards.GetCard(bonus.Value), CardContainer, RoundManager.Instance.SelfClientPlayer, false);
                CurrentCard.transform.localScale = CardRotationSample.localScale;
                CurrentCard.transform.rotation = CardRotationSample.rotation;
                CurrentCard.transform.position = CardRotationSample.position;
                CurrentCard.SetOrderInLayer(1);
                UnlockText.enabled = true;
                UnlockImage.enabled = true;
                CardMask.enabled = true;
                Color newColor = ClientUtils.HTMLColorToColor(CurrentCard.CardInfo.GetCardColor());
                UnlockImage.color = new Color(newColor.r / 1.5f, newColor.g / 1.5f, newColor.b / 1.5f);
                break;
            }
        }
    }
}