﻿using System;
using System.Collections;
using System.Collections.Generic;

public class BattleEffectsManager : MonoSingleton<BattleEffectsManager>
{
    private BattleEffectsManager()
    {
    }

    public Effects Effect_Main;
    public Effects Effect_RefreshBattleGroundOnAddMech;
    public Effects Effect_TipSlotBloom;

    public bool isExecuting_Main;
    public bool isExecuting_RefreshBattleGroundOnAddMech;
    public bool isExecuting_TipSlotBloom;

    public static float AnimationSpeed = 1.5f;

    void Start()
    {
        Effect_Main = new Effects("Effect_M");
        Effect_RefreshBattleGroundOnAddMech = new Effects("Effect_R");
        Effect_TipSlotBloom = new Effects("Effect_T");
    }

    void Update()
    {
        Effect_Main.Update();
        Effect_RefreshBattleGroundOnAddMech.Update();
        Effect_TipSlotBloom.Update();

        isExecuting_Main = Effect_Main.IsExcuting;
        isExecuting_RefreshBattleGroundOnAddMech = Effect_RefreshBattleGroundOnAddMech.IsExcuting;
        isExecuting_TipSlotBloom = Effect_TipSlotBloom.IsExcuting;
    }

    public void ResetAll()
    {
        Effect_Main.AllEffectsEnd();
        Effect_RefreshBattleGroundOnAddMech.AllEffectsEnd();
        Effect_TipSlotBloom.AllEffectsEnd();

        Effect_Main = new Effects("Effect_M");
        Effect_RefreshBattleGroundOnAddMech = new Effects("Effect_R");
        Effect_TipSlotBloom = new Effects("Effect_T");
        isExecuting_Main = false;
        isExecuting_RefreshBattleGroundOnAddMech = false;
        isExecuting_TipSlotBloom = false;
    }

    public class Effects
    {
        public bool IsExcuting = false;

        public Queue<Stack<SideEffect>> EffectsQueue = new Queue<Stack<SideEffect>>();
        private SideEffect CurrentEffect;
        public string Name;

        public Effects(string name)
        {
            Name = name;
        }

        public void Update()
        {
            if (!IsExcuting && EffectsQueue.Count != 0)
            {
                Stack<SideEffect> ses = EffectsQueue.Peek();
                while (ses.Count != 0)
                {
                    SideEffect se = ses.Pop();
                    Instance.StartCoroutine(se.Enumerator);
                    CurrentEffect = se;
                    IsExcuting = true;
                    if (BattleManager.Instance.ShowBEMMessages) ClientLog.Instance.PrintBattleEffectsStart("+ [" + Name + "] StartEffect: " + se.MethodName + " id: " + se.EffectId);
                }

                EffectsQueue.Dequeue();
            }
        }

        public SideEffect GetCurrentSideEffect()
        {
            return CurrentEffect;
        }

        private int effectId = 0;

        public int GenerateEffectId()
        {
            return effectId++;
        }

        public int EffectsShow(IEnumerator enumerator, string methodName)
        {
            int id = GenerateEffectId();
            SideEffect se = new SideEffect(enumerator, methodName, id);
            Stack<SideEffect> ses = new Stack<SideEffect>();
            ses.Push(se);
            EffectsQueue.Enqueue(ses);
            return id;
        }

        public void EffectEnd()
        {
            if (BattleManager.Instance.ShowBEMMessages) ClientLog.Instance.PrintWarning("end");

            if (CurrentEffect != null)
            {
                if (CurrentEffect.Enumerator != null)
                {
                    try
                    {
                        Instance.StopCoroutine(CurrentEffect.Enumerator);
                        if (BattleManager.Instance.ShowBEMMessages) ClientLog.Instance.PrintBattleEffectsEnd("- [" + Name + "] EndEffect: " + CurrentEffect.MethodName + " id: " + CurrentEffect.EffectId);
                    }
                    catch (Exception e)
                    {
                        if (BattleManager.Instance.ShowBEMMessages) ClientLog.Instance.PrintWarning(e.ToString());
                    }
                }
                else
                {
                    if (BattleManager.Instance.ShowBEMMessages) ClientLog.Instance.PrintWarning("CurrentEffect.Enumerator = null");
                }

                CurrentEffect = null;
            }
            else
            {
                if (BattleManager.Instance.ShowBEMMessages) ClientLog.Instance.PrintWarning("CurrentEffect = null");
            }

            IsExcuting = false;
        }

        public void AllEffectsEnd()
        {
            EffectEnd();
            EffectsQueue.Clear();
        }
    }

    public class SideEffect
    {
        public IEnumerator Enumerator;
        public string MethodName;
        public int EffectId;

        public SideEffect(IEnumerator enumerator, string methodName, int effectId)
        {
            Enumerator = enumerator;
            MethodName = methodName;
            EffectId = effectId;
        }
    }
}