using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-250)]
public class BullfightHudController : MonoBehaviour
{
    [SerializeField] private string hudCanvasName = "HUD_Canvas";
    [SerializeField] private string legacyHudCanvasName = "P_LPSP_UI_Canvas";
    [SerializeField] private string legacyHudCanvasCloneName = "P_LPSP_UI_Canvas(Clone)";
    [SerializeField] private string healthBarName = "HealthBar";
    [SerializeField] private string staminaBarName = "StaminaBar";
    [SerializeField] private string playerHealthLabelName = "PlayerHealthLabel";
    [SerializeField] private string playerStaminaLabelName = "PlayerStaminaLabel";
    [SerializeField] private string bullHealthBarName = "BullHealthBar";
    [SerializeField] private string weaponAmmoName = "Weapon & Ammo";
    [SerializeField] private string ammoName = "Ammo";
    [SerializeField] private string crosshairName = "Crosshair";
    [SerializeField] private string tutorialName = "Tutorial";
    [SerializeField] private string promptName = "Prompt";
    [SerializeField] private string textTimescaleName = "Text Timescale";
    [SerializeField] private string textTutorialPromptName = "Text Tutorial Prompt";
    [SerializeField] private string textTutorialTextName = "Text Tutorial Text";
    [SerializeField] private string textTutorialName = "Text Tutorial";
    [SerializeField] private string textAmmunitionCurrentName = "Text Ammunition Current";
    [SerializeField] private string textAmmunitionTotalName = "Text Ammunition Total";
    [SerializeField] private string textAmmunitionDividerName = "Text Ammunition Divider";
    [SerializeField] private string crosshairClassicName = "Crosshair Classic";
    [SerializeField] private string crosshairDotAdjusterName = "Crosshair Dot Adjuster";
    [SerializeField] private string bullBossRootName = "BullBossHudRoot";
    [SerializeField] private string bullBossTitleName = "BullBossTitle";
    [SerializeField] private string bullBossTopLineName = "BullBossTopLine";
    [SerializeField] private string bullBossLeftWingName = "BullBossLeftWing";
    [SerializeField] private string bullBossRightWingName = "BullBossRightWing";
    [SerializeField] private string bullBossLeftCapName = "BullBossLeftCap";
    [SerializeField] private string bullBossRightCapName = "BullBossRightCap";
    [SerializeField] private string bullBossSegmentLeftName = "BullBossSegmentLeft";
    [SerializeField] private string bullBossSegmentRightName = "BullBossSegmentRight";
    [SerializeField] private string phaseRootName = "BullfightPhaseRoot";
    [SerializeField] private string phaseLabelName = "BullfightPhaseLabel";
    [SerializeField] private string phaseAccentName = "BullfightPhaseAccent";
    [SerializeField] private string phaseTwoOverlayRootName = "BullfightPhaseTwoOverlayRoot";
    [SerializeField] private string phaseTwoTitleName = "BullfightPhaseTwoTitle";
    [SerializeField] private string phaseTwoSubtitleName = "BullfightPhaseTwoSubtitle";
    [SerializeField] private string phaseTwoStatusName = "BullfightPhaseTwoStatus";
    [SerializeField] private string phaseTwoRoundName = "BullfightPhaseTwoRound";
    [SerializeField] private string phaseTwoScoreName = "BullfightPhaseTwoScore";

