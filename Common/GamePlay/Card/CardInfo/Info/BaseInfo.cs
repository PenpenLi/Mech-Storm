﻿using System.Collections.Generic;

public struct BaseInfo
{
    public int PictureID;
    public string CardName;
    public string CardName_en;
    public string CardDescRaw;
    public bool Hide;
    public int Metal;
    public int Energy;
    public int Coin;
    public int EffectFactor;
    public DragPurpose DragPurpose;
    public CardTypes CardType;

    public BaseInfo(int pictureID, string cardName, string cardName_en, string cardDescRaw, bool hide, int metal, int energy, int coin, int effectFactor, DragPurpose dragPurpose, CardTypes cardType)
    {
        PictureID = pictureID;
        CardName = cardName;
        CardName_en = cardName_en;
        CardDescRaw = cardDescRaw;
        Hide = hide;
        Metal = metal;
        Energy = energy;
        Coin = coin;
        EffectFactor = effectFactor;
        DragPurpose = dragPurpose;
        CardType = cardType;
    }

    private static string GetHightLightColor()
    {
        return GamePlaySettings.CardHightLightColor;
    }

    private static string GetImportantColor()
    {
        return GamePlaySettings.CardImportantColor;
    }

    public static string AddHightLightColorToText(string hightLightText)
    {
        return "<color=\"" + GetHightLightColor() + "\">" + hightLightText + "</color>";
    }

    public static string AddImportantColorToText(string hightLightText)
    {
        return "<color=\"" + GetImportantColor() + "\">" + hightLightText + "</color>";
    }

    public void Serialize(DataStream writer)
    {
        writer.WriteSInt32(PictureID);
        writer.WriteString8(CardName);
        writer.WriteString8(CardName_en);
        writer.WriteString8(CardDescRaw);
        writer.WriteByte(Hide ? (byte) 0x01 : (byte) 0x00);
        writer.WriteSInt32(Metal);
        writer.WriteSInt32(Energy);
        writer.WriteSInt32(Coin);
        writer.WriteSInt32(EffectFactor);
        writer.WriteSInt32((int) DragPurpose);
        writer.WriteSInt32((int) CardType);
    }

    public static BaseInfo Deserialze(DataStream reader)
    {
        int PictureID = reader.ReadSInt32();
        string CardName = reader.ReadString8();
        string CardName_en = reader.ReadString8();
        string CardDesc = reader.ReadString8();
        bool Hide = reader.ReadByte() == 0x01;
        int Metal = reader.ReadSInt32();
        int Energy = reader.ReadSInt32();
        int Coin = reader.ReadSInt32();
        int EffectFactor = reader.ReadSInt32();
        DragPurpose DragPurpose = (DragPurpose) reader.ReadSInt32();
        CardTypes CardType = (CardTypes) reader.ReadSInt32();
        return new BaseInfo(PictureID, CardName, CardName_en, CardDesc, Hide, Metal, Energy, Coin, EffectFactor, DragPurpose, CardType);
    }
}

public enum CardTypes
{
    Retinue,
    Spell,
    Energy,
    Equip,
}

public enum DragPurpose
{
    None = 0,
    Summon = 1,
    Equip = 2,
    Target = 3,
}