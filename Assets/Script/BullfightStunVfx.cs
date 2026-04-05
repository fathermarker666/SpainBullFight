using UnityEngine;
using UnityEngine.UI;
using InfimaGames.LowPolyShooterPack;

public class BullfightStunVfx : MonoBehaviour
{
    public PlayerStats playerStats;

    [Header("Visual")]
    public float fadeSpeed = 6f;
    public float grayAlpha = 0.5f;
    public float vignetteAlpha = 0.82f;
    public float damageFlashAlpha = 0.42f;
    public float damageFlashFadeSpeed = 4.5f;
    public float lowHealthThreshold = 0.25f;
    public float lowHealthPulseSpeed = 1.8f;
    public float lowHealthOverlayAlpha = 0.18f;
    public Color lowHealthColor = new Color(0.92f, 0.22f, 0.22f, 1f);

    [Header("Dizzy Feel")]
    public float lingerDuration = 1.7f;
    public float pulseFrequency = 2.1f;
    public float vignetteOverscan = 1.18f;
    public float shockDuration = 0.65f;
    public float shockStrength = 1.1f;
    public float cameraFovKick = 13f;
    public float cameraFovWave = 7f;
    public float cameraFovRecoverSpeed = 6f;

    private Canvas overlayCanvas;
    private Image grayOverlay;
    private Image damageOverlay;
    private Image lowHealthOverlay;
    private RawImage vignetteOverlay;
    private RectTransform vignetteRect;
    private float currentWeight;
    private float damageFlashWeight;
    private Texture2D vignetteTexture;
    private float lingerTimer;
    private float shockTimer;
    private float effectTime;
    private bool wasStunnedLastFrame;
    private Camera worldCamera;
    private float baseFieldOfView;
    private bool hasBaseFieldOfView;

    private void Awake()
    {
        ResolveReferencesIfNeeded();
        EnsureOverlay();
    }

    private void OnEnable()
    {
        ResolveReferencesIfNeeded();
    }

    private void Update()
    {
        if (playerStats == null)
            ResolveReferencesIfNeeded();

        bool isStunned = playerStats != null && playerStats.isStunned;
        UpdateStunState(isStunned);

        float target = GetTargetWeight(isStunned);
        currentWeight = Mathf.MoveTowards(currentWeight, target, Time.unscaledDeltaTime * fadeSpeed);
        damageFlashWeight = Mathf.MoveTowards(damageFlashWeight, 0f, Time.unscaledDeltaTime * damageFlashFadeSpeed);
        shockTimer = Mathf.Max(0f, shockTimer - Time.unscaledDeltaTime);
        effectTime = currentWeight > 0.001f ? effectTime + Time.unscaledDeltaTime : 0f;

        ApplyWeight();
        ApplyDistortion();
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null)
            return;

        GameObject root = new GameObject("BullfightStunOverlay");
        root.transform.SetParent(transform, false);

        overlayCanvas = root.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 5000;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        root.AddComponent<GraphicRaycaster>().enabled = false;

        GameObject gray = new GameObject("GrayOverlay");
        gray.transform.SetParent(root.transform, false);
        grayOverlay = gray.AddComponent<Image>();
        grayOverlay.raycastTarget = false;
        grayOverlay.color = new Color(0.5f, 0.5f, 0.5f, 0f);
        StretchFullScreen(grayOverlay.rectTransform);

        GameObject damage = new GameObject("DamageOverlay");
        damage.transform.SetParent(root.transform, false);
        damageOverlay = damage.AddComponent<Image>();
        damageOverlay.raycastTarget = false;
        damageOverlay.color = new Color(0.9f, 0.1f, 0.1f, 0f);
        StretchFullScreen(damageOverlay.rectTransform);

        GameObject lowHealth = new GameObject("LowHealthOverlay");
        lowHealth.transform.SetParent(root.transform, false);
        lowHealthOverlay = lowHealth.AddComponent<Image>();
        lowHealthOverlay.raycastTarget = false;
        lowHealthOverlay.color = new Color(lowHealthColor.r, lowHealthColor.g, lowHealthColor.b, 0f);
        StretchFullScreen(lowHealthOverlay.rectTransform);

