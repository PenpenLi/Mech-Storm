﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class BattleGroundManager : MonoBehaviour
{
    internal bool BattleGroundIsFull;
    internal bool BattleGroundIsEmpty = true;
    private Vector3 _defaultRetinuePosition = Vector3.zero;

    internal ClientPlayer ClientPlayer;
    public List<ModuleRetinue> Retinues = new List<ModuleRetinue>();

    private int retinueCount;

    private int RetinueCount //接到协议就计算随从数量，不会等到动画放完
    {
        get { return retinueCount; }
        set
        {
            retinueCount = value;
            BattleGroundIsFull = retinueCount >= GamePlaySettings.MaxRetinueNumber;
            BattleGroundIsEmpty = retinueCount == 0;
        }
    }

    public void Reset()
    {
        foreach (ModuleRetinue moduleRetinue in Retinues)
        {
            moduleRetinue.PoolRecycle();
        }

        ClientPlayer = null;
        previewRetinuePlace = (int) previewRetinuePlaces.NoPreviewRetinueNow;
        Retinues.Clear();
        RetinueCount = 0;
        RemoveRetinues.Clear();
    }

    #region 位置计算

    internal int ComputePosition(Vector3 dragLastPosition)
    {
        int index = Mathf.RoundToInt(Mathf.Floor(dragLastPosition.x / GameManager.GM.RetinueInterval - (Retinues.Count + 1) % 2 * 0.5f) + (Retinues.Count / 2 + 1));
        if (index < 0) index = 0;
        if (index >= Retinues.Count) index = Retinues.Count;
        return index;
    }

    internal ModuleRetinue CheckRetinueOnPosition(Vector3 dragLastPosition)
    {
        var index = Mathf.RoundToInt(Mathf.Floor(dragLastPosition.x / GameManager.GM.RetinueInterval - (Retinues.Count + 1) % 2 * 0.5f) + (Retinues.Count / 2 + 1));
        if (index < 0 || index >= Retinues.Count)
            return null;
        return Retinues[index];
    }

    #endregion

    #region 召唤和移除

    public ModuleRetinue AddRetinue_PrePass(CardInfo_Retinue retinueCardInfo, int retinueId, int clientRetinueTempId)
    {
        if (ClientPlayer == null) return null;
        if (previewRetinuePlace != (int) previewRetinuePlaces.NoPreviewRetinueNow)
        {
            previewRetinuePlace = (int) previewRetinuePlaces.NoPreviewRetinueNow;
        }

        bool isSummonedBeforeByPreview = false;
        if (clientRetinueTempId >= 0)
        {
            foreach (ModuleRetinue moduleRetinue in Retinues)
            {
                if (moduleRetinue.M_ClientTempRetinueID == clientRetinueTempId) //匹配
                {
                    moduleRetinue.M_RetinueID = retinueId; //赋予正常ID
                    moduleRetinue.M_ClientTempRetinueID = (int) ModuleRetinue.ClientTempRetinueID.Normal; //恢复普通
                    isSummonedBeforeByPreview = true;
                    break;
                }
            }
        }

        if (!isSummonedBeforeByPreview)
        {
            ModuleRetinue retinue = GameObjectPoolManager.GOPM.Pool_ModuleRetinuePool.AllocateGameObject(transform).GetComponent<ModuleRetinue>();
            retinue.transform.position = GameObjectPoolManager.GOPM.Pool_ModuleRetinuePool.transform.position;
            retinue.Initiate(retinueCardInfo, ClientPlayer);
            retinue.transform.Rotate(Vector3.up, 180);
            retinue.M_RetinueID = retinueId;
            addPrePassRetinueQueue.Enqueue(retinue);
            RetinueCount++;
            return retinue;
        }

        return null;
    }


    private Queue<ModuleRetinue> addPrePassRetinueQueue = new Queue<ModuleRetinue>();

    public void AddRetinue(int retinuePlaceIndex)
    {
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_Main, retinuePlaceIndex), "Co_RefreshBattleGroundAnim");
    }

    public void RemoveRetinue(int retinueId)
    {
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RemoveRetinue(GetRetinue(retinueId)), "Co_RemoveRetinue");
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_Main), "Co_RefreshBattleGroundAnim");
    }

    public List<ModuleRetinue> RemoveRetinues = new List<ModuleRetinue>(); //即将要被移除的随从名单

    public void RemoveRetinueTogatherAdd(int retinueId)
    {
        ModuleRetinue retinue = GetRetinue(retinueId);
        retinue.CannotAttackBecauseDie = true;
        RetinueCount--;
        RemoveRetinues.Add(retinue);
    }

    public void RemoveRetinueTogather()
    {
        foreach (ModuleRetinue removeRetinue in RemoveRetinues)
        {
            removeRetinue.PoolRecycle();
            Retinues.Remove(removeRetinue);
            ClientLog.CL.Print("remove:" + removeRetinue.M_RetinueID);
        }

        RemoveRetinues.Clear();
    }

    public void RemoveRetinueTogatherEnd()
    {
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_Main), "Co_RefreshBattleGroundAnim");
    }

    IEnumerator Co_RemoveRetinue(ModuleRetinue retinue)
    {
        retinue.PoolRecycle();
        Retinues.Remove(retinue);
        RetinueCount--;
        yield return null;
        BattleEffectsManager.BEM.Effect_Main.EffectEnd();
    }

    #endregion

    #region 出牌召唤预览

    private int previewRetinuePlace;

    private enum previewRetinuePlaces
    {
        NoPreviewRetinueNow = -1
    }

    public void AddRetinuePreview(int placeIndex)
    {
        if (Retinues.Count == 0) return;
        if (previewRetinuePlace == (int) previewRetinuePlaces.NoPreviewRetinueNow || previewRetinuePlace != placeIndex)
        {
            previewRetinuePlace = placeIndex;
            BattleEffectsManager.BEM.Effect_RefreshBattleGroundOnAddRetinue.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_RefreshBattleGroundOnAddRetinue), "Co_RefreshBattleGroundAnim");
        }
    }


    public void RemoveRetinuePreview()
    {
        if (previewRetinuePlace != (int) previewRetinuePlaces.NoPreviewRetinueNow)
        {
            previewRetinuePlace = (int) previewRetinuePlaces.NoPreviewRetinueNow;
            BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_RefreshBattleGroundAnim(BattleEffectsManager.BEM.Effect_Main), "Co_RefreshBattleGroundAnim");
        }
    }

    #endregion

    #region 能指定目标的随从的预召唤

    private static int clientRetinueTempId = 0;

    public static int GenerateClientRetinueTempId()
    {
        return clientRetinueTempId++;
    }

    public delegate void SummonRetinueTarget(int targetRetinueId, bool isClientRetinueTempId = false);

    private CardRetinue currentSummonPreviewRetinueCard;
    public ModuleRetinue CurrentSummonPreviewRetinue;

    public void SummonRetinuePreview(CardRetinue retinueCard, int retinuePlaceIndex, TargetSideEffect.TargetRange targetRange) //用于具有指定目标的副作用的随从的召唤预览、显示指定箭头
    {
        currentSummonPreviewRetinueCard = retinueCard;
        ModuleRetinue retinue = AddRetinue_PrePass((CardInfo_Retinue) retinueCard.CardInfo, (int) ModuleRetinue.RetinueID.Empty, (int) ModuleRetinue.ClientTempRetinueID.SummonPreviewNotConfirm);
        CurrentSummonPreviewRetinue = retinue;
        AddRetinue(retinuePlaceIndex);
        DragManager.DM.SummonRetinueTargetHandler = SummonRetinueTargetConfirm;
        DragManager.DM.StartArrowAiming(retinue, targetRange);
    }

    public void SummonRetinueTargetConfirm(int targetRetinueId, bool isClientRetinueTempId)
    {
        if (targetRetinueId == (int) DragManager.TargetSelect.None) //未选择目标
        {
            RemoveRetinue((int) ModuleRetinue.RetinueID.Empty);
            ClientPlayer.MyHandManager.CancelSummonRetinuePreview();
        }
        else
        {
            StartCoroutine(Co_RetrySummonRequest(CurrentSummonPreviewRetinue, currentSummonPreviewRetinueCard.M_CardInstanceId, targetRetinueId, isClientRetinueTempId));
        }
    }

    IEnumerator Co_RetrySummonRequest(ModuleRetinue retinue, int cardInstanceId, int targetRetinueId, bool isClientRetinueTempId)
    {
        while (true)
        {
            int battleGroundIndex = Retinues.IndexOf(retinue); //确定的时候再获取位置信息（召唤的过程中可能会有协议没有跑完，会有随从生成）
            if (battleGroundIndex != -1)
            {
                retinue.M_ClientTempRetinueID = GenerateClientRetinueTempId();
                SummonRetinueRequest request = new SummonRetinueRequest(Client.CS.Proxy.ClientId, cardInstanceId, battleGroundIndex, new MyCardGameCommon.Vector3(0, 0, 0), targetRetinueId, isClientRetinueTempId, retinue.M_ClientTempRetinueID);
                Client.CS.Proxy.SendMessage(request);
                break;
            }

            yield return null;
        }

        yield return null;
    }

    #endregion


    IEnumerator Co_RefreshBattleGroundAnim(BattleEffectsManager.Effects myParentEffects) //不新增随从,刷新
    {
        return Co_RefreshBattleGroundAnim(myParentEffects, (int) retinuePlaceIndex.NoNewRetinue);
    }

    private enum retinuePlaceIndex
    {
        NoNewRetinue = -1,
    }

    IEnumerator Co_RefreshBattleGroundAnim(BattleEffectsManager.Effects myParentEffects, int retinuePlaceIndex)
    {
        if (retinuePlaceIndex != (int) BattleGroundManager.retinuePlaceIndex.NoNewRetinue) //新增随从
        {
            ModuleRetinue retinue = addPrePassRetinueQueue.Dequeue();

            Retinues.Insert(retinuePlaceIndex, retinue);
            retinue.OnSummon();
            retinue.transform.localPosition = _defaultRetinuePosition;
            retinue.transform.transform.Translate(Vector3.left * (Retinues.IndexOf(retinue) - Retinues.Count / 2.0f + 0.5f) * GameManager.GM.RetinueInterval, Space.Self);
        }

        float duration = 0.05f;
        float tick = 0;

        Vector3[] translations = new Vector3[Retinues.Count];

        int actualPlaceCount = previewRetinuePlace == (int) previewRetinuePlaces.NoPreviewRetinueNow ? Retinues.Count : Retinues.Count + 1;

        List<ModuleRetinue> movingRetinues = new List<ModuleRetinue>();

        for (int i = 0; i < Retinues.Count; i++)
        {
            movingRetinues.Add(Retinues[i]);

            int actualPlace = i;
            if (previewRetinuePlace != (int) previewRetinuePlaces.NoPreviewRetinueNow && i >= previewRetinuePlace)
            {
                actualPlace += 1;
            }

            Vector3 ori = Retinues[i].transform.localPosition;
            Vector3 offset = Vector3.left * (actualPlace - actualPlaceCount / 2.0f + 0.5f) * GameManager.GM.RetinueInterval;

            Retinues[i].transform.localPosition = _defaultRetinuePosition;
            Retinues[i].transform.Translate(offset, Space.Self);

            translations[i] = Retinues[i].transform.localPosition - ori;
            Retinues[i].transform.localPosition = ori;
        }


        while (tick <= duration)
        {
            float timeDelta = Mathf.Min(duration - tick, Time.deltaTime);

            for (int i = 0; i < movingRetinues.Count; i++)
            {
                movingRetinues[i].transform.localPosition += translations[i] * timeDelta / duration;
            }

            tick += Time.deltaTime;

            yield return null;
        }

        yield return null;

        myParentEffects.EffectEnd();
    }

    #region 装备牌相关操作会显示提示性Slot边框

    List<SlotAnchor> relatedSlotAnchors = new List<SlotAnchor>();

    private IEnumerator currentShowSlotBloom;

    public void ShowTipSlotBlooms(SlotTypes slotType)
    {
        StopShowSlotBloom();
        foreach (ModuleRetinue retinue in Retinues)
        {
            if (retinue.SlotAnchor1.M_Slot.MSlotTypes == slotType)
            {
                relatedSlotAnchors.Add(retinue.SlotAnchor1);
            }

            if (retinue.SlotAnchor2.M_Slot.MSlotTypes == slotType)
            {
                relatedSlotAnchors.Add(retinue.SlotAnchor2);
            }

            if (retinue.SlotAnchor3.M_Slot.MSlotTypes == slotType)
            {
                relatedSlotAnchors.Add(retinue.SlotAnchor3);
            }

            if (retinue.SlotAnchor4.M_Slot.MSlotTypes == slotType)
            {
                relatedSlotAnchors.Add(retinue.SlotAnchor4);
            }
        }

        currentShowSlotBloom = Co_ShowSlotBloom();
        BattleEffectsManager.BEM.Effect_TipSlotBloom.EffectsShow(currentShowSlotBloom, "Co_ShowSlotBloom");
    }

    IEnumerator Co_ShowSlotBloom()
    {
        while (true)
        {
            foreach (SlotAnchor sa in relatedSlotAnchors)
            {
                sa.ShowHoverGO();
            }

            yield return new WaitForSeconds(0.4f);
            foreach (SlotAnchor sa in relatedSlotAnchors)
            {
                sa.HideHoverShowGO();
            }

            yield return new WaitForSeconds(0.4f);
        }
    }

    public void StopShowSlotBloom()
    {
        if (currentShowSlotBloom != null)
        {
            BattleEffectsManager.SideEffect cur_Effect = BattleEffectsManager.BEM.Effect_TipSlotBloom.GetCurrentSideEffect();
            if (cur_Effect != null && cur_Effect.Enumerator == currentShowSlotBloom)
            {
                BattleEffectsManager.BEM.Effect_TipSlotBloom.EffectEnd();
                ClientLog.CL.PrintWarning("Stop Effect_TipSlotBloom");
                currentShowSlotBloom = null;
            }
        }

        foreach (SlotAnchor sa in relatedSlotAnchors)
        {
            sa.HideHoverShowGO();
        }

        relatedSlotAnchors.Clear();
    }

    #endregion

    public void EquipWeapon(CardInfo_Weapon cardInfo, int retinueId)
    {
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_EquipWeapon(cardInfo, retinueId), "Co_EquipWeapon");
    }

    IEnumerator Co_EquipWeapon(CardInfo_Weapon cardInfo, int retinueId)
    {
        ModuleRetinue retinue = GetRetinue(retinueId);
        ModuleWeapon newModueWeapon = GameObjectPoolManager.GOPM.Pool_ModuleWeaponPool.AllocateGameObject(retinue.transform).GetComponent<ModuleWeapon>();
        newModueWeapon.M_ModuleRetinue = retinue;
        newModueWeapon.Initiate(cardInfo, ClientPlayer);
        retinue.M_Weapon = newModueWeapon;
        yield return new WaitForSeconds(0.2f);
        BattleEffectsManager.BEM.Effect_Main.EffectEnd();
    }

    public void EquipShield(CardInfo_Shield cardInfo, int retinueId)
    {
        BattleEffectsManager.BEM.Effect_Main.EffectsShow(Co_EquipShield(cardInfo, retinueId), "Co_EquipShield");
    }

    IEnumerator Co_EquipShield(CardInfo_Shield cardInfo, int retinueId)
    {
        ModuleRetinue retinue = GetRetinue(retinueId);
        ModuleShield newModuleShield = GameObjectPoolManager.GOPM.Pool_ModuleShieldPool.AllocateGameObject(retinue.transform).GetComponent<ModuleShield>();
        newModuleShield.M_ModuleRetinue = retinue;
        newModuleShield.Initiate(cardInfo, ClientPlayer);
        retinue.M_Shield = newModuleShield;
        yield return null;
        BattleEffectsManager.BEM.Effect_Main.EffectEnd();
    }


    #region Utils

    public ModuleRetinue GetRetinue(int retinueId)
    {
        foreach (ModuleRetinue moduleRetinue in Retinues)
        {
            if (moduleRetinue.M_RetinueID == retinueId)
            {
                return moduleRetinue;
            }
        }

        foreach (ModuleRetinue moduleRetinue in addPrePassRetinueQueue) //预加载的随从也要遍历一遍
        {
            if (moduleRetinue.M_RetinueID == retinueId)
            {
                return moduleRetinue;
            }
        }

        return null;
    }

    public int GetRetinuePlaceIndex(ModuleRetinue moduleRetinue)
    {
        return Retinues.IndexOf(moduleRetinue);
    }

    #endregion

    #region GameProcess

    internal void BeginRound()
    {
        foreach (var mr in Retinues) mr.OnBeginRound();
    }

    internal void EndRound()
    {
        foreach (var mr in Retinues) mr.OnEndRound();
    }

    #endregion
}