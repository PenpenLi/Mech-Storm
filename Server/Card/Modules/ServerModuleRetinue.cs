﻿using System;
using System.Reflection;
using SideEffects;

internal class ServerModuleRetinue : ServerModuleBase
{
    protected override void Initiate()
    {
        M_RetinueLeftLife = CardInfo.LifeInfo.Life;
        M_RetinueTotalLife = CardInfo.LifeInfo.Life;
        M_RetinueAttack = CardInfo.BattleInfo.BasicAttack;
        M_RetinueArmor = CardInfo.BattleInfo.BasicArmor;
        M_RetinueShield = CardInfo.BattleInfo.BasicShield;
        M_IsDead = false;
    }

    protected override void InitializeSideEffects()
    {
        foreach (SideEffectBundle.SideEffectExecute see in CardInfo.SideEffects.GetSideEffects())
        {
            see.SideEffectBase.Player = ServerPlayer;
            see.SideEffectBase.M_ExecuterInfo = new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID);
            if (see.SideEffectBase is AddSelfWeaponEnergy seb) seb.RetinueID = M_RetinueID;
        }

        foreach (SideEffectBundle.SideEffectExecute see in CardInfo.SideEffects_OnBattleGround.GetSideEffects())
        {
            see.SideEffectBase.Player = ServerPlayer;
            see.SideEffectBase.M_ExecuterInfo = new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID);
            if (see.SideEffectBase is AddSelfWeaponEnergy seb) seb.RetinueID = M_RetinueID;
        }
    }

    public override CardInfo_Base GetCurrentCardInfo()
    {
        return new CardInfo_Retinue(
            cardID: CardInfo.CardID,
            baseInfo: CardInfo.BaseInfo,
            upgradeInfo: CardInfo.UpgradeInfo,
            lifeInfo: CardInfo.LifeInfo,
            battleInfo: CardInfo.BattleInfo,
            retinueInfo: CardInfo.RetinueInfo,
            sideEffects: CardInfo.SideEffects,
            sideEffects_OnBattleGround: CardInfo.SideEffects_OnBattleGround);
    }

    #region 属性

    private int m_RetinueID;

    public int M_RetinueID
    {
        get { return m_RetinueID; }
        set { m_RetinueID = value; }
    }


    private int m_UsedClientRetinueTempId;

    public int M_UsedClientRetinueTempId //曾用过的客户端临时Id
    {
        get { return m_UsedClientRetinueTempId; }
        set { m_UsedClientRetinueTempId = value; }
    }

    private bool m_IsDead;

    public bool M_IsDead
    {
        get { return m_IsDead; }
        set { m_IsDead = value; }
    }

    private int m_RetinueLeftLife;

    public int M_RetinueLeftLife
    {
        get { return m_RetinueLeftLife; }
        set
        {
            if (M_IsDead) return;
            int before = m_RetinueLeftLife;
            m_RetinueLeftLife = value;
            if (isInitialized && before != m_RetinueLeftLife)
            {
                RetinueAttributesChangeRequest request = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addLeftLife: m_RetinueLeftLife - before);
                ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            }

            if (before < m_RetinueLeftLife)
            {
                OnBeHealed(m_RetinueLeftLife - before);
            }

            if (before > m_RetinueLeftLife)
            {
                OnBeDamaged(before - m_RetinueLeftLife);
            }

            if (m_RetinueLeftLife <= 0)
            {
                OnDieTogather();
            }
        }
    }


    private int m_RetinueTotalLife;

    public int M_RetinueTotalLife
    {
        get { return m_RetinueTotalLife; }
        set
        {
            if (M_IsDead) return;
            int before = m_RetinueTotalLife;
            m_RetinueTotalLife = value;
            if (isInitialized && before != m_RetinueTotalLife)
            {
                RetinueAttributesChangeRequest request = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addMaxLife: m_RetinueTotalLife - before);
                ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            }
        }
    }

    public bool CheckAlive()
    {
        if (M_RetinueLeftLife == 0)
        {
            OnDieTogather();
            return false;
        }

        return true;
    }

    private int m_RetinueAttack;

    public int M_RetinueAttack
    {
        get { return m_RetinueAttack; }
        set
        {
            int before = m_RetinueAttack;
            m_RetinueAttack = value;
            if (M_Weapon != null)
            {
                M_Weapon.CardInfo.WeaponInfo.Attack = value;
            }

            if (isInitialized && before != m_RetinueAttack)
            {
                RetinueAttributesChangeRequest request = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addAttack: m_RetinueAttack - before);
                ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            }
        }
    }

    private int m_RetinueWeaponEnergy;

    public int M_RetinueWeaponEnergy
    {
        get { return m_RetinueWeaponEnergy; }
        set
        {
            int before = m_RetinueWeaponEnergy;
            m_RetinueWeaponEnergy = value;
            if (M_Weapon != null)
            {
                M_Weapon.CardInfo.WeaponInfo.Energy = value;
            }

            if (isInitialized && before != m_RetinueWeaponEnergy)
            {
                int beforeAttack = m_RetinueAttack;
                if (m_RetinueWeaponEnergy == 0) m_RetinueAttack = CardInfo.BattleInfo.BasicAttack;
                else m_RetinueAttack = CardInfo.BattleInfo.BasicAttack + M_Weapon.CardInfo.WeaponInfo.Attack;
                RetinueAttributesChangeRequest request = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addAttack: m_RetinueAttack - beforeAttack, addWeaponEnergy: m_RetinueWeaponEnergy - before);
                ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            }
        }
    }

    private int m_RetinueWeaponEnergyMax;

    public int M_RetinueWeaponEnergyMax
    {
        get { return m_RetinueWeaponEnergyMax; }
        set
        {
            int before = m_RetinueWeaponEnergyMax;
            m_RetinueWeaponEnergyMax = value;
            if (M_Weapon != null)
            {
                M_Weapon.CardInfo.WeaponInfo.EnergyMax = value;
            }

            if (isInitialized && before != m_RetinueWeaponEnergyMax)
            {
                RetinueAttributesChangeRequest request = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addWeaponEnergyMax: m_RetinueWeaponEnergyMax - before);
                ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            }
        }
    }

    private int m_RetinueArmor;

    public int M_RetinueArmor
    {
        get { return m_RetinueArmor; }
        set
        {
            int before = m_RetinueArmor;
            m_RetinueArmor = value;
            if (M_Shield != null)
            {
                M_Shield.CardInfo.ShieldInfo.Armor = value;
            }

            if (isInitialized && before != m_RetinueArmor)
            {
                RetinueAttributesChangeRequest request = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addArmor: m_RetinueArmor - before);
                ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            }
        }
    }

    public int RetinueShieldFull;

    private int m_RetinueShield;

    public int M_RetinueShield
    {
        get { return m_RetinueShield; }
        set
        {
            RetinueShieldFull = Math.Max(value, RetinueShieldFull);
            int before = m_RetinueShield;
            m_RetinueShield = value;
            if (M_Shield != null)
            {
                M_Shield.CardInfo.ShieldInfo.Shield = value;
            }

            if (isInitialized && before != m_RetinueShield)
            {
                RetinueAttributesChangeRequest request = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addShield: m_RetinueShield - before);
                ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            }
        }
    }

    #endregion

    #region 拼装上的模块

    #region 武器相关

    private ServerModuleWeapon m_Weapon;

    public ServerModuleWeapon M_Weapon
    {
        get { return m_Weapon; }
        set
        {
            if (m_Weapon != null && value == null)
            {
                On_WeaponDown();
            }
            else if (m_Weapon == null && value != null)
            {
                On_WeaponEquiped(value);
            }
            else if (m_Weapon != value)
            {
                On_WeaponChanged(value);
                return;
            }
        }
    }

    void On_WeaponDown()
    {
        if (m_Weapon != null)
        {
            ServerPlayer.MyCardDeckManager.M_CurrentCardDeck.RecycleCardInstanceID(m_Weapon.OriginCardInstanceId);
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnEquipDie, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, M_RetinueID, equipId: m_Weapon.M_EquipID));
            m_Weapon.UnRegisterSideEffect();

            EquipWeaponServerRequest request = new EquipWeaponServerRequest(ServerPlayer.ClientId, null, M_RetinueID, 0, m_Weapon.M_EquipID);
            ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            m_Weapon = null;

            int att_before = m_RetinueAttack;
            int we_before = m_RetinueWeaponEnergy;
            int weMax_before = m_RetinueWeaponEnergyMax;

            m_RetinueAttack = CardInfo.BattleInfo.BasicAttack;
            m_RetinueWeaponEnergy = 0;
            m_RetinueWeaponEnergyMax = 0;

            RetinueAttributesChangeRequest request2 = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addAttack: m_RetinueAttack - att_before, addWeaponEnergy: m_RetinueWeaponEnergy - we_before, addWeaponEnergyMax: m_RetinueWeaponEnergyMax - weMax_before);
            ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request2);
        }
    }

    void On_WeaponEquiped(ServerModuleWeapon newWeapon)
    {
        m_Weapon = newWeapon;
        EquipWeaponServerRequest request = new EquipWeaponServerRequest(ServerPlayer.ClientId, (CardInfo_Equip) newWeapon.GetCurrentCardInfo(), M_RetinueID, newWeapon.M_WeaponPlaceIndex, m_Weapon.M_EquipID);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);

        int att_before = m_RetinueAttack;
        int we_before = m_RetinueWeaponEnergy;
        int weMax_before = m_RetinueWeaponEnergyMax;
        m_RetinueAttack += newWeapon.CardInfo.WeaponInfo.Attack;
        m_RetinueWeaponEnergyMax += newWeapon.CardInfo.WeaponInfo.EnergyMax;
        m_RetinueWeaponEnergy += newWeapon.CardInfo.WeaponInfo.Energy;
        if (m_RetinueWeaponEnergy == 0) m_RetinueAttack = CardInfo.BattleInfo.BasicAttack;

        RetinueAttributesChangeRequest request2 = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addAttack: m_RetinueAttack - att_before, addWeaponEnergy: m_RetinueWeaponEnergy - we_before, addWeaponEnergyMax: m_RetinueWeaponEnergyMax - weMax_before);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request2);
    }

    void On_WeaponChanged(ServerModuleWeapon newWeapon)
    {
        if (m_Weapon != null)
        {
            ServerPlayer.MyCardDeckManager.M_CurrentCardDeck.RecycleCardInstanceID(m_Weapon.OriginCardInstanceId);
            m_Weapon.UnRegisterSideEffect();
        }

        m_Weapon = newWeapon;
        EquipWeaponServerRequest request = new EquipWeaponServerRequest(ServerPlayer.ClientId, (CardInfo_Equip) newWeapon.GetCurrentCardInfo(), M_RetinueID, newWeapon.M_WeaponPlaceIndex, m_Weapon.M_EquipID);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);

        int att_before = m_RetinueAttack;
        int we_before = m_RetinueWeaponEnergy;
        int weMax_before = m_RetinueWeaponEnergyMax;
        m_RetinueAttack = CardInfo.BattleInfo.BasicAttack + newWeapon.CardInfo.WeaponInfo.Attack;
        m_RetinueWeaponEnergyMax = newWeapon.CardInfo.WeaponInfo.EnergyMax;
        m_RetinueWeaponEnergy = newWeapon.CardInfo.WeaponInfo.Energy;
        if (m_RetinueWeaponEnergy == 0) m_RetinueAttack = CardInfo.BattleInfo.BasicAttack;

        RetinueAttributesChangeRequest request2 = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addAttack: m_RetinueAttack - att_before, addWeaponEnergy: m_RetinueWeaponEnergy - we_before, addWeaponEnergyMax: m_RetinueWeaponEnergyMax - weMax_before);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request2);
    }

    #endregion


    #region 防具相关

    private ServerModuleShield m_Shield;

    public ServerModuleShield M_Shield
    {
        get { return m_Shield; }
        set
        {
            if (m_Shield != null && value == null)
            {
                On_ShieldDown();
            }
            else if (m_Shield == null && value != null)
            {
                On_ShieldEquiped(value);
            }
            else if (m_Shield != value)
            {
                On_ShieldChanged(value);
                return;
            }
        }
    }

    void On_ShieldDown()
    {
        if (m_Shield != null)
        {
            ServerPlayer.MyCardDeckManager.M_CurrentCardDeck.RecycleCardInstanceID(m_Shield.OriginCardInstanceId);
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnEquipDie, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, M_RetinueID, equipId: m_Shield.M_EquipID));
            m_Shield.UnRegisterSideEffect();

            EquipShieldServerRequest request = new EquipShieldServerRequest(ServerPlayer.ClientId, null, M_RetinueID, 0, m_Shield.M_EquipID);
            ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            m_Shield = null;

            int shield_before = m_RetinueShield;
            int armor_before = m_RetinueArmor;
            m_RetinueShield = 0;
            m_RetinueArmor = 0;

            RetinueAttributesChangeRequest request2 = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addShield: m_RetinueShield - shield_before, addArmor: m_RetinueArmor - armor_before);
            ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request2);
        }
    }

    void On_ShieldEquiped(ServerModuleShield newShield)
    {
        m_Shield = newShield;
        EquipShieldServerRequest request = new EquipShieldServerRequest(ServerPlayer.ClientId, (CardInfo_Equip) newShield.GetCurrentCardInfo(), M_RetinueID, newShield.M_ShieldPlaceIndex, m_Shield.M_EquipID);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);

        int shield_before = m_RetinueShield;
        int armor_before = m_RetinueArmor;
        m_RetinueArmor += newShield.CardInfo.ShieldInfo.Armor;
        m_RetinueShield += newShield.CardInfo.ShieldInfo.Shield;

        RetinueAttributesChangeRequest request2 = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addShield: m_RetinueShield - shield_before, addArmor: m_RetinueArmor - armor_before);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request2);
    }

    void On_ShieldChanged(ServerModuleShield newShield) //更换防具时机体基础护甲护盾恢复
    {
        if (m_Shield != null)
        {
            ServerPlayer.MyCardDeckManager.M_CurrentCardDeck.RecycleCardInstanceID(m_Shield.OriginCardInstanceId);
            m_Shield.UnRegisterSideEffect();
        }

        m_Shield = newShield;
        EquipShieldServerRequest request = new EquipShieldServerRequest(ServerPlayer.ClientId, (CardInfo_Equip) newShield.GetCurrentCardInfo(), M_RetinueID, newShield.M_ShieldPlaceIndex, m_Shield.M_EquipID);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);

        int shield_before = m_RetinueShield;
        int armor_before = m_RetinueArmor;
        m_RetinueShield = CardInfo.BattleInfo.BasicShield + newShield.CardInfo.ShieldInfo.Shield;
        m_RetinueArmor = CardInfo.BattleInfo.BasicArmor + newShield.CardInfo.ShieldInfo.Armor;

        RetinueAttributesChangeRequest request2 = new RetinueAttributesChangeRequest(ServerPlayer.ClientId, M_RetinueID, addShield: m_RetinueShield - shield_before, addArmor: m_RetinueArmor - armor_before);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request2);
    }

    #endregion

    #region 飞行背包相关

    private ServerModulePack m_Pack;

    public ServerModulePack M_Pack
    {
        get { return m_Pack; }
        set
        {
            if (m_Pack != null && value == null)
            {
                On_PackDown();
            }
            else if (m_Pack == null && value != null)
            {
                On_PackEquiped(value);
            }
            else if (m_Pack != value)
            {
                On_PackChanged(value);
                return;
            }
        }
    }

    void On_PackDown()
    {
        if (m_Pack != null)
        {
            ServerPlayer.MyCardDeckManager.M_CurrentCardDeck.RecycleCardInstanceID(m_Pack.OriginCardInstanceId);
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnEquipDie, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, M_RetinueID, equipId: m_Pack.M_EquipID));
            m_Pack.UnRegisterSideEffect();

            EquipPackServerRequest request = new EquipPackServerRequest(ServerPlayer.ClientId, null, M_RetinueID, 0, m_Pack.M_EquipID);
            ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            m_Pack = null;
        }
    }

    void On_PackEquiped(ServerModulePack newPack)
    {
        m_Pack = newPack;
        EquipPackServerRequest request = new EquipPackServerRequest(ServerPlayer.ClientId, (CardInfo_Equip) newPack.GetCurrentCardInfo(), M_RetinueID, newPack.M_PackPlaceIndex, m_Pack.M_EquipID);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
    }

    void On_PackChanged(ServerModulePack newPack)
    {
        if (m_Pack != null)
        {
            ServerPlayer.MyCardDeckManager.M_CurrentCardDeck.RecycleCardInstanceID(m_Pack.OriginCardInstanceId);
            m_Pack.UnRegisterSideEffect();
        }

        m_Pack = newPack;
        EquipPackServerRequest request = new EquipPackServerRequest(ServerPlayer.ClientId, (CardInfo_Equip) newPack.GetCurrentCardInfo(), M_RetinueID, newPack.M_PackPlaceIndex, m_Pack.M_EquipID);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
    }

    #endregion

    #region MA相关

    private ServerModuleMA m_MA;

    public ServerModuleMA M_MA
    {
        get { return m_MA; }
        set
        {
            if (m_MA != null && value == null)
            {
                On_MADown();
            }
            else if (m_MA == null && value != null)
            {
                On_MAEquiped(value);
            }
            else if (m_MA != value)
            {
                On_MAChanged(value);
                return;
            }
        }
    }

    void On_MADown()
    {
        if (m_MA != null)
        {
            ServerPlayer.MyCardDeckManager.M_CurrentCardDeck.RecycleCardInstanceID(m_MA.OriginCardInstanceId);
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnEquipDie, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, M_RetinueID, equipId: m_MA.M_EquipID));
            m_MA.UnRegisterSideEffect();

            EquipMAServerRequest request = new EquipMAServerRequest(ServerPlayer.ClientId, null, M_RetinueID, 0, m_MA.M_EquipID);
            ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
            m_MA = null;
        }
    }

    void On_MAEquiped(ServerModuleMA newMA)
    {
        m_MA = newMA;
        EquipMAServerRequest request = new EquipMAServerRequest(ServerPlayer.ClientId, (CardInfo_Equip) newMA.GetCurrentCardInfo(), M_RetinueID, newMA.M_MAPlaceIndex, m_MA.M_EquipID);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
    }

    void On_MAChanged(ServerModuleMA newMA)
    {
        if (m_MA != null)
        {
            ServerPlayer.MyCardDeckManager.M_CurrentCardDeck.RecycleCardInstanceID(m_MA.OriginCardInstanceId);
            m_MA.UnRegisterSideEffect();
        }

        m_MA = newMA;
        EquipMAServerRequest request = new EquipMAServerRequest(ServerPlayer.ClientId, (CardInfo_Equip) newMA.GetCurrentCardInfo(), M_RetinueID, newMA.M_MAPlaceIndex, m_MA.M_EquipID);
        ServerPlayer.MyClientProxy.MyServerGameManager.Broadcast_AddRequestToOperationResponse(request);
    }

    #endregion

    #endregion

    #region 模块交互

    public void BeAttacked(int attackNumber) //攻击和被攻击仅发送伤害数值给客户端，具体计算分别处理
    {
        if (M_IsDead) return;
        OnBeAttacked();
        int remainAttackNumber = attackNumber;

        //小于等于护盾的伤害的全部免除，护盾无任何损失，大于护盾的伤害，每超过一点，护盾受到一点伤害，如果扣为0，则护盾破坏
        if (M_RetinueShield > 0)
        {
            if (M_RetinueShield >= remainAttackNumber)
            {
                remainAttackNumber = 0;
                return;
            }
            else
            {
                int shieldDecrease = remainAttackNumber - M_RetinueShield;
                remainAttackNumber -= M_RetinueShield;
                m_RetinueShield -= Math.Min(m_RetinueShield, shieldDecrease);
                if (m_RetinueShield == 0 && m_RetinueArmor == 0)
                {
                    M_Shield = null;
                }
            }
        }

        if (M_RetinueArmor > 0)
        {
            if (M_RetinueArmor >= remainAttackNumber)
            {
                m_RetinueArmor = M_RetinueArmor - remainAttackNumber;
                remainAttackNumber = 0;
                return;
            }
            else
            {
                remainAttackNumber -= M_RetinueArmor;
                m_RetinueArmor = 0;
                if (m_RetinueShield == 0 && m_RetinueArmor == 0)
                {
                    M_Shield = null;
                }
            }
        }

        if (M_RetinueLeftLife <= remainAttackNumber)
        {
            m_RetinueLeftLife -= M_RetinueLeftLife;
            remainAttackNumber -= M_RetinueLeftLife;
        }
        else
        {
            m_RetinueLeftLife -= remainAttackNumber;
            remainAttackNumber = 0;
            return;
        }
    }

    private void OnBeAttacked()
    {
    }

    private enum AttackLevel
    {
        Sword = 0,
        Gun = 1,
        SniperGun = 2
    }

    private AttackLevel M_AttackLevel
    {
        get
        {
            if (M_Weapon == null || M_RetinueWeaponEnergy == 0) return AttackLevel.Sword;
            if (M_Weapon.M_WeaponType == WeaponTypes.Gun) return AttackLevel.Gun;
            if (M_Weapon.M_WeaponType == WeaponTypes.SniperGun) return AttackLevel.SniperGun;
            return AttackLevel.Sword;
        }
    }

    public void Attack(ServerModuleRetinue targetRetinue, bool isCounterAttack) //服务器客户单分别计算
    {
        if (M_IsDead) return;

        if (!isCounterAttack) OnAttack();
        int damage = 0;

        bool canCounter = !isCounterAttack && M_AttackLevel <= targetRetinue.M_AttackLevel; //对方能否反击

        if (M_Weapon != null && M_RetinueWeaponEnergy != 0)
        {
            switch (M_Weapon.M_WeaponType)
            {
                case WeaponTypes.Sword:
                    damage = M_RetinueAttack * M_RetinueWeaponEnergy;
                    targetRetinue.BeAttacked(damage);
                    OnMakeDamage(damage);
                    if (M_RetinueWeaponEnergy < M_RetinueWeaponEnergyMax) m_RetinueWeaponEnergy++;
                    if (canCounter) targetRetinue.Attack(this, true); //对方反击
                    break;
                case WeaponTypes.Gun: //有远程武器避免反击
                    int tmp = M_RetinueWeaponEnergy;
                    if (isCounterAttack) //如果是用枪反击，只反击一个子弹
                    {
                        tmp = 1;
                    }

                    for (int i = 0; i < tmp; i++)
                    {
                        targetRetinue.BeAttacked(M_RetinueAttack);
                        OnMakeDamage(M_RetinueAttack);
                        m_RetinueWeaponEnergy--;
                        if (targetRetinue.M_RetinueLeftLife <= 0) break;
                    }

                    if (canCounter) targetRetinue.Attack(this, true); //对方反击
                    break;
                case WeaponTypes.SniperGun:
                    if (isCounterAttack) break; //狙击枪无法反击
                    targetRetinue.BeAttacked(M_RetinueAttack);
                    OnMakeDamage(M_RetinueAttack);
                    m_RetinueWeaponEnergy--;
                    if (targetRetinue.M_RetinueLeftLife <= 0) break;
                    if (canCounter) targetRetinue.Attack(this, true); //对方反击
                    break;
            }
        }
        else //没有武器
        {
            damage = M_RetinueAttack;
            targetRetinue.BeAttacked(damage);
            OnMakeDamage(damage);
            if (canCounter) targetRetinue.Attack(this, true); //对方反击
        }

        //死亡结算
        if (isCounterAttack) return; //逻辑集中在攻击方处理，反击方不处理后续效果


        if (M_RetinueLeftLife == 0 && targetRetinue.M_RetinueLeftLife != 0) //攻击方挂了
        {
            OnDieTogather();
        }
        else if (M_RetinueLeftLife != 0 && targetRetinue.M_RetinueLeftLife == 0) //反击方挂了
        {
            targetRetinue.OnDieTogather();
            SideEffectBase.ExecuterInfo ei = new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID, targetRetinueId: targetRetinue.M_RetinueID);
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnRetinueKill, ei);
            if (CardInfo.RetinueInfo.IsSoldier) ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnSoldierKill, ei);
            else ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnHeroKill, ei);
        }
        else if (M_RetinueLeftLife == 0 && targetRetinue.M_RetinueLeftLife == 0) //全挂了
        {
            if (M_RetinueID > targetRetinue.M_RetinueID) //随从上场顺序决定死亡顺序
            {
                OnDieTogather();
                targetRetinue.OnDieTogather();
            }
            else
            {
                targetRetinue.OnDieTogather();
                OnDieTogather();
            }
        }
    }

    public void OnDieTogather()
    {
        if (M_IsDead) return;
        M_IsDead = true;
        M_Weapon = null;
        M_Shield = null;
        M_Pack = null;
        M_MA = null;
        ServerPlayer.MyGameManager.AddDieTogatherRetinuesInfo(M_RetinueID);
        SideEffectBase.ExecuterInfo info = new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID);
        ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnRetinueDie, info);
        if (CardInfo.RetinueInfo.IsSoldier) ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnSoldierDie, info);
        else ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnHeroDie, info);
    }

    public void UnregisterEvent()
    {
        ServerPlayer.MyGameManager.EventManager.UnRegisterEvent(CardInfo.SideEffects);
    }

    private void OnMakeDamage(int damage)
    {
        ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnRetinueMakeDamage, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        if (CardInfo.RetinueInfo.IsSoldier)
        {
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnSoldierMakeDamage, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        }
        else
        {
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnHeroMakeDamage, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        }
    }

    private void OnBeDamaged(int i)
    {
        ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnRetinueInjured, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        if (CardInfo.RetinueInfo.IsSoldier)
        {
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnSoldierInjured, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        }
        else
        {
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnHeroInjured, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        }
    }

    private void OnBeHealed(int i)
    {
        ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnRetinueBeHealed, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        if (CardInfo.RetinueInfo.IsSoldier)
        {
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnSoldierBeHealed, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        }
        else
        {
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnHeroBeHealed, new SideEffectBase.ExecuterInfo(ServerPlayer.ClientId, retinueId: M_RetinueID));
        }
    }

    public void AttackShip(ServerPlayer ship) //计算结果全部由服务器下发
    {
        if (M_IsDead) return;
        OnAttack();
        int damage = 0;

        if (M_Weapon != null && M_RetinueWeaponEnergy != 0) //有武器避免反击
        {
            switch (M_Weapon.M_WeaponType)
            {
                case WeaponTypes.Sword:
                    damage = M_RetinueAttack * M_RetinueWeaponEnergy * 2;
                    ship.DamageLifeAboveZero(damage);
                    OnMakeDamage(damage);
                    if (M_RetinueWeaponEnergy < M_RetinueWeaponEnergyMax) M_RetinueWeaponEnergy++;
                    break;
                case WeaponTypes.Gun:
                    int tmp = M_RetinueWeaponEnergy;
                    for (int i = 0; i < tmp; i++)
                    {
                        ship.DamageLifeAboveZero(M_RetinueAttack);
                        OnMakeDamage(M_RetinueAttack);
                        M_RetinueWeaponEnergy--;
                    }

                    break;
                case WeaponTypes.SniperGun:
                    ship.DamageLifeAboveZero(M_RetinueAttack);
                    OnMakeDamage(M_RetinueAttack);
                    M_RetinueWeaponEnergy--;
                    break;
            }
        }
        else
        {
            damage = M_RetinueAttack * 2;
            ship.DamageLifeAboveZero(damage);
            OnMakeDamage(damage);
        }

        if (M_RetinueLeftLife == 0) //攻击方挂了
        {
            OnDieTogather();
        }
    }

    private void OnAttack()
    {
        SideEffectBase.ExecuterInfo ei = new SideEffectBase.ExecuterInfo(clientId: ServerPlayer.ClientId, retinueId: M_RetinueID);
        ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnRetinueAttack, ei);
        if (CardInfo.RetinueInfo.IsSoldier)
        {
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnSoldierAttack, ei);
        }
        else
        {
            ServerPlayer.MyGameManager.EventManager.Invoke(SideEffectBundle.TriggerTime.OnHeroAttack, ei);
        }
    }

    #endregion
}