        GameObject vignette = new GameObject("VignetteOverlay");
        vignette.transform.SetParent(root.transform, false);
        vignetteOverlay = vignette.AddComponent<RawImage>();
        vignetteOverlay.raycastTarget = false;
        vignetteOverlay.color = new Color(0f, 0f, 0f, 0f);
        vignetteOverlay.texture = CreateVignetteTexture();
        vignetteRect = vignetteOverlay.rectTransform;
        StretchFullScreen(vignetteRect);
        ApplyVignetteOverscan();
    }

    private void ApplyWeight()
    {
        float shockWeight = GetShockWeight();
        float pulse = GetPulse01();
        float visualWeight = Mathf.Clamp01(currentWeight + shockWeight * 0.2f);
        float boostedGrayAlpha = grayAlpha * Mathf.Lerp(1f, 1.28f, pulse) * Mathf.Lerp(1f, 1.35f, shockWeight);
        float boostedVignetteAlpha = vignetteAlpha * Mathf.Lerp(1f, 1.35f, pulse) * Mathf.Lerp(1f, 1.5f, shockWeight);

        if (grayOverlay != null)
            grayOverlay.color = new Color(0.5f, 0.5f, 0.5f, boostedGrayAlpha * visualWeight);

        if (damageOverlay != null)
            damageOverlay.color = new Color(0.9f, 0.1f, 0.1f, damageFlashAlpha * damageFlashWeight);

        if (lowHealthOverlay != null)
        {
            bool lowHealthActive = playerStats != null && !playerStats.IsDead && playerStats.HealthNormalized <= lowHealthThreshold;
            float lowHealthPulse = lowHealthActive
                ? 0.45f + (0.55f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * lowHealthPulseSpeed * Mathf.PI * 2f)))
                : 0f;
            lowHealthOverlay.color = new Color(lowHealthColor.r, lowHealthColor.g, lowHealthColor.b, lowHealthOverlayAlpha * lowHealthPulse);
        }

        if (vignetteOverlay != null)
            vignetteOverlay.color = new Color(0f, 0f, 0f, boostedVignetteAlpha * visualWeight);
    }

    public void TriggerDamageFlash()
    {
        damageFlashWeight = 1f;
        if (damageOverlay != null)
            damageOverlay.transform.SetAsLastSibling();
        ApplyWeight();
    }

    private Texture2D CreateVignetteTexture()
    {
        if (vignetteTexture != null)
            return vignetteTexture;

        const int size = 256;
        vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        vignetteTexture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxDistance = center.magnitude;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / maxDistance;
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 1f, distance));
                vignetteTexture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }

        vignetteTexture.Apply();
        return vignetteTexture;
    }

    private void ResolveReferencesIfNeeded()
    {
        if (playerStats == null)
            playerStats = BullfightSceneCache.GetLocalOrScene<PlayerStats>(this);
    }

    private void UpdateStunState(bool isStunned)
    {
        if (isStunned)
        {
            lingerTimer = lingerDuration;
            if (!wasStunnedLastFrame)
                shockTimer = shockDuration;
        }
        else if (lingerTimer > 0f)
        {
            lingerTimer = Mathf.Max(0f, lingerTimer - Time.unscaledDeltaTime);
        }

        wasStunnedLastFrame = isStunned;
    }

    private float GetTargetWeight(bool isStunned)
    {
        if (isStunned)
            return 1f;

        if (lingerDuration <= 0f)
            return 0f;

        return Mathf.Clamp01(lingerTimer / lingerDuration) * 0.9f;
    }

    private void ApplyDistortion()
    {
        float shockWeight = GetShockWeight();
        float pulse = GetPulseSigned();
        float visualWeight = Mathf.Clamp01(currentWeight + shockWeight * shockStrength);

        ApplyCameraFov(visualWeight, pulse, shockWeight);
    }

    private void ApplyCameraFov(float visualWeight, float pulse, float shockWeight)
    {
        EnsureCameraReference();
        if (worldCamera == null)
            return;

        if (!hasBaseFieldOfView || visualWeight <= 0.001f)
        {
            baseFieldOfView = worldCamera.fieldOfView;
            hasBaseFieldOfView = true;
        }

        float targetOffset = (cameraFovKick * visualWeight) + (cameraFovWave * currentWeight * Mathf.Abs(pulse)) + (cameraFovWave * 0.6f * shockWeight);
        float targetFov = baseFieldOfView + targetOffset;
        worldCamera.fieldOfView = Mathf.Lerp(worldCamera.fieldOfView, targetFov, Time.unscaledDeltaTime * cameraFovRecoverSpeed);
    }

    private void EnsureCameraReference()
    {
        if (worldCamera != null)
            return;

        Character character = GetComponent<Character>() ?? (playerStats != null ? playerStats.GetComponent<Character>() : null);
        worldCamera = character != null ? character.GetCameraWorld() : Camera.main;
    }

    private float GetShockWeight()
    {
        if (shockDuration <= 0f)
            return 0f;

        return Mathf.Clamp01(shockTimer / shockDuration);
    }

    private float GetPulse01()
    {
        return 0.5f + 0.5f * GetPulseSigned();
    }

    private float GetPulseSigned()
    {
        return Mathf.Sin(effectTime * pulseFrequency * Mathf.PI * 2f);
    }

    private void OnDisable()
    {
        ResetPresentation();
    }

    private void OnDestroy()
    {
        ResetPresentation();
    }

    private void ResetPresentation()
    {
        if (vignetteRect != null)
            vignetteRect.localScale = new Vector3(vignetteOverscan, vignetteOverscan, 1f);

        if (worldCamera != null && hasBaseFieldOfView)
            worldCamera.fieldOfView = baseFieldOfView;
    }

    private void ApplyVignetteOverscan()
    {
        if (vignetteRect != null)
            vignetteRect.localScale = new Vector3(vignetteOverscan, vignetteOverscan, 1f);
    }

    private static void StretchFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}

