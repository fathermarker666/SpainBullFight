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
    [SerializeField] private string tutorialOverlayRootName = "BullfightTutorialOverlayRoot";
    [SerializeField] private string tutorialTitleName = "BullfightTutorialTitle";
    [SerializeField] private string tutorialInstructionName = "BullfightTutorialInstruction";
    [SerializeField] private string tutorialStatusName = "BullfightTutorialStatus";

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
    [SerializeField] private Vector2 tutorialOverlaySize = new Vector2(1040f, 220f);
    [SerializeField] private Vector2 tutorialTitlePosition = new Vector2(0f, 34f);
    [SerializeField] private Vector2 tutorialInstructionPosition = new Vector2(0f, -10f);
    [SerializeField] private Vector2 tutorialStatusPosition = new Vector2(0f, -58f);
    [SerializeField] private int phaseTwoTitleFontSize = 58;
    [SerializeField] private int phaseTwoSubtitleFontSize = 30;
    [SerializeField] private int phaseTwoStatusFontSize = 24;
    [SerializeField] private int phaseTwoInfoFontSize = 20;
    [SerializeField] private int tutorialTitleFontSize = 50;
    [SerializeField] private int tutorialInstructionFontSize = 28;
    [SerializeField] private int tutorialStatusFontSize = 22;
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
    private Canvas hudCanvas;
    private RectTransform hudCanvasRect;
    private Slider playerHealthSlider;
    private Slider playerStaminaSlider;
    private Slider bullHealthSlider;
    private RectTransform bullBossRoot;
    private Image bullBossSegmentLeft;
    private Image bullBossSegmentRight;
    private RectTransform phaseRoot;
    private Text phaseLabel;
    private Image phaseAccent;
    private RectTransform phaseTwoOverlayRoot;
    private Text phaseTwoTitle;
    private Text phaseTwoSubtitle;
    private Text phaseTwoStatus;
    private Text phaseTwoRound;
    private Text phaseTwoScore;
    private RectTransform tutorialOverlayRoot;
    private Text tutorialTitle;
    private Text tutorialInstruction;
    private Text tutorialStatus;
    private Color cachedBullFillColor;
    private bool hasCachedBullFillColor;
    private bool layoutDirty = true;
    private bool legacyUiDisabled;
    private bool staminaBarVisible = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<BullfightHudController>(true) != null)
            return;

        GameObject host = new GameObject("BullfightHudController");
        host.AddComponent<BullfightHudController>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        MarkLayoutDirty();
        RebuildHudLayout();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LateUpdate()
    {
        if (layoutDirty || !HasValidHudLayoutReferences())
            RebuildHudLayout();

        RefreshHudState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MarkLayoutDirty();
        RebuildHudLayout();
    }

    private void MarkLayoutDirty()
    {
        hudCanvas = null;
        hudCanvasRect = null;
        playerHealthSlider = null;
        playerStaminaSlider = null;
        bullHealthSlider = null;
        bullBossRoot = null;
        bullBossSegmentLeft = null;
        bullBossSegmentRight = null;
        phaseRoot = null;
        phaseLabel = null;
        phaseAccent = null;
        phaseTwoOverlayRoot = null;
        phaseTwoTitle = null;
        phaseTwoSubtitle = null;
        phaseTwoStatus = null;
        phaseTwoRound = null;
        phaseTwoScore = null;
        tutorialOverlayRoot = null;
        tutorialTitle = null;
        tutorialInstruction = null;
        tutorialStatus = null;
        hasCachedBullFillColor = false;
        legacyUiDisabled = false;
        layoutDirty = true;
    }

    private bool HasValidHudLayoutReferences()
    {
        return hudCanvas != null &&
               hudCanvasRect != null &&
               playerHealthSlider != null &&
               playerStaminaSlider != null &&
               bullHealthSlider != null;
    }

    private void EnsureGameFlowReference()
    {
        if (gameFlow == null)
            gameFlow = BullfightSceneCache.FindObject<BullfightGameFlow>();
    }

    private void RebuildHudLayout()
    {
        EnsureGameFlowReference();

        hudCanvas = BullfightSceneCache.FindSceneObjectByName<Canvas>(hudCanvasName);
        hudCanvasRect = hudCanvas != null ? hudCanvas.GetComponent<RectTransform>() : null;
        if (hudCanvas == null || hudCanvasRect == null)
            return;

        playerHealthSlider = BullfightSceneCache.FindSceneObjectByName<Slider>(healthBarName);
        playerStaminaSlider = BullfightSceneCache.FindSceneObjectByName<Slider>(staminaBarName);
        bullHealthSlider = BullfightSceneCache.FindSceneObjectByName<Slider>(bullHealthBarName);
        if (playerHealthSlider == null || playerStaminaSlider == null || bullHealthSlider == null)
            return;

        ConfigureCanvas(hudCanvas, hudCanvasRect);

        PlaceSlider(playerHealthSlider, healthBarOffset * playerHudScale, playerBarBaseSize * playerHudScale);
        PlaceSlider(playerStaminaSlider, staminaBarOffset * playerHudScale, playerBarBaseSize * playerHudScale);
        RemoveStaminaSegments(playerStaminaSlider);
        PlacePlayerBarLabel(playerHealthSlider, playerHealthLabelName, "\u73a9\u5bb6hp", playerHealthLabelColor, playerHealthLabelOffset * playerHudScale);
        PlacePlayerBarLabel(playerStaminaSlider, playerStaminaLabelName, "\u73a9\u5bb6\u9ad4\u529b", playerStaminaLabelColor, playerStaminaLabelOffset * playerHudScale);
        PlaceBullHealthBar(bullHealthSlider);
        EnsurePhaseDisplayUi();
        EnsurePhaseTwoOverlayUi();
        EnsureTutorialOverlayUi();
        DisableLegacyShooterUiOnce();
        layoutDirty = false;
        RefreshHudState();
    }

    private static void ConfigureCanvas(Canvas canvas, RectTransform canvasRect)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        canvas.targetDisplay = 0;

        canvasRect.localScale = Vector3.one;
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    public void SetStaminaBarVisible(bool visible)
    {
        staminaBarVisible = visible;
        ApplyStaminaBarVisibility();
    }

    private void ApplyStaminaBarVisibility()
    {
        if (playerStaminaSlider != null)
            playerStaminaSlider.gameObject.SetActive(staminaBarVisible);
    }

    private void RefreshHudState()
    {
        EnsureGameFlowReference();
        UpdatePhaseDisplay();
        UpdatePhaseTwoHud();
        UpdateTutorialHud();
        ApplyStaminaBarVisibility();
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

    private void PlaceBullHealthBar(Slider slider)
    {
        if (slider == null || hudCanvasRect == null)
            return;

        bullBossRoot = GetOrCreateUiRect(hudCanvasRect, bullBossRootName);
        bullBossRoot.anchorMin = new Vector2(0.5f, 1f);
        bullBossRoot.anchorMax = new Vector2(0.5f, 1f);
        bullBossRoot.pivot = new Vector2(0.5f, 1f);
        bullBossRoot.sizeDelta = bullBossRootSize;
        bullBossRoot.anchoredPosition = bullBossRootOffset;
        bullBossRoot.localScale = Vector3.one;
        bullBossRoot.localRotation = Quaternion.identity;

        RectTransform rect = slider.GetComponent<RectTransform>();
        if (rect == null)
            return;

        rect.SetParent(bullBossRoot, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = bullHealthBarSize;
        rect.anchoredPosition = bullHealthBarOffset;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        slider.direction = Slider.Direction.LeftToRight;

        Text title = GetOrCreateUiText(bullBossRoot, bullBossTitleName);
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

        ConfigureDecorLine(GetOrCreateUiImage(bullBossRoot, bullBossTopLineName), new Vector2(0f, -2f), new Vector2(742f, 3f));
        ConfigureDecorLine(GetOrCreateUiImage(bullBossRoot, bullBossLeftWingName), new Vector2(-395f, -13f), new Vector2(70f, 6f));
        ConfigureDecorLine(GetOrCreateUiImage(bullBossRoot, bullBossRightWingName), new Vector2(395f, -13f), new Vector2(70f, 6f));
        ConfigureDecorLine(GetOrCreateUiImage(bullBossRoot, bullBossLeftCapName), new Vector2(-364f, -13f), new Vector2(6f, 18f));
        ConfigureDecorLine(GetOrCreateUiImage(bullBossRoot, bullBossRightCapName), new Vector2(364f, -13f), new Vector2(6f, 18f));
        bullBossSegmentLeft = GetOrCreateUiImage(bullBossRoot, bullBossSegmentLeftName);
        bullBossSegmentRight = GetOrCreateUiImage(bullBossRoot, bullBossSegmentRightName);
        ConfigureBullSegmentLine(bullBossSegmentLeft, -1f);
        ConfigureBullSegmentLine(bullBossSegmentRight, 1f);
    }

    private void EnsurePhaseDisplayUi()
    {
        if (hudCanvasRect == null)
            return;

        phaseRoot = GetOrCreateUiRect(hudCanvasRect, phaseRootName);
        phaseRoot.anchorMin = new Vector2(1f, 1f);
        phaseRoot.anchorMax = new Vector2(1f, 1f);
        phaseRoot.pivot = new Vector2(1f, 1f);
        phaseRoot.sizeDelta = phaseRootSize;
        phaseRoot.anchoredPosition = phaseRootOffset;
        phaseRoot.localScale = Vector3.one;
        phaseRoot.localRotation = Quaternion.identity;

        phaseLabel = GetOrCreateUiText(phaseRoot, phaseLabelName);
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

        phaseAccent = GetOrCreateUiImage(phaseRoot, phaseAccentName);
        RectTransform accentRect = phaseAccent.rectTransform;
        accentRect.anchorMin = new Vector2(1f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(1f, 1f);
        accentRect.sizeDelta = phaseAccentSize;
        accentRect.anchoredPosition = phaseAccentOffset;
        accentRect.localScale = Vector3.one;
        accentRect.localRotation = Quaternion.identity;
        phaseAccent.color = bullBossGold;
        phaseAccent.raycastTarget = false;
    }

    private void EnsurePhaseTwoOverlayUi()
    {
        if (hudCanvasRect == null)
            return;

        phaseTwoOverlayRoot = GetOrCreateUiRect(hudCanvasRect, phaseTwoOverlayRootName);
        phaseTwoOverlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
        phaseTwoOverlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
        phaseTwoOverlayRoot.pivot = new Vector2(0.5f, 0.5f);
        phaseTwoOverlayRoot.sizeDelta = phaseTwoOverlaySize;
        phaseTwoOverlayRoot.anchoredPosition = new Vector2(0f, 150f);
        phaseTwoOverlayRoot.localScale = Vector3.one;
        phaseTwoOverlayRoot.localRotation = Quaternion.identity;

        phaseTwoTitle = GetOrCreateUiText(phaseTwoOverlayRoot, phaseTwoTitleName);
        phaseTwoSubtitle = GetOrCreateUiText(phaseTwoOverlayRoot, phaseTwoSubtitleName);
        phaseTwoStatus = GetOrCreateUiText(phaseTwoOverlayRoot, phaseTwoStatusName);

        if (bullBossRoot != null)
        {
            phaseTwoRound = GetOrCreateUiText(bullBossRoot, phaseTwoRoundName);
            phaseTwoScore = GetOrCreateUiText(bullBossRoot, phaseTwoScoreName);
        }
    }

    private void EnsureTutorialOverlayUi()
    {
        if (hudCanvasRect == null)
            return;

        tutorialOverlayRoot = GetOrCreateUiRect(hudCanvasRect, tutorialOverlayRootName);
        tutorialOverlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
        tutorialOverlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
        tutorialOverlayRoot.pivot = new Vector2(0.5f, 0.5f);
        tutorialOverlayRoot.sizeDelta = tutorialOverlaySize;
        tutorialOverlayRoot.anchoredPosition = new Vector2(0f, 150f);
        tutorialOverlayRoot.localScale = Vector3.one;
        tutorialOverlayRoot.localRotation = Quaternion.identity;

        tutorialTitle = GetOrCreateUiText(tutorialOverlayRoot, tutorialTitleName);
        tutorialInstruction = GetOrCreateUiText(tutorialOverlayRoot, tutorialInstructionName);
        tutorialStatus = GetOrCreateUiText(tutorialOverlayRoot, tutorialStatusName);
    }

    private void ConfigureDecorLine(Image image, Vector2 anchoredPosition, Vector2 size)
    {
        if (image == null)
            return;

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
        if (image == null)
            return;

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
        if (phaseLabel == null || gameFlow == null)
            return;

        phaseLabel.text = GetPhaseLabel(gameFlow.currentPhase);
    }

    private void UpdatePhaseTwoHud()
    {
        UpdatePhaseTwoOverlay();
        UpdateBossRoundInfo();
        UpdatePhaseTwoBars();
        UpdateBullHealthStyling();
    }

    private void UpdateTutorialHud()
    {
        if (tutorialOverlayRoot == null)
            return;

        bool shouldShow = gameFlow != null && gameFlow.ShouldShowTutorialOverlay();
        tutorialOverlayRoot.gameObject.SetActive(shouldShow);
        if (!shouldShow)
            return;

        ConfigureCenteredText(tutorialTitle, tutorialTitlePosition, tutorialTitleFontSize, bullBossTitleColor, FontStyle.Bold, gameFlow.CurrentTutorialTitle);
        ConfigureCenteredText(tutorialInstruction, tutorialInstructionPosition, tutorialInstructionFontSize, bullBossTitleColor, FontStyle.Normal, gameFlow.CurrentTutorialInstruction);
        ConfigureCenteredText(tutorialStatus, tutorialStatusPosition, tutorialStatusFontSize, phaseTwoStatusColor, FontStyle.Italic, gameFlow.CurrentTutorialStatus);
        SyncHudTutorialPromptTextsFromGameFlow(gameFlow.CurrentTutorialInstruction);
    }

    private void SyncHudTutorialPromptTextsFromGameFlow(string instructionText)
    {
        if (hudCanvasRect == null || string.IsNullOrEmpty(instructionText))
            return;

        TryAssignTextUnderHierarchy(hudCanvasRect, textTutorialPromptName, instructionText);
        TryAssignTextUnderHierarchy(hudCanvasRect, textTutorialTextName, instructionText);
        TryAssignTextUnderHierarchy(hudCanvasRect, textTutorialName, instructionText);
    }

    private static void TryAssignTextUnderHierarchy(Transform root, string objectName, string text)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
            return;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == objectName)
            {
                Text label = child.GetComponent<Text>();
                if (label != null && label.gameObject.activeInHierarchy)
                    label.text = text;
            }

            TryAssignTextUnderHierarchy(child, objectName, text);
        }
    }

    private void UpdatePhaseTwoOverlay()
    {
        if (phaseTwoOverlayRoot == null)
            return;

        bool shouldShow = gameFlow != null && gameFlow.ShouldShowPhaseTwoOverlay();
        phaseTwoOverlayRoot.gameObject.SetActive(shouldShow);
        if (!shouldShow)
            return;

        GetPhaseTwoOverlayContent(out string titleText, out string subtitleText, out string statusText);
        ConfigureCenteredText(phaseTwoTitle, phaseTwoTitlePosition, phaseTwoTitleFontSize, bullBossTitleColor, FontStyle.Bold, titleText);
        ConfigureCenteredText(phaseTwoSubtitle, phaseTwoSubtitlePosition, phaseTwoSubtitleFontSize, bullBossTitleColor, FontStyle.Normal, subtitleText);
        ConfigureCenteredText(phaseTwoStatus, phaseTwoStatusPosition, phaseTwoStatusFontSize, phaseTwoStatusColor, FontStyle.Italic, statusText);
    }

    private void UpdateBossRoundInfo()
    {
        if (phaseTwoRound == null || phaseTwoScore == null)
            return;

        bool shouldShow = gameFlow != null &&
                          gameFlow.ShouldShowPhaseTwoOverlay() &&
                          gameFlow.IsPhaseTwoCalibrated &&
                          gameFlow.CurrentPhaseTwoState != BullfightGameFlow.PhaseTwoState.Intro &&
                          gameFlow.CurrentPhaseTwoState != BullfightGameFlow.PhaseTwoState.Calibration &&
                          gameFlow.CurrentPhaseTwoState != BullfightGameFlow.PhaseTwoState.Standoff;

        phaseTwoRound.gameObject.SetActive(shouldShow);
        phaseTwoScore.gameObject.SetActive(shouldShow);
        if (!shouldShow)
            return;

        ConfigureBossInfoText(phaseTwoRound, phaseTwoRoundPosition, $"\u7b2c {gameFlow.PhaseTwoRoundIndex} / {gameFlow.PhaseTwoMaxRounds} \u56de\u5408");
        string scoreLine = $"\u4f60 {gameFlow.PhaseTwoPlayerHitCount} : {gameFlow.PhaseTwoBullHitCount} \u725b";
        if (gameFlow.playerStats != null && gameFlow.playerStats.isPhaseTwoActive)
        {
            PlayerStats ps = gameFlow.playerStats;
            int filled = Mathf.Max(0, ps.phaseTwoPlayerHealth);
            int empty = Mathf.Max(0, ps.phaseTwoMaxLives - filled);
            string hearts = new string('\u2665', filled) + new string('\u00b7', empty);
            scoreLine += $"\nLIFE: {hearts}";
        }

        ConfigureBossInfoText(phaseTwoScore, phaseTwoScorePosition, scoreLine);
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

        SetBossSegmentVisible(bullBossSegmentLeft, phaseTwoActive);
        SetBossSegmentVisible(bullBossSegmentRight, phaseTwoActive);
    }

    private void GetPhaseTwoOverlayContent(out string titleText, out string subtitleText, out string statusText)
    {
        titleText = string.Empty;
        subtitleText = string.Empty;
        statusText = string.Empty;

        if (gameFlow == null)
            return;

        string calibrationKeyLabel = "G";
        string stabKeyLabel = "E";
        if (gameFlow.playerController != null)
        {
            calibrationKeyLabel = gameFlow.playerController.GetPhaseTwoCalibrationKeyLabel();
            stabKeyLabel = gameFlow.playerController.GetPhaseTwoStabKeyLabel();
        }

        switch (gameFlow.CurrentPhaseTwoState)
        {
            case BullfightGameFlow.PhaseTwoState.Intro:
                titleText = "\u968e\u6bb5\u4e8c";
                subtitleText = gameFlow.IsPhaseTwoQuestionVisible ? "\u771f\u7684\u53ea\u6709\u6bba\u4e86\u4ed6\u9019\u500b\u8fa6\u6cd5\u55ce?" : string.Empty;
                break;
            case BullfightGameFlow.PhaseTwoState.Calibration:
                titleText = "\u6821\u6e96";
                subtitleText = $"\u6309\u4f4f {calibrationKeyLabel} \u6821\u6e96";
                statusText = $"{Mathf.RoundToInt(gameFlow.PhaseTwoCalibrationProgress * 100f)}%";
                break;
            case BullfightGameFlow.PhaseTwoState.Standoff:
                titleText = "\u5c0d\u5cd9";
                subtitleText = $"\u4fdd\u6301\u6c89\u9ed8 15 \u79d2\uff0c\u6216\u6309 {stabKeyLabel} \u6253\u7834\u5c0d\u5cd9";
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
                            ? $"\u7b49\u5f85\u725b\u9732\u51fa\u7834\u7dbb\uff0c\u7136\u5f8c\u6309 {stabKeyLabel} \u523a\u64ca"
                            : gameFlow.CurrentPhaseTwoReflectionLine;
                        statusText = gameFlow.CurrentRoundHasPerfectAdvantage
                            ? "\u4e0a\u4e00\u64ca\u7559\u4e0b\u7684\u7834\u7dbb\u9084\u5728\u64f4\u5927..."
                            : "\u725b\u6b63\u5728\u84c4\u529b...";
                    }
                }
                else
                {
                    subtitleText = $"\u73fe\u5728\u6309 {stabKeyLabel} \u523a\u64ca";
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
        if (text == null)
            return;

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
        if (text == null)
            return;

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

    private static void SetBossSegmentVisible(Image segment, bool visible)
    {
        if (segment != null)
            segment.gameObject.SetActive(visible);
    }

    private static string GetPhaseLabel(BullfightGameFlow.GamePhase phase)
    {
        return phase switch
        {
            BullfightGameFlow.GamePhase.PhaseZeroTutorial => "\u7b2c0\u968e\u6bb5",
            BullfightGameFlow.GamePhase.PhaseOne => "\u968e\u6bb5\u4e00",
            BullfightGameFlow.GamePhase.PhaseTwo => "\u968e\u6bb5\u4e8c",
            BullfightGameFlow.GamePhase.Ending => "\u7d50\u5c40\u6f14\u51fa",
            _ => "\u672a\u77e5\u968e\u6bb5"
        };
    }

    private static RectTransform GetOrCreateUiRect(RectTransform parent, string objectName)
    {
        if (parent == null)
            return null;

        Transform existing = parent.Find(objectName);
        if (existing != null)
            return existing as RectTransform;

        GameObject go = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static Image GetOrCreateUiImage(RectTransform parent, string objectName)
    {
        if (parent == null)
            return null;

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
        if (parent == null)
            return null;

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

    private void DisableLegacyShooterUiOnce()
    {
        if (legacyUiDisabled)
            return;

        var sceneTransforms = BullfightSceneCache.GetSceneObjects<Transform>();
        for (int i = 0; i < sceneTransforms.Count; i++)
        {
            Transform sceneTransform = sceneTransforms[i];
            if (sceneTransform == null)
                continue;

            if (!IsLegacyShooterUiObject(sceneTransform.name) && !IsChildOfLegacyShooterCanvas(sceneTransform))
                continue;

            sceneTransform.gameObject.SetActive(false);
        }

        legacyUiDisabled = true;
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
}