    [SerializeField] private Vector2 healthBarOffset = new Vector2(24f, 56f);
    [SerializeField] private Vector2 staminaBarOffset = new Vector2(24f, 22f);
    [SerializeField] private Vector2 playerHealthLabelOffset = new Vector2(0f, 8f);
    [SerializeField] private Vector2 playerStaminaLabelOffset = new Vector2(0f, 0f);
    [SerializeField] private Vector2 bullBossRootSize = new Vector2(860f, 84f);
    [SerializeField] private Vector2 bullBossRootOffset = new Vector2(0f, -10f);
    [SerializeField] private Vector2 bullHealthBarSize = new Vector2(720f, 30f);
    [SerializeField] private Vector2 bullHealthBarOffset = new Vector2(0f, -12f);
    [SerializeField] private int bullBossTitleFontSize = 28;
    [SerializeField] private int playerBarLabelFontSize = 16;
    [SerializeField] private Vector2 phaseRootSize = new Vector2(220f, 52f);
    [SerializeField] private Vector2 phaseRootOffset = new Vector2(-28f, -24f);
    [SerializeField] private Vector2 phaseAccentSize = new Vector2(140f, 3f);
    [SerializeField] private Vector2 phaseAccentOffset = new Vector2(0f, -36f);
    [SerializeField] private int phaseFontSize = 22;
    [SerializeField] private Vector2 phaseTwoOverlaySize = new Vector2(1080f, 280f);
    [SerializeField] private Vector2 phaseTwoTitlePosition = new Vector2(0f, 36f);
    [SerializeField] private Vector2 phaseTwoSubtitlePosition = new Vector2(0f, -12f);
    [SerializeField] private Vector2 phaseTwoStatusPosition = new Vector2(0f, -64f);
    [SerializeField] private Vector2 phaseTwoRoundPosition = new Vector2(0f, -74f);
    [SerializeField] private Vector2 phaseTwoScorePosition = new Vector2(0f, -102f);
    [SerializeField] private int phaseTwoTitleFontSize = 58;
    [SerializeField] private int phaseTwoSubtitleFontSize = 30;
    [SerializeField] private int phaseTwoStatusFontSize = 24;
    [SerializeField] private int phaseTwoInfoFontSize = 20;
    [SerializeField] private Color bullBossGold = new Color(0.87f, 0.67f, 0.18f, 1f);
    [SerializeField] private Color bullBossTitleColor = new Color(0.97f, 0.93f, 0.82f, 1f);
    [SerializeField] private Color phaseTextColor = new Color(0.97f, 0.93f, 0.82f, 1f);
    [SerializeField] private Color playerHealthLabelColor = new Color(0.92f, 0.16f, 0.16f, 1f);
    [SerializeField] private Color playerStaminaLabelColor = new Color(0.2f, 0.52f, 0.96f, 1f);
    [SerializeField] private Color phaseTwoStatusColor = new Color(0.95f, 0.83f, 0.88f, 1f);
    [SerializeField] private Color bullPhaseTwoFillColor = new Color(0.56f, 0.2f, 0.92f, 1f);
    [SerializeField] private float playerHudScale = 2f;
    [SerializeField] private Vector2 playerBarBaseSize = new Vector2(160f, 20f);
    [SerializeField] private Vector2 playerBarLabelBaseSize = new Vector2(160f, 24f);
    private BullfightGameFlow gameFlow;
    private Slider playerHealthSlider;
    private Slider playerStaminaSlider;
    private Slider bullHealthSlider;
    private Color cachedBullFillColor;
    private bool hasCachedBullFillColor;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject host = new GameObject("BullfightHudController");
        host.AddComponent<BullfightHudController>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyHudLayout();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LateUpdate()
    {
        UpdatePhaseDisplay();
        UpdatePhaseTwoHud();
        DisableLegacyShooterUi();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyHudLayout();
    }

    private void ApplyHudLayout()
    {
        Canvas hudCanvas = FindCanvasByName(hudCanvasName);
        if (hudCanvas != null)
        {
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.worldCamera = null;
            hudCanvas.targetDisplay = 0;

            RectTransform canvasRect = hudCanvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.localScale = Vector3.one;
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.offsetMin = Vector2.zero;
                canvasRect.offsetMax = Vector2.zero;
            }

            CanvasScaler scaler = hudCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        playerHealthSlider = FindSliderByName(healthBarName);
        playerStaminaSlider = FindSliderByName(staminaBarName);
        bullHealthSlider = FindSliderByName(bullHealthBarName);

        PlaceSlider(playerHealthSlider, healthBarOffset * playerHudScale, playerBarBaseSize * playerHudScale);
        PlaceSlider(playerStaminaSlider, staminaBarOffset * playerHudScale, playerBarBaseSize * playerHudScale);
        RemoveStaminaSegments(playerStaminaSlider);
        PlacePlayerBarLabel(playerHealthSlider, playerHealthLabelName, "\u73a9\u5bb6hp", playerHealthLabelColor, playerHealthLabelOffset * playerHudScale);
        PlacePlayerBarLabel(playerStaminaSlider, playerStaminaLabelName, "\u73a9\u5bb6\u9ad4\u529b", playerStaminaLabelColor, playerStaminaLabelOffset * playerHudScale);
        PlaceBullHealthBar(bullHealthSlider, hudCanvas);
        UpdatePhaseDisplay();
        UpdatePhaseTwoHud();
        DisableLegacyShooterUi();
    }

