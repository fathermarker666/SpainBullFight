using InfimaGames.LowPolyShooterPack;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(250)]
public class BullfightPauseSettingsUI : MonoBehaviour
{
    private const float StickDeadzone = 0.2f;
    private const float VolumeAdjustSpeed = 0.65f;

    private CameraLook cameraLook;
    private bool cameraLookWasEnabled;

    private static Font cachedFont;

    private Canvas canvas;
    private GameObject panelRoot;
    private RectTransform bgmFillRect;
    private RectTransform sfxFillRect;
    private Text bgmValueLabel;
    private Text sfxValueLabel;
    private Text helpLabel;

    private Text toggleHintLabel;

    private BullfightAudioController audioController;
    private BullfightPlayerController playerController;
    private float previousTimeScale = 1f;
    private float previousFixedDeltaTime = 0.02f;
    private bool menuOpen;
    private CursorLockMode previousCursorLockMode = CursorLockMode.Locked;
    private bool previousCursorVisible;
    private bool playerControllerWasEnabled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<BullfightPauseSettingsUI>(true) != null)
            return;

        GameObject host = new GameObject("BullfightPauseSettingsUI");
        host.AddComponent<BullfightPauseSettingsUI>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        previousFixedDeltaTime = Time.fixedDeltaTime;
        SceneManager.sceneLoaded += OnSceneLoaded;
        BuildUi();
        SetPanelVisible(false);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (menuOpen)
            ResumeGameplay();
    }

    private void Update()
    {
        ResolveReferencesIfNeeded();

        if (WasTogglePressed())
        {
            if (menuOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        if (!menuOpen)
            return;
        if (menuOpen && cameraLook != null)
            cameraLook.enabled = false;

        UpdateVolumeInput();
        RefreshLabels();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResolveReferencesIfNeeded();
        RefreshLabels();
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("BullfightSettingsCanvas");
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = CreatePanel(canvasObject.transform, "SetPanel", new Color(0.08f, 0.02f, 0.02f, 0.9f), new Vector2(1100f, 560f));
        RectTransform rootRect = panelRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;

        toggleHintLabel = CreateText(
     canvasObject.transform,
     "ToggleHint",
     "ESC / L3 開啟設定",
     22,
     FontStyle.Bold,
     TextAnchor.MiddleCenter,
     new Vector2(850f, -40f),
     new Vector2(320f, 40f)
 );

        CreateText(panelRoot.transform, "Title", "SET", 42, FontStyle.Bold, TextAnchor.UpperCenter, new Vector2(0f, -34f), new Vector2(520f, 60f));

        CreateVolumeColumn(panelRoot.transform, "BGM", "Background", new Vector2(-260f, -10f), out bgmFillRect, out bgmValueLabel);
        CreateVolumeColumn(panelRoot.transform, "SFX", "Effects", new Vector2(260f, -10f), out sfxFillRect, out sfxValueLabel);

        helpLabel = CreateText(
            canvasObject.transform,
            "Help",
            "ESC / L3: close   Left Stick / W,S: BGM   Right Stick / Up,Down: SFX",
            20,
            FontStyle.Normal,
            TextAnchor.MiddleCenter,
            new Vector2(0f, 250f),
            new Vector2(980f, 48f)
        );
        helpLabel.transform.SetAsLastSibling();

        RectTransform helpRect = helpLabel.GetComponent<RectTransform>();
        helpRect.anchorMin = new Vector2(0.5f, 0.5f);
        helpRect.anchorMax = new Vector2(0.5f, 0.5f);
        helpRect.pivot = new Vector2(0.5f, 0.5f);

        helpLabel.transform.SetAsLastSibling();
    }

    private void CreateVolumeColumn(Transform parent, string prefix, string title, Vector2 anchoredPosition, out RectTransform fillRect, out Text valueLabel)
    {
        GameObject column = CreatePanel(parent, prefix + "Column", new Color(0.18f, 0.05f, 0.05f, 0.95f), new Vector2(380f, 430f));
        RectTransform columnRect = column.GetComponent<RectTransform>();
        columnRect.anchorMin = new Vector2(0.5f, 0.5f);
        columnRect.anchorMax = new Vector2(0.5f, 0.5f);
        columnRect.anchoredPosition = anchoredPosition;

        GameObject barBack = CreatePanel(
            column.transform,
            prefix + "BarBack",
            new Color(0.12f, 0.12f, 0.12f, 1f),
            new Vector2(104f, 190f)
        );
        RectTransform barRect = barBack.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0.5f);
        barRect.anchorMax = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = new Vector2(0f, 55f);

        GameObject fill = new GameObject(prefix + "Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(barBack.transform, false);
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = new Color(0.84f, 0.18f, 0.12f, 1f);

        fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(8f, 8f);
        fillRect.offsetMax = new Vector2(-8f, -8f);
        fillRect.pivot = new Vector2(0.5f, 0f);

        Text hintLabel = CreateText(
            column.transform,
            prefix + "Hint",
            "Push stick up / down",
            15,
            FontStyle.Italic,
            TextAnchor.MiddleLeft,
            new Vector2(-15f, -98f),
            new Vector2(210f, 24f)
        );

        Text stickLabel = CreateText(
            column.transform,
            prefix + "Stick",
            prefix == "BGM" ? "Left Stick / W,S" : "Right Stick / Up,Down",
            17,
            FontStyle.Normal,
            TextAnchor.MiddleLeft,
            new Vector2(-15f, -122f),
            new Vector2(210f, 26f)
        );

        Text titleLabel = CreateText(
            column.transform,
            prefix + "Title",
            title,
            30,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -182f),
            new Vector2(280f, 40f)
        );

        valueLabel = CreateText(
            column.transform,
            prefix + "Value",
            "100%",
            24,
            FontStyle.Bold,
            TextAnchor.MiddleCenter,
            new Vector2(0f, -220f),
            new Vector2(180f, 36f)
        );

        hintLabel.transform.SetAsLastSibling();
        stickLabel.transform.SetAsLastSibling();
        titleLabel.transform.SetAsLastSibling();
        valueLabel.transform.SetAsLastSibling();
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.color = color;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        return panel;
    }

    private static Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle style, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = GetUiFont();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = new Color(0.97f, 0.93f, 0.84f, 1f);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return text;
    }

    private static Font GetUiFont()
    {
        if (cachedFont != null)
            return cachedFont;

        cachedFont = Font.CreateDynamicFontFromOSFont(new[]
        {
            "Arial",
            "Microsoft JhengHei UI",
            "Microsoft JhengHei",
            "Segoe UI"
        }, 18);

        return cachedFont;
    }

    private void ResolveReferencesIfNeeded()
    {
        if (cameraLook == null)
        {
            PlayerStats stats = FindObjectOfType<PlayerStats>();
            if (stats != null)
                cameraLook = stats.GetComponentInChildren<CameraLook>();
        }
        if (audioController == null)
            audioController = BullfightSceneCache.FindObject<BullfightAudioController>();

        if (playerController == null)
            playerController = BullfightSceneCache.FindObject<BullfightPlayerController>();
    }

    private bool WasTogglePressed()
    {
        bool keyboardToggle = Keyboard.current != null &&
                              Keyboard.current.escapeKey.wasPressedThisFrame;
        bool gamepadToggle = Gamepad.current != null && Gamepad.current.leftStickButton.wasPressedThisFrame;
        return keyboardToggle || gamepadToggle;
    }

    private void OpenMenu()
    {
        ResolveReferencesIfNeeded();
        previousTimeScale = Time.timeScale;
        previousFixedDeltaTime = Time.fixedDeltaTime;
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        playerControllerWasEnabled = playerController != null && playerController.enabled;
        cameraLookWasEnabled = cameraLook != null && cameraLook.enabled;

        if (cameraLook != null)
            cameraLook.enabled = false;

        if (playerController != null)
        {
            playerController.ClearInputBuffers();
            playerController.enabled = false;
        }

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        menuOpen = true;
        SetPanelVisible(true);
        RefreshLabels();
        if (helpLabel != null)
            helpLabel.transform.SetAsLastSibling();
    }

    private void CloseMenu()
    {
        ResumeGameplay();
        menuOpen = false;
        SetPanelVisible(false);
    }

    private void ResumeGameplay()
    {
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        Time.fixedDeltaTime = previousFixedDeltaTime > 0f ? previousFixedDeltaTime : 0.02f;
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
        if (cameraLook != null)
            cameraLook.enabled = cameraLookWasEnabled;

        if (playerController != null)
        {
            playerController.ClearInputBuffers();
            playerController.enabled = playerControllerWasEnabled;
        }
    }

    private void UpdateVolumeInput()
    {
        if (audioController == null)
            return;

        float bgmInput = ReadAxis(Gamepad.current != null ? Gamepad.current.leftStick.ReadValue().y : 0f,
            (Keyboard.current != null && Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current != null && Keyboard.current.sKey.isPressed ? 1f : 0f));
        float sfxInput = ReadAxis(Gamepad.current != null ? Gamepad.current.rightStick.ReadValue().y : 0f,
            (Keyboard.current != null && Keyboard.current.upArrowKey.isPressed ? 1f : 0f) - (Keyboard.current != null && Keyboard.current.downArrowKey.isPressed ? 1f : 0f));

        if (Mathf.Abs(bgmInput) > 0.001f)
            audioController.SetBgmVolume(audioController.BgmVolume + bgmInput * VolumeAdjustSpeed * Time.unscaledDeltaTime);

        if (Mathf.Abs(sfxInput) > 0.001f)
            audioController.SetSfxVolume(audioController.SfxVolume + sfxInput * VolumeAdjustSpeed * Time.unscaledDeltaTime);
    }

    private static float ReadAxis(float stickValue, float keyboardValue)
    {
        if (Mathf.Abs(stickValue) >= StickDeadzone)
            return stickValue;

        return keyboardValue;
    }

    private void RefreshLabels()
    {
        if (audioController == null || bgmFillRect == null || sfxFillRect == null)
            return;

        float bgmVolume = audioController.BgmVolume;
        float sfxVolume = audioController.SfxVolume;
        ApplyBarLevel(bgmFillRect, bgmVolume);
        ApplyBarLevel(sfxFillRect, sfxVolume);
        bgmValueLabel.text = Mathf.RoundToInt(bgmVolume * 100f) + "%";
        sfxValueLabel.text = Mathf.RoundToInt(sfxVolume * 100f) + "%";
        helpLabel.text = menuOpen ? "ESC / L3: close   Left Stick / W,S: BGM   Right Stick / Up,Down: SFX" : helpLabel.text;
    }

    private static void ApplyBarLevel(RectTransform rect, float normalized)
    {
        if (rect == null)
            return;

        float clamped = Mathf.Clamp01(normalized);
        rect.anchorMax = new Vector2(1f, clamped);
        rect.offsetMax = new Vector2(-8f, clamped <= 0.001f ? -120f : -8f);
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(visible);

        if (helpLabel != null)
            helpLabel.gameObject.SetActive(visible);

        if (toggleHintLabel != null)
            toggleHintLabel.gameObject.SetActive(!visible);
    }
}
