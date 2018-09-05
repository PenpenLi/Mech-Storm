﻿using System;
using System.Collections.Generic;

public class CardInfo_Equip : CardInfo_Base
{
    public SlotTypes M_SlotType;

    public CardInfo_Equip()
    {
    }

    public CardInfo_Equip(int cardID, BaseInfo baseInfo, UpgradeInfo upgradeInfo, SlotTypes slotType, WeaponInfo weaponInfo, ShieldInfo shieldInfo, List<SideEffectBase> sideEffects_OnEndRound, List<SideEffectBase> sideEffects_OnPlayOut, List<SideEffectBase> sideEffects_OnSummoned, List<SideEffectBase> sideEffects_OnDie)
        : base(cardID: cardID,
            baseInfo: baseInfo,
            sideEffects_OnEndRound: sideEffects_OnEndRound,
            sideEffects_OnPlayOut: sideEffects_OnPlayOut,
            sideEffects_OnSummoned: sideEffects_OnSummoned,
            sideEffects_OnDie: sideEffects_OnDie)
    {
        M_SlotType = slotType;
        switch (M_SlotType)
        {
            case SlotTypes.Weapon:
            {
                WeaponInfo = weaponInfo;
                break;
            }
            case SlotTypes.Shield:
            {
                ShieldInfo = shieldInfo;
                break;
            }
        }

        UpgradeInfo = upgradeInfo;
    }

    public override string GetCardDescShow()
    {
        string CardDescShow = BaseInfo.CardDescRaw;

        switch (M_SlotType)
        {
            case SlotTypes.Weapon:
            {
                if (WeaponInfo.WeaponType == WeaponTypes.Sword)
                {
                    CardDescShow += "攻击力: " + BaseInfo.AddHightLightColorToText(BaseInfo.HightLightColor, WeaponInfo.Attack.ToString()) + " 点\n";
                    CardDescShow += "充能: " + BaseInfo.AddHightLightColorToText(BaseInfo.HightLightColor, WeaponInfo.Energy + "/" + WeaponInfo.EnergyMax) + "\n";
                }
                else if (WeaponInfo.WeaponType == WeaponTypes.Gun)
                {
                    CardDescShow += "弹丸伤害: " + BaseInfo.AddHightLightColorToText(BaseInfo.HightLightColor, WeaponInfo.Attack.ToString()) + " 点\n";
                    CardDescShow += "弹药: " + BaseInfo.AddHightLightColorToText(BaseInfo.HightLightColor, WeaponInfo.Energy + "/" + WeaponInfo.EnergyMax) + "\n";
                }

                break;
            }
            case SlotTypes.Shield:
            {
                if (ShieldInfo.ShieldType == ShieldTypes.Armor)
                {
                    CardDescShow += "阻挡 " + BaseInfo.AddHightLightColorToText(BaseInfo.HightLightColor, ShieldInfo.Armor.ToString()) + " 点伤害\n";
                }
                else if (ShieldInfo.ShieldType == ShieldTypes.Shield)
                {
                    CardDescShow += "受到的伤害减少 " + BaseInfo.AddHightLightColorToText(BaseInfo.HightLightColor, ShieldInfo.Shield.ToString()) + " 点\n";
                }

                break;
            }
        }

        CardDescShow += base.GetCardDescShow();

        CardDescShow = CardDescShow.TrimEnd(";\n".ToCharArray());

        return CardDescShow;
    }

    public override CardInfo_Base Clone()
    {
        CardInfo_Base temp = base.Clone();
        CardInfo_Equip cb = new CardInfo_Equip(
            cardID: CardID,
            baseInfo: BaseInfo,
            upgradeInfo: UpgradeInfo,
            slotType: M_SlotType,
            weaponInfo: WeaponInfo,
            shieldInfo: ShieldInfo,
            sideEffects_OnEndRound: temp.SideEffects_OnEndRound,
            sideEffects_OnPlayOut: temp.SideEffects_OnPlayOut,
            sideEffects_OnSummoned: temp.SideEffects_OnSummoned,
            sideEffects_OnDie: temp.SideEffects_OnDie);
        return cb;
    }
}