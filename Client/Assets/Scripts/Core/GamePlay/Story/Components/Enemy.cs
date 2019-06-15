﻿using System.Collections.Generic;
using System.Xml;

public class Enemy : Level
{
    public BuildInfo BuildInfo;
    public EnemyType EnemyType;
    private int hardFactor;
    public List<BonusGroup> AlwaysBonusGroup;
    public List<BonusGroup> OptionalBonusGroup;

    public Enemy(LevelThemeCategory levelThemeCategory, int levelPicID, SortedDictionary<string, string> levelNames, BuildInfo buildInfo, EnemyType enemyType, int hardFactor, List<BonusGroup> alwaysBonusGroup, List<BonusGroup> optionalBonusGroup)
        : base(LevelType.Enemy, levelThemeCategory, levelPicID, levelNames)
    {
        BuildInfo = buildInfo;
        EnemyType = enemyType;
        this.hardFactor = hardFactor;
        AlwaysBonusGroup = alwaysBonusGroup;
        OptionalBonusGroup = optionalBonusGroup;
    }

    public override Level Clone()
    {
        return new Enemy(LevelThemeCategory, LevelPicID, CloneVariantUtils.SortedDictionary(LevelNames), BuildInfo.Clone(), EnemyType, hardFactor, CloneVariantUtils.List(AlwaysBonusGroup), CloneVariantUtils.List(OptionalBonusGroup));
    }

    public override Level Variant()
    {
        //TODO
        return null;
    }

    /// <summary>
    /// Can only be executed in StoryEditor/CardEditor/LevelEditor
    /// </summary>
    public override bool DeleteCard(int cardID)
    {
        if (BuildInfo.M_BuildCards.CardSelectInfos.ContainsKey(cardID))
        {
            BuildInfo.M_BuildCards.CardSelectInfos.Remove(cardID);
            return true;
        }

        return false;
    }

    protected override void ChildrenExportToXML(XmlElement level_ele)
    {
        XmlDocument doc = level_ele.OwnerDocument;
        XmlElement enemy_ele = doc.CreateElement("EnemyInfo");
        level_ele.AppendChild(enemy_ele);

        enemy_ele.SetAttribute("enemyType", EnemyType.ToString());
        BuildInfo.ExportToXML(enemy_ele);

        XmlElement bonusGroupInfos_ele = doc.CreateElement("BonusGroupInfos");
        enemy_ele.AppendChild(bonusGroupInfos_ele);

        foreach (BonusGroup bg in AlwaysBonusGroup)
        {
            bg.ExportToXML(bonusGroupInfos_ele);
        }

        foreach (BonusGroup bg in OptionalBonusGroup)
        {
            bg.ExportToXML(bonusGroupInfos_ele);
        }
    }

    public override void Serialize(DataStream writer)
    {
        base.Serialize(writer);
        BuildInfo.Serialize(writer);
        writer.WriteSInt32((int) EnemyType);
        writer.WriteSInt32(hardFactor);

        writer.WriteSInt32(AlwaysBonusGroup.Count);
        foreach (BonusGroup bonus in AlwaysBonusGroup)
        {
            bonus.Serialize(writer);
        }

        writer.WriteSInt32(OptionalBonusGroup.Count);
        foreach (BonusGroup bonus in OptionalBonusGroup)
        {
            bonus.Serialize(writer);
        }
    }
}