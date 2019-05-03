﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
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

        SaveCardButton.onClick.AddListener(SaveCard);
        ResetCardButton.onClick.AddListener(ResetCard);
        DeleteCardButton.onClick.AddListener(DeleteCard);

        LanguageManager.Instance.RegisterTextKeys(
            new List<(Text, string)>
            {
                (CardEditorWindowText, "CardEditorWindow_CardEditorWindowText"),
                (LanguageLabelText, "SettingMenu_Languages"),
                (SaveCardButtonText, "CardEditorWindow_SaveCardButtonText"),
                (ResetCardButtonText, "CardEditorWindow_ResetCardButtonText"),
                (DeleteCardButtonText, "CardEditorWindow_DeleteCardButtonText"),
                (CardTotalCountText, "CardEditorWindow_CardTotalCountText"),
            });

        LanguageDropdown.ClearOptions();
        LanguageDropdown.AddOptions(LanguageManager.Instance.LanguageDescs);
        CardTotalCountNumberText.text = AllCards.CardDict.Count.ToString();

        InitializeCardPropertyForm();
        InitializePreviewCardGrid();
        InitializePicSelectGrid();
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
            ChangeCard(cur_PreviewCard.CardInfo);
        }
    }

    [SerializeField] private Text CardEditorWindowText;
    [SerializeField] private Text CardTotalCountText;
    [SerializeField] private Text CardTotalCountNumberText;
    [SerializeField] private Text LanguageLabelText;
    [SerializeField] private Dropdown LanguageDropdown;

    #region Left CardProperties

    public Transform CardPropertiesContainer;

    private List<CardPropertyFormRow> MyPropertiesRows = new List<CardPropertyFormRow>();
    private List<CardPropertyFormRow> CardPropertiesCommon = new List<CardPropertyFormRow>();
    private List<CardPropertyFormRow> SlotPropertiesRows = new List<CardPropertyFormRow>();
    private List<CardPropertyFormRow> WeaponPropertiesRows = new List<CardPropertyFormRow>();
    private List<CardPropertyFormRow> ShieldPropertiesRows = new List<CardPropertyFormRow>();
    private Dictionary<CardTypes, List<CardPropertyFormRow>> CardTypePropertiesDict = new Dictionary<CardTypes, List<CardPropertyFormRow>>();
    private Dictionary<SlotTypes, List<CardPropertyFormRow>> SlotTypePropertiesDict = new Dictionary<SlotTypes, List<CardPropertyFormRow>>();
    private Dictionary<WeaponTypes, List<CardPropertyFormRow>> WeaponTypePropertiesDict = new Dictionary<WeaponTypes, List<CardPropertyFormRow>>();
    private Dictionary<ShieldTypes, List<CardPropertyFormRow>> ShieldTypePropertiesDict = new Dictionary<ShieldTypes, List<CardPropertyFormRow>>();

    private CardPropertyForm_SideEffectBundle Row_SideEffectBundle = null;

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
        CardPropertyFormRow Row_CardID = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardIDLabelText", OnCardIDChange, out SetCardID);
        CardPropertyFormRow Row_CardPicID = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardPicIDLabelText", OnCardPicIDChange, out SetCardPicID);
        CardPropertyFormRow Row_CardUpgradeID = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardUpgradeIDLabelText", OnCardUpgradeIDChange, out SetCardUpgradeID, null, OnUpgradeIDButtonClick);
        CardPropertyFormRow Row_CardDegradeID = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardDegradeIDLabelText", OnCardDegradeIDChange, out SetCardDegradeID, null, OnDegradeIDButtonClick);
        CardPropertyFormRow Row_CardName_zh = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardNameLabelText_zh", OnCardNameChange_zh, out SetCardName_zh);
        CardPropertyFormRow Row_CardName_en = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardNameLabelText_en", OnCardNameChange_en, out SetCardName_en);
        CardPropertyFormRow Row_CardCoinCost = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardCoinCostLabelText", OnCardCoinCostChange, out SetCardCoinCost);
        CardPropertyFormRow Row_CardMetalCost = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardMetalCostLabelText", OnCardMetalCostChange, out SetCardMetalCost);
        CardPropertyFormRow Row_CardEnergyCost = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardEnergyCostLabelText", OnCardEnergyCostChange, out SetCardEnergyCost);
        CardPropertyFormRow Row_CardSelectLimit = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_CardSelectLimitLabelText", OnCardSelectLimitChange, out SetCardSelectLimit);
        CardPropertyFormRow Row_CardIsTemp = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_CardIsTempLabelText", OnCardIsTempChange, out SetCardIsTemp);
        CardPropertyFormRow Row_CardIsHide = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_CardIsHideLabelText", OnCardIsHideChange, out SetCardIsHide);

        CardPropertyFormRow Row_RetinueLife = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_RetinueLifeLabelText", OnRetinueLifeChange, out SetRetinueLife);
        CardPropertyFormRow Row_RetinueAttack = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_RetinueAttackLabelText", OnRetinueAttackChange, out SetRetinueAttack);
        CardPropertyFormRow Row_RetinueArmor = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_RetinueArmorLabelText", OnRetinueArmorChange, out SetRetinueArmor);
        CardPropertyFormRow Row_RetinueShield = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_RetinueShieldLabelText", OnRetinueShieldChange, out SetRetinueShield);
        CardPropertyFormRow Row_RetinueWeaponSlot = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueWeaponSlotLabelText", OnRetinueWeaponSlotChange, out SetRetinueWeaponSlot);
        CardPropertyFormRow Row_RetinueShieldSlot = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueShieldSlotLabelText", OnRetinueShieldSlotChange, out SetRetinueShieldSlot);
        CardPropertyFormRow Row_RetinuePackSlot = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinuePackSlotLabelText", OnRetinuePackSlotChange, out SetRetinuePackSlot);
        CardPropertyFormRow Row_RetinueMASlot = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueMASlotLabelText", OnRetinueMASlotChange, out SetRetinueMASlot);
        CardPropertyFormRow Row_RetinueIsDefense = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueIsDefenseLabelText", OnRetinueIsDefenseChange, out SetRetinueIsDefense);
        CardPropertyFormRow Row_RetinueIsSniper = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueIsSniperLabelText", OnRetinueIsSniperChange, out SetRetinueIsSniper);
        CardPropertyFormRow Row_RetinueIsCharger = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueIsChargerLabelText", OnRetinueIsChargerChange, out SetRetinueIsCharger);
        CardPropertyFormRow Row_RetinueIsFrenzy = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueIsFrenzyLabelText", OnRetinueIsFrenzyChange, out SetRetinueIsFrenzy);
        CardPropertyFormRow Row_RetinueIsSoldier = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Toggle, "CardEditorWindow_RetinueIsSoldierLabelText", OnRetinueIsSoldierChange, out SetRetinueIsSoldier);

        CardPropertyFormRow Row_SlotType = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Dropdown, "CardEditorWindow_SlotType", OnSlotTypeChange, out SetSlotType, slotTypeList);

        CardPropertyFormRow Row_WeaponType = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Dropdown, "CardEditorWindow_WeaponTypeLabelText", OnWeaponTypeChange, out SetWeaponType, weaponTypeList);
        CardPropertyFormRow Row_WeaponSwordAttack = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_WeaponSwordAttackLabelText", OnWeaponSwordAttackChange, out SetWeaponSwordAttack);
        CardPropertyFormRow Row_WeaponSwordEnergy = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_WeaponSwordEnergyLabelText", OnWeaponSwordEnergyChange, out SetWeaponSwordEnergy);
        CardPropertyFormRow Row_WeaponSwordMaxEnergy = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_WeaponSwordMaxEnergyLabelText", OnWeaponSwordMaxEnergyChange, out SetWeaponSwordMaxEnergy);
        CardPropertyFormRow Row_WeaponGunAttack = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_WeaponGunAttackLabelText", OnWeaponGunAttackChange, out SetWeaponGunAttack);
        CardPropertyFormRow Row_WeaponGunBullet = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_WeaponGunBulletLabelText", OnWeaponGunBulletChange, out SetWeaponGunBullet);
        CardPropertyFormRow Row_WeaponGunMaxBullet = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_WeaponGunMaxBulletLabelText", OnWeaponGunMaxBulletChange, out SetWeaponGunMaxBullet);

        CardPropertyFormRow Row_ShieldType = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.Dropdown, "CardEditorWindow_ShieldTypeLabelText", OnShieldTypeChange, out SetShieldType, shieldTypeList);
        CardPropertyFormRow Row_ShieldBasicArmor = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_ShieldBasicArmorLabelText", OnShieldBasicArmorChange, out SetShieldBasicArmor);
        CardPropertyFormRow Row_ShieldBasicShield = GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType.InputField, "CardEditorWindow_ShieldBasicShieldLabelText", OnShieldBasicShieldChange, out SetShieldBasicShield);

        Row_SideEffectBundle?.PoolRecycle();
        Row_SideEffectBundle = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.CardPropertyForm_SideEffectBundle].AllocateGameObject<CardPropertyForm_SideEffectBundle>(CardPropertiesContainer);

        Row_SideEffectBundle.Initialize(null, null, null);

        CardPropertiesCommon = new List<CardPropertyFormRow>
        {
            Row_CardType,
            Row_CardID,
            Row_CardPicID,
            Row_CardUpgradeID,
            Row_CardDegradeID,
            Row_CardName_zh,
            Row_CardName_en,
            Row_CardCoinCost,
            Row_CardMetalCost,
            Row_CardEnergyCost,
            Row_CardSelectLimit,
            Row_CardIsTemp,
            Row_CardIsHide,
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
            Row_RetinueMASlot,
            Row_RetinueIsSoldier,
            Row_RetinueIsDefense,
            Row_RetinueIsSniper,
            Row_RetinueIsCharger,
            Row_RetinueIsFrenzy,
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

        FormatTwoToggleIntoOneRow();
    }

    private CardPropertyFormRow GeneralizeRow(CardPropertyFormRow.CardPropertyFormRowType type, string labelKey, UnityAction<string> onValueChange, out UnityAction<string> setValue, List<string> dropdownOptionList = null, UnityAction<string> onButtonClick = null)
    {
        CardPropertyFormRow cpfr = CardPropertyFormRow.BaseInitialize(type, CardPropertiesContainer, labelKey, onValueChange, out setValue, dropdownOptionList, onButtonClick);
        MyPropertiesRows.Add(cpfr);
        return cpfr;
    }

    List<CardPropertyFormRow> twoToggleRows = new List<CardPropertyFormRow>();

    private void FormatTwoToggleIntoOneRow()
    {
        foreach (CardPropertyFormRow row in twoToggleRows)
        {
            CardPropertyFormRow_TwoToggleRow twoRow = (CardPropertyFormRow_TwoToggleRow) row;
            int index = row.transform.GetSiblingIndex();
            twoRow.ToggleLeft?.transform.SetParent(CardPropertiesContainer);
            twoRow.ToggleRight?.transform.SetParent(CardPropertiesContainer);
            twoRow.ToggleRight?.transform.SetSiblingIndex(index);
            twoRow.ToggleLeft?.transform.SetSiblingIndex(index);
            twoRow.PoolRecycle();
        }

        int count = 0;
        CardPropertyFormRow_Toggle lastToggle = null;
        UnityAction<string> temp = new UnityAction<string>(delegate(string arg0) { });
        foreach (CardPropertyFormRow row in MyPropertiesRows)
        {
            if (row is CardPropertyFormRow_Toggle toggleRow)
            {
                if (toggleRow.gameObject.activeInHierarchy)
                {
                    count++;
                    if (count == 2)
                    {
                        CardPropertyFormRow cpfr = CardPropertyFormRow.BaseInitialize(CardPropertyFormRow.CardPropertyFormRowType.TwoToggle, CardPropertiesContainer, "Nulll", null, out temp, null, null);
                        int index = toggleRow.transform.GetSiblingIndex();
                        cpfr.transform.SetSiblingIndex(index);
                        ((CardPropertyFormRow_TwoToggleRow) cpfr).ToggleLeft = lastToggle;
                        ((CardPropertyFormRow_TwoToggleRow) cpfr).ToggleRight = toggleRow;
                        lastToggle.transform.SetParent(cpfr.transform);
                        toggleRow.transform.SetParent(cpfr.transform);
                        count = 0;
                        twoToggleRows.Add(cpfr);
                    }

                    lastToggle = toggleRow;
                }
            }
            else
            {
                count = 0;
            }
        }
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
                CardInfo_Base newCardInfoBase = CardInfo_Base.ConvertCardInfo(cur_PreviewCard.CardInfo, type);
                ChangeCard(newCardInfoBase);
                OnChangeCardTypeByEdit = false;
            }
        }

        FormatTwoToggleIntoOneRow();
    }

    private UnityAction<string> SetCardID;

    private void OnCardIDChange(string value_str)
    {
        if (int.TryParse(value_str, out int value))
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.CardID = value;
            }

            foreach (KeyValuePair<int, CardPreviewButton> kv in CardPreviewButtons)
            {
                kv.Value.IsEdit = false;
            }

            if (CardPreviewButtons.ContainsKey(value))
            {
                CardPreviewButtons[value].IsEdit = true;
                if (-CardPreviewButtons[value].transform.localPosition.y - ExistingCardGridContainer.transform.localPosition.y > ((RectTransform) ExistingCardGridContainer.transform.parent).rect.height * 0.8f
                    || -CardPreviewButtons[value].transform.localPosition.y - ExistingCardGridContainer.transform.localPosition.y < ((RectTransform) ExistingCardGridContainer.transform.parent).rect.height * 0.2f
                )
                {
                    ExistingCardGridContainer.transform.localPosition = new Vector3(ExistingCardGridContainer.transform.localPosition.x, -CardPreviewButtons[value].transform.localPosition.y - 260f, ExistingCardGridContainer.transform.localPosition.z);
                }
            }
        }
    }

    private UnityAction<string> SetCardPicID;

    private void OnCardPicIDChange(string value_str)
    {
        if (int.TryParse(value_str, out int value))
        {
            if (cur_PreviewCard)
            {
                bool picIDValid = ClientUtils.IsCardPictureValid(value);
                if (picIDValid)
                {
                    cur_PreviewCard.CardInfo.BaseInfo.PictureID = value;
                    cur_PreviewCard.ChangeCardPicture(value);
                }
                else
                {
                    cur_PreviewCard.CardInfo.BaseInfo.PictureID = 999;
                    cur_PreviewCard.ChangeCardPicture(999);
                }
            }
        }
    }

    private UnityAction<string> SetCardUpgradeID;

    private void OnCardUpgradeIDChange(string value_str)
    {
        if (int.TryParse(value_str, out int value))
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.UpgradeInfo.UpgradeCardID = value;
            }
        }
    }

    private void OnUpgradeIDButtonClick(string value_str)
    {
        if (value_str == "-1" || value_str == "")
        {
            return;
        }

        if (int.TryParse(value_str, out int value))
        {
            bool suc = ChangeCard(value);
            if (suc)
            {
            }
            else
            {
                NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("CardEditorWindow_NoSuchCard"), 0, 0.5f);
            }
        }
    }

    private UnityAction<string> SetCardDegradeID;

    private void OnCardDegradeIDChange(string value_str)
    {
        if (int.TryParse(value_str, out int value))
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.UpgradeInfo.DegradeCardID = value;
            }
        }
    }

    private void OnDegradeIDButtonClick(string value_str)
    {
        if (value_str == "-1" || value_str == "")
        {
            return;
        }

        if (int.TryParse(value_str, out int value))
        {
            bool suc = ChangeCard(value);
            if (suc)
            {
            }
            else
            {
                NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("CardEditorWindow_NoSuchCard"), 0, 0.5f);
            }
        }
    }

    private UnityAction<string> SetCardName_zh;

    private void OnCardNameChange_zh(string value_str)
    {
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.BaseInfo.CardNames["zh"] = value_str;
            cur_PreviewCard.M_Name = cur_PreviewCard.CardInfo.BaseInfo.CardNames[LanguageManager.Instance.GetCurrentLanguage()];
        }
    }

    private UnityAction<string> SetCardName_en;

    private void OnCardNameChange_en(string value_str)
    {
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.BaseInfo.CardNames["en"] = value_str;
            cur_PreviewCard.M_Name = cur_PreviewCard.CardInfo.BaseInfo.CardNames[LanguageManager.Instance.GetCurrentLanguage()];
        }
    }

    private UnityAction<string> SetCardCoinCost;

    private void OnCardCoinCostChange(string value_str)
    {
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.BaseInfo.Energy = value;
                cur_PreviewCard.M_Energy = value;
            }
        }
    }

    private UnityAction<string> SetCardSelectLimit;

    private void OnCardSelectLimitChange(string value_str)
    {
        if (int.TryParse(value_str, out int value))
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.BaseInfo.LimitNum = value;
                cur_PreviewCard.ChangeCardSelectLimit(value, true);
            }
        }
    }

    private UnityAction<string> SetCardIsTemp;

    private void OnCardIsTempChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.BaseInfo.IsTemp = value;
            cur_PreviewCard.RefreshCardTextLanguage();
        }
    }

    private UnityAction<string> SetCardIsHide;

    private void OnCardIsHideChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.BaseInfo.IsHide = value;
            cur_PreviewCard.RefreshCardTextLanguage();
        }
    }

    private UnityAction<string> SetRetinueLife;

    private void OnRetinueLifeChange(string value_str)
    {
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
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
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.Slots[0] = value ? SlotTypes.Weapon : SlotTypes.None;
            if (cur_PreviewCard as CardRetinue)
            {
                ((CardRetinue) cur_PreviewCard).InitSlots();
            }
        }
    }

    private UnityAction<string> SetRetinueShieldSlot;

    private void OnRetinueShieldSlotChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.Slots[1] = value ? SlotTypes.Shield : SlotTypes.None;
            if (cur_PreviewCard as CardRetinue)
            {
                ((CardRetinue) cur_PreviewCard).InitSlots();
            }
        }
    }

    private UnityAction<string> SetRetinuePackSlot;

    private void OnRetinuePackSlotChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.Slots[2] = value ? SlotTypes.Pack : SlotTypes.None;
            if (cur_PreviewCard as CardRetinue)
            {
                ((CardRetinue) cur_PreviewCard).InitSlots();
            }
        }
    }

    private UnityAction<string> SetRetinueMASlot;

    private void OnRetinueMASlotChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.Slots[3] = value ? SlotTypes.MA : SlotTypes.None;
            if (cur_PreviewCard as CardRetinue)
            {
                ((CardRetinue) cur_PreviewCard).InitSlots();
            }
        }
    }

    private UnityAction<string> SetRetinueIsDefense;

    private void OnRetinueIsDefenseChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.IsDefense = value;
            cur_PreviewCard.RefreshCardAllColors();
            cur_PreviewCard.RefreshCardTextLanguage();
        }
    }

    private UnityAction<string> SetRetinueIsSniper;

    private void OnRetinueIsSniperChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.IsSniper = value;
            cur_PreviewCard.RefreshCardAllColors();
            cur_PreviewCard.RefreshCardTextLanguage();
        }
    }

    private UnityAction<string> SetRetinueIsCharger;

    private void OnRetinueIsChargerChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.IsCharger = value;
            cur_PreviewCard.RefreshCardAllColors();
            cur_PreviewCard.RefreshCardTextLanguage();
        }
    }

    private UnityAction<string> SetRetinueIsFrenzy;

    private void OnRetinueIsFrenzyChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.IsFrenzy = value;
            cur_PreviewCard.RefreshCardAllColors();
            cur_PreviewCard.RefreshCardTextLanguage();
        }
    }

    private UnityAction<string> SetRetinueIsSoldier;

    private void OnRetinueIsSoldierChange(string value_str)
    {
        bool value = value_str.Equals("True");
        if (cur_PreviewCard)
        {
            cur_PreviewCard.CardInfo.RetinueInfo.IsSoldier = value;
            cur_PreviewCard.RefreshCardAllColors();
            cur_PreviewCard.RefreshCardTextLanguage();
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

        FormatTwoToggleIntoOneRow();
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

        FormatTwoToggleIntoOneRow();
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
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
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

        FormatTwoToggleIntoOneRow();
    }

    private UnityAction<string> SetShieldBasicArmor;

    private void OnShieldBasicArmorChange(string value_str)
    {
        if (int.TryParse(value_str, out int value))
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
        if (int.TryParse(value_str, out int value))
        {
            if (cur_PreviewCard)
            {
                cur_PreviewCard.CardInfo.ShieldInfo.Shield = value;
                cur_PreviewCard.RefreshCardTextLanguage();
                cur_PreviewCard.RefreshCardAllColors();
            }
        }
    }

    #region SideEffect

    #endregion

    #endregion

    #region Center CardPreview

    [SerializeField] private ScrollRect CardPreviewButtonScrollRect;
    [SerializeField] private Transform CardPreviewContainer;
    [SerializeField] private RawImage CardPreviewRawImage;
    private CardBase cur_PreviewCard;
    private CardBase cur_PreviewCard_Up;
    private CardBase cur_PreviewCard_De;
    private bool isPreviewExistingCards = false;

    private void ChangeCard(CardInfo_Base ci)
    {
        isPreviewExistingCards = true;

        cur_PreviewCard?.PoolRecycle();
        cur_PreviewCard = CardBase.InstantiateCardByCardInfo(ci, CardPreviewContainer, CardBase.CardShowMode.CardSelect);
        cur_PreviewCard.transform.localScale = Vector3.one * 35;
        cur_PreviewCard.transform.localPosition = new Vector3(-25, 0, 0);
        cur_PreviewCard.ShowCardBloom(true);

        cur_PreviewCard_Up?.PoolRecycle();
        cur_PreviewCard_Up = null;
        if (AllCards.CardDict.ContainsKey(ci.UpgradeInfo.UpgradeCardID))
        {
            CardInfo_Base cardInfoBase_Up = AllCards.GetCard(ci.UpgradeInfo.UpgradeCardID);
            cur_PreviewCard_Up = CardBase.InstantiateCardByCardInfo(cardInfoBase_Up, CardPreviewContainer, CardBase.CardShowMode.CardSelect);
            cur_PreviewCard_Up.transform.localScale = Vector3.one * 25;
            cur_PreviewCard_Up.transform.localPosition = new Vector3(475, 0, 0);
            cur_PreviewCard_Up.ShowCardBloom(true);
        }

        cur_PreviewCard_De?.PoolRecycle();
        cur_PreviewCard_De = null;
        if (AllCards.CardDict.ContainsKey(ci.UpgradeInfo.DegradeCardID))
        {
            CardInfo_Base cardInfoBase_De = AllCards.GetCard(ci.UpgradeInfo.DegradeCardID);
            cur_PreviewCard_De = CardBase.InstantiateCardByCardInfo(cardInfoBase_De, CardPreviewContainer, CardBase.CardShowMode.CardSelect);
            cur_PreviewCard_De.transform.localScale = Vector3.one * 25;
            cur_PreviewCard_De.transform.localPosition = new Vector3(-500, 0, 0);
            cur_PreviewCard_De.ShowCardBloom(true);
        }

        SetCardType(cur_PreviewCard.CardInfo.BaseInfo.CardType.ToString());
        SetCardID(string.Format("{0:000}", ci.CardID));
        SetCardPicID(string.Format("{0:000}", ci.BaseInfo.PictureID));
        SetCardUpgradeID(ci.UpgradeInfo.UpgradeCardID.ToString());
        SetCardDegradeID(ci.UpgradeInfo.DegradeCardID.ToString());
        SetCardName_zh(cur_PreviewCard.CardInfo.BaseInfo.CardNames["zh"]);
        SetCardName_en(cur_PreviewCard.CardInfo.BaseInfo.CardNames["en"]);
        SetCardCoinCost(cur_PreviewCard.CardInfo.BaseInfo.Coin.ToString());
        SetCardMetalCost(cur_PreviewCard.CardInfo.BaseInfo.Metal.ToString());
        SetCardEnergyCost(cur_PreviewCard.CardInfo.BaseInfo.Energy.ToString());
        SetCardSelectLimit(cur_PreviewCard.CardInfo.BaseInfo.LimitNum.ToString());
        SetCardIsTemp(cur_PreviewCard.CardInfo.BaseInfo.IsTemp.ToString());
        SetCardIsHide(cur_PreviewCard.CardInfo.BaseInfo.IsHide.ToString());

        switch (ci.BaseInfo.CardType)
        {
            case CardTypes.Retinue:
            {
                SetRetinueLife(ci.LifeInfo.Life.ToString());
                SetRetinueAttack(ci.BattleInfo.BasicAttack.ToString());
                SetRetinueArmor(ci.BattleInfo.BasicArmor.ToString());
                SetRetinueShield(ci.BattleInfo.BasicShield.ToString());
                SetRetinueIsSoldier((ci.RetinueInfo.IsSoldier).ToString());
                SetRetinueIsDefense((ci.RetinueInfo.IsDefense).ToString());
                SetRetinueIsSniper((ci.RetinueInfo.IsSniper).ToString());
                SetRetinueIsCharger((ci.RetinueInfo.IsCharger).ToString());
                SetRetinueIsFrenzy((ci.RetinueInfo.IsFrenzy).ToString());
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

        Row_SideEffectBundle.Initialize(cur_PreviewCard.CardInfo, cur_PreviewCard.CardInfo.SideEffectBundle, cur_PreviewCard.RefreshCardTextLanguage);

        cur_PreviewCard.RefreshCardTextLanguage();
        cur_PreviewCard.RefreshCardAllColors();
        FormatTwoToggleIntoOneRow();

        StartCoroutine(ClientUtils.UpdateLayout((RectTransform) CardPropertiesContainer));

        isPreviewExistingCards = false;
    }

    public bool ChangeCard(int cardID)
    {
        if (AllCards.CardDict.ContainsKey(cardID))
        {
            ChangeCard(AllCards.GetCard(cardID));
            return true;
        }

        return false;
    }

    [SerializeField] private Button SaveCardButton;
    [SerializeField] private Button ResetCardButton;
    [SerializeField] private Button DeleteCardButton;
    [SerializeField] private Text SaveCardButtonText;
    [SerializeField] private Text ResetCardButtonText;
    [SerializeField] private Text DeleteCardButtonText;

    private bool SaveFlag = false;

    void Update()
    {
        if (SaveFlag)
        {
            SaveCard();
            SaveFlag = false;
        }
        else
        {
#if PLATFORM_STANDALONE_OSX
            if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.S))
            {
                SaveFlag = true;
            }
#elif PLATFORM_STANDALONE
             if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                SaveFlag = true;
            }
#endif
        }

        int curCardID = -1;
        if (cur_PreviewCard)
        {
            curCardID = cur_PreviewCard.CardInfo.CardID;
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            foreach (KeyValuePair<int, CardInfo_Base> kv in AllCards.CardDict)
            {
                if (kv.Key > curCardID)
                {
                    ChangeCard(kv.Key);
                    break;
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            int changeCardID = 0;
            foreach (KeyValuePair<int, CardInfo_Base> kv in AllCards.CardDict)
            {
                if (kv.Key >= curCardID)
                {
                    ChangeCard(changeCardID);
                    break;
                }

                changeCardID = kv.Key;
            }
        }

        int gridColumns = Mathf.RoundToInt(((RectTransform) ExistingCardGridContainer.transform).rect.width - ExistingCardGridContainer.padding.left) / Mathf.RoundToInt(ExistingCardGridContainer.cellSize.x + ExistingCardGridContainer.spacing.x);

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            int count = 0;
            foreach (KeyValuePair<int, CardInfo_Base> kv in AllCards.CardDict)
            {
                if (kv.Key > curCardID)
                {
                    count++;
                    if (count >= gridColumns)
                    {
                        ChangeCard(kv.Key);
                        break;
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            int[] before = new int[gridColumns];
            for (int i = 0; i < before.Length; i++)
            {
                before[i] = -1;
            }

            foreach (KeyValuePair<int, CardInfo_Base> kv in AllCards.CardDict)
            {
                if (kv.Key >= curCardID)
                {
                    if (before[0] != -1)
                    {
                        ChangeCard(before[0]);
                        break;
                    }
                }

                for (int i = 0; i < before.Length - 1; i++)
                {
                    before[i] = before[i + 1];
                }

                before[before.Length - 1] = kv.Key;
            }
        }

        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            if (cur_PreviewCard)
            {
                DeleteCard();
            }
        }
        
        float size = Mathf.Min(1024f, ((RectTransform) CardPreviewContainer.transform).rect.width);
        ((RectTransform) CardPreviewRawImage.transform).sizeDelta = new Vector2(size, size);
    }

    public void SaveCard()
    {
        if (cur_PreviewCard)
        {
            ConfirmPanel cp = UIManager.Instance.ShowUIForms<ConfirmPanel>();
            string info = "";
            if (AllCards.CardDict.ContainsKey(cur_PreviewCard.CardInfo.CardID))
            {
                info = string.Format(LanguageManager.Instance.GetText("CardEditorPanel_ConfirmSaveCard"), cur_PreviewCard.CardInfo.CardID);
            }
            else
            {
                info = string.Format(LanguageManager.Instance.GetText("CardEditorPanel_ConfirmCreateNewCard"), cur_PreviewCard.CardInfo.CardID);
            }

            cp.Initialize(
                info,
                LanguageManager.Instance.GetText("Common_Yes"),
                LanguageManager.Instance.GetText("Common_No"),
                delegate
                {
                    AllCards.RefreshCardXML(cur_PreviewCard.CardInfo);
                    AllCards.ReloadCardXML();
                    InitializePreviewCardGrid();
                    cp.CloseUIForm();
                    NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("CardEditorPanel_SaveCardSuccess"), 0, 1f);
                    CardTotalCountNumberText.text = AllCards.CardDict.Count.ToString();
                },
                cp.CloseUIForm);
        }
        else
        {
            NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("CardEditorPanel_EmptyCardID"), 0, 1f);
        }
    }

    public void ResetCard()
    {
        if (cur_PreviewCard)
        {
            ChangeCard(cur_PreviewCard.CardInfo.CardID);
            NoticeManager.Instance.ShowInfoPanelCenter(string.Format(LanguageManager.Instance.GetText("CardEditorWindow_ResetCardNotice"), cur_PreviewCard.CardInfo.CardID), 0, 1f);
        }
    }

    public void DeleteCard()
    {
        if (cur_PreviewCard)
        {
            ConfirmPanel cp = UIManager.Instance.ShowUIForms<ConfirmPanel>();
            cp.Initialize(
                string.Format(LanguageManager.Instance.GetText("CardEditorPanel_ConfirmDeleteCard"), cur_PreviewCard.CardInfo.CardID),
                LanguageManager.Instance.GetText("Common_Yes"),
                LanguageManager.Instance.GetText("Common_No"),
                delegate
                {
                    AllCards.DeleteCard(cur_PreviewCard.CardInfo.CardID);
                    AllCards.ReloadCardXML();
                    InitializePreviewCardGrid();
                    cp.CloseUIForm();
                    NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("CardEditorPanel_DeleteCardSuccess"), 0, 1f);
                    CardTotalCountNumberText.text = AllCards.CardDict.Count.ToString();
                },
                cp.CloseUIForm);
        }
        else
        {
            NoticeManager.Instance.ShowInfoPanelCenter(LanguageManager.Instance.GetText("CardEditorPanel_EmptyCardID"), 0, 1f);
        }
    }

    #endregion

    #region Right CardPics

    [SerializeField] private GridLayoutGroup ExistingCardGridContainer;
    private SortedDictionary<int, CardPreviewButton> CardPreviewButtons = new SortedDictionary<int, CardPreviewButton>();

    private void InitializePreviewCardGrid()
    {
        foreach (KeyValuePair<int, CardPreviewButton> kv in CardPreviewButtons)
        {
            kv.Value.PoolRecycle();
        }

        CardPreviewButtons.Clear();
        foreach (KeyValuePair<int, CardInfo_Base> kv in AllCards.CardDict)
        {
            CardPreviewButton cpb = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.CardPreviewButton].AllocateGameObject<CardPreviewButton>(ExistingCardGridContainer.transform);
            cpb.Initialize(kv.Value, delegate { ChangeCard(kv.Key); });
            CardPreviewButtons.Add(kv.Key, cpb);
        }
    }

    #endregion

    #region PicSelectPanel

    [SerializeField] private GameObject PicSelectGridPanel;
    [SerializeField] private GridLayoutGroup PicSelectGridContainer;
    [SerializeField] private Button PicSelectGridOpenButton;
    [SerializeField] private Button PicSelectGridCloseButton;
    private List<PicPreviewButton> PicPreviewButtons = new List<PicPreviewButton>();

    private void InitializePicSelectGrid()
    {
        PicSelectGridPanel.SetActive(false);
        PicSelectGridCloseButton.gameObject.SetActive(false);
        foreach (PicPreviewButton ppb in PicPreviewButtons)
        {
            ppb.PoolRecycle();
        }

        PicPreviewButtons.Clear();

        SortedDictionary<int, Sprite> SpriteDict = new SortedDictionary<int, Sprite>();
        for (int i = 0; i <= 10; i++)
        {
            SpriteAtlas sa = AtlasManager.LoadAtlas("CardPics_" + i);
            Sprite[] Sprites = new Sprite[sa.spriteCount];
            sa.GetSprites(Sprites);
            foreach (Sprite sprite in Sprites)
            {
                SpriteDict.Add(int.Parse(sprite.name.Replace("(Clone)", "")), sprite);
            }
        }

        foreach (KeyValuePair<int, Sprite> kv in SpriteDict)
        {
            PicPreviewButton ppb = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.PicPreviewButton].AllocateGameObject<PicPreviewButton>(PicSelectGridContainer.transform);
            ppb.Initialize(kv.Value, delegate
            {
                SetCardPicID(kv.Key.ToString());
                PicSelectGridPanel.SetActive(false);
                PicSelectGridCloseButton.gameObject.SetActive(false);
                PicSelectGridOpenButton.gameObject.SetActive(true);
            });
            PicPreviewButtons.Add(ppb);
        }
    }

    public void OnPicSelectGridOpenButtonClick()
    {
        PicSelectGridPanel.SetActive(true);
        PicSelectGridCloseButton.gameObject.SetActive(true);
        PicSelectGridOpenButton.gameObject.SetActive(false);
    }

    public void OnPicSelectGridCloseButtonClick()
    {
        PicSelectGridPanel.SetActive(false);
        PicSelectGridCloseButton.gameObject.SetActive(false);
        PicSelectGridOpenButton.gameObject.SetActive(true);
    }

    #endregion
}