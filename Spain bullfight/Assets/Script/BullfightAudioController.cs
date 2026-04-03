using System.Collections.Generic;
using UnityEngine;

public class BullfightAudioController : MonoBehaviour
{
    [System.Serializable]
    private class AudioCue
    {
        public AudioClip clip;
        [Range(0f, 1.5f)] public float volume = 1f;
        [Range(0.5f, 1.5f)] public float pitch = 1f;
        [Range(0f, 1f)] public float spatialBlend = 0f;
    }

    private enum PlaceholderCue
    {
        ClothRaise,
        ClothLower,
        Capa,
        Evade,
        Banderillas,
        PlayerHit,
        PlayerStun,
        PlayerRecover,
        BullTelegraph,
        BullCharge,
        BullFatigue,
        BullHurt,
        BullDeath,
        TimingPerfect,
        TimingGood,
        TimingMiss
    }

    private delegate float ProceduralSample(int sampleIndex, float time, float progress);

    [Header("General")]
    [SerializeField] private bool useProceduralFallback = true;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.9f;
    [SerializeField] private Transform playerAnchor;
    [SerializeField] private Transform bullAnchor;

    [Header("Player")]
    [SerializeField] private AudioCue clothRaiseCue = new AudioCue { volume = 0.35f, pitch = 1.08f, spatialBlend = 0.05f };
    [SerializeField] private AudioCue clothLowerCue = new AudioCue { volume = 0.3f, pitch = 0.96f, spatialBlend = 0.05f };
    [SerializeField] private AudioCue capaCue = new AudioCue { volume = 0.6f, pitch = 1.04f, spatialBlend = 0.1f };
    [SerializeField] private AudioCue evadeCue = new AudioCue { volume = 0.55f, pitch = 1.08f, spatialBlend = 0.1f };
    [SerializeField] private AudioCue banderillasCue = new AudioCue { volume = 0.7f, pitch = 0.98f, spatialBlend = 0.2f };
    [SerializeField] private AudioCue playerHitCue = new AudioCue { volume = 0.78f, pitch = 0.94f, spatialBlend = 0.15f };
    [SerializeField] private AudioCue playerStunCue = new AudioCue { volume = 0.55f, pitch = 0.9f, spatialBlend = 0f };
    [SerializeField] private AudioCue playerRecoverCue = new AudioCue { volume = 0.45f, pitch = 1.05f, spatialBlend = 0f };

    [Header("Bull")]
    [SerializeField] private AudioCue bullTelegraphCue = new AudioCue { volume = 0.8f, pitch = 0.96f, spatialBlend = 1f };
    [SerializeField] private AudioCue bullChargeCue = new AudioCue { volume = 0.9f, pitch = 1f, spatialBlend = 1f };
    [SerializeField] private AudioCue bullFatigueCue = new AudioCue { volume = 0.75f, pitch = 0.92f, spatialBlend = 1f };
    [SerializeField] private AudioCue bullHurtCue = new AudioCue { volume = 0.85f, pitch = 0.95f, spatialBlend = 1f };
    [SerializeField] private AudioCue bullDeathCue = new AudioCue { volume = 1f, pitch = 0.9f, spatialBlend = 1f };

    [Header("QTE")]
    [SerializeField] private AudioCue timingPerfectCue = new AudioCue { volume = 0.45f, pitch = 1.05f, spatialBlend = 0f };
    [SerializeField] private AudioCue timingGoodCue = new AudioCue { volume = 0.42f, pitch = 1f, spatialBlend = 0f };
    [SerializeField] private AudioCue timingMissCue = new AudioCue { volume = 0.5f, pitch = 0.95f, spatialBlend = 0f };

    private readonly Dictionary<PlaceholderCue, AudioClip> placeholderClips = new Dictionary<PlaceholderCue, AudioClip>();

    private PlayerStats playerStats;
    private BullStats bullStats;
    private BullAI bullAI;
    private BullTimingRing timingRing;

    private PlayerStats subscribedPlayerStats;
    private BullStats subscribedBullStats;
    private BullTimingRing subscribedTimingRing;
    private BullAI trackedBullAI;
    private BullAI.BullState? lastBullState;

    private void Awake()
    {
        ResolveReferencesIfNeeded();
    }

    private void OnEnable()
    {
        ResolveReferencesIfNeeded();
        RefreshBindings();
    }

    private void Start()
    {
        ResolveReferencesIfNeeded();
        RefreshBindings();
    }

    private void Update()
    {
        if (HasMissingReferences())
            ResolveReferencesIfNeeded();

        if (playerAnchor == null && playerStats != null)
            playerAnchor = playerStats.transform;

        if (bullAnchor == null && bullAI != null)
            bullAnchor = bullAI.transform;

        RefreshBindings();
        SyncBullStateAudio();
    }

    private void OnDisable()
    {
        UnbindAll();
    }

    private bool HasMissingReferences()
    {
        return playerStats == null || bullAI == null || bullStats == null || timingRing == null;
    }

    private void ResolveReferencesIfNeeded()
    {
        if (playerStats == null)
            playerStats = BullfightSceneCache.GetLocalOrScene<PlayerStats>(this);

        if (playerAnchor == null)
            playerAnchor = playerStats != null ? playerStats.transform : transform;

        if (bullAI == null)
            bullAI = BullfightSceneCache.FindObject<BullAI>();

        if (bullStats == null)
            bullStats = bullAI != null ? bullAI.GetComponent<BullStats>() : BullfightSceneCache.FindObject<BullStats>();

        if (bullAnchor == null && bullAI != null)
            bullAnchor = bullAI.transform;

        if (timingRing == null)
            timingRing = BullfightSceneCache.FindObject<BullTimingRing>();
    }

    private void RefreshBindings()
    {
        if (subscribedPlayerStats != playerStats)
        {
            if (subscribedPlayerStats != null)
                UnbindPlayer(subscribedPlayerStats);

            subscribedPlayerStats = playerStats;

            if (subscribedPlayerStats != null)
                BindPlayer(subscribedPlayerStats);
        }

        if (subscribedBullStats != bullStats)
        {
            if (subscribedBullStats != null)
                UnbindBull(subscribedBullStats);

            subscribedBullStats = bullStats;

            if (subscribedBullStats != null)
                BindBull(subscribedBullStats);
        }

        if (subscribedTimingRing != timingRing)
        {
            if (subscribedTimingRing != null)
                UnbindTiming(subscribedTimingRing);

            subscribedTimingRing = timingRing;

            if (subscribedTimingRing != null)
                BindTiming(subscribedTimingRing);
        }

        if (trackedBullAI != bullAI)
        {
            trackedBullAI = bullAI;
            lastBullState = trackedBullAI != null ? trackedBullAI.currentState : (BullAI.BullState?)null;
            if (trackedBullAI != null)
                bullAnchor = trackedBullAI.transform;
        }
    }

    private void BindPlayer(PlayerStats stats)
    {
        stats.OnHoldingClothChanged += HandleHoldingClothChanged;
        stats.OnCapaPerformed += HandleCapaPerformed;
        stats.OnEvadePerformed += HandleEvadePerformed;
        stats.OnBanderillasPerformed += HandleBanderillasPerformed;
        stats.OnDamaged += HandlePlayerDamaged;
        stats.OnStunStateChanged += HandleStunStateChanged;
    }

    private void UnbindPlayer(PlayerStats stats)
    {
        stats.OnHoldingClothChanged -= HandleHoldingClothChanged;
        stats.OnCapaPerformed -= HandleCapaPerformed;
        stats.OnEvadePerformed -= HandleEvadePerformed;
        stats.OnBanderillasPerformed -= HandleBanderillasPerformed;
        stats.OnDamaged -= HandlePlayerDamaged;
        stats.OnStunStateChanged -= HandleStunStateChanged;
    }

    private void BindBull(BullStats stats)
    {
        stats.OnDamaged += HandleBullDamaged;
        stats.OnDefeated += HandleBullDefeated;
    }

    private void UnbindBull(BullStats stats)
    {
        stats.OnDamaged -= HandleBullDamaged;
        stats.OnDefeated -= HandleBullDefeated;
    }

    private void BindTiming(BullTimingRing ring)
    {
        ring.OnTimingResolved += HandleTimingResolved;
    }

    private void UnbindTiming(BullTimingRing ring)
    {
        ring.OnTimingResolved -= HandleTimingResolved;
    }

    private void UnbindAll()
    {
        if (subscribedPlayerStats != null)
            UnbindPlayer(subscribedPlayerStats);

        if (subscribedBullStats != null)
            UnbindBull(subscribedBullStats);

        if (subscribedTimingRing != null)
            UnbindTiming(subscribedTimingRing);

        subscribedPlayerStats = null;
        subscribedBullStats = null;
        subscribedTimingRing = null;
    }

    private void SyncBullStateAudio()
    {
        if (trackedBullAI == null || !lastBullState.HasValue)
            return;

        if (trackedBullAI.currentState == lastBullState.Value)
            return;

        BullAI.BullState nextState = trackedBullAI.currentState;
        switch (nextState)
        {
            case BullAI.BullState.Telegraphing:
                PlayCue(bullTelegraphCue, PlaceholderCue.BullTelegraph, bullAnchor);
                break;
            case BullAI.BullState.Charging:
                PlayCue(bullChargeCue, PlaceholderCue.BullCharge, bullAnchor);
                break;
            case BullAI.BullState.Fatigued:
                PlayCue(bullFatigueCue, PlaceholderCue.BullFatigue, bullAnchor);
                break;
        }

        lastBullState = nextState;
    }

    private void HandleHoldingClothChanged(bool isHolding)
    {
        PlayCue(isHolding ? clothRaiseCue : clothLowerCue, isHolding ? PlaceholderCue.ClothRaise : PlaceholderCue.ClothLower, playerAnchor);
    }

    private void HandleCapaPerformed()
    {
        PlayCue(capaCue, PlaceholderCue.Capa, playerAnchor);
    }

    private void HandleEvadePerformed()
    {
        PlayCue(evadeCue, PlaceholderCue.Evade, playerAnchor);
    }

    private void HandleBanderillasPerformed()
    {
        PlayCue(banderillasCue, PlaceholderCue.Banderillas, playerAnchor);
    }

    private void HandlePlayerDamaged(float _)
    {
        PlayCue(playerHitCue, PlaceholderCue.PlayerHit, playerAnchor);
    }

    private void HandleStunStateChanged(bool isStunned)
    {
        PlayCue(isStunned ? playerStunCue : playerRecoverCue, isStunned ? PlaceholderCue.PlayerStun : PlaceholderCue.PlayerRecover, playerAnchor);
    }

    private void HandleBullDamaged(float _)
    {
        PlayCue(bullHurtCue, PlaceholderCue.BullHurt, bullAnchor);
    }

    private void HandleBullDefeated()
    {
        PlayCue(bullDeathCue, PlaceholderCue.BullDeath, bullAnchor);
    }

    private void HandleTimingResolved(string result)
    {
        switch (result)
        {
            case "Perfect!":
                PlayCue(timingPerfectCue, PlaceholderCue.TimingPerfect, null);
                break;
            case "Good":
                PlayCue(timingGoodCue, PlaceholderCue.TimingGood, null);
                break;
            default:
                PlayCue(timingMissCue, PlaceholderCue.TimingMiss, null);
                break;
        }
    }

    private void PlayCue(AudioCue configuredCue, PlaceholderCue fallbackCue, Transform anchor)
    {
        if (masterVolume <= 0f || configuredCue == null)
            return;

        AudioClip clip = configuredCue.clip != null ? configuredCue.clip : GetPlaceholderClip(fallbackCue);
        if (clip == null)
            return;

        Transform parent = anchor != null ? anchor : transform;
        GameObject audioObject = new GameObject("BullfightSfx_" + fallbackCue);
        audioObject.transform.SetParent(parent, false);
        audioObject.transform.localPosition = Vector3.zero;
        audioObject.transform.localRotation = Quaternion.identity;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = clip;
        source.volume = Mathf.Max(0f, configuredCue.volume) * masterVolume;
        source.pitch = Mathf.Approximately(configuredCue.pitch, 0f) ? 1f : configuredCue.pitch;
        source.spatialBlend = Mathf.Clamp01(configuredCue.spatialBlend);
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = 18f;
        source.dopplerLevel = 0f;
        source.Play();

        float lifetime = clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)) + 0.1f;
        Destroy(audioObject, lifetime);
    }

    private AudioClip GetPlaceholderClip(PlaceholderCue cue)
    {
        if (!useProceduralFallback)
            return null;

        AudioClip clip;
        if (placeholderClips.TryGetValue(cue, out clip))
            return clip;

        clip = CreatePlaceholderClip(cue);
        if (clip != null)
            placeholderClips[cue] = clip;

        return clip;
    }

    private static AudioClip CreatePlaceholderClip(PlaceholderCue cue)
    {
        switch (cue)
        {
            case PlaceholderCue.ClothRaise:
                return CreateClip("ClothRaise", 0.16f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.18f, 0.35f);
                    float tone = Sine(time, Mathf.Lerp(420f, 700f, progress));
                    float rustle = Noise(sampleIndex * 5) * 0.3f;
                    return (tone * 0.1f + rustle * 0.03f) * envelope;
                });
            case PlaceholderCue.ClothLower:
                return CreateClip("ClothLower", 0.16f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.15f, 0.45f);
                    float tone = Sine(time, Mathf.Lerp(650f, 360f, progress));
                    float rustle = Noise(sampleIndex * 7) * 0.25f;
                    return (tone * 0.08f + rustle * 0.03f) * envelope;
                });
            case PlaceholderCue.Capa:
                return CreateClip("Capa", 0.22f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.08f, 0.22f);
                    float swoosh = Noise(sampleIndex * 11) * 0.08f;
                    float tone = Sine(time, Mathf.Lerp(520f, 240f, progress)) * 0.05f;
                    return (swoosh + tone) * envelope;
                });
            case PlaceholderCue.Evade:
                return CreateClip("Evade", 0.14f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.04f, 0.25f);
                    float chirp = Sine(time, Mathf.Lerp(260f, 980f, progress)) * 0.12f;
                    float hiss = Noise(sampleIndex * 13) * 0.02f;
                    return (chirp + hiss) * envelope;
                });
            case PlaceholderCue.Banderillas:
                return CreateClip("Banderillas", 0.18f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.02f, 0.14f);
                    float stab = Square(time, Mathf.Lerp(170f, 130f, progress)) * 0.08f;
                    float crack = Noise(sampleIndex * 17) * 0.06f;
                    return (stab + crack) * envelope;
                });
            case PlaceholderCue.PlayerHit:
                return CreateClip("PlayerHit", 0.22f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.01f, 0.4f);
                    float body = Sine(time, Mathf.Lerp(180f, 85f, progress)) * 0.12f;
                    float impact = Noise(sampleIndex * 19) * 0.07f;
                    return (body + impact) * envelope;
                });
            case PlaceholderCue.PlayerStun:
                return CreateClip("PlayerStun", 0.45f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.05f, 0.25f);
                    float wobble = Mathf.Lerp(150f, 210f, 0.5f + 0.5f * Mathf.Sin(progress * Mathf.PI * 6f));
                    float tone = Sine(time, wobble) * 0.08f;
                    float haze = Noise(sampleIndex * 23) * 0.03f;
                    return (tone + haze) * envelope;
                });
            case PlaceholderCue.PlayerRecover:
                return CreateClip("PlayerRecover", 0.2f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.05f, 0.3f);
                    float toneA = Sine(time, Mathf.Lerp(240f, 420f, progress)) * 0.08f;
                    float toneB = Sine(time, Mathf.Lerp(360f, 560f, progress)) * 0.04f;
                    return (toneA + toneB) * envelope;
                });
            case PlaceholderCue.BullTelegraph:
                return CreateClip("BullTelegraph", 0.38f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.06f, 0.2f);
                    float growl = Sine(time, Mathf.Lerp(95f, 72f, progress)) * 0.14f;
                    float rasp = Noise(sampleIndex * 29) * 0.04f;
                    return (growl + rasp) * envelope;
                });
            case PlaceholderCue.BullCharge:
                return CreateClip("BullCharge", 0.34f, delegate(int sampleIndex, float time, float progress)
                {
                    float pulse = Mathf.Clamp01(Mathf.Sin(progress * Mathf.PI * 4f));
                    float envelope = Envelope(progress, 0.03f, 0.15f) * (0.6f + pulse * 0.4f);
                    float thump = Sine(time, Mathf.Lerp(80f, 58f, progress)) * 0.14f;
                    float grit = Noise(sampleIndex * 31) * 0.05f;
                    return (thump + grit) * envelope;
                });
            case PlaceholderCue.BullFatigue:
                return CreateClip("BullFatigue", 0.48f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.04f, 0.25f);
                    float breath = Noise(sampleIndex * 37) * 0.08f;
                    float low = Sine(time, Mathf.Lerp(120f, 80f, progress)) * 0.05f;
                    return (breath + low) * envelope;
                });
            case PlaceholderCue.BullHurt:
                return CreateClip("BullHurt", 0.28f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.02f, 0.18f);
                    float cry = Sine(time, Mathf.Lerp(160f, 110f, progress)) * 0.12f;
                    float snap = Noise(sampleIndex * 41) * 0.05f;
                    return (cry + snap) * envelope;
                });
            case PlaceholderCue.BullDeath:
                return CreateClip("BullDeath", 0.82f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.02f, 0.2f);
                    float tone = Sine(time, Mathf.Lerp(150f, 48f, progress)) * 0.12f;
                    float rumble = Sine(time, Mathf.Lerp(80f, 35f, progress)) * 0.08f;
                    float breath = Noise(sampleIndex * 43) * 0.03f;
                    return (tone + rumble + breath) * envelope;
                });
            case PlaceholderCue.TimingPerfect:
                return CreateClip("TimingPerfect", 0.18f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.02f, 0.35f);
                    float noteA = Sine(time, 880f) * 0.08f;
                    float noteB = Sine(time, 1320f) * 0.04f;
                    return (noteA + noteB) * envelope;
                });
            case PlaceholderCue.TimingGood:
                return CreateClip("TimingGood", 0.15f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.02f, 0.3f);
                    float tone = Sine(time, Mathf.Lerp(620f, 780f, progress)) * 0.08f;
                    return tone * envelope;
                });
            case PlaceholderCue.TimingMiss:
                return CreateClip("TimingMiss", 0.18f, delegate(int sampleIndex, float time, float progress)
                {
                    float envelope = Envelope(progress, 0.01f, 0.28f);
                    float buzz = Square(time, Mathf.Lerp(220f, 150f, progress)) * 0.06f;
                    float grit = Noise(sampleIndex * 47) * 0.03f;
                    return (buzz + grit) * envelope;
                });
            default:
                return null;
        }
    }

    private static AudioClip CreateClip(string name, float duration, ProceduralSample sampler)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(duration * sampleRate));
        float[] data = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            float progress = sampleCount == 1 ? 0f : i / (float)(sampleCount - 1);
            data[i] = Mathf.Clamp(sampler(i, time, progress), -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static float Envelope(float progress, float attack, float release)
    {
        float fadeIn = attack <= 0f ? 1f : Mathf.Clamp01(progress / attack);
        float fadeOut = release <= 0f ? 1f : Mathf.Clamp01((1f - progress) / release);
        fadeIn = fadeIn * fadeIn * (3f - 2f * fadeIn);
        fadeOut = fadeOut * fadeOut * (3f - 2f * fadeOut);
        return fadeIn * fadeOut;
    }

    private static float Sine(float time, float frequency)
    {
        return Mathf.Sin(time * frequency * Mathf.PI * 2f);
    }

    private static float Square(float time, float frequency)
    {
        return Mathf.Sign(Sine(time, frequency));
    }

    private static float Noise(int seed)
    {
        int value = (seed << 13) ^ seed;
        return 1f - ((value * (value * value * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824f;
    }
}