public class BullfightPerfectDodgeVfx : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Visual")]
    [SerializeField] private Color overlayColor = new Color(0.18f, 0.72f, 1f, 0.46f);
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private float scrollSpeed = 0.32f;
    [SerializeField] private float pulseSpeed = 2.4f;
    [SerializeField] private float overscan = 1.12f;

    private Canvas overlayCanvas;
    private RawImage streakOverlay;
    private RectTransform overlayRect;
    private Texture2D streakTexture;
    private float currentWeight;
    private float scrollOffset;
    private float pulseTime;

    private void Awake()
    {
        ResolveReferencesIfNeeded();
        EnsureOverlay();
    }

    private void OnEnable()
    {
        ResolveReferencesIfNeeded();
    }

    private void Update()
    {
        if (playerStats == null)
            ResolveReferencesIfNeeded();

        bool active = playerStats != null && playerStats.IsPerfectDodgeBuffActive;
        currentWeight = Mathf.MoveTowards(currentWeight, active ? 1f : 0f, Time.unscaledDeltaTime * fadeSpeed);
        if (currentWeight <= 0.001f && streakOverlay != null)
        {
            streakOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
            return;
        }

        scrollOffset += Time.unscaledDeltaTime * scrollSpeed;
        pulseTime += Time.unscaledDeltaTime * pulseSpeed;
        float pulse = 0.82f + (Mathf.Sin(pulseTime * Mathf.PI * 2f) * 0.18f);

        if (streakOverlay != null)
        {
            streakOverlay.uvRect = new Rect(0f, scrollOffset, 1f, 1.6f);
            streakOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, overlayColor.a * currentWeight * pulse);
        }
    }

    private void ResolveReferencesIfNeeded()
    {
        if (playerStats == null)
            playerStats = BullfightSceneCache.GetLocalOrScene<PlayerStats>(this);
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null)
            return;

        GameObject root = new GameObject("BullfightPerfectDodgeOverlay");
        root.transform.SetParent(transform, false);

        overlayCanvas = root.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 4900;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>().enabled = false;

        GameObject overlay = new GameObject("PerfectDodgeFlow");
        overlay.transform.SetParent(root.transform, false);
        streakOverlay = overlay.AddComponent<RawImage>();
        streakOverlay.raycastTarget = false;
        streakOverlay.texture = CreateStreakTexture();
        streakOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
        overlayRect = streakOverlay.rectTransform;
        StretchOverlay(overlayRect);
        overlayRect.localScale = new Vector3(overscan, overscan, 1f);
    }

    private Texture2D CreateStreakTexture()
    {
        if (streakTexture != null)
            return streakTexture;

        const int width = 256;
        const int height = 256;
        streakTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        for (int y = 0; y < height; y++)
        {
            float verticalPulse = 0.25f + (0.75f * Mathf.Pow(Mathf.Sin((y / (float)height) * Mathf.PI), 2f));
            for (int x = 0; x < width; x++)
            {
                float stripeA = Mathf.Abs(Mathf.Sin(((x / (float)width) * Mathf.PI * 12f) + (y * 0.03f)));
                float stripeB = Mathf.Abs(Mathf.Sin(((x / (float)width) * Mathf.PI * 26f) - (y * 0.05f)));
                float alpha = Mathf.Clamp01((stripeA * 0.45f) + (stripeB * 0.35f) - 0.35f) * verticalPulse;
                streakTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        streakTexture.Apply();
        return streakTexture;
    }

    private static void StretchOverlay(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }
}
