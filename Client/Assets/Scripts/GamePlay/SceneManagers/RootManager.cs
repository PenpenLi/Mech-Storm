﻿using System.Collections;
using UnityEngine;

public class RootManager : MonoSingleton<RootManager>
{
    protected RootManager()
    {
    }

    public bool ShowClientLogs = false;

    void Awake()
    {
        Utils.DebugLog = ClientLog.Instance.PrintError;
        AllColors.AddAllColors(Application.streamingAssetsPath + "/Config/Colors.xml");
        AllSideEffects.AddAllSideEffects(Application.streamingAssetsPath + "/Config/SideEffects.xml");
        AllBuffs.AddAllBuffs(Application.streamingAssetsPath + "/Config/Buffs.xml");
        AllCards.AddAllCards(Application.streamingAssetsPath + "/Config/Cards.xml");
        AllCards.RefreshAllCardXML();
    }

    #region 其他

    [SerializeField] private ImageEffectBlurBox ImageEffectBlurBox;

    public void StartBlurBackGround()
    {
        if (ImageEffectBlurBox)
        {
            if (StartBlurBackGroundCoroutine != null) StopCoroutine(StartBlurBackGroundCoroutine);
            ImageEffectBlurBox.enabled = true;
            ImageEffectBlurBox.BlurSize = 0.5f;
        }
    }

    public void StopBlurBackGround()
    {
        if (ImageEffectBlurBox)
        {
            if (StartBlurBackGroundCoroutine != null) StopCoroutine(StartBlurBackGroundCoroutine);
            ImageEffectBlurBox.enabled = false;
            ImageEffectBlurBox.BlurSize = 0.5f;
        }
    }

    public void StartBlurBackGround(float duration)
    {
        if (ImageEffectBlurBox)
        {
            StartBlurBackGroundCoroutine = StartCoroutine(Co_StartBlurBackGroundShow(duration));
        }
    }

    private Coroutine StartBlurBackGroundCoroutine;

    IEnumerator Co_StartBlurBackGroundShow(float duration)
    {
        if (ImageEffectBlurBox)
        {
            if (StartBlurBackGroundCoroutine != null) StopCoroutine(StartBlurBackGroundCoroutine);
            ImageEffectBlurBox.enabled = true;
            float blurSizeStart = 0;
            float blurSizeEnd = 0.5f;
            int frame = Mathf.RoundToInt(duration / 0.05f);
            for (int i = 0; i < frame; i++)
            {
                float blurSize = blurSizeStart + (blurSizeEnd - blurSizeStart) / frame * i;
                ImageEffectBlurBox.BlurSize = blurSize;
                yield return new WaitForSeconds(duration / frame);
            }

            ImageEffectBlurBox.BlurSize = blurSizeEnd;
        }

        yield return null;
    }

    #endregion
}