    private static void PlaceSlider(Slider slider, Vector2 offset, Vector2 size)
    {
        if (slider == null)
            return;

        RectTransform rect = slider.GetComponent<RectTransform>();
        if (rect == null)
            return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = offset;
        rect.sizeDelta = size;
    }

    private void PlacePlayerBarLabel(Slider slider, string labelObjectName, string labelText, Color labelColor, Vector2 labelOffset)
    {
        if (slider == null)
            return;

        Text label = GetOrCreateUiText(slider.transform as RectTransform, labelObjectName);
        if (label == null)
            return;

        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 0f);
        labelRect.anchoredPosition = labelOffset;
        labelRect.sizeDelta = playerBarLabelBaseSize * playerHudScale;
        labelRect.localScale = Vector3.one;
        labelRect.localRotation = Quaternion.identity;

        label.alignment = TextAnchor.LowerLeft;
        label.fontSize = Mathf.RoundToInt(playerBarLabelFontSize * playerHudScale);
        label.fontStyle = FontStyle.Bold;
        label.color = labelColor;
        label.text = labelText;
    }

    private void PlaceBullHealthBar(Slider slider, Canvas hudCanvas)
    {
        if (slider == null || hudCanvas == null)
            return;

        RectTransform canvasRect = hudCanvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return;

        RectTransform bossRoot = GetOrCreateUiRect(canvasRect, bullBossRootName);
        bossRoot.anchorMin = new Vector2(0.5f, 1f);
        bossRoot.anchorMax = new Vector2(0.5f, 1f);
        bossRoot.pivot = new Vector2(0.5f, 1f);
        bossRoot.sizeDelta = bullBossRootSize;
        bossRoot.anchoredPosition = bullBossRootOffset;
        bossRoot.localScale = Vector3.one;
        bossRoot.localRotation = Quaternion.identity;

        RectTransform rect = slider.GetComponent<RectTransform>();
        if (rect == null)
            return;

        rect.SetParent(bossRoot, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = bullHealthBarSize;
        rect.anchoredPosition = bullHealthBarOffset;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        slider.direction = Slider.Direction.LeftToRight;

        Text title = GetOrCreateUiText(bossRoot, bullBossTitleName);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(220f, 28f);
        titleRect.anchoredPosition = new Vector2(0f, -46f);
        titleRect.localScale = Vector3.one;
        titleRect.localRotation = Quaternion.identity;
        title.alignment = TextAnchor.MiddleCenter;
        title.fontSize = bullBossTitleFontSize;
        title.fontStyle = FontStyle.Bold;
        title.color = bullBossTitleColor;
        title.text = "BULL HP";

        ConfigureDecorLine(GetOrCreateUiImage(bossRoot, bullBossTopLineName), new Vector2(0f, -2f), new Vector2(742f, 3f));
        ConfigureDecorLine(GetOrCreateUiImage(bossRoot, bullBossLeftWingName), new Vector2(-395f, -13f), new Vector2(70f, 6f));
        ConfigureDecorLine(GetOrCreateUiImage(bossRoot, bullBossRightWingName), new Vector2(395f, -13f), new Vector2(70f, 6f));
        ConfigureDecorLine(GetOrCreateUiImage(bossRoot, bullBossLeftCapName), new Vector2(-364f, -13f), new Vector2(6f, 18f));
        ConfigureDecorLine(GetOrCreateUiImage(bossRoot, bullBossRightCapName), new Vector2(364f, -13f), new Vector2(6f, 18f));
        ConfigureBullSegmentLine(GetOrCreateUiImage(bossRoot, bullBossSegmentLeftName), -1f);
        ConfigureBullSegmentLine(GetOrCreateUiImage(bossRoot, bullBossSegmentRightName), 1f);
    }

    private void ConfigureDecorLine(Image image, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        image.color = bullBossGold;
        image.raycastTarget = false;
    }

    private static void RemoveStaminaSegments(Slider slider)
    {
        if (slider == null)
            return;

        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        if (sliderRect == null)
            return;

        for (int childIndex = sliderRect.childCount - 1; childIndex >= 0; childIndex--)
        {
            Transform child = sliderRect.GetChild(childIndex);
            if (child != null && child.name.StartsWith("StaminaSegmentDivider_"))
                Destroy(child.gameObject);
        }
    }

    private void ConfigureBullSegmentLine(Image image, float direction)
    {
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2((bullHealthBarSize.x / 6f) * direction, bullHealthBarOffset.y - (bullHealthBarSize.y * 0.5f));
        rect.sizeDelta = new Vector2(4f, bullHealthBarSize.y + 10f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        image.color = bullBossTitleColor;
        image.raycastTarget = false;
        image.gameObject.SetActive(false);
    }

    private void UpdatePhaseDisplay()
    {
        Canvas hudCanvas = FindCanvasByName(hudCanvasName);
        if (hudCanvas == null)
            return;

        RectTransform canvasRect = hudCanvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return;

        if (gameFlow == null)
            gameFlow = FindObjectOfType<BullfightGameFlow>(true);

        if (gameFlow == null)
            return;

        RectTransform phaseRoot = GetOrCreateUiRect(canvasRect, phaseRootName);
        phaseRoot.anchorMin = new Vector2(1f, 1f);
        phaseRoot.anchorMax = new Vector2(1f, 1f);
        phaseRoot.pivot = new Vector2(1f, 1f);
        phaseRoot.sizeDelta = phaseRootSize;
        phaseRoot.anchoredPosition = phaseRootOffset;
        phaseRoot.localScale = Vector3.one;
        phaseRoot.localRotation = Quaternion.identity;

        Text phaseLabel = GetOrCreateUiText(phaseRoot, phaseLabelName);
        RectTransform phaseLabelRect = phaseLabel.rectTransform;
        phaseLabelRect.anchorMin = new Vector2(1f, 1f);
        phaseLabelRect.anchorMax = new Vector2(1f, 1f);
        phaseLabelRect.pivot = new Vector2(1f, 1f);
        phaseLabelRect.sizeDelta = phaseRootSize;
        phaseLabelRect.anchoredPosition = Vector2.zero;
        phaseLabelRect.localScale = Vector3.one;
        phaseLabelRect.localRotation = Quaternion.identity;
        phaseLabel.alignment = TextAnchor.UpperRight;
        phaseLabel.fontSize = phaseFontSize;
        phaseLabel.fontStyle = FontStyle.Bold;
        phaseLabel.color = phaseTextColor;
        phaseLabel.text = GetPhaseLabel(gameFlow.currentPhase);

        Image accent = GetOrCreateUiImage(phaseRoot, phaseAccentName);
        RectTransform accentRect = accent.rectTransform;
        accentRect.anchorMin = new Vector2(1f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(1f, 1f);
        accentRect.sizeDelta = phaseAccentSize;
        accentRect.anchoredPosition = phaseAccentOffset;
        accentRect.localScale = Vector3.one;
        accentRect.localRotation = Quaternion.identity;
        accent.color = bullBossGold;
        accent.raycastTarget = false;
    }

    private void UpdatePhaseTwoHud()
    {
        Canvas hudCanvas = FindCanvasByName(hudCanvasName);
        if (hudCanvas == null)
            return;

        RectTransform canvasRect = hudCanvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return;

        if (gameFlow == null)
            gameFlow = FindObjectOfType<BullfightGameFlow>(true);

        UpdatePhaseTwoOverlay(canvasRect);
        UpdateBossRoundInfo();
        UpdatePhaseTwoBars();
        UpdateBullHealthStyling();
    }

    private void UpdatePhaseTwoOverlay(RectTransform canvasRect)
    {
        RectTransform overlayRoot = GetOrCreateUiRect(canvasRect, phaseTwoOverlayRootName);
        bool shouldShow = gameFlow != null && gameFlow.ShouldShowPhaseTwoOverlay();
        overlayRoot.gameObject.SetActive(shouldShow);
        if (!shouldShow)
            return;

        overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
        overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
        overlayRoot.pivot = new Vector2(0.5f, 0.5f);
        overlayRoot.sizeDelta = phaseTwoOverlaySize;
        overlayRoot.anchoredPosition = new Vector2(0f, 150f);
        overlayRoot.localScale = Vector3.one;
        overlayRoot.localRotation = Quaternion.identity;

        GetPhaseTwoOverlayContent(out string titleText, out string subtitleText, out string statusText);
        ConfigureCenteredText(GetOrCreateUiText(overlayRoot, phaseTwoTitleName), phaseTwoTitlePosition, phaseTwoTitleFontSize, bullBossTitleColor, FontStyle.Bold, titleText);
        ConfigureCenteredText(GetOrCreateUiText(overlayRoot, phaseTwoSubtitleName), phaseTwoSubtitlePosition, phaseTwoSubtitleFontSize, bullBossTitleColor, FontStyle.Normal, subtitleText);
        ConfigureCenteredText(GetOrCreateUiText(overlayRoot, phaseTwoStatusName), phaseTwoStatusPosition, phaseTwoStatusFontSize, phaseTwoStatusColor, FontStyle.Italic, statusText);
    }

    private void UpdateBossRoundInfo()
    {
        RectTransform bossRoot = FindUiRectByName(bullBossRootName);
        if (bossRoot == null)
            return;

        Text roundText = GetOrCreateUiText(bossRoot, phaseTwoRoundName);
        Text scoreText = GetOrCreateUiText(bossRoot, phaseTwoScoreName);

        bool shouldShow = gameFlow != null &&
                          gameFlow.ShouldShowPhaseTwoOverlay() &&
                          gameFlow.IsPhaseTwoCalibrated &&
                          gameFlow.CurrentPhaseTwoState != BullfightGameFlow.PhaseTwoState.Intro &&
                          gameFlow.CurrentPhaseTwoState != BullfightGameFlow.PhaseTwoState.Calibration &&
                          gameFlow.CurrentPhaseTwoState != BullfightGameFlow.PhaseTwoState.Standoff;

        roundText.gameObject.SetActive(shouldShow);
        scoreText.gameObject.SetActive(shouldShow);
        if (!shouldShow)
            return;

        /*

        ConfigureBossInfoText(roundText, phaseTwoRoundPosition, $"第 {gameFlow.PhaseTwoRoundIndex} / {gameFlow.PhaseTwoMaxRounds} 回合");
        ConfigureBossInfoText(scoreText, phaseTwoScorePosition, $"你 {gameFlow.PhaseTwoPlayerHitCount} : {gameFlow.PhaseTwoBullHitCount} 牛");
    }

        */
        ConfigureBossInfoText(roundText, phaseTwoRoundPosition, $"\u7b2c {gameFlow.PhaseTwoRoundIndex} / {gameFlow.PhaseTwoMaxRounds} \u56de\u5408");
        ConfigureBossInfoText(scoreText, phaseTwoScorePosition, $"\u4f60 {gameFlow.PhaseTwoPlayerHitCount} : {gameFlow.PhaseTwoBullHitCount} \u725b");
    }

    private void UpdatePhaseTwoBars()
    {
        if (gameFlow == null || !gameFlow.ShouldShowPhaseTwoOverlay())
            return;

        if (playerHealthSlider != null)
            playerHealthSlider.value = gameFlow.GetPhaseTwoPlayerHealthNormalized();

        if (bullHealthSlider != null)
            bullHealthSlider.value = gameFlow.GetPhaseTwoBullHealthNormalized();
    }

    private void UpdateBullHealthStyling()
    {
        if (bullHealthSlider == null)
            return;

        Image fillImage = bullHealthSlider.fillRect != null ? bullHealthSlider.fillRect.GetComponent<Image>() : null;
        if (fillImage != null && !hasCachedBullFillColor)
        {
            cachedBullFillColor = fillImage.color;
            hasCachedBullFillColor = true;
        }

        bool phaseTwoActive = gameFlow != null && gameFlow.ShouldShowPhaseTwoOverlay();
        if (fillImage != null && hasCachedBullFillColor)
            fillImage.color = phaseTwoActive ? bullPhaseTwoFillColor : cachedBullFillColor;

        SetBossSegmentVisible(bullBossSegmentLeftName, phaseTwoActive);
        SetBossSegmentVisible(bullBossSegmentRightName, phaseTwoActive);
    }

    private void GetPhaseTwoOverlayContent(out string titleText, out string subtitleText, out string statusText)
    {
        titleText = string.Empty;
        subtitleText = string.Empty;
        statusText = string.Empty;

        if (gameFlow == null)
            return;

        switch (gameFlow.CurrentPhaseTwoState)
        {
            case BullfightGameFlow.PhaseTwoState.Intro:
                titleText = "\u968e\u6bb5\u4e8c";
                subtitleText = gameFlow.IsPhaseTwoQuestionVisible ? "\u771f\u7684\u53ea\u6709\u6bba\u4e86\u4ed6\u9019\u500b\u8fa6\u6cd5\u55ce?" : string.Empty;
                break;
            case BullfightGameFlow.PhaseTwoState.Calibration:
                titleText = "\u6821\u6e96";
                subtitleText = "\u6309\u4f4f G \u6821\u6e96";
                statusText = $"{Mathf.RoundToInt(gameFlow.PhaseTwoCalibrationProgress * 100f)}%";
                break;
            case BullfightGameFlow.PhaseTwoState.Standoff:
                titleText = "\u5c0d\u5cd9";
                subtitleText = "\u4fdd\u6301\u6c89\u9ed8 15 \u79d2\uff0c\u6216\u6309 E \u6253\u7834\u5c0d\u5cd9";
                statusText = $"{Mathf.CeilToInt(gameFlow.PhaseTwoMercyTimeRemaining)}s";
                break;
            case BullfightGameFlow.PhaseTwoState.RoundPrepare:
            case BullfightGameFlow.PhaseTwoState.RoundWindow:
                titleText = gameFlow.CurrentRoundHasPerfectAdvantage ? "\u7834\u7dbb" : "\u5c0d\u6c7a";
                if (gameFlow.CurrentPhaseTwoState == BullfightGameFlow.PhaseTwoState.RoundPrepare)
                {
                    if (gameFlow.IsPhaseTwoRoundStanceConfirming)
                    {
                        titleText = "\u5b9a\u52e2";
                        subtitleText = "\u6bcf\u4e00\u64ca\u4e4b\u524d\uff0c\u5148\u7a69\u4f4f\u81ea\u5df1";
                        statusText = $"{Mathf.RoundToInt(gameFlow.PhaseTwoRoundStanceProgress * 100f)}%";
                    }
                    else
                    {
                        subtitleText = string.IsNullOrWhiteSpace(gameFlow.CurrentPhaseTwoReflectionLine)
                            ? "\u7b49\u5f85\u725b\u9732\u51fa\u7834\u7dbb\uff0c\u7136\u5f8c\u6309 E \u523a\u64ca"
                            : gameFlow.CurrentPhaseTwoReflectionLine;
                        statusText = gameFlow.CurrentRoundHasPerfectAdvantage
                            ? "\u4e0a\u4e00\u64ca\u7559\u4e0b\u7684\u7834\u7dbb\u9084\u5728\u64f4\u5927..."
                            : "\u725b\u6b63\u5728\u84c4\u529b...";
                    }
                }
                else
                {
                    subtitleText = "\u73fe\u5728\u6309 E \u523a\u64ca";
                    statusText = "\u73fe\u5728\u51fa\u624b";
                }
                break;
            case BullfightGameFlow.PhaseTwoState.RoundResolve:
                titleText = gameFlow.LastPhaseTwoResult switch
                {
                    "Perfect!" => "PERFECT",
                    "Good" => "GOOD",
                    "Miss" => "MISS",
                    _ => string.Empty
                };
                subtitleText = gameFlow.LastPhaseTwoResult switch
                {
                    "Perfect!" => "\u4e0b\u4e00\u56de\u5408\u7834\u7dbb\u66f4\u5927",
                    "Good" => "\u4f60\u6210\u529f\u963b\u6b62\u4e86\u725b\u7684\u653b\u64ca",
                    "Miss" => "\u4f60\u6c92\u80fd\u963b\u6b62\u725b\u7684\u653b\u64ca",
                    _ => string.Empty
                };
                break;
        }
    }

    private void ConfigureBossInfoText(Text text, Vector2 anchoredPosition, string value)
    {
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(560f, 28f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = phaseTwoInfoFontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = bullBossTitleColor;
        text.text = value;
    }

    private void ConfigureCenteredText(Text text, Vector2 anchoredPosition, int fontSize, Color color, FontStyle style, string value)
    {
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(980f, 48f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.text = value;
        text.gameObject.SetActive(!string.IsNullOrEmpty(value));
    }

    private void SetBossSegmentVisible(string objectName, bool visible)
    {
        RectTransform rect = FindUiRectByName(objectName);
        if (rect != null)
            rect.gameObject.SetActive(visible);
    }

    private static string GetPhaseLabel(BullfightGameFlow.GamePhase phase)
    {
        return phase switch
        {
            BullfightGameFlow.GamePhase.PhaseOne => "\u968e\u6bb5\u4e00",
            BullfightGameFlow.GamePhase.PhaseTwo => "\u968e\u6bb5\u4e8c",
            BullfightGameFlow.GamePhase.Ending => "\u7d50\u5c40\u6f14\u51fa",
            _ => "\u672a\u77e5\u968e\u6bb5"
        };
    }

    private static RectTransform GetOrCreateUiRect(RectTransform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null)
            return existing as RectTransform;

        GameObject go = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static RectTransform FindUiRectByName(string objectName)
    {
        foreach (RectTransform rect in Resources.FindObjectsOfTypeAll<RectTransform>())
        {
            if (rect == null || rect.name != objectName)
                continue;

            if (!rect.gameObject.scene.IsValid())
                continue;

            return rect;
        }

        return null;
    }

    private static Image GetOrCreateUiImage(RectTransform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null)
            return existing.GetComponent<Image>();

        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return go.GetComponent<Image>();
    }

    private static Text GetOrCreateUiText(RectTransform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null)
            return existing.GetComponent<Text>();

        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private void DisableLegacyShooterUi()
    {
        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform == null)
                continue;

            if (!transform.gameObject.scene.IsValid())
                continue;

            if (!IsLegacyShooterUiObject(transform.name) && !IsChildOfLegacyShooterCanvas(transform))
                continue;

            transform.gameObject.SetActive(false);
        }
    }

    private bool IsLegacyShooterUiObject(string objectName)
    {
        return objectName == legacyHudCanvasName ||
               objectName == legacyHudCanvasCloneName ||
               objectName == weaponAmmoName ||
               objectName == ammoName ||
               objectName == crosshairName ||
               objectName == tutorialName ||
               objectName == promptName ||
               objectName == textTimescaleName ||
               objectName == textTutorialPromptName ||
               objectName == textTutorialTextName ||
               objectName == textTutorialName ||
               objectName == textAmmunitionCurrentName ||
               objectName == textAmmunitionTotalName ||
               objectName == textAmmunitionDividerName ||
               objectName == crosshairClassicName ||
               objectName == crosshairDotAdjusterName;
    }

    private bool IsChildOfLegacyShooterCanvas(Transform candidate)
    {
        Transform current = candidate;
        while (current != null)
        {
            if (current.name == legacyHudCanvasName || current.name == legacyHudCanvasCloneName)
                return true;

            current = current.parent;
        }

        return false;
    }

    private static Canvas FindCanvasByName(string canvasName)
    {
        foreach (Canvas canvas in Resources.FindObjectsOfTypeAll<Canvas>())
        {
            if (canvas == null || canvas.name != canvasName)
                continue;

            if (!canvas.gameObject.scene.IsValid())
                continue;

            return canvas;
        }

        return null;
    }

    private static Slider FindSliderByName(string sliderName)
    {
        foreach (Slider slider in Resources.FindObjectsOfTypeAll<Slider>())
        {
            if (slider == null || slider.name != sliderName)
                continue;

            if (!slider.gameObject.scene.IsValid())
                continue;

            return slider;
        }

        return null;
    }
}
