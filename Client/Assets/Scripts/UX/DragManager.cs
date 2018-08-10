﻿using UnityEngine;
using System.Collections;

internal class DragManager : MonoBehaviour
{
    /// <summary>
    /// 一次拖动一个卡牌、随从、英雄
    /// </summary>
    private static DragManager dm;

    public static DragManager DM
    {
        get
        {
            if (!dm)
            {
                dm = FindObjectOfType(typeof(DragManager)) as DragManager;
            }

            return dm;
        }
    }

    void Awake()
    {
        modulesLayer = 1 << LayerMask.NameToLayer("Modules");
        retinueLayer = 1 << LayerMask.NameToLayer("Retinues");
        cardLayer = 1 << LayerMask.NameToLayer("Cards");
    }

    void Start()
    {
    }

    int modulesLayer;
    int retinueLayer;
    int cardLayer;

    internal DragComponent CurrentDrag;
    internal Arrow CurrentArrow;


    void Update()
    {
        if (SelectCardDeckManager.SCDM.IsShowing()) return;
        if (!IsSummonPreview)
        {
            CommonDrag();
        }
        else
        {
            SummonPreviewDrag();
        }
    }


    private void CommonDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!CurrentDrag)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit raycast;
                Physics.Raycast(ray, out raycast, 10f, cardLayer | retinueLayer | modulesLayer);
                if (raycast.collider != null)
                {
                    ColliderReplace colliderReplace = raycast.collider.gameObject.GetComponent<ColliderReplace>();
                    if (colliderReplace)
                    {
                        CurrentDrag = colliderReplace.MyCallerCard.GetComponent<DragComponent>();
                    }
                    else
                    {
                        CurrentDrag = raycast.collider.gameObject.GetComponent<DragComponent>();
                    }

                    CurrentDrag.IsOnDrag = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (CurrentDrag)
            {
                CurrentDrag.IsOnDrag = false;
                CurrentDrag = null;
            }
        }
    }

    # region 召唤随从指定目标预览

    internal bool IsSummonPreview;
    private ModuleRetinue CurrentSummonPreviewRetinue;
    public BattleGroundManager.SummonRetinueTarget SummonRetinueTargetHandler;
    public TargetSideEffect.TargetRange SummonRetinueTargetRange;

    public enum TargetSelect
    {
        None = -2
    }

    public void StartArrowAiming(ModuleRetinue retinue, TargetSideEffect.TargetRange targetRange)
    {
        IsSummonPreview = true;
        CurrentSummonPreviewRetinue = retinue;
        SummonRetinueTargetRange = targetRange;
    }

    private void SummonPreviewDrag()
    {
        if (!CurrentArrow || !(CurrentArrow is ArrowArrow)) CurrentArrow = GameObjectPoolManager.GOPM.Pool_ArrowArrowPool.AllocateGameObject(transform).GetComponent<ArrowArrow>();
        Vector3 cameraPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CurrentArrow.Render(CurrentSummonPreviewRetinue.transform.position, cameraPosition);
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycast;
            Physics.Raycast(ray, out raycast, 10f, retinueLayer);
            if (raycast.collider == null) //没有选中目标，则撤销
            {
                SummonRetinueTargetHandler(-2);
                CurrentArrow.PoolRecycle();
                IsSummonPreview = false;
            }
            else
            {
                ModuleRetinue retinue = raycast.collider.GetComponent<ModuleRetinue>();
                if (retinue == null)
                {
                    SummonRetinueTargetHandler(-2);
                    CurrentArrow.PoolRecycle();
                    IsSummonPreview = false;
                }
                else
                {
                    if (RoundManager.RM.SelfClientPlayer.MyBattleGroundManager.CurrentSummonPreviewRetinue == retinue)//不可指向自己
                    {
                        SummonRetinueTargetHandler(-2);
                        CurrentArrow.PoolRecycle();
                        IsSummonPreview = false;
                    }
                    else
                    {
                        int targetRetinueID = retinue.M_RetinueID;
                        bool isClientRetinueTempId = false;
                        if (retinue.M_RetinueID == -1) //如果该随从还未从服务器取得ID，则用tempID
                        {
                            targetRetinueID = retinue.M_ClientTempRetinueID;
                            isClientRetinueTempId = true;
                        }

                        switch (SummonRetinueTargetRange)
                        {
                            case TargetSideEffect.TargetRange.None:
                                SummonRetinueTargetHandler(-2);
                                break;
                            case TargetSideEffect.TargetRange.SelfBattleGround:
                                if (retinue.ClientPlayer == RoundManager.RM.SelfClientPlayer) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetSideEffect.TargetRange.EnemyBattleGround:
                                if (retinue.ClientPlayer == RoundManager.RM.EnemyClientPlayer) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetSideEffect.TargetRange.SelfSodiers:
                                if (retinue.ClientPlayer == RoundManager.RM.SelfClientPlayer && retinue.CardInfo.BattleInfo.IsSodier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetSideEffect.TargetRange.EnemySodiers:
                                if (retinue.ClientPlayer == RoundManager.RM.EnemyClientPlayer && retinue.CardInfo.BattleInfo.IsSodier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetSideEffect.TargetRange.SelfHeros:
                                if (retinue.ClientPlayer == RoundManager.RM.SelfClientPlayer && !retinue.CardInfo.BattleInfo.IsSodier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetSideEffect.TargetRange.EnemyHeros:
                                if (retinue.ClientPlayer == RoundManager.RM.EnemyClientPlayer && !retinue.CardInfo.BattleInfo.IsSodier) SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                else SummonRetinueTargetHandler(-2);
                                break;
                            case TargetSideEffect.TargetRange.All:
                                SummonRetinueTargetHandler(targetRetinueID, isClientRetinueTempId);
                                break;
                        }
                    }


                    CurrentArrow.PoolRecycle();
                    IsSummonPreview = false;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1))
        {
            SummonRetinueTargetHandler(-2);
            CurrentArrow.PoolRecycle();
            IsSummonPreview = false;
        }
    }

    #endregion
}