﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class CardShield : CardBase
{
    protected override void Awake()
    {
        base.Awake();
        gameObjectPool = GameObjectPoolManager.GOPM.Pool_ShieldCardPool;
    }

    void Start()
    {
    }

    void Update()
    {
    }


    #region 卡牌上各模块

    public TextMesh ShieldName;

    public Text ShieldDesc;

    private string m_ShieldName;

    public string M_ShieldName
    {
        get { return m_ShieldName; }

        set
        {
            m_ShieldName = value;
            ShieldName.text = M_ShieldName;
        }
    }

    private string m_ShieldDesc;

    public string M_ShieldDesc
    {
        get { return m_ShieldDesc; }

        set
        {
            m_ShieldDesc = value;
            ShieldDesc.text = M_ShieldDesc;
        }
    }

    # endregion

    public override void Initiate(CardInfo_Base cardInfo, ClientPlayer clientPlayer)
    {
        base.Initiate(cardInfo, clientPlayer);
        ClientPlayer = clientPlayer;
        CardInfo = cardInfo;
        M_ShieldName = CardInfo.BaseInfo.CardName;
        M_ShieldDesc = ((CardInfo_Shield) cardInfo).GetCardDescShow();
    }

    public override void DragComponent_OnMouseUp(BoardAreaTypes boardAreaType, List<SlotAnchor> slotAnchors, ModuleRetinue moduleRetinue, Vector3 dragLastPosition, Vector3 dragBeginPosition, Quaternion dragBeginQuaternion)
    {
        base.DragComponent_OnMouseUp(boardAreaType, slotAnchors, moduleRetinue, dragLastPosition, dragBeginPosition, dragBeginQuaternion);

        if (boardAreaType != ClientPlayer.MyHandArea) //离开手牌区域
            foreach (var sa in slotAnchors)
                if (sa.M_Slot.MSlotTypes == SlotTypes.Shield && sa.M_Slot.ClientPlayer == ClientPlayer)
                {
                    summonShieldRequest(sa.M_ModuleRetinue, dragLastPosition);
                    ClientPlayer.MyBattleGroundManager.StopShowSlotBloom();
                    return;
                }

        transform.SetPositionAndRotation(dragBeginPosition, dragBeginQuaternion); //如果脱手地方还在手中，则收回
        ClientPlayer.MyBattleGroundManager.StopShowSlotBloom();
        ClientPlayer.MyHandManager.RefreshCardsPlace();
    }


    public override float DragComponnet_DragDistance()
    {
        return 5f;
    }

    #region 卡牌效果

    //装备武器
    private void summonShieldRequest(ModuleRetinue moduleRetinue, Vector3 dragLastPosition)
    {
        EquipShieldRequest request = new EquipShieldRequest(Client.CS.Proxy.ClientId, M_CardInstanceId, moduleRetinue.M_RetinueID, 0, new MyCardGameCommon.Vector3(dragLastPosition.x, dragLastPosition.y, dragLastPosition.z));
        Client.CS.Proxy.SendMessage(request);
    }

    #endregion
}