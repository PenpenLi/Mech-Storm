﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ClientRequestBase : Request
{
    public int clientId;

    public ClientRequestBase()
    {

    }

    public ClientRequestBase(int clientId)
    {
        this.clientId = clientId;
    }

    public override void Serialize(DataStream writer)
    {
        base.Serialize(writer);
        writer.WriteSInt32(clientId);
    }

    public override void Deserialize(DataStream reader)
    {
        base.Deserialize(reader);
        clientId = reader.ReadSInt32();
    }

    public override string DeserializeLog()
    {
        string log = base.DeserializeLog();
        log += " [clientId] " + clientId;
        return log;
    }
}