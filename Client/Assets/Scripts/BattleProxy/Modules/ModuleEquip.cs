﻿internal abstract class ModuleEquip : ModuleBase
{
    internal ModuleMech M_ModuleMech;

    private int m_EquipID;

    public int M_EquipID
    {
        get { return m_EquipID; }

        set { m_EquipID = value; }
    }

    protected override void InitializeSideEffects()
    {
        foreach (SideEffectExecute see in CardInfo.SideEffectBundle.SideEffectExecutes)
        {
            foreach (SideEffectBase se in see.SideEffectBases)
            {
                se.Player = BattlePlayer;
                se.M_ExecutorInfo = new ExecutorInfo(
                    clientId: BattlePlayer.ClientId,
                    sideEffectExecutorID: see.ID,
                    mechId: M_ModuleMech.M_MechID,
                    equipId: M_EquipID
                );
            }
        }

        foreach (SideEffectExecute see in CardInfo.SideEffectBundle_BattleGroundAura.SideEffectExecutes)
        {
            foreach (SideEffectBase se in see.SideEffectBases)
            {
                se.Player = BattlePlayer;
                se.M_ExecutorInfo = new ExecutorInfo(
                    clientId: BattlePlayer.ClientId,
                    sideEffectExecutorID: see.ID,
                    mechId: M_ModuleMech.M_MechID,
                    equipId: M_EquipID
                );
            }
        }
    }

    public override CardInfo_Base GetCurrentCardInfo()
    {
        return new CardInfo_Equip(
            cardID: CardInfo.CardID,
            baseInfo: CardInfo.BaseInfo,
            upgradeInfo: CardInfo.UpgradeInfo,
            equipInfo: CardInfo.EquipInfo,
            weaponInfo: CardInfo.WeaponInfo,
            shieldInfo: CardInfo.ShieldInfo,
            packInfo: CardInfo.PackInfo,
            maInfo: CardInfo.MAInfo,
            sideEffectBundle: CardInfo.SideEffectBundle,
            sideEffectBundle_BattleGroundAura: CardInfo.SideEffectBundle_BattleGroundAura);
    }
}