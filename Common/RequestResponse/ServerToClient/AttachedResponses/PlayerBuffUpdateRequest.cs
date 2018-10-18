﻿public class PlayerBuffUpdateRequest : ServerRequestBase
{
    public int clientId;
    public int playerBuffId;
    public int value;

    public PlayerBuffUpdateRequest()
    {
    }

    public PlayerBuffUpdateRequest(int clientId, int playerBuffId,  int value)
    {
        this.clientId = clientId;
        this.playerBuffId = playerBuffId;
        this.value = value;
    }

    public override NetProtocols GetProtocol()
    {
        return NetProtocols.SE_PLAYER_BUFF_UPDATE_REQUEST;
    }

    public override string GetProtocolName()
    {
        return "SE_PLAYER_BUFF_UPDATE_REQUEST";
    }

    public override void Serialize(DataStream writer)
    {
        base.Serialize(writer);
        writer.WriteSInt32(clientId);
        writer.WriteSInt32(playerBuffId);
        writer.WriteSInt32(value);
    }

    public override void Deserialize(DataStream reader)
    {
        base.Deserialize(reader);
        clientId = reader.ReadSInt32();
        playerBuffId = reader.ReadSInt32();
        value = reader.ReadSInt32();
    }

    public override string DeserializeLog()
    {
        string log = base.DeserializeLog();
        log += " [clientId]=" + clientId;
        log += " [playerBuffId]=" + playerBuffId;
        log += " [value]=" + value;
        return log;
    }
}