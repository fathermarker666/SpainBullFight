using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BullfightStartMenu : MonoBehaviour
{
    [Header("Text")]
    public string titleText = "\u897f\u73ed\u7259\u9b25\u725b";
    public string subtitleText = "\u7b2c\u4e00\u4eba\u7a31\u9b25\u725b\u9ad4\u9a57";
    public string startButtonText = "\u958b\u59cb\u904a\u6232";
    public string tutorialButtonText = "\u65b0\u624b\u6559\u5b78";
    public string hintText = "\u9078\u64c7\u300c\u958b\u59cb\u904a\u6232\u300d\u6216\u300c\u65b0\u624b\u6559\u5b78\u300d";

    [Header("Layout")]
    public Vector2 panelSize = new Vector2(760f, 430f);
    public Vector2 titlePosition = new Vector2(0f, 110f);
    public Vector2 subtitlePosition = new Vector2(0f, 40f);
    public Vector2 hintPosition = new Vector2(0f, -26f);
    public Vector2 buttonCenterPosition = new Vector2(0f, -126f);
    public Vector2 buttonSize = new Vector2(260f, 72f);
    public float buttonSpacing = 36f;

    [Header("Colors")]
    public Color backdropColor = new Color(0.07f, 0.02f, 0.02f, 0.72f);
    public Color panelColor = new Color(0.14f, 0.04f, 0.04f, 0.9f);
    public Color borderColor = new Color(0.84f, 0.71f, 0.49f, 0.9f);
    public Color accentColor = new Color(0.72f, 0.12f, 0.12f, 1f);
    public Color titleColor = new Color(0.98f, 0.92f, 0.78f, 1f);
    public Color subtitleColor = new Color(0.9f, 0.82f, 0.66f, 1f);
    public Color hintColor = new Color(0.86f, 0.84f, 0.8f, 0.92f);
    public Color buttonColor = new Color(0.56f, 0.09f, 0.09f, 1f);
    public Color buttonTextColor = new Color(1f, 0.95f, 0.86f, 1f);

    private Canvas canvas;
    private GameObject root;
    private Button startButton;
    private Button tutorialButton;
    private bool started;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureStartMenuExists()
    {
        if (FindObjectOfType<BullfightStartMenu>(true) != null)
            return;

        GameObject startMenuObject = new("BullfightStartMenu");
        _ = startMenuObject.AddComponent<BullfightStartMenu>();
    }

    private void Start()
    {
        BuildMenu();
        ShowMenu();
    }

    private void Update()
    {
        if (started || canvas == null || !canvas.gameObject.activeSelf)
            return;

        EnsureButtonSelected();

        if (!WasConfirmRequestedThisFrame() || EventSystem.current == null)
            return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == tutorialButton?.gameObject)
            BeginTutorial();
        else
            BeginGame();
    }

    public void BeginGame()
    {
        if (started)
            return;

        started = true;
        HideMenu();
    }

    public void BeginTutorial()
    {
        if (started)
            return;

        started = true;
        HideMenu();

        BullfightGameFlow gameFlow = FindObjectOfType<BullfightGameFlow>(true);
        if (gameFlow == null)
        {
            GameObject gameFlowObject = new("BullfightGameFlow");
            gameFlow = gameFlowObject.AddComponent<BullfightGameFlow>();
        }

        gameFlow.BeginTutorial();
    }

    private void HideMenu()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void BuildMenu()
    {
        if (root != null)
            return;

        GameObject canvasObject = new GameObject("BullfightStartMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 4000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject backdrop = CreateImage("Backdrop", canvasObject.transform, backdropColor);
        StretchToFullScreen(backdrop.GetComponent<RectTransform>());

        root = CreateImage("MenuPanel", canvasObject.transform, panelColor);
        RectTransform panelRect = root.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = Vector2.zero;

        CreateBorder(root.transform, new Vector2(0f, panelSize.y * 0.5f - 10f), new Vector2(panelSize.x - 26f, 4f));
        CreateBorder(root.transform, new Vector2(0f, -panelSize.y * 0.5f + 10f), new Vector2(panelSize.x - 26f, 4f));
        CreateBorder(root.transform, new Vector2(-panelSize.x * 0.5f + 10f, 0f), new Vector2(4f, panelSize.y - 26f));
        CreateBorder(root.transform, new Vector2(panelSize.x * 0.5f - 10f, 0f), new Vector2(4f, panelSize.y - 26f));
        CreateImageBand(root.transform, new Vector2(0f, 82f), new Vector2(panelSize.x - 80f, 54f), accentColor);

        Text title = CreateText("Title", root.transform, titleText, 54, titleColor, FontStyle.Bold);
        ConfigureTextRect(title.rectTransform, titlePosition, new Vector2(660f, 84f));

        Text subtitle = CreateText("Subtitle", root.transform, subtitleText, 24, subtitleColor, FontStyle.Normal);
        ConfigureTextRect(subtitle.rectTransform, subtitlePosition, new Vector2(620f, 42f));

        Text hint = CreateText("Hint", root.transform, hintText, 20, hintColor, FontStyle.Italic);
        ConfigureTextRect(hint.rectTransform, hintPosition, new Vector2(660f, 34f));

        float horizontalOffset = (buttonSize.x * 0.5f) + (buttonSpacing * 0.5f);
        startButton = CreateMenuButton(root.transform, "StartButton", startButtonText, buttonCenterPosition + new Vector2(-horizontalOffset, 0f));
        tutorialButton = CreateMenuButton(root.transform, "TutorialButton", tutorialButtonText, buttonCenterPosition + new Vector2(horizontalOffset, 0f));

        ConfigureButtonNavigation(startButton, tutorialButton, selectOnLeft: null, selectOnRight: tutorialButton);
        ConfigureButtonNavigation(tutorialButton, startButton, selectOnLeft: startButton, selectOnRight: null);

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(BeginGame);

        tutorialButton.onClick.RemoveAllListeners();
        tutorialButton.onClick.AddListener(BeginTutorial);
    }

    private void ShowMenu()
    {
        started = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (root != null)
            root.SetActive(true);

        EnsureButtonSelected();
    }

    private void EnsureButtonSelected()
    {
        if (startButton == null || EventSystem.current == null)
            return;

        if (EventSystem.current.currentSelectedGameObject != null)
            return;

        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    private static bool WasConfirmRequestedThisFrame()
    {
        bool keyboardRequested = Keyboard.current != null &&
                                 (Keyboard.current.enterKey.wasPressedThisFrame ||
                                  Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                                  Keyboard.current.spaceKey.wasPressedThisFrame);

        bool gamepadRequested = Gamepad.current != null &&
                                (Gamepad.current.startButton.wasPressedThisFrame ||
                                 Gamepad.current.buttonSouth.wasPressedThisFrame);

        return keyboardRequested || gamepadRequested;
    }

    private Button CreateMenuButton(Transform parent, string objectName, string label, Vector2 position)
    {
        GameObject buttonObject = CreateImage(objectName, parent, buttonColor);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = buttonSize;
        buttonRect.anchoredPosition = position;

        CreateBorder(buttonObject.transform, new Vector2(0f, buttonSize.y * 0.5f - 2f), new Vector2(buttonSize.x - 10f, 3f));
        CreateBorder(buttonObject.transform, new Vector2(0f, -buttonSize.y * 0.5f + 2f), new Vector2(buttonSize.x - 10f, 3f));

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = new Color(0.68f, 0.16f, 0.14f, 1f);
        colors.pressedColor = new Color(0.42f, 0.06f, 0.06f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        Text buttonLabel = CreateText("ButtonLabel", buttonObject.transform, label, 30, buttonTextColor, FontStyle.Bold);
        ConfigureTextRect(buttonLabel.rectTransform, Vector2.zero, buttonSize);
        return button;
    }

    private static void ConfigureButtonNavigation(Button button, Button fallback, Selectable selectOnLeft, Selectable selectOnRight)
    {
        if (button == null)
            return;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnLeft = selectOnLeft ?? fallback;
        navigation.selectOnRight = selectOnRight ?? fallback;
        navigation.selectOnUp = button;
        navigation.selectOnDown = button;
        button.navigation = navigation;
    }

    private static GameObject CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        return gameObject;
    }

    private static Text CreateText(string objectName, Transform parent, string content, int fontSize, Color color)
    {
        return CreateText(objectName, parent, content, fontSize, color, FontStyle.Normal);
    }

    private static Text CreateText(string objectName, Transform parent, string content, int fontSize, Color color, FontStyle fontStyle)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        Text text = gameObject.GetComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private GameObject CreateBorder(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject border = CreateImage("Border", parent, borderColor);
        RectTransform rect = border.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return border;
    }

    private GameObject CreateImageBand(Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject band = CreateImage("AccentBand", parent, color);
        RectTransform rect = band.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return band;
    }

    private static void ConfigureTextRect(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
    }

    private static void StretchToFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
