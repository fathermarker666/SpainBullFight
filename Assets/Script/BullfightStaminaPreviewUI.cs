using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-80)]
public class BullfightStaminaPreviewUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private string staminaBarName = "StaminaBar";

    [Header("Guide Lines")]
    [SerializeField] private float guideLineWidth = 6f;
    [SerializeField] private float guideLabelSpacing = 28f;
    [SerializeField] private int guideLabelFontSize = 18;
    [SerializeField] private Color capaLineColor = new Color(0.96f, 0.38f, 0.22f, 0.95f);
    [SerializeField] private Color attackLineColor = new Color(1f, 0.84f, 0.22f, 0.95f);
    [SerializeField] private Color dashLineColor = new Color(0.2f, 0.82f, 0.98f, 0.95f);
    [SerializeField] private Color blockedLineColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);
    [SerializeField] private Color capaFillColor = new Color(0.95f, 0.34f, 0.18f, 1f);

    private PlayerStats playerStats;
    private BullfightPlayerController playerController;
    private BullAI bullAI;
    private BullfightGameFlow gameFlow;
    private Slider staminaSlider;
    private RectTransform fillAreaRect;
    private RectTransform guideRoot;
    private Image staminaFillImage;
    private Image capaGuideImage;
    private Image attackGuideImage;
    private Image dashGuideImage;
    private Text capaGuideLabel;
    private Text attackGuideLabel;
    private Text dashGuideLabel;
    private Color cachedFillColor;
    private bool hasCachedFillColor;

    private void Awake()
    {
        AutoAssignReferences();
        EnsurePreviewUi();
        HideAllGuides();
    }

    private void Update()
    {
        AutoAssignReferences();
        EnsurePreviewUi();
        RefreshPreview();
    }

    private void AutoAssignReferences()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>() ?? FindObjectOfType<PlayerStats>(true);

        if (playerController == null)
            playerController = GetComponent<BullfightPlayerController>() ?? FindObjectOfType<BullfightPlayerController>(true);

        if (bullAI == null)
            bullAI = FindObjectOfType<BullAI>(true);

        if (gameFlow == null)
            gameFlow = FindObjectOfType<BullfightGameFlow>(true);

        if (playerStats != null && playerStats.staminaBar != null)
            staminaSlider = playerStats.staminaBar;
        else if (staminaSlider == null)
            staminaSlider = FindStaminaSlider();
    }

    private void EnsurePreviewUi()
    {
        if (staminaSlider == null)
            return;

        if (fillAreaRect == null)
            fillAreaRect = ResolveFillAreaRect(staminaSlider);

        if (fillAreaRect == null)
            return;

        if (guideRoot == null)
            guideRoot = GetOrCreateGuideRoot(fillAreaRect);

        if (staminaFillImage == null && staminaSlider.fillRect != null)
            staminaFillImage = staminaSlider.fillRect.GetComponent<Image>();

        if (staminaFillImage != null && !hasCachedFillColor)
        {
            cachedFillColor = staminaFillImage.color;
            hasCachedFillColor = true;
        }

        if (capaGuideImage == null)
            capaGuideImage = CreateGuideImage("CapaGuideLine");

        if (attackGuideImage == null)
            attackGuideImage = CreateGuideImage("AttackGuideLine");

        if (dashGuideImage == null)
            dashGuideImage = CreateGuideImage("DashGuideLine");

        if (capaGuideLabel == null)
            capaGuideLabel = CreateGuideLabel("CapaGuideLabel");

        if (attackGuideLabel == null)
            attackGuideLabel = CreateGuideLabel("AttackGuideLabel");

        if (dashGuideLabel == null)
            dashGuideLabel = CreateGuideLabel("DashGuideLabel");

        DisableLegacyPreviewObjects();
    }

    private void RefreshPreview()
    {
        if (playerStats == null || staminaSlider == null || fillAreaRect == null || guideRoot == null)
        {
            RestoreFillColor();
            HideAllGuides();
            return;
        }

        if (!playerStats.CanAct() || (gameFlow != null && gameFlow.currentPhase == BullfightGameFlow.GamePhase.PhaseTwo))
        {
            RestoreFillColor();
            HideAllGuides();
            return;
        }

        if (playerStats.isHoldingCloth)
        {
            ApplyCapaFillColor();
            HideGuide(attackGuideImage, attackGuideLabel);
            HideGuide(dashGuideImage, dashGuideLabel);
            UpdateGuide(capaGuideImage, capaGuideLabel, true, playerStats.capaCost, capaLineColor, playerStats.HasEnoughStamina(playerStats.capaCost), "CAPA");
            return;
        }

        RestoreFillColor();
        HideGuide(capaGuideImage, capaGuideLabel);
        UpdateGuide(dashGuideImage, dashGuideLabel, true, playerStats.dashCost, dashLineColor, playerStats.HasEnoughStamina(playerStats.dashCost), "DASH");

        bool attackVisible = IsAttackPreviewAvailable();
        bool attackAffordable = attackVisible && playerStats.HasEnoughStamina(playerStats.banderillasCost);
        UpdateGuide(attackGuideImage, attackGuideLabel, attackVisible, playerStats.banderillasCost, attackLineColor, attackAffordable, "ATTACK");
    }

    private bool IsAttackPreviewAvailable()
    {
        if (bullAI == null || playerStats == null)
            return false;

        return bullAI.CurrentDistanceToPlayer <= bullAI.banderillasRange;
    }

    private void UpdateGuide(Image guideImage, Text guideLabel, bool visible, float cost, Color readyColor, bool hasEnoughStamina, string labelText)
    {
        if (guideImage == null || guideLabel == null)
            return;

        if (!visible)
        {
            HideGuide(guideImage, guideLabel);
            return;
        }

        float projectedNormalized = Mathf.Clamp01(playerStats.GetProjectedStaminaNormalized(cost));
        float lineX = fillAreaRect.rect.width * projectedNormalized;
        Color displayColor = hasEnoughStamina ? readyColor : blockedLineColor;

        RectTransform lineRect = guideImage.rectTransform;
        lineRect.anchorMin = new Vector2(0f, 0f);
        lineRect.anchorMax = new Vector2(0f, 1f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = new Vector2(lineX, 0f);
        lineRect.sizeDelta = new Vector2(guideLineWidth, 0f);
        lineRect.localScale = Vector3.one;
        lineRect.localRotation = Quaternion.identity;

        RectTransform labelRect = guideLabel.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = new Vector2(lineX, -(fillAreaRect.rect.height + guideLabelSpacing));
        labelRect.sizeDelta = new Vector2(128f, 24f);
        labelRect.localScale = Vector3.one;
        labelRect.localRotation = Quaternion.identity;

        guideImage.color = displayColor;
        guideImage.enabled = true;

        guideLabel.text = labelText;
        guideLabel.color = displayColor;
        guideLabel.enabled = true;
    }

    private void HideAllGuides()
    {
        HideGuide(capaGuideImage, capaGuideLabel);
        HideGuide(attackGuideImage, attackGuideLabel);
        HideGuide(dashGuideImage, dashGuideLabel);
    }

    private static void HideGuide(Image guideImage, Text guideLabel)
    {
        if (guideImage != null)
            guideImage.enabled = false;

        if (guideLabel != null)
            guideLabel.enabled = false;
    }

    private void ApplyCapaFillColor()
    {
        if (staminaFillImage != null)
            staminaFillImage.color = capaFillColor;
    }

    private void RestoreFillColor()
    {
        if (staminaFillImage != null && hasCachedFillColor)
            staminaFillImage.color = cachedFillColor;
    }

    private Image CreateGuideImage(string objectName)
    {
        GameObject guideObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = guideObject.GetComponent<RectTransform>();
        rect.SetParent(guideRoot, false);

        Image image = guideObject.GetComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
        image.enabled = false;
        return image;
    }

    private Text CreateGuideLabel(string objectName)
    {
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.SetParent(guideRoot, false);

        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = guideLabelFontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.UpperCenter;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        label.enabled = false;
        return label;
    }

    private RectTransform GetOrCreateGuideRoot(RectTransform parent)
    {
        Transform existing = parent.Find("StaminaActionGuideRoot");
        RectTransform root;
        if (existing != null)
            root = existing as RectTransform;
        else
        {
            GameObject go = new GameObject("StaminaActionGuideRoot", typeof(RectTransform));
            root = go.GetComponent<RectTransform>();
            root.SetParent(parent, false);
        }

        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.localScale = Vector3.one;
        root.localRotation = Quaternion.identity;
        return root;
    }

    private void DisableLegacyPreviewObjects()
    {
        string[] legacyObjects =
        {
            "StaminaPreviewLoss",
            "StaminaPreviewMarker",
            "StaminaPreviewText"
        };

        foreach (string legacyName in legacyObjects)
        {
            Transform fillChild = fillAreaRect != null ? fillAreaRect.Find(legacyName) : null;
            if (fillChild != null)
                fillChild.gameObject.SetActive(false);

            Transform sliderChild = staminaSlider != null ? staminaSlider.transform.Find(legacyName) : null;
            if (sliderChild != null)
                sliderChild.gameObject.SetActive(false);
        }
    }

    private RectTransform ResolveFillAreaRect(Slider slider)
    {
        if (slider.fillRect != null && slider.fillRect.parent is RectTransform fillParent)
            return fillParent;

        return slider.GetComponent<RectTransform>();
    }

    private Slider FindStaminaSlider()
    {
        foreach (Slider slider in Resources.FindObjectsOfTypeAll<Slider>())
        {
            if (slider == null || !slider.gameObject.scene.IsValid())
                continue;

            if (slider.name == staminaBarName)
                return slider;
        }

        return null;
    }
}
