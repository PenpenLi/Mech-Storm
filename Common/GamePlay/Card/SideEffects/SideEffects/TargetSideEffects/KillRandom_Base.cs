﻿public class KillRandom_Base : TargetSideEffect
{
    public override string GenerateDesc(bool isEnglish)
    {
        return HightlightStringFormat(HightlightColor, isEnglish ? DescRaw_en : DescRaw, GetChineseDescOfTargetRange(M_TargetRange, isEnglish, false, true));
    }

    public override void Serialze(DataStream writer)
    {
        base.Serialze(writer);
    }

    protected override void Deserialze(DataStream reader)
    {
        base.Deserialze(reader);
    }

    public override int CalculateDamage()
    {
        return 0;
    }

    public override int CalculateHeal()
    {
        return 0;
    }

    protected override void CloneParams(SideEffectBase copy)
    {
        base.CloneParams(copy);
    }
}