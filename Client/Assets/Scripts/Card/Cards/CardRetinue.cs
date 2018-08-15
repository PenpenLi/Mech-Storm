﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class CardRetinue : CardBase
{
    public override void PoolRecycle()
    {
        base.PoolRecycle();
        if (Weapon)
        {
            Weapon.PoolRecycle();
            Weapon = null;
        }

        if (Shield)
        {
            Shield.PoolRecycle();
            Shield = null;
        }

        if (Pack)
        {
            //Pack.PoolRecycle();
            Pack = null;
        }

        if (MA)
        {
            //MA.PoolRecycle();
            MA = null;
        }

        if (Pack) Pack = null;
        if (MA) MA = null;
    }

    #region 卡牌上各模块

    [SerializeField] private Text Text_RetinueName;
    [SerializeField] private Text Text_RetinueDesc;

    [SerializeField] private GameObject Block_RetinueTotalLife;
    GameObject GoNumberSet_RetinueTotalLife;
    CardNumberSet CardNumberSet_RetinueTotalLife;


    public override void Initiate(CardInfo_Base cardInfo, ClientPlayer clientPlayer, bool isCardSelect)
    {
        base.Initiate(cardInfo, clientPlayer, isCardSelect);
        M_RetinueName = cardInfo.BaseInfo.CardName;
        M_RetinueDesc = ((CardInfo_Retinue) cardInfo).GetCardDescShow();
        M_RetinueLeftLife = cardInfo.LifeInfo.Life;
        M_RetinueTotalLife = cardInfo.LifeInfo.TotalLife;
        M_RetinueAttack = cardInfo.BattleInfo.BasicAttack;
        M_RetinueArmor = cardInfo.BattleInfo.BasicArmor;
        M_RetinueShield = cardInfo.BattleInfo.BasicShield;

        Slot1.ClientPlayer = ClientPlayer;
        Slot1.MSlotTypes = cardInfo.SlotInfo.Slot1;
        Slot2.ClientPlayer = ClientPlayer;
        Slot2.MSlotTypes = cardInfo.SlotInfo.Slot2;
        Slot3.ClientPlayer = ClientPlayer;
        Slot3.MSlotTypes = cardInfo.SlotInfo.Slot3;
        Slot4.ClientPlayer = ClientPlayer;
        Slot4.MSlotTypes = cardInfo.SlotInfo.Slot4;
    }

    private string m_RetinueName;

    public string M_RetinueName
    {
        get { return m_RetinueName; }
        set
        {
            m_RetinueName = value;
            Text_RetinueName.text = value;
        }
    }

    private string m_RetinueDesc;

    public string M_RetinueDesc
    {
        get { return m_RetinueDesc; }
        set
        {
            m_RetinueDesc = value;
            Text_RetinueDesc.text = value;
        }
    }

    private int m_RetinueLeftLife;

    public int M_RetinueLeftLife
    {
        get { return m_RetinueLeftLife; }
        set { m_RetinueLeftLife = value; }
    }

    private int m_RetinueTotalLife;

    public int M_RetinueTotalLife
    {
        get { return m_RetinueTotalLife; }
        set
        {
            m_RetinueTotalLife = value;
            initiateNumbers(ref GoNumberSet_RetinueTotalLife, ref CardNumberSet_RetinueTotalLife, NumberSize.Big, CardNumberSet.TextAlign.Right, Block_RetinueTotalLife);
            CardNumberSet_RetinueTotalLife.Number = m_RetinueTotalLife;
        }
    }

    private int m_RetinueAttack;

    public int M_RetinueAttack
    {
        get { return m_RetinueAttack; }
        set { m_RetinueAttack = value; }
    }

    private int m_RetinueArmor;

    public int M_RetinueArmor
    {
        get { return m_RetinueArmor; }
        set { m_RetinueArmor = value; }
    }

    private int m_RetinueShield;

    public int M_RetinueShield
    {
        get { return m_RetinueShield; }
        set { m_RetinueShield = value; }
    }

    #endregion

    #region 拼装上的模块

    [SerializeField] private Slot Slot1;
    [SerializeField] private Slot Slot2;
    [SerializeField] private Slot Slot3;
    [SerializeField] private Slot Slot4;
    internal GameObject Pack;
    internal ModuleWeapon Weapon;
    internal ModuleShield Shield;
    internal GameObject MA;

    # endregion

    public override void DragComponent_OnMousePressed(BoardAreaTypes boardAreaType, List<Slot> slots, ModuleRetinue moduleRetinue, Vector3 dragLastPosition)
    {
        base.DragComponent_OnMousePressed(boardAreaType, slots, moduleRetinue, dragLastPosition);

        if (boardAreaType == ClientPlayer.MyBattleGroundArea && !ClientPlayer.MyBattleGroundManager.BattleGroundIsFull) //拖随从牌到战场区域
        {
            int previewPosition = ClientPlayer.MyBattleGroundManager.ComputePosition(dragLastPosition);
            ClientPlayer.MyBattleGroundManager.AddRetinuePreview(previewPosition);
        }
        else //离开战场区域
        {
            ClientPlayer.MyBattleGroundManager.RemoveRetinuePreview();
        }
    }

    public override void DragComponent_OnMouseUp(BoardAreaTypes boardAreaType, List<Slot> slots, ModuleRetinue moduleRetinue, Vector3 dragLastPosition, Vector3 dragBeginPosition, Quaternion dragBeginQuaternion)
    {
        base.DragComponent_OnMouseUp(boardAreaType, slots, moduleRetinue, dragLastPosition, dragBeginPosition, dragBeginQuaternion);

        bool summonTarget = false; //召唤时是否需要指定目标

        TargetSideEffect.TargetRange TargetRange = TargetSideEffect.TargetRange.None; //指定目标所属范围
        foreach (SideEffectBase se in CardInfo.SideEffects_OnSummoned)
        {
            if (se is TargetSideEffect)
            {
                if (((TargetSideEffect) se).IsNeedChoise)
                {
                    summonTarget = true;
                    TargetRange = ((TargetSideEffect) se).M_TargetRange;
                }
            }
        }

        if (!summonTarget) //无指定目标的副作用
        {
            if (boardAreaType != ClientPlayer.MyHandArea) //脱手即出牌
            {
                summonRetinueRequest(dragLastPosition, TARGET_RETINUE_SELECT_NONE);
            }
            else
            {
                ClientPlayer.MyBattleGroundManager.RemoveRetinuePreview();
                transform.SetPositionAndRotation(dragBeginPosition, dragBeginQuaternion); //如果脱手地方还在手中，则收回
                ClientPlayer.MyHandManager.RefreshCardsPlace();
            }
        }
        else
        {
            if (boardAreaType != ClientPlayer.MyHandArea) //脱手即假召唤，开始展示指定目标箭头
            {
                summonRetinuePreview(dragLastPosition, TargetRange);
            }
            else
            {
                ClientPlayer.MyBattleGroundManager.RemoveRetinuePreview();
                transform.SetPositionAndRotation(dragBeginPosition, dragBeginQuaternion); //如果脱手地方还在手中，则收回
                ClientPlayer.MyHandManager.RefreshCardsPlace();
            }
        }
    }


    #region 卡牌效果

    public const int TARGET_RETINUE_SELECT_NONE = -2;

    //召唤随从
    private void summonRetinueRequest(Vector3 dragLastPosition, int targetRetinueId)
    {
        if (ClientPlayer.MyBattleGroundManager.BattleGroundIsFull)
        {
            ClientPlayer.MyHandManager.RefreshCardsPlace();
            return;
        }

        int battleGroundIndex = ClientPlayer.MyBattleGroundManager.ComputePosition(dragLastPosition);
        SummonRetinueRequest request = new SummonRetinueRequest(Client.CS.Proxy.ClientId, M_CardInstanceId, battleGroundIndex, new MyCardGameCommon.Vector3(dragLastPosition.x, dragLastPosition.y, dragLastPosition.z), targetRetinueId, false, ModuleRetinue.CLIENT_TEMP_RETINUE_ID_NORMAL);
        Client.CS.Proxy.SendMessage(request);
    }


    //假召唤随从
    private void summonRetinuePreview(Vector3 dragLastPosition, TargetSideEffect.TargetRange targetRange)
    {
        if (ClientPlayer.MyBattleGroundManager.BattleGroundIsFull)
        {
            ClientPlayer.MyHandManager.RefreshCardsPlace();
            return;
        }

        ClientPlayer.MyHandManager.SetCurrentSummonRetinuePreviewCard(this);

        int battleGroundIndex = ClientPlayer.MyBattleGroundManager.ComputePosition(dragLastPosition);
        if (ClientPlayer.MyBattleGroundManager.BattleGroundIsEmpty)
        {
            SummonRetinueRequest request = new SummonRetinueRequest(Client.CS.Proxy.ClientId, M_CardInstanceId, battleGroundIndex, new MyCardGameCommon.Vector3(dragLastPosition.x, dragLastPosition.y, dragLastPosition.z), TARGET_RETINUE_SELECT_NONE, false, ModuleRetinue.CLIENT_TEMP_RETINUE_ID_NORMAL);
            Client.CS.Proxy.SendMessage(request);
        }
        else
        {
            ClientPlayer.MyBattleGroundManager.SummonRetinuePreview(this, battleGroundIndex, targetRange);
        }
    }

    #endregion
}