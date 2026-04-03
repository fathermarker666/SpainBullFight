using InfimaGames.LowPolyShooterPack;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class BullfightPhaseTwoPresentation : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private float vignetteIntensity = 0.4f;
    [SerializeField] private float vignetteSmoothness = 0.75f;
    [SerializeField] private float postExposure = -0.35f;
    [SerializeField] private float saturation = -18f;
    [SerializeField] private float contrast = 18f;
    [SerializeField] private Color colorFilter = new Color(0.76f, 0.36f, 0.3f, 1f);
    [SerializeField] private float chromaticAberrationIntensity = 0.08f;

    [Header("Lighting")]
    [SerializeField] private Color phaseTwoAmbientSkyColor = new Color(0.18f, 0.08f, 0.08f, 1f);
    [SerializeField] private Color phaseTwoAmbientEquatorColor = new Color(0.11f, 0.04f, 0.04f, 1f);
    [SerializeField] private Color phaseTwoAmbientGroundColor = new Color(0.05f, 0.02f, 0.02f, 1f);
    [SerializeField] private Color phaseTwoDirectionalTint = new Color(1f, 0.68f, 0.58f, 1f);

    [Header("Resources")]
    [SerializeField] private string editorPostProcessResourcesPath = "Packages/com.unity.postprocessing/PostProcessing/PostProcessResources.asset";

    private Camera targetCamera;
    private Light directionalLight;
    private PostProcessLayer postProcessLayer;
    private PostProcessVolume postProcessVolume;
    private PostProcessProfile runtimeProfile;
    private PostProcessResources postProcessResources;

    private AmbientMode cachedAmbientMode;
    private float cachedAmbientIntensity;
    private Color cachedAmbientSkyColor;
    private Color cachedAmbientEquatorColor;
    private Color cachedAmbientGroundColor;
    private float cachedDirectionalIntensity;
    private Color cachedDirectionalColor;
    private LayerMask cachedVolumeLayer;
    private Transform cachedVolumeTrigger;
    private PostProcessLayer.Antialiasing cachedAntialiasing;

    private bool phaseTwoActive;
    private bool addedPostProcessLayer;
    private bool hasCachedLighting;
    private bool hasCachedLayerSettings;
    private bool missingResourcesLogged;

    public void EnterPhaseTwo(float ambientIntensityTarget, float directionalLightMultiplier)
    {
        EnsureReferences();
        CacheLightingState();
        CacheLayerState();
        ApplyLighting(ambientIntensityTarget, directionalLightMultiplier);
        ApplyPostProcessing();
        phaseTwoActive = true;
    }

    public void ExitPhaseTwo()
    {
        if (!phaseTwoActive && !hasCachedLighting && !hasCachedLayerSettings)
            return;

        RestoreLighting();
        RestorePostProcessing();
        phaseTwoActive = false;
    }

    private void OnDestroy()
    {
        ExitPhaseTwo();

        if (postProcessVolume != null)
            Destroy(postProcessVolume.gameObject);

        if (runtimeProfile != null)
            Destroy(runtimeProfile);
    }

    private void EnsureReferences()
    {
        Character shooterCharacter = FindObjectOfType<Character>(true);
        Camera resolvedCamera = shooterCharacter != null ? shooterCharacter.GetCameraWorld() : Camera.main;
        if (resolvedCamera == null)
            resolvedCamera = FindObjectOfType<Camera>(true);

        if (resolvedCamera != null && targetCamera != resolvedCamera)
        {
            targetCamera = resolvedCamera;
            postProcessLayer = null;
            hasCachedLayerSettings = false;
        }

        if (directionalLight == null)
            directionalLight = RenderSettings.sun != null ? RenderSettings.sun : FindObjectOfType<Light>(true);
    }

    private void CacheLightingState()
    {
        if (hasCachedLighting)
            return;

        cachedAmbientMode = RenderSettings.ambientMode;
        cachedAmbientIntensity = RenderSettings.ambientIntensity;
        cachedAmbientSkyColor = RenderSettings.ambientSkyColor;
        cachedAmbientEquatorColor = RenderSettings.ambientEquatorColor;
        cachedAmbientGroundColor = RenderSettings.ambientGroundColor;

        if (directionalLight != null)
        {
            cachedDirectionalIntensity = directionalLight.intensity;
            cachedDirectionalColor = directionalLight.color;
        }

        hasCachedLighting = true;
    }

    private void CacheLayerState()
    {
        if (hasCachedLayerSettings || targetCamera == null)
            return;

        postProcessLayer = targetCamera.GetComponent<PostProcessLayer>();
        addedPostProcessLayer = postProcessLayer == null;

        if (postProcessLayer == null)
            postProcessLayer = targetCamera.gameObject.AddComponent<PostProcessLayer>();

        cachedVolumeLayer = postProcessLayer.volumeLayer;
        cachedVolumeTrigger = postProcessLayer.volumeTrigger;
        cachedAntialiasing = postProcessLayer.antialiasingMode;
        hasCachedLayerSettings = true;
    }

    private void ApplyLighting(float ambientIntensityTarget, float directionalLightMultiplier)
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientIntensity = ambientIntensityTarget;
        RenderSettings.ambientSkyColor = phaseTwoAmbientSkyColor;
        RenderSettings.ambientEquatorColor = phaseTwoAmbientEquatorColor;
        RenderSettings.ambientGroundColor = phaseTwoAmbientGroundColor;

        if (directionalLight == null)
            return;

        directionalLight.intensity = cachedDirectionalIntensity * directionalLightMultiplier;
        directionalLight.color = Color.Lerp(cachedDirectionalColor, phaseTwoDirectionalTint, 0.4f);
    }

    private void ApplyPostProcessing()
    {
        if (postProcessLayer == null || targetCamera == null)
            return;

        PostProcessResources resources = ResolveResources();
        if (resources == null)
        {
            if (!missingResourcesLogged)
            {
                missingResourcesLogged = true;
                Debug.LogWarning("BullfightPhaseTwoPresentation could not find PostProcessResources. Lighting changes will still apply.");
            }
            return;
        }

        postProcessLayer.Init(resources);
        postProcessLayer.volumeTrigger = targetCamera.transform;
        postProcessLayer.volumeLayer = 1 << gameObject.layer;
        postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;

        EnsureRuntimeVolume();
        if (postProcessVolume != null)
            postProcessVolume.enabled = true;
    }

    private void EnsureRuntimeVolume()
    {
        if (postProcessVolume == null)
        {
            GameObject volumeObject = new GameObject("BullfightPhaseTwoVolume");
            volumeObject.transform.SetParent(transform, false);
            volumeObject.layer = gameObject.layer;
            postProcessVolume = volumeObject.AddComponent<PostProcessVolume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 100f;
            postProcessVolume.weight = 1f;
        }

        if (runtimeProfile == null)
        {
            runtimeProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
            runtimeProfile.name = "BullfightPhaseTwoProfile";
            ConfigureProfile(runtimeProfile);
        }

        postProcessVolume.gameObject.layer = gameObject.layer;
        postProcessVolume.sharedProfile = runtimeProfile;
        postProcessVolume.weight = 1f;
    }

    private void ConfigureProfile(PostProcessProfile profile)
    {
        Vignette vignette = profile.AddSettings<Vignette>();
        vignette.enabled.Override(true);
        vignette.intensity.Override(vignetteIntensity);
        vignette.smoothness.Override(vignetteSmoothness);
        vignette.rounded.Override(false);

        ColorGrading colorGrading = profile.AddSettings<ColorGrading>();
        colorGrading.enabled.Override(true);
        colorGrading.postExposure.Override(postExposure);
        colorGrading.saturation.Override(saturation);
        colorGrading.contrast.Override(contrast);
        colorGrading.colorFilter.Override(colorFilter);

        ChromaticAberration chromaticAberration = profile.AddSettings<ChromaticAberration>();
        chromaticAberration.enabled.Override(true);
        chromaticAberration.intensity.Override(chromaticAberrationIntensity);
    }

    private void RestoreLighting()
    {
        if (!hasCachedLighting)
            return;

        RenderSettings.ambientMode = cachedAmbientMode;
        RenderSettings.ambientIntensity = cachedAmbientIntensity;
        RenderSettings.ambientSkyColor = cachedAmbientSkyColor;
        RenderSettings.ambientEquatorColor = cachedAmbientEquatorColor;
        RenderSettings.ambientGroundColor = cachedAmbientGroundColor;

        if (directionalLight != null)
        {
            directionalLight.intensity = cachedDirectionalIntensity;
            directionalLight.color = cachedDirectionalColor;
        }

        hasCachedLighting = false;
    }

    private void RestorePostProcessing()
    {
        if (postProcessVolume != null)
            postProcessVolume.enabled = false;

        if (!hasCachedLayerSettings)
            return;

        if (addedPostProcessLayer)
        {
            Destroy(postProcessLayer);
            postProcessLayer = null;
        }
        else if (postProcessLayer != null)
        {
            postProcessLayer.volumeLayer = cachedVolumeLayer;
            postProcessLayer.volumeTrigger = cachedVolumeTrigger;
            postProcessLayer.antialiasingMode = cachedAntialiasing;
        }

        addedPostProcessLayer = false;
        hasCachedLayerSettings = false;
    }

    private PostProcessResources ResolveResources()
    {
        if (postProcessResources != null)
            return postProcessResources;

#if UNITY_EDITOR
        postProcessResources = UnityEditor.AssetDatabase.LoadAssetAtPath<PostProcessResources>(editorPostProcessResourcesPath);
#endif

        if (postProcessResources != null)
            return postProcessResources;

        PostProcessResources[] availableResources = Resources.FindObjectsOfTypeAll<PostProcessResources>();
        if (availableResources != null && availableResources.Length > 0)
            postProcessResources = availableResources[0];

        return postProcessResources;
    }
}
