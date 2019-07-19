﻿using System;
using System.Collections.Generic;
using SideEffects;

/// <summary>
/// 为了兼容联机对战模式，单人模式的AI也用一个ClientProxy来处理逻辑
/// AI不需要任何Socket有关的功能
/// 固定PlayerA为玩家，PlayerB为AI
/// </summary>
public class BattleProxyAI : BattleProxy
{
    public Enemy Enemy;

    public BattleProxyAI(int clientId, string userName, Enemy enemy) : base(clientId, userName, null, null)
    {
        Enemy = enemy;
        SendMessage = sendMessage;
    }

    private void sendMessage(ServerRequestBase request)
    {
        if (request is ResponseBundleBase r)
        {
            foreach (ServerRequestBase req in r.AttachedRequests)
            {
                if (req is PlayerTurnRequest ptr) //只监听回合开始
                {
                    if (ptr.clientId == ClientID)
                    {
                        AIOperation();
                    }
                }
            }
        }
    }

    public void ReceiveMessage(ClientRequestBase request)
    {
    }

    protected void Response()
    {
    }

    #region AIOperation

    HashSet<int> TriedCards = new HashSet<int>();
    HashSet<int> TriedMechs = new HashSet<int>();

    public void ResetTriedCards()
    {
        TriedCards.Clear();
    }

    public void AIOperation()
    {
        while (true)
        {
            bool lastOperation = false;
            bool failedAgain = false;
            while (true)
            {
                CardBase card = FindCardUsable();
                ModuleMech mech = FindMechMovable();
                if (card == null && mech == null)
                {
                    if (!lastOperation)
                    {
                        failedAgain = true; //如果连续两次失败，则停止
                    }

                    break;
                }
                else
                {
                    if (card != null)
                    {
                        if (TryUseCard(card)) //成功
                        {
                            lastOperation = true;
                        }
                        else
                        {
                            TriedCards.Add(card.M_CardInstanceId); //尝试过的卡牌不再尝试
                        }
                    }

                    if (mech != null)
                    {
                        if (TryAttack(mech)) //成功
                        {
                            lastOperation = true;
                        }
                        else
                        {
                            TriedMechs.Add(mech.M_MechID); //尝试过的随从不再尝试
                        }
                    }
                }
            }

            TriedCards.Clear();
            TriedMechs.Clear();

            if (failedAgain) break;
        }

        BattleGameManager.OnEndRoundRequest(new EndRoundRequest(ClientID));
    }

    /// <summary>
    /// 尝试使用卡牌，如果卡牌需要指定目标，但没有合适目标，则使用失败，返回false
    /// </summary>
    /// <returns></returns>
    private bool TryUseCard(CardBase card)
    {
        TargetInfo ti = card.CardInfo.TargetInfo;
        HashSet<Type> sefs = GetSideEffectFunctionsByCardInfo(card.CardInfo);

        if (card.CardInfo.BaseInfo.CardType == CardTypes.Spell || card.CardInfo.BaseInfo.CardType == CardTypes.Energy)
        {
            if (ti.HasNoTarget)
            {
                BattleGameManager.OnClientUseSpellCardRequest(new UseSpellCardRequest(ClientID, card.M_CardInstanceId));
                return true;
            }
            else if (ti.HasTargetMech)
            {
                ModuleMech mech = GetTargetMechByTargetInfo(sefs, ti);
                if (mech != null)
                {
                    DebugLog.PrintError("SpelltoMech: " + card.CardInfo.BaseInfo.CardNames["zh"] + "  " + mech.CardInfo.BaseInfo.CardNames["zh"]);
                    BattleGameManager.OnClientUseSpellCardToMechRequest(new UseSpellCardToMechRequest(ClientID, card.M_CardInstanceId, new List<(int, bool)> {(mech.M_MechID, false)}));
                    return true;
                }
            }
            else if (ti.HasTargetShip)
            {
                switch (ti.targetShipRange)
                {
                    case TargetRange.EnemyShip:
                    {
                        BattleGameManager.OnClientUseSpellCardToShipRequest(new UseSpellCardToShipRequest(ClientID, card.M_CardInstanceId, new List<int> {BattleGameManager.PlayerA.ClientId}));
                        return true;
                    }
                    case TargetRange.SelfShip:
                    {
                        BattleGameManager.OnClientUseSpellCardToShipRequest(new UseSpellCardToShipRequest(ClientID, card.M_CardInstanceId, new List<int> {ClientID}));
                        return true;
                    }
                }
            }
            else if (ti.HasTargetEquip) //Todo
            {
                switch (ti.targetMechRange)
                {
                    case TargetRange.EnemyMechs:
                    {
                        break;
                    }
                    case TargetRange.EnemyHeroes:
                    {
                        break;
                    }
                    case TargetRange.EnemySoldiers:
                    {
                        break;
                    }
                    case TargetRange.SelfMechs:
                    {
                        break;
                    }
                    case TargetRange.SelfHeroes:
                    {
                        break;
                    }
                    case TargetRange.SelfSoldiers:
                    {
                        break;
                    }
                }
            }
        }
        else if (card.CardInfo.BaseInfo.CardType == CardTypes.Mech)
        {
            if (MyPlayer.BattleGroundManager.BattleGroundIsFull) return false;

            bool canSummonDirectly = false;
            canSummonDirectly |= ti.HasNoTarget;
            canSummonDirectly |= (ti.targetMechRange == TargetRange.SelfMechs && MyPlayer.BattleGroundManager.MechCount == 0);
            canSummonDirectly |= (ti.targetMechRange == TargetRange.SelfHeroes && MyPlayer.BattleGroundManager.HeroCount == 0);
            canSummonDirectly |= (ti.targetMechRange == TargetRange.SelfSoldiers && MyPlayer.BattleGroundManager.SoldierCount == 0);
            canSummonDirectly |= (ti.targetMechRange == TargetRange.EnemyMechs && EnemyPlayer.BattleGroundManager.MechCount == 0);
            canSummonDirectly |= (ti.targetMechRange == TargetRange.EnemyHeroes && EnemyPlayer.BattleGroundManager.HeroCount == 0);
            canSummonDirectly |= (ti.targetMechRange == TargetRange.EnemySoldiers && EnemyPlayer.BattleGroundManager.SoldierCount == 0);
            //Todo 针对装备等还没处理

            if (canSummonDirectly)
            {
                BattleGameManager.OnClientSummonMechRequest(new SummonMechRequest(ClientID, card.M_CardInstanceId, MyPlayer.BattleGroundManager.MechCount, (int) Const.SpecialMechID.ClientTempMechIDNormal, null, null));
                return true;
            }

            if (ti.HasTargetMech)
            {
                ModuleMech mech = GetTargetMechByTargetInfo(sefs, ti);
                if (mech != null)
                {
                    BattleGameManager.OnClientSummonMechRequest(new SummonMechRequest(ClientID, card.M_CardInstanceId, MyPlayer.BattleGroundManager.MechCount, (int) Const.SpecialMechID.ClientTempMechIDNormal, new List<int> {mech.M_MechID}, new List<bool> {false}));
                    return true;
                }
            }
            else if (ti.HasTargetShip)
            {
                //Todo 针对战舰等还没处理
            }
            else if (ti.HasTargetEquip)
            {
                //Todo 针对装备等还没处理
            }
        }
        else if (card.CardInfo.BaseInfo.CardType == CardTypes.Equip)
        {
            //筛选最适合装备的机甲
            ModuleMech mech = SelectMechToEquipWeapon(MyPlayer.BattleGroundManager.Heroes, (CardInfo_Equip) card.CardInfo);
            if (mech == null) mech = SelectMechToEquipWeapon(MyPlayer.BattleGroundManager.Soldiers, (CardInfo_Equip) card.CardInfo);

            if (mech != null)
            {
                switch (card.CardInfo.EquipInfo.SlotType)
                {
                    case SlotTypes.Weapon:
                    {
                        BattleGameManager.OnClientEquipWeaponRequest(new EquipWeaponRequest(ClientID, card.M_CardInstanceId, mech.M_MechID));
                        return true;
                    }
                    case SlotTypes.Shield:
                    {
                        BattleGameManager.OnClientEquipShieldRequest(new EquipShieldRequest(ClientID, card.M_CardInstanceId, mech.M_MechID));
                        return true;
                    }
                    case SlotTypes.Pack:
                    {
                        BattleGameManager.OnClientEquipPackRequest(new EquipPackRequest(ClientID, card.M_CardInstanceId, mech.M_MechID));
                        return true;
                    }
                    case SlotTypes.MA:
                    {
                        BattleGameManager.OnClientEquipMARequest(new EquipMARequest(ClientID, card.M_CardInstanceId, mech.M_MechID));
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool TryAttack(ModuleMech mech)
    {
        if (EnemyPlayer.CheckModuleMechCanAttackMe(mech)) //优先打脸
        {
            BattleGameManager.OnClientMechAttackShipRequest(new MechAttackShipRequest(ClientID, mech.M_MechID));
            return true;
        }

        foreach (ModuleMech targetMech in EnemyPlayer.BattleGroundManager.Mechs)
        {
            if (targetMech.CheckMechCanAttackMe(mech))
            {
                BattleGameManager.OnClientMechAttackMechRequest(new MechAttackMechRequest(ClientID, mech.M_MechID, EnemyPlayer.ClientId, targetMech.M_MechID));
                return true;
            }
        }

        return false;
    }

    private HashSet<Type> GetSideEffectFunctionsByCardInfo(CardInfo_Base cardInfo)
    {
        HashSet<Type> res = new HashSet<Type>();

        foreach (SideEffectExecute see in cardInfo.SideEffectBundle.SideEffectExecutes)
        {
            foreach (SideEffectBase se in see.SideEffectBases)
            {
                if (se is IPositive)
                {
                    res.Add(typeof(IPositive));
                }

                if (se is INegative)
                {
                    res.Add(typeof(INegative));
                }

                if (se is IStrengthen)
                {
                    res.Add(typeof(IStrengthen));
                }

                if (se is IWeaken)
                {
                    res.Add(typeof(IWeaken));
                }

                if (se is IPriorUsed)
                {
                    res.Add(typeof(IPriorUsed));
                }

                if (se is IPostUsed)
                {
                    res.Add(typeof(IPostUsed));
                }

                if (se is IDefend)
                {
                    res.Add(typeof(IDefend));
                }

                if (se is IShipEnergy)
                {
                    res.Add(typeof(IShipEnergy));
                }

                if (se is AddLife addLife)
                {
                    if ((addLife.TargetRange & TargetRange.SelfShip) != 0)
                    {
                        res.Add(typeof(IShipLife));
                    }
                }

                if (se is Heal heal)
                {
                    if ((heal.TargetRange & TargetRange.SelfShip) != 0)
                    {
                        res.Add(typeof(IShipLife));
                    }
                }
            }
        }

        return res;
    }

    private ModuleMech GetTargetMechByTargetInfo(HashSet<Type> sefs, TargetInfo ti)
    {
        BattlePlayer targetPlayer = null;
        MechTypes targetMechType = MechTypes.All;
        if ((ti.targetMechRange | TargetRange.EnemyMechs) == TargetRange.EnemyMechs)
        {
            targetPlayer = BattleGameManager.PlayerA;
        }
        else if ((ti.targetMechRange | TargetRange.SelfMechs) == TargetRange.SelfMechs)
        {
            targetPlayer = BattleGameManager.PlayerB;
        }

        if ((ti.targetMechRange | TargetRange.Heroes) == TargetRange.Heroes)
        {
            targetMechType = MechTypes.Hero;
        }
        else if ((ti.targetMechRange | TargetRange.Soldiers) == TargetRange.Soldiers)
        {
            targetMechType = MechTypes.Soldier;
        }
        else
        {
            targetMechType = MechTypes.All;
        }

        ModuleMech mech = null;
        if (targetPlayer != null)
        {
            mech = targetPlayer.BattleGroundManager.GetRandomAliveMechExcept(targetMechType, -1);
        }
        else // 依据卡牌增减益效果确定施加于哪一方
        {
            if ((sefs.Contains(typeof(INegative))) || sefs.Contains(typeof(IWeaken)))
            {
                targetPlayer = BattleGameManager.PlayerA;
                mech = targetPlayer.BattleGroundManager.GetRandomAliveMechExcept(targetMechType, -1);
            }
            else if (sefs.Contains(typeof(IPositive)) || sefs.Contains(typeof(IStrengthen)) || sefs.Contains(typeof(IShipEnergy)) || sefs.Contains(typeof(IShipLife)) || sefs.Contains(typeof(IDefend)))
            {
                targetPlayer = BattleGameManager.PlayerB;
                mech = targetPlayer.BattleGroundManager.GetRandomAliveMechExcept(targetMechType, -1);
            }
            else
            {
                mech = BattleGameManager.GetRandomAliveMechExcept(targetMechType, -1);
            }
        }

        return mech;
    }

    internal bool CheckMechCanEquipMe(ModuleMech mech, CardInfo_Equip equipInfo)
    {
        if (MyPlayer == mech.BattlePlayer && mech.CardInfo.MechInfo.HasSlotType(equipInfo.EquipInfo.SlotType) && !mech.M_IsDead)
        {
            if (equipInfo.EquipInfo.SlotType == SlotTypes.Weapon && equipInfo.WeaponInfo.WeaponType == WeaponTypes.SniperGun)
            {
                if (mech.CardInfo.MechInfo.IsSniper)
                {
                    return true; //狙击枪只能装在狙击手上
                }
                else
                {
                    return false;
                }
            }
            else if (equipInfo.EquipInfo.SlotType == SlotTypes.MA)
            {
                if (mech.IsAllEquipExceptMA)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    private CardBase FindCardUsable()
    {
        List<int> noTriedUsableCards = new List<int>();
        foreach (int id in MyPlayer.HandManager.UsableCards)
        {
            if (!TriedCards.Contains(id))
            {
                noTriedUsableCards.Add(id);
            }
        }

        if (noTriedUsableCards.Count == 1) return MyPlayer.HandManager.GetCardByCardInstanceId(noTriedUsableCards[0]);
        CardBase energyCardID = null;
        CardBase spellCardID = null;
        CardBase equipCardID = null;
        CardBase mechCardID = null;
        foreach (int id in noTriedUsableCards)
        {
            CardBase card = MyPlayer.HandManager.GetCardByCardInstanceId(id);
            if (card.CardInfo.BaseInfo.CardType == CardTypes.Energy)
            {
                energyCardID = card;
            }

            if (card.CardInfo.BaseInfo.CardType == CardTypes.Spell)
            {
                spellCardID = card;
            }

            if (card.CardInfo.BaseInfo.CardType == CardTypes.Equip)
            {
                equipCardID = card;
            }

            if (card.CardInfo.BaseInfo.CardType == CardTypes.Mech)
            {
                mechCardID = card;
            }
        }

        if (energyCardID != null)
        {
            return energyCardID;
        }

        if (spellCardID != null)
        {
            return spellCardID;
        }

        if (equipCardID != null)
        {
            return equipCardID;
        }

        if (mechCardID != null)
        {
            return mechCardID;
        }

        return null;
    }

    private ModuleMech FindMechMovable()
    {
        foreach (int id in MyPlayer.BattleGroundManager.CanAttackMechs)
        {
            if (!TriedMechs.Contains(id))
            {
                return MyPlayer.BattleGroundManager.GetMech(id);
            }
        }

        return null;
    }

    private bool TryAttack()
    {
        return false;
    }

    #endregion

    #region AI_Mark_System 评分系统

    private ModuleMech SelectMechToEquipWeapon(List<ModuleMech> mechs, CardInfo_Equip cardInfo)
    {
        List<ModuleMech> mechs_NoWeapon = new List<ModuleMech>(); //优先给没有武器的装备

        List<ModuleMech> optionalMech = new List<ModuleMech>();
        foreach (ModuleMech mech in mechs) //满足可以装备的前提
        {
            if (CheckMechCanEquipMe(mech, cardInfo))
            {
                optionalMech.Add(mech);
            }
        }

        foreach (ModuleMech mech in optionalMech)
        {
            if (mech.M_Weapon == null)
            {
                mechs_NoWeapon.Add(mech);
            }
        }

        if (mechs_NoWeapon.Count != 0) //没有武器的里面，挑最强的
        {
            return GetMechByEvaluation(mechs_NoWeapon, EvaluationOption.Mech, EvaluationDirection.Max);
        }
        else //都有武器情况下，给武器最差的装备
        {
            return GetMechByEvaluation(mechs_NoWeapon, EvaluationOption.Weapon, EvaluationDirection.Min);
        }
    }

    enum EvaluationOption
    {
        Mech,
        Weapon,
        Shield,
        Pack,
        MA
    }

    enum EvaluationDirection
    {
        Max,
        Min
    }

    delegate float GetMechByEvaluationDelegate(ModuleMech mech);

    Dictionary<EvaluationOption, GetMechByEvaluationDelegate> EvaluationMethodDict = new Dictionary<EvaluationOption, GetMechByEvaluationDelegate>
    {
        {EvaluationOption.Mech, CountMechValue},
        {EvaluationOption.Weapon, CountMechWeaponValue},
        {EvaluationOption.Shield, CountMechShieldValue},
        {EvaluationOption.Pack, CountMechPackValue},
        {EvaluationOption.MA, CountMechMAValue}
    };

    private ModuleMech GetMechByEvaluation(List<ModuleMech> optionalMech, EvaluationOption evaluationOption, EvaluationDirection evaluationDirection)
    {
        ModuleMech res = null;
        float resMard = 0;
        foreach (ModuleMech mech in optionalMech)
        {
            float mark = EvaluationMethodDict[evaluationOption](mech);

            if (evaluationDirection == EvaluationDirection.Max)
            {
                if (mark > resMard)
                {
                    res = mech;
                    resMard = mark;
                }
            }
            else
            {
                if (mark < resMard)
                {
                    res = mech;
                    resMard = mark;
                }
            }
        }

        return res;
    }

    private static float CountMechValue(ModuleMech mech)
    {
        float mark = 0;

        mark += mech.CardInfo.BaseInfo.BaseValue();
        mark += mech.M_MechLeftLife;
        mark += mech.M_MechShield * 3 + mech.M_MechArmor;

        mark += CountMechWeaponValue(mech);
        mark += CountMechShieldValue(mech);
        mark += CountMechPackValue(mech);
        mark += CountMechMAValue(mech);

        return mark;
    }

    private static float CountMechWeaponValue(ModuleMech mech)
    {
        float mark = 0;
        if (mech.M_Weapon != null)
        {
            mark += mech.M_Weapon.CardInfo.BaseInfo.BaseValue();
            mark += mech.M_MechWeaponEnergy * mech.M_MechAttack;
        }

        return mark;
    }

    private static float CountMechShieldValue(ModuleMech mech)
    {
        float mark = 0;
        if (mech.M_Shield != null)
        {
            mark += mech.M_Shield.CardInfo.BaseInfo.BaseValue();
        }

        return mark;
    }

    private static float CountMechPackValue(ModuleMech mech)
    {
        float mark = 0;
        if (mech.M_Pack != null)
        {
            mark += mech.M_Pack.CardInfo.BaseInfo.BaseValue();
        }

        return mark;
    }

    private static float CountMechMAValue(ModuleMech mech)
    {
        float mark = 0;

        if (mech.M_MA != null)
        {
            mark += mech.M_MA.CardInfo.BaseInfo.BaseValue();
        }

        return mark;
    }

    #endregion
}