﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardEditorPanel : BaseUIForm
{
    private CardEditorPanel()
    {
    }

    void Awake()
    {
        UIType.IsClearStack = false;
        UIType.IsESCClose = false;
        UIType.IsClickElsewhereClose = false;
        UIType.UIForms_Type = UIFormTypes.Fixed;
        UIType.UIForm_LucencyType = UIFormLucencyTypes.ImPenetrable;
        UIType.UIForms_ShowMode = UIFormShowModes.HideOther;
        UIType.IsClearStack = true;

        LanguageManager.Instance.RegisterTextKeys(
            new List<(Text, string)>
            {
                (CardEditorWindowText, "CardEditorWindow_CardEditorWindowText"),
                (LanguageLabelText, "SettingMenu_Languages"),
            });

        LanguageDropdown.ClearOptions();
        LanguageDropdown.AddOptions(LanguageManager.Instance.LanguageDescs);

        InitializeCardPropertyForm();
        InitializePreviewCardGrid();
    }

    public override void Display()
    {
        base.Display();
        LanguageDropdown.onValueChanged.RemoveAllListeners();
        LanguageDropdown.value = LanguageManager.Instance.LanguagesShorts.IndexOf(LanguageManager.Instance.GetCurrentLanguage());
        LanguageDropdown.onValueChanged.AddListener(LanguageManager.Instance.LanguageDropdownChange);
        LanguageDropdown.onValueChanged.AddListener(OnLanguageChange);
    }

    private void OnLanguageChange(int _)
    {
        if (cur_PreviewCard)
        {
            ChangeCard(cur_PreviewCard.CardInfo.CardID);
        }
    }

    [SerializeField] private Text CardEditorWindowText;
    [SerializeField] private Text LanguageLabelText;
    [SerializeField] private Dropdown LanguageDropdown;

    #region Left CardProperties

    [SerializeField] private Transform CardPropertiesContainer;

    private List<CardPropertyFormRow> MyPropertiesRows = new List<CardPropertyFormRow>();
    private List<CardPropertyFormRow> CardPropertiesCommon = new List<CardPropertyFormRow>();
    private List<CardPropertyFormRow> SlotPropertiesRows = new List<CardPropertyFormRow>();
    private List<CardPropertyFormRow> WeaponPropertiesRows = new List<CardPropertyFormRow>();
    private List<CardPropertyFormRow> ShieldPropertiesRows = new List<CardPropertyFormRow>();
    private Dictionary<CardTypes, List<CardPropertyFormRow>> CardTypePropertiesDict = new Dictionary<CardTypes, List<CardPropertyFormRow>>();
    private Dictionary<SlotTypes, List<CardPropertyFormRow>> SlotTypePropertiesDict = new Dictionary<SlotTypes, List<CardPropertyFormRow>>();
    private Dictionary<WeaponTypes, List<CardPropertyFormRow>> WeaponTypePropertiesDict = new Dictionary<WeaponTypes, List<CardPropertyFormRow>>();
    private Dictionary<ShieldTypes, List<CardPropertyFormRow>> ShieldTypePropertiesDict = new Dictionary<ShieldTypes, List<CardPropertyFormRow>>();

    private void InitializeCardPropertyForm()
    {
        foreach (CardPropertyFormRow cpfr in MyPropertiesRows)
        {
            cpfr.PoolRecycle();
        }

        MyPropertiesRows.Clear();
        CardTypePropertiesDict.Clear();

        IEnumerable<CardTypes> types_card = Enum.GetValues(typeof(CardTypes)) as IEnumerable<CardTypes>;
        List<string> cardTypeList = new List<string>();
        foreach (CardTypes cardType in types_card)
        {
            cardTypeList.Add(cardType.ToString());
            CardTypePropertiesDict.Add(cardType, new List<CardPropertyFormRow>());
        }

        IEnumerable<SlotTypes> types_slot = Enum.GetValues(typeof(SlotTypes)) as IEnumerable<SlotTypes>;
        List<string> slotTypeList = new List<string>();
        foreach (SlotTypes slotType in types_slot)
        {
            slotTypeList.Add(slotType.ToString());
            SlotTypePropertiesDict.Add(slotType, new List<CardPropertyFormRow>());
        }

        IEnumerable<WeaponTypes> types_weapon = Enum.GetValues(typeof(WeaponTypes)) as IEnumerable<WeaponTypes>;
        List<string> weaponTypeList = new List<string>();
        foreach (WeaponTypes weaponType in types_weapon)
        {
            weaponTypeList.Add(weaponType.ToString());
            WeaponTypePropertiesDict.Add(weaponType, new List<CardPropertyFormRow>());
        }

        IEnumerable<ShieldTypes> types_shield = Enum.GetValues(typeof(ShieldTypes)) as IEnumerable<ShieldTypes>;
        List<string> shieldTypeList = new List<string>();
        foreach (ShieldTypes shieldType in types_shield)
        {
            shieldTypeList.Add(shieldType.ToString());
            ShieldTypePropertiesDict.Add(shieldType, new List<CardPropertyFormRow>());
        }

        CardPropertyFormRow Row_CardType = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Dropdown, "CardEditorWindow_CardType", OnCardTypeChange, out SetCardType, cardTypeList);
        CardPropertyFormRow Row_CardID = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_CardIDLabelText", OnCardIDChange, out SetCardID);
        CardPropertyFormRow Row_CardName = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_CardNameLabelText", OnCardNameChange, out SetCardName);
        CardPropertyFormRow Row_CardCoinCost = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_CardCoinCostLabelText", OnCardCoinCostChange, out SetCardCoinCost);
        CardPropertyFormRow Row_CardMetalCost = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_CardMetalCostLabelText", OnCardMetalCostChange, out SetCardMetalCost);
        CardPropertyFormRow Row_CardEnergyCost = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_CardEnergyCostLabelText", OnCardEnergyCostChange, out SetCardEnergyCost);

        CardPropertyFormRow Row_RetinueLife = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_RetinueLifeLabelText", OnRetinueLifeChange, out SetRetinueLife);
        CardPropertyFormRow Row_RetinueAttack = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_RetinueAttackLabelText", OnRetinueAttackChange, out SetRetinueAttack);
        CardPropertyFormRow Row_RetinueArmor = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_RetinueArmorLabelText", OnRetinueArmorChange, out SetRetinueArmor);
        CardPropertyFormRow Row_RetinueShield = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_RetinueShieldLabelText", OnRetinueShieldChange, out SetRetinueShield);
        CardPropertyFormRow Row_RetinueWeaponSlot = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueWeaponSlotLabelText", OnRetinueWeaponSlotChange, out SetRetinueWeaponSlot);
        CardPropertyFormRow Row_RetinueShieldSlot = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueShieldSlotLabelText", OnRetinueShieldSlotChange, out SetRetinueShieldSlot);
        CardPropertyFormRow Row_RetinuePackSlot = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinuePackSlotLabelText", OnRetinuePackSlotChange, out SetRetinuePackSlot);
        CardPropertyFormRow Row_RetinueMASlot = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueMASlotLabelText", OnRetinueMASlotChange, out SetRetinueMASlot);

        CardPropertyFormRow Row_SlotType = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Dropdown, "CardEditorWindow_SlotType", OnSlotTypeChange, out SetSlotType, slotTypeList);

        CardPropertyFormRow Row_WeaponType = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Dropdown, "CardEditorWindow_WeaponTypeLabelText", OnWeaponTypeChange, out SetWeaponType, weaponTypeList);
        CardPropertyFormRow Row_WeaponSwordAttack = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_WeaponSwordAttackLabelText", OnWeaponSwordAttackChange, out SetWeaponSwordAttack);
        CardPropertyFormRow Row_WeaponSwordEnergy = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_WeaponSwordEnergyLabelText", OnWeaponSwordEnergyChange, out SetWeaponSwordEnergy);
        CardPropertyFormRow Row_WeaponSwordMaxEnergy = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_WeaponSwordMaxEnergyLabelText", OnWeaponSwordMaxEnergyChange, out SetWeaponSwordMaxEnergy);
        CardPropertyFormRow Row_WeaponGunAttack = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_WeaponGunAttackLabelText", OnWeaponGunAttackChange, out SetWeaponGunAttack);
        CardPropertyFormRow Row_WeaponGunBullet = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_WeaponGunBulletLabelText", OnWeaponGunBulletChange, out SetWeaponGunBullet);
        CardPropertyFormRow Row_WeaponGunMaxBullet = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_WeaponGunMaxBulletLabelText", OnWeaponGunMaxBulletChange, out SetWeaponGunMaxBullet);

        CardPropertyFormRow Row_ShieldType = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Dropdown, "CardEditorWindow_ShieldTypeLabelText", OnShieldTypeChange, out SetShieldType, shieldTypeList);
        CardPropertyFormRow Row_ShieldBasicArmor = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_ShieldBasicArmorLabelText", OnShieldBasicArmorChange, out SetShieldBasicArmor);
        CardPropertyFormRow Row_ShieldBasicShield = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputFiled, "CardEditorWindow_ShieldBasicShieldLabelText", OnShieldBasicShieldChange, out SetShieldBasicShield);

        CardPropertiesCommon = new List<CardPropertyFormRow>
        {
            Row_CardType,
            Row_CardID,
            Row_CardName,
            Row_CardCoinCost,
            Row_CardMetalCost,
            Row_CardEnergyCost,
        };
        CardTypePropertiesDict[CardTypes.Retinue] = new List<CardPropertyFormRow>
        {
            Row_RetinueLife,
            Row_RetinueAttack,
            Row_RetinueArmor,
            Row_RetinueShield,
            Row_RetinueWeaponSlot,
            Row_RetinueShieldSlot,
            Row_RetinuePackSlot,
            Row_RetinueMASlot
        };
        CardTypePropertiesDict[CardTypes.Energy] = new List<CardPropertyFormRow>
        {
        };
        CardTypePropertiesDict[CardTypes.Spell] = new List<CardPropertyFormRow>
        {
        };
        CardTypePropertiesDict[CardTypes.Equip] = new List<CardPropertyFormRow>
        {
            Row_SlotType,
        };

        SlotPropertiesRows = new List<CardPropertyFormRow>
        {
            Row_WeaponType,
            Row_WeaponSwordAttack,
            Row_WeaponSwordEnergy,
            Row_WeaponSwordMaxEnergy,
            Row_WeaponGunAttack,
            Row_WeaponGunBullet,
            Row_WeaponGunMaxBullet,
            Row_ShieldType,
            Row_ShieldBasicArmor,
            Row_ShieldBasicShield
        };
        SlotTypePropertiesDict[SlotTypes.Weapon] = new List<CardPropertyFormRow>
        {
            Row_WeaponType,
        };
        SlotTypePropertiesDict[SlotTypes.Shield] = new List<CardPropertyFormRow>
        {
            Row_ShieldType,
        };

        WeaponPropertiesRows = new List<CardPropertyFormRow>
        {
            Row_WeaponSwordAttack,
            Row_WeaponSwordEnergy,
            Row_WeaponSwordMaxEnergy,
            Row_WeaponGunAttack,
            Row_WeaponGunBullet,
            Row_WeaponGunMaxBullet
        };
        WeaponTypePropertiesDict[WeaponTypes.None] = new List<CardPropertyFormRow>
        {
        };
        WeaponTypePropertiesDict[WeaponTypes.Sword] = new List<CardPropertyFormRow>
        {
            Row_WeaponSwordAttack,
            Row_WeaponSwordEnergy,
            Row_WeaponSwordMaxEnergy,
        };
        WeaponTypePropertiesDict[WeaponTypes.Gun] = new List<CardPropertyFormRow>
        {
            Row_WeaponGunAttack,
            Row_WeaponGunBullet,
            Row_WeaponGunMaxBullet
        };
        WeaponTypePropertiesDict[WeaponTypes.SniperGun] = new List<CardPropertyFormRow>
        {
            Row_WeaponGunAttack,
            Row_WeaponGunBullet,
            Row_WeaponGunMaxBullet
        };

        ShieldPropertiesRows.AddRange(new[]
        {
            Row_ShieldBasicArmor,
            Row_ShieldBasicShield
        });
        ShieldTypePropertiesDict[ShieldTypes.Mixed] = new List<CardPropertyFormRow>
        {
            Row_ShieldBasicArmor,
            Row_ShieldBasicShield
        };
        ShieldTypePropertiesDict[ShieldTypes.Armor] = new List<CardPropertyFormRow>
        {
            Row_ShieldBasicArmor
        };
        ShieldTypePropertiesDict[ShieldTypes.Shield] = new List<CardPropertyFormRow>
        {
            Row_ShieldBasicShield
        };

        SetCardType("Spell");
        SetCardType("Retinue");
    }

    private CardPropertyFormRow GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType type, string labelKey, UnityAction<string> onValueChange, out UnityAction<string> setValue, List<string> dropdownOptionList = null)
    {
        CardPropertyFormRow cpfr = CardPropertyFormRow.BaseInitialize(type, CardPropertiesContainer, labelKey, onValueChange, out setValue, dropdownOptionList);
        MyPropertiesRows.Add(cpfr);
        return cpfr;
    }

    private bool OnChangeCardTypeByEdit = false;
    private UnityAction<string> SetCardType;

    private void OnCardTypeChange(string value_str)
    {
        CardTypes type = (CardTypes) Enum.Parse(typeof(CardTypes), value_str);
        foreach (CardPropertyFormRow cpfr in CardPropertiesCommon)
        {
            cpfr.gameObject.SetActive(true);
        }

        List<CardPropertyFormRow> targets = CardTypePropertiesDict[type];
        foreach (CardPropertyFormRow cpfr in MyPropertiesRows)
        {
            if (!CardPropertiesCommon.Contains(cpfr))
            {
                cpfr.gameObject.SetActive(targets.Contains(cpfr));
            }
        }

        if (!isPreviewExistingCards && type == CardTypes.Equip)
        {
            SetSlotType("None");
        }

        if (cur_PreviewCard)
        {
            if (!OnChangeCardTypeByEdit)
            {
                OnChangeCardTypeByEdit = true;
                switch (type)
                {
                    case CardTypes.Retinue:
                    {
                        CardInfo_Retinue newCI = CardInfo_Base.ConvertCardInfo<CardInfo_Retinue>(cur_PreviewCard.CardInfo);
                        ChangeCard(newCI);
                        break;
                    }
                    case CardTypes.Equip:
                    {
                        CardInfo_Equip newCI = CardInfo_Base.ConvertCardInfo<CardInfo_Equip>(cur_PreviewCard.CardInfo);
                        ChangeCard(newCI);
                        break;
                    }
                    case CardTypes.Spell:
                    {
                        CardInfo_Spell newCI = CardInfo_Base.ConvertCardInfo<CardInfo_Spell>(cur_PreviewCard.CardInfo);
                        ChangeCard(newCI);
                        break;
                    }
                    case CardTypes.Energy:
                    {
                        CardInfo_Spell newCI = CardInfo_Base.ConvertCardInfo<CardInfo_Spell>(cur_PreviewCard.CardInfo);
                        ChangeCard(newCI);
                        break;
                    }
                }

                OnChangeCardTypeByEdit = false;
            }
        }
    }

    private UnityAction<string> SetCardID;

    private void OnCardIDChange(string value_str)
    {
        int cardID = -1;
        int.TryParse(value_str, out cardID);
        if (cardID != -1)
        {
        }
    }

    private UnityAction<string> SetCardName;

    private void OnCardNameChange(string value_str)
    {
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.BaseInfo.CardNames[LanguageManager.Instance.GetCurrentLanguage()] = value_str;
            cur_PreviewCard.M_Name = value_str;
        }
    }

    private UnityAction<string> SetCardCoinCost;

    private void OnCardCoinCostChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.BaseInfo.Coin = value;
                cur_PreviewCard.M_Coin = value;
            }
        }
    }

    private UnityAction<string> SetCardMetalCost;

    private void OnCardMetalCostChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.BaseInfo.Metal = value;
                cur_PreviewCard.M_Metal = value;
            }
        }
    }

    private UnityAction<string> SetCardEnergyCost;

    private void OnCardEnergyCostChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.BaseInfo.Energy = value;
                cur_PreviewCard.M_Energy = value;
            }
        }
    }

    private UnityAction<string> SetRetinueLife;

    private void OnRetinueLifeChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.LifeInfo.TotalLife = value;
                cur_PreviewCard.CardInfo.LifeInfo.Life = value;
                if (cur_PreviewCard as CardRetinue)
                {
                    ((CardRetinue) cur_PreviewCard).M_RetinueTotalLife = value;
                }
            }
        }
    }

    private UnityAction<string> SetRetinueAttack;

    private void OnRetinueAttackChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.BattleInfo.BasicAttack = value;
                if (cur_PreviewCard as CardRetinue)
                {
                    ((CardRetinue) cur_PreviewCard).M_RetinueAttack = value;
                    cur_PreviewCard.RefreshCardTextLanguage();
                    cur_PreviewCard.RefreshCardAllColors();
                }
            }
        }
    }

    private UnityAction<string> SetRetinueArmor;

    private void OnRetinueArmorChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.BattleInfo.BasicArmor = value;
                if (cur_PreviewCard as CardRetinue)
                {
                    ((CardRetinue) cur_PreviewCard).M_RetinueArmor = value;
                    cur_PreviewCard.RefreshCardTextLanguage();
                    cur_PreviewCard.RefreshCardAllColors();
                }
            }
        }
    }

    private UnityAction<string> SetRetinueShield;

    private void OnRetinueShieldChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.BattleInfo.BasicShield = value;
                if (cur_PreviewCard as CardRetinue)
                {
                    ((CardRetinue) cur_PreviewCard).M_RetinueShield = value;
                    cur_PreviewCard.RefreshCardTextLanguage();
                    cur_PreviewCard.RefreshCardAllColors();
                }
            }
        }
    }

    private UnityAction<string> SetRetinueWeaponSlot;

    private void OnRetinueWeaponSlotChange(string value_str)
    {
        bool hasSlot = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.Slots[0] = hasSlot ? SlotTypes.Weapon : SlotTypes.None;
            if (cur_PreviewCard as CardRetinue)
            {
                ((CardRetinue) cur_PreviewCard).InitSlots();
            }
        }
    }

    private UnityAction<string> SetRetinueShieldSlot;

    private void OnRetinueShieldSlotChange(string value_str)
    {
        bool hasSlot = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.Slots[1] = hasSlot ? SlotTypes.Shield : SlotTypes.None;
            if (cur_PreviewCard as CardRetinue)
            {
                ((CardRetinue) cur_PreviewCard).InitSlots();
            }
        }
    }

    private UnityAction<string> SetRetinuePackSlot;

    private void OnRetinuePackSlotChange(string value_str)
    {
        bool hasSlot = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.Slots[2] = hasSlot ? SlotTypes.Pack : SlotTypes.None;
            if (cur_PreviewCard as CardRetinue)
            {
                ((CardRetinue) cur_PreviewCard).InitSlots();
            }
        }
    }

    private UnityAction<string> SetRetinueMASlot;

    private void OnRetinueMASlotChange(string value_str)
    {
        bool hasSlot = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.Slots[3] = hasSlot ? SlotTypes.MA : SlotTypes.None;
            if (cur_PreviewCard as CardRetinue)
            {
                ((CardRetinue) cur_PreviewCard).InitSlots();
            }
        }
    }

    private UnityAction<string> SetSlotType;

    private void OnSlotTypeChange(string value_str)
    {
        SlotTypes type = (SlotTypes) Enum.Parse(typeof(SlotTypes), value_str);
        foreach (CardPropertyFormRow cpfr in SlotPropertiesRows)
        {
            cpfr.gameObject.SetActive(false);
        }

        List<CardPropertyFormRow> targets_slot = SlotTypePropertiesDict[type];
        foreach (CardPropertyFormRow cpfr in targets_slot)
        {
            cpfr.gameObject.SetActive(targets_slot.Contains(cpfr));
        }

        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.EquipInfo.SlotType = type;
            cur_PreviewCard.RefreshCardTextLanguage();
            cur_PreviewCard.RefreshCardAllColors();
        }

        if (!isPreviewExistingCards)
        {
            SetWeaponType("None");
            SetShieldType("None");
        }
    }

    private UnityAction<string> SetWeaponType;

    private void OnWeaponTypeChange(string value_str)
    {
        WeaponTypes type = (WeaponTypes) Enum.Parse(typeof(WeaponTypes), value_str);
        List<CardPropertyFormRow> targets_weapon = WeaponTypePropertiesDict[type];
        foreach (CardPropertyFormRow cpfr in WeaponPropertiesRows)
        {
            cpfr.gameObject.SetActive(targets_weapon.Contains(cpfr));
        }

        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.WeaponInfo.WeaponType = type;
            cur_PreviewCard.RefreshCardTextLanguage();
            cur_PreviewCard.RefreshCardAllColors();
        }
    }

    private UnityAction<string> SetWeaponSwordAttack;

    private void OnWeaponSwordAttackChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.WeaponInfo.Attack = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    private UnityAction<string> SetWeaponSwordEnergy;

    private void OnWeaponSwordEnergyChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.WeaponInfo.Energy = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    private UnityAction<string> SetWeaponSwordMaxEnergy;

    private void OnWeaponSwordMaxEnergyChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.WeaponInfo.EnergyMax = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    private UnityAction<string> SetWeaponGunAttack;

    private void OnWeaponGunAttackChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.WeaponInfo.Attack = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    private UnityAction<string> SetWeaponGunBullet;

    private void OnWeaponGunBulletChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.WeaponInfo.Energy = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    private UnityAction<string> SetWeaponGunMaxBullet;

    private void OnWeaponGunMaxBulletChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.WeaponInfo.EnergyMax = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    private UnityAction<string> SetShieldType;

    private void OnShieldTypeChange(string value_str)
    {
        ShieldTypes type = (ShieldTypes) Enum.Parse(typeof(ShieldTypes), value_str);
        List<CardPropertyFormRow> targets_shield = ShieldTypePropertiesDict[type];
        foreach (CardPropertyFormRow cpfr in ShieldPropertiesRows)
        {
            cpfr.gameObject.SetActive(targets_shield.Contains(cpfr));
        }

        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.ShieldInfo.ShieldType = type;
            cur_PreviewCard.RefreshCardTextLanguage();
            cur_PreviewCard.RefreshCardAllColors();
        }
    }

    private UnityAction<string> SetShieldBasicArmor;

    private void OnShieldBasicArmorChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.ShieldInfo.Armor = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    private UnityAction<string> SetShieldBasicShield;

    private void OnShieldBasicShieldChange(string value_str)
    {
        int value = -1;
        int.TryParse(value_str, out value);
        if (value != -1)
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.ShieldInfo.Shield = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    #endregion

    #region Center CardPreview

    [SerializeField] private Transform CardPreviewContainer;
    private CardBase cur_PreviewCard;
    private bool isPreviewExistingCards = false;

    private void ChangeCard(CardInfo_Base ci)
    {
        isPreviewExistingCards = true;

        cur_PreviewCard?.PoolRecycle();
        cur_PreviewCard = CardBase.InstantiateCardByCardInfo(ci, CardPreviewContainer, CardBase.CardShowMode.CardSelect);
        cur_PreviewCard.transform.localScale = Vector3.one * 30;

        SetCardType(cur_PreviewCard.CardInfo.BaseInfo.CardType.ToString());
        SetCardID(string.Format("{0:000}", ci.CardID));
        SetCardName(cur_PreviewCard.CardInfo.BaseInfo.CardNames[LanguageManager.Instance.GetCurrentLanguage()]);
        SetCardCoinCost(cur_PreviewCard.CardInfo.BaseInfo.Coin.ToString());
        SetCardMetalCost(cur_PreviewCard.CardInfo.BaseInfo.Metal.ToString());
        SetCardEnergyCost(cur_PreviewCard.CardInfo.BaseInfo.Energy.ToString());

        switch (ci.BaseInfo.CardType)
        {
            case CardTypes.Retinue:
            {
                SetRetinueLife(ci.LifeInfo.Life.ToString());
                SetRetinueAttack(ci.BattleInfo.BasicAttack.ToString());
                SetRetinueArmor(ci.BattleInfo.BasicArmor.ToString());
                SetRetinueShield(ci.BattleInfo.BasicShield.ToString());
                SetRetinueWeaponSlot((ci.RetinueInfo.Slots[0] == SlotTypes.Weapon).ToString());
                SetRetinueShieldSlot((ci.RetinueInfo.Slots[1] == SlotTypes.Shield).ToString());
                SetRetinuePackSlot((ci.RetinueInfo.Slots[2] == SlotTypes.Pack).ToString());
                SetRetinueMASlot((ci.RetinueInfo.Slots[3] == SlotTypes.MA).ToString());
                break;
            }
            case CardTypes.Equip:
            {
                SetSlotType(ci.EquipInfo.SlotType.ToString());
                switch (ci.EquipInfo.SlotType)
                {
                    case SlotTypes.Weapon:
                    {
                        SetWeaponType(ci.WeaponInfo.WeaponType.ToString());
                        switch (ci.WeaponInfo.WeaponType)
                        {
                            case WeaponTypes.Sword:
                            {
                                SetWeaponSwordAttack(ci.WeaponInfo.Attack.ToString());
                                SetWeaponSwordEnergy(ci.WeaponInfo.Energy.ToString());
                                SetWeaponSwordMaxEnergy(ci.WeaponInfo.EnergyMax.ToString());
                                break;
                            }
                            case WeaponTypes.Gun:
                            {
                                SetWeaponGunAttack(ci.WeaponInfo.Attack.ToString());
                                SetWeaponGunBullet(ci.WeaponInfo.Energy.ToString());
                                SetWeaponGunMaxBullet(ci.WeaponInfo.EnergyMax.ToString());
                                break;
                            }
                            case WeaponTypes.SniperGun:
                            {
                                SetWeaponGunAttack(ci.WeaponInfo.Attack.ToString());
                                SetWeaponGunBullet(ci.WeaponInfo.Energy.ToString());
                                SetWeaponGunMaxBullet(ci.WeaponInfo.EnergyMax.ToString());
                                break;
                            }
                        }

                        break;
                    }

                    case SlotTypes.Shield:
                    {
                        SetShieldType(ci.ShieldInfo.ShieldType.ToString());
                        SetShieldBasicArmor(ci.ShieldInfo.Armor.ToString());
                        SetShieldBasicShield(ci.ShieldInfo.Shield.ToString());
                        break;
                    }
                }

                break;
            }
        }

        cur_PreviewCard.RefreshCardTextLanguage();
        cur_PreviewCard.RefreshCardAllColors();

        isPreviewExistingCards = false;
    }

    private void ChangeCard(int cardID)
    {
        if (AllCards.CardDict.ContainsKey(cardID))
        {
            ChangeCard(AllCards.GetCard(cardID));
        }
    }

    #endregion

    #region Right CardPics

    [SerializeField] private GridLayoutGroup ExistingCardGridContainer;

    private void InitializePreviewCardGrid()
    {
        foreach (KeyValuePair<int, CardInfo_Base> kv in AllCards.CardDict)
        {
            CardPreviewButton cpb = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.CardPreviewButton].AllocateGameObject<CardPreviewButton>(ExistingCardGridContainer.transform);
            cpb.Initialize(kv.Value, delegate { ChangeCard(kv.Key); });
        }
    }

    #endregion
}