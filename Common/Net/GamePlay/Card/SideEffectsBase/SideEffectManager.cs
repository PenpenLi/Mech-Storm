﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class SideEffectManager
{
    static Dictionary<string, Type> mSideEffecMap = new Dictionary<string, Type>();

    static SideEffectManager()
    {
    }

    public static void AddSideEffectTypes<T>() where T : SideEffectBase
    {
        mSideEffecMap.Add(typeof(T).ToString(), typeof(T));
    }

    public static SideEffectBase GetNewSideEffec(string SideEffectName)
    {
        Type type = mSideEffecMap[SideEffectName];
        SideEffectBase newSE = (SideEffectBase) type.Assembly.CreateInstance(type.ToString());
        return newSE;
    }
}