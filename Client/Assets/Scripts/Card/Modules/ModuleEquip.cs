﻿using System.Collections;
using UnityEngine;

public abstract class ModuleEquip : ModuleBase
{
    public int M_EquipID;
    internal ModuleRetinue M_ModuleRetinue;
    public TextMesh Name;
    public TextMesh Name_en;

    public Renderer M_Bloom;
    public Renderer M_BloomSE;
    public Renderer M_BloomSE_Sub;

    public Animator EquipAnim;
    public Animator BloomSEAnim;
    public Animator BloomSE_SubAnim;

    public override void PoolRecycle()
    {
        base.PoolRecycle();
        if (EquipAnim) EquipAnim.SetTrigger("Hide");
    }

    public override void Initiate(CardInfo_Base cardInfo, ClientPlayer clientPlayer)
    {
        base.Initiate(cardInfo, clientPlayer);
        if (M_Bloom) M_Bloom.gameObject.SetActive(false);
        if (M_BloomSE) M_BloomSE.gameObject.SetActive(false);
        if (M_BloomSE_Sub) M_BloomSE_Sub.gameObject.SetActive(false);
    }

    public virtual void SetPreview()
    {
        if (M_Bloom) M_Bloom.gameObject.SetActive(true);
    }

    public virtual void SetNoPreview()
    {
    }

    public override void MouseHoverComponent_OnHover1Begin(Vector3 mousePosition)
    {
        base.MouseHoverComponent_OnHover1Begin(mousePosition);
        if (M_Bloom) M_Bloom.gameObject.SetActive(true);
    }

    public override void MouseHoverComponent_OnHover1End()
    {
        base.MouseHoverComponent_OnHover1End();
        if (M_Bloom) M_Bloom.gameObject.SetActive(false);
    }

    public override void MouseHoverComponent_OnMousePressEnterImmediately(Vector3 mousePosition)
    {
        base.MouseHoverComponent_OnMousePressEnterImmediately(mousePosition);
        M_Bloom.gameObject.SetActive(true);
        M_BloomSE.gameObject.SetActive(true);
        M_BloomSE_Sub.gameObject.SetActive(true);
        BloomSEAnim.SetTrigger("OnSE");
        BloomSE_SubAnim.SetTrigger("OnSE");
    }
    public override void MouseHoverComponent_OnMousePressLeaveImmediately()
    {
        base.MouseHoverComponent_OnMousePressLeaveImmediately();
        M_Bloom.gameObject.SetActive(false);
        BloomSEAnim.SetTrigger("Reset");
        BloomSE_SubAnim.SetTrigger("Reset");
        M_BloomSE.gameObject.SetActive(false);
        M_BloomSE_Sub.gameObject.SetActive(false);
    }

    public override void DragComponent_SetStates(ref bool canDrag, ref DragPurpose dragPurpose)
    {
        canDrag = false;
        dragPurpose = CardInfo.BaseInfo.DragPurpose;
    }

    public override void ChangeColor(Color color)
    {
        base.ChangeColor(color);
        ClientUtils.ChangeColor(M_Bloom, color);
        ClientUtils.ChangeColor(M_BloomSE, color);
    }

    #region SE

    public override void OnShowEffects(SideEffectBundle.TriggerTime triggerTime, SideEffectBundle.TriggerRange triggerRange)
    {
        BattleEffectsManager.Instance.Effect_Main.EffectsShow(Co_ShowSideEffectBloom(ClientUtils.HTMLColorToColor(CardInfo.BaseInfo.GetCardColor()), 0.5f), "ShowSideEffectBloom");
    }

    IEnumerator Co_ShowSideEffectBloom(Color color, float duration)
    {
        M_BloomSE.gameObject.SetActive(true);
        M_BloomSE_Sub.gameObject.SetActive(true);
        BloomSEAnim.SetTrigger("OnSE");
        BloomSE_SubAnim.SetTrigger("OnSE");
        ClientUtils.ChangeColor(M_BloomSE, color);
        ClientUtils.ChangeColor(M_BloomSE_Sub, color);
        AudioManager.Instance.SoundPlay("sfx/OnSE");
        yield return new WaitForSeconds(duration);
        BloomSEAnim.SetTrigger("Reset");
        BloomSE_SubAnim.SetTrigger("Reset");
        M_BloomSE.gameObject.SetActive(false);
        M_BloomSE_Sub.gameObject.SetActive(false);
        BattleEffectsManager.Instance.Effect_Main.EffectEnd();
    }

    #endregion
}