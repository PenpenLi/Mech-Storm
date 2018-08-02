﻿using UnityEngine;

internal class GameManager : MonoBehaviour
{
    private static GameManager _gm;

    public static GameManager GM
    {
        get
        {
            if (!_gm) _gm = FindObjectOfType<GameManager>();
            return _gm;
        }
    }

    private GameManager()
    {
    }


    private void Awake()
    {
        initialize();
    }

    private void Start()
    {
        _rbe = Camera.main.GetComponent<ImageEffectBlurBox>();
    }

    private void Update()
    {
    }


    #region GamePlay

    #endregion

    #region 游戏全局参数

    public bool  UseInspectorParams = false;
    public bool  ShowBEMMessages = false;

    public float HandCardSize = 1.0f;
    public float HandCardInterval = 1.0f;
    public float HandCardRotate = 1.0f;
    public float HandCardOffset = 0.6f;
    public float PullOutCardSize = 3.0f;
    public float DetailCardSize = 3.0f;
    public float DetailCardModuleSize = 2.5f;
    public float DetailCardSizeRetinue = 4.0f;

    public float RetinueInterval = 3.5f;
    public float RetinueDetailPreviewDelaySeconds = 0.7f;

    internal Vector3 CardShowPosition;
    public float CardShowScale=2f;

    public float ShowCardDuration = 0.7f;
    public float ShowCardRotateDuration = 0.1f;
    public float ShowCardFlyTime = 0.2f;

    private void initialize()
    {
        if (UseInspectorParams) return;
        CardBloomColor = HTMLColorToColor("#FFFFFF");
        RetinueBloomColor = HTMLColorToColor("#06FF00");
        RetinueOnHoverBloomColor = HTMLColorToColor("#FF0000");
        Slot1Color = HTMLColorToColor("#FF0000");
        Slot2Color = HTMLColorToColor("#FFED00");
        Slot3Color = HTMLColorToColor("#00FF6B");
        Slot4Color = HTMLColorToColor("#2D37FF");

        InjuredLifeNumberColor = HTMLColorToColor("#FF0015");
        DefaultLifeNumberColor = HTMLColorToColor("#FFFFFF");
        OverFlowTotalLifeColor = HTMLColorToColor("#00FF28");

        CardShowPosition = new Vector3(10, 3, 0);
    }

    public Color CardBloomColor;
    public Color RetinueBloomColor;
    public Color RetinueOnHoverBloomColor;
    public Color Slot1Color;
    public Color Slot2Color;
    public Color Slot3Color;
    public Color Slot4Color;

    public Color DefaultLifeNumberColor;
    public Color InjuredLifeNumberColor;
    public Color OverFlowTotalLifeColor;

    public static Color HTMLColorToColor(string htmlColor)
    {
        Color cl = new Color();
        ColorUtility.TryParseHtmlString(htmlColor, out cl);
        return cl;
    }

    #endregion

    #region 其他

    public AudioSource MainAudioSource;
    private ImageEffectBlurBox _rbe;

    public void PlayAudioClip(AudioClip ac)
    {
        MainAudioSource.clip = ac;
        MainAudioSource.Play();
    }

    public void StartBlurBackGround()
    {
        _rbe.enabled = true;
    }


    public void StopBlurBackGround()
    {
        _rbe.enabled = false;
    }

    #endregion
}