﻿public class GamePlaySettings
{
    public static int MaxHandCard = 30;
    public static int DrawCardPerRound = 2;
    public static int FirstDrawCard = 5;
    public static int SecondDrawCard = 6;
    public static int MaxRetinueNumber = 7;

    public static int MaxHeroNumber = 4;

    public static int BeginCost = 20;
    public static int MaxCost = 30;
    public static int CostIncrease = 1;

    public static bool SuffleBuild = true;

    public static int PlayerDefaultMoney = 10000;
    public static int PlayerDefaultLife = 100;
    public static int PlayerDefaultLifeMax = 200;
    public static int PlayerDefaultLifeMin = 50;
    public static int PlayerDefaultMagic = 10;
    public static int PlayerDefaultMagicMax = 50;

    public static int LifeToMoney = 50;
    public static int MagicToMoney = 50;

    public static int PlayerDefaultMaxMoney
    {
        get { return PlayerDefaultMoney + (PlayerDefaultLife - PlayerDefaultLifeMin) * LifeToMoney + PlayerDefaultMagic * MagicToMoney; }
    }
}