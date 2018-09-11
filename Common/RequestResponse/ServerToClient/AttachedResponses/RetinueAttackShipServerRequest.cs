﻿public class RetinueAttackShipServerRequest : ServerRequestBase
{
    public int AttackRetinueClientId;
    public int AttackRetinueId;

    public RetinueAttackShipServerRequest()
    {
    }

    public RetinueAttackShipServerRequest(int attackRetinueClientId, int attackRetinueId)
    {
        AttackRetinueClientId = attackRetinueClientId;
        AttackRetinueId = attackRetinueId;
    }

    public override int GetProtocol()
    {
        return NetProtocols.SE_RETINUE_ATTACK_SHIP_SERVER_REQUEST;
    }

    public override string GetProtocolName()
    {
        return "SE_RETINUE_ATTACK_SHIP_SERVER_REQUEST";
    }

    public override void Serialize(DataStream writer)
    {
        base.Serialize(writer);
        writer.WriteSInt32(AttackRetinueClientId);
        writer.WriteSInt32(AttackRetinueId);
    }

    public override void Deserialize(DataStream reader)
    {
        base.Deserialize(reader);
        AttackRetinueClientId = reader.ReadSInt32();
        AttackRetinueId = reader.ReadSInt32();
    }

    public override string DeserializeLog()
    {
        string log = base.DeserializeLog();
        log += " [AttackRetinueClientId]=" + AttackRetinueClientId;
        log += " [AttackRetinueId]=" + AttackRetinueId;
        return log;
    }
}