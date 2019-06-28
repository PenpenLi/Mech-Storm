﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChapterMapNode : PoolObject
{
    [SerializeField] private Image PicImage;
    [SerializeField] private Image SelectedBorder;
    [SerializeField] private Button Button;
    [SerializeField] private GameObject InfoPanel;
    [SerializeField] private Text LevelNameLabel;
    [SerializeField] private Text LevelNameText;
    [SerializeField] private Text LevelTypeLabel;
    [SerializeField] private Text LevelTypeText;

    public HashSet<int> AdjacentRoutes = new HashSet<int>();

    void Awake()
    {
        LanguageManager.Instance.RegisterTextKey(LevelNameLabel, "ChapterMap_LevelNameLabel");
        LanguageManager.Instance.RegisterTextKey(LevelTypeLabel, "ChapterMap_LevelTypeLabel");
    }

    public override void PoolRecycle()
    {
        IsSelected = false;
        IsHovered = false;
        OnHovered = null;
        Button.onClick.RemoveAllListeners();
        Cur_Level = null;
        base.PoolRecycle();
    }

    private int NodeIndex;

    private bool isSelected;

    public bool IsSelected
    {
        get { return isSelected; }
        set
        {
            isSelected = value;
            SelectedBorder.enabled = value;
        }
    }

    private bool isHovered;

    public bool IsHovered
    {
        get { return isHovered; }
        set
        {
            isHovered = value;
            InfoPanel.SetActive(isHovered);
            if (isHovered)
            {
                transform.SetAsFirstSibling();
                OnHovered?.Invoke(this);
            }
        }
    }

    private UnityAction<ChapterMapNode> OnHovered;

    public void Initialize(int nodeIndex, UnityAction<int> onSelected, UnityAction<ChapterMapNode> onHovered, LevelType levelType = LevelType.Enemy, EnemyType enemyType = EnemyType.Soldier)
    {
        AdjacentRoutes.Clear();
        OnHovered = onHovered;
        IsSelected = false;
        NodeIndex = nodeIndex;

        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(delegate { onSelected(NodeIndex); });

        int picID = 0;
        transform.localScale = Vector3.one * 1f;
        switch (levelType)
        {
            case LevelType.Enemy:
            {
                switch (enemyType)
                {
                    case EnemyType.Soldier:
                    {
                        picID = (int) AllCards.SpecialPicIDs.LockedEmeny;
                        break;
                    }
                    case EnemyType.Boss:
                    {
                        Button.transform.localScale = Vector3.one * 1.5f;
                        picID = (int) AllCards.SpecialPicIDs.LockedBoss;
                        break;
                    }
                }

                break;
            }
            case LevelType.Rest:
            {
                picID = (int) AllCards.SpecialPicIDs.Rest;
                break;
            }
            case LevelType.Shop:
            {
                picID = (int) AllCards.SpecialPicIDs.Shop;
                break;
            }
            case LevelType.Start:
            {
                picID = (int) AllCards.SpecialPicIDs.Skills;
                break;
            }
            case LevelType.Treasure:
            {
                Button.transform.localScale = Vector3.one * 1.2f;
                picID = (int) AllCards.SpecialPicIDs.Treasure;
                break;
            }
        }

        ClientUtils.ChangeImagePicture(PicImage, picID);
    }

    public Level Cur_Level;

    public void SetLevel(Level level)
    {
        Cur_Level = level;
        LevelNameText.text = level.LevelNames[LanguageManager.Instance.GetCurrentLanguage()];
        LevelTypeText.text = level.LevelType.ToString();
        ClientUtils.ChangeImagePicture(PicImage, Cur_Level.LevelPicID);
    }
}