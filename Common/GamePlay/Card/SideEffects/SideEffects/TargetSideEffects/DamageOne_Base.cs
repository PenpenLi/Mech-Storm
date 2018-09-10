﻿public class DamageOne_Base : TargetSideEffect
{
    public int Value;
    public int Factor = 1;

    public int FinalValue
    {
        get { return Value * Factor; }
    }

    public override string GenerateDesc(bool isEnglish)
    {
        return HightlightStringFormat(HightlightColor, isEnglish ? DescRaw_en : DescRaw, ((M_TargetRange == TargetRange.SelfShip || M_TargetRange == TargetRange.EnemyShip) ? "" : isEnglish ? "a " : "一个") + GetChineseDescOfTargetRange(M_TargetRange, isEnglish), FinalValue);
    }

    public override void Serialze(DataStream writer)
    {
        base.Serialze(writer);
        writer.WriteSInt32(Value);
    }

    protected override void Deserialze(DataStream reader)
    {
        base.Deserialze(reader);
        Value = reader.ReadSInt32();
    }

    public override int CalculateDamage()
    {
        return FinalValue;
    }

    public override int CalculateHeal()
    {
        return 0;
    }

    public void SetEffetFactor(int factor)
    {
        Factor = factor;
    }

    protected override void CloneParams(SideEffectBase copy)
    {
        base.CloneParams(copy);
        ((DamageOne_Base) copy).Value = Value;
        ((DamageOne_Base) copy).Factor = Factor;
    }
}