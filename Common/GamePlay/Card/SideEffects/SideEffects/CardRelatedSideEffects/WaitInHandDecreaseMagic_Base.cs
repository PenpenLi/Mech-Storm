﻿public class WaitInHandDecreaseEnergy_Base : CardRelatedSideEffect,IEffectFactor
{
    public int Value;
    public int Factor = 1;

    public int FinalValue
    {
        get { return Value * Factor; }

    }
    public override string GenerateDesc()
    {
        return HightlightStringFormat(HightlightColor, DescRaw, FinalValue);
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

    public void SetEffetFactor(int factor)
    {
        Factor = factor;
    }

    protected override void CloneParams(SideEffectBase copy)
    {
        base.CloneParams(copy);
        ((WaitInHandDecreaseEnergy_Base) copy).Value = Value;
        ((WaitInHandDecreaseEnergy_Base) copy).Factor = Factor;
    }
}