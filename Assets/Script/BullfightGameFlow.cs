using InfimaGames.LowPolyShooterPack;
using UnityEngine;

public class BullfightGameFlow : MonoBehaviour
{
    public enum GamePhase
    {
        PhaseOne,
        PhaseTwo,
        Ending
    }

    public enum PhaseTwoState
    {
        None,
        Intro,
        Calibration,
        Standoff,
        RoundPrepare,
        RoundWindow,
        RoundResolve
    }

    public enum EndingType
    {
        None,
        Glory,
        Tragedy,
        Mercy
    }

    [Header("References")]
    public PlayerStats playerStats;
    public BullStats bullStats;
    public BullAI bullAI;
    public BullfightPlayerController playerController;
    public BullTimingRing timingRing;
    public BullfightSpawnManager spawnManager;
    public BullfightPhaseTwoPresentation phaseTwoPresentation;

    [Header("Phase Two")]
    public float phaseTwoTimeScale = 0.75f;
    public float mercyEndingDelay = 15f;
    public float introDuration = 2.8f;
    public float introQuestionDelay = 0.75f;
    public float calibrationHoldDuration = 1f;
    public float roundStanceConfirmDuration = 0.7f;
    public float roundPrepareDuration = 0.55f;
    public float roundWindowDuration = 1.35f;
    public float roundResolveDuration = 0.85f;
    public float interRoundFaceoffDuration = 0.75f;
    public int maxRounds = 5;
    public int winsToFinish = 3;
    public float perfectTimingEase = 0.08f;
    public float perfectTelegraphBonus = 0.2f;
    public float phaseTwoBullFrontDistance = 2.2f;
    public float phaseTwoBullSideOffset = 0f;
    public float phaseTwoAmbientIntensity = 0.65f;
    public float phaseTwoDirectionalLightMultiplier = 0.8f;
    [TextArea(2, 3)] public string[] phaseTwoReflectionLines =
    {
        "你眼前的，真的是敵人嗎？",
        "如果牠只是想活下去，誰又先舉起了武器？",
        "觀眾想看的，是勝利，還是鮮血？",
        "這一擊若成功，你真的會比較輕鬆嗎？",
        "如果你們都不出手，誰才有資格說這場決鬥必須繼續？"
    };

    [Header("Debug Shortcuts")]
    public bool enableDebugShortcuts = true;
    public KeyCode debugRefillStaminaKey = KeyCode.Alpha7;
    public KeyCode debugPhaseTwoKey = KeyCode.Alpha8;
    public KeyCode debugKillBullKey = KeyCode.Alpha9;
    public KeyCode debugKillPlayerKey = KeyCode.Alpha0;
    public float debugPhaseTwoDamage = 200f;
    public float bullDeathEndingDelay = 1.5f;

    public GamePhase currentPhase = GamePhase.PhaseOne;
    public EndingType currentEnding = EndingType.None;

    public PhaseTwoState CurrentPhaseTwoState => phaseTwoState;
    public int PhaseTwoRoundIndex => phaseTwoRoundIndex;
    public int PhaseTwoBullHitCount => bullHitCount;
    public int PhaseTwoPlayerHitCount => playerHitCount;
    public int PhaseTwoMaxRounds => Mathf.Max(1, maxRounds);
    public int PhaseTwoWinsToFinish => Mathf.Max(1, winsToFinish);
    public bool IsPhaseTwoCalibrated => phaseTwoCalibrated;
    public bool CurrentRoundHasPerfectAdvantage => currentRoundHasPerfectAdvantage;
    public bool NextRoundHasPerfectAdvantage => nextRoundHasPerfectAdvantage;
    public string LastPhaseTwoResult => lastPhaseTwoResult;
    public float PhaseTwoCalibrationProgress => calibrationHoldDuration <= 0f ? 1f : Mathf.Clamp01(calibrationHoldTimer / calibrationHoldDuration);
    public float PhaseTwoRoundStanceProgress => GetRoundStanceProgress();
    public float PhaseTwoMercyTimeRemaining => Mathf.Max(0f, mercyEndingDelay - mercyTimer);
    public bool IsPhaseTwoRoundStanceConfirming => currentPhase == GamePhase.PhaseTwo &&
                                                   phaseTwoState == PhaseTwoState.RoundPrepare &&
                                                   phaseTwoStateElapsed < GetRoundStanceDuration();
    public string CurrentPhaseTwoReflectionLine => GetPhaseTwoReflectionLine();
    public bool IsPhaseTwoQuestionVisible => currentPhase == GamePhase.PhaseTwo && phaseTwoState == PhaseTwoState.Intro && phaseTwoStateElapsed >= introQuestionDelay;

    private PhaseTwoState phaseTwoState = PhaseTwoState.None;
    private float mercyTimer;
    private float bullDeathTimer;
    private float phaseTwoStateElapsed;
    private float calibrationHoldTimer;
    private float activeRoundWindowDuration;
    private int bullHitCount;
    private int playerHitCount;
    private int phaseTwoRoundIndex;
    private bool phaseTwoCalibrated;
    private bool phaseTwoHasCommittedAttack;
    private bool nextRoundHasPerfectAdvantage;
    private bool currentRoundHasPerfectAdvantage;
    private bool phaseTwoPrepareTelegraphStarted;
    private bool phaseTwoResolveBullResetApplied;
    private string lastPhaseTwoResult = string.Empty;

    private void Awake()
    {
        ResetState();

        if (FindObjectOfType<BullfightStartMenu>(true) == null)
        {
            GameObject startMenuObject = new("BullfightStartMenu");
            _ = startMenuObject.AddComponent<BullfightStartMenu>();
        }

        AutoAssignReferences();

        if (bullAI != null)
            bullAI.enabled = true;
    }

    private void Start()
    {
        ResetState();
        AutoAssignReferences();
    }

    private void Update()
    {
        if (currentPhase == GamePhase.Ending)
            return;

        AutoAssignReferences();
        HandleDebugShortcuts();

        if (playerStats != null && playerStats.IsDead)
        {
            SetEnding(EndingType.Tragedy);
            return;
        }

        if (bullStats != null && bullStats.currentHealth <= 0f)
        {
            if (bullAI != null)
                bullAI.ForceDeathState();

            bullDeathTimer += Time.unscaledDeltaTime;

            if (bullDeathTimer >= bullDeathEndingDelay)
                SetEnding(EndingType.Glory);

            return;
        }

        bullDeathTimer = 0f;

        switch (currentPhase)
        {
            case GamePhase.PhaseOne:
                UpdatePhaseOne();
                break;
            case GamePhase.PhaseTwo:
                UpdatePhaseTwo();
                break;
        }
    }

    public void RegisterStabResult(string result)
    {
        if (currentPhase != GamePhase.PhaseTwo || phaseTwoState != PhaseTwoState.RoundWindow)
            return;

        HandlePhaseTwoStabResult(result);
    }

    public float GetPhaseTwoBullHealthNormalized()
    {
        return 1f - (bullHitCount / (float)Mathf.Max(1, winsToFinish));
    }

    public float GetPhaseTwoPlayerHealthNormalized()
    {
        return 1f - (playerHitCount / (float)Mathf.Max(1, winsToFinish));
    }

    public bool ShouldShowPhaseTwoOverlay()
    {
        return currentPhase == GamePhase.PhaseTwo && currentEnding == EndingType.None;
    }

    private void UpdatePhaseOne()
    {
        if (bullStats != null && bullStats.InFinalPhase)
            EnterPhaseTwo();
    }

    private void UpdatePhaseTwo()
    {
        if (playerStats != null)
            playerStats.SetHoldingCloth(false);

        switch (phaseTwoState)
        {
            case PhaseTwoState.Intro:
                UpdatePhaseTwoIntro();
                break;
            case PhaseTwoState.Calibration:
                UpdateCalibration();
                break;
            case PhaseTwoState.Standoff:
                UpdateStandoff();
                break;
            case PhaseTwoState.RoundPrepare:
                UpdateRoundPrepare();
                break;
            case PhaseTwoState.RoundWindow:
                UpdateRoundWindow();
                break;
            case PhaseTwoState.RoundResolve:
                UpdateRoundResolve();
                break;
        }
    }

    private void UpdatePhaseTwoIntro()
    {
        phaseTwoStateElapsed += Time.unscaledDeltaTime;
        if (phaseTwoStateElapsed >= introDuration)
            EnterCalibration();
    }

    private void UpdateCalibration()
    {
        bool calibrating = playerController != null && playerController.IsPhaseTwoCalibrationHeld();
        calibrationHoldTimer = calibrating ? calibrationHoldTimer + Time.unscaledDeltaTime : 0f;

        if (calibrationHoldTimer < calibrationHoldDuration)
            return;

        phaseTwoCalibrated = true;
        mercyTimer = 0f;
        calibrationHoldTimer = calibrationHoldDuration;
        lastPhaseTwoResult = string.Empty;
        EnterStandoff();
    }

    private void UpdateStandoff()
    {
        phaseTwoStateElapsed += Time.unscaledDeltaTime;

        if (playerController != null && playerController.IsPhaseTwoStabPressedThisFrame())
        {
            phaseTwoHasCommittedAttack = true;
            mercyTimer = 0f;
            StartNextPhaseTwoRound();
            return;
        }

        mercyTimer += Time.unscaledDeltaTime;
        if (mercyTimer >= mercyEndingDelay)
            SetEnding(EndingType.Mercy);
    }

    private void UpdateRoundPrepare()
    {
        phaseTwoStateElapsed += Time.unscaledDeltaTime;

        if (!phaseTwoPrepareTelegraphStarted && phaseTwoStateElapsed >= GetRoundStanceDuration())
        {
            phaseTwoPrepareTelegraphStarted = true;
            bullAI?.PlayPhaseTwoTelegraph();
        }

        if (phaseTwoStateElapsed >= GetActiveRoundPrepareDuration())
            BeginPhaseTwoAttackWindow();
    }

    private void UpdateRoundWindow()
    {
        phaseTwoStateElapsed += Time.unscaledDeltaTime;

        if (timingRing != null)
            timingRing.SetTelegraphProgress(phaseTwoStateElapsed / Mathf.Max(0.01f, activeRoundWindowDuration));

        if (phaseTwoStateElapsed < activeRoundWindowDuration)
            return;

        if (!phaseTwoHasCommittedAttack)
        {
            if (timingRing != null)
            {
                timingRing.HideImmediate();
                timingRing.ResetTimingWindow();
            }

            lastPhaseTwoResult = string.Empty;
            phaseTwoState = PhaseTwoState.RoundPrepare;
            phaseTwoStateElapsed = 0f;
            return;
        }

        if (timingRing != null && timingRing.IsActive)
        {
            timingRing.ResolveExternal("Miss");
            return;
        }

        HandlePhaseTwoStabResult("Miss");
    }

    private void UpdateRoundResolve()
    {
        phaseTwoStateElapsed += Time.unscaledDeltaTime;

        if (!phaseTwoResolveBullResetApplied && phaseTwoStateElapsed >= GetRoundResolveBullResetDelay())
        {
            phaseTwoResolveBullResetApplied = true;
            ResetBullForPhaseTwoRound();
            bullAI?.PlayPhaseTwoRoundResetIdle();
        }

        if (phaseTwoStateElapsed < GetActiveRoundResolveDuration())
            return;

        if (CheckPhaseTwoVictory())
            return;

        StartNextPhaseTwoRound();
    }

    private void HandlePhaseTwoStabResult(string result)
    {
        if (currentPhase != GamePhase.PhaseTwo || phaseTwoState != PhaseTwoState.RoundWindow)
            return;

        lastPhaseTwoResult = result;
        mercyTimer = 0f;
        phaseTwoHasCommittedAttack = true;

        bool isSuccess = result == "Perfect!" || result == "Good";

        if (isSuccess)
        {
            bullHitCount++;
            if (result == "Perfect!")
                nextRoundHasPerfectAdvantage = true;

            bullAI?.PlayPhaseTwoHitReaction(result == "Perfect!");
        }
        else
        {
            playerHitCount++;
            playerStats?.TakeDamage(0f);
            bullAI?.PlayPhaseTwoAttackFollowThrough();
        }

        if (timingRing != null)
        {
            timingRing.HideImmediate();
            timingRing.ResetTimingWindow();
        }

        if (CheckPhaseTwoVictory())
            return;

        phaseTwoState = PhaseTwoState.RoundResolve;
        phaseTwoStateElapsed = 0f;
        phaseTwoResolveBullResetApplied = false;
    }

    private bool CheckPhaseTwoVictory()
    {
        if (bullHitCount >= Mathf.Max(1, winsToFinish))
        {
            if (bullAI != null)
                bullAI.ForceDeathState();

            if (bullStats != null && bullStats.currentHealth > 0f)
            {
                bullStats.SetHealth(0f);
                return true;
            }

            SetEnding(EndingType.Glory);
            return true;
        }

        if (playerHitCount >= Mathf.Max(1, winsToFinish))
        {
            SetEnding(EndingType.Tragedy);
            return true;
        }

        if (phaseTwoRoundIndex >= Mathf.Max(1, maxRounds))
        {
            SetEnding(bullHitCount > playerHitCount ? EndingType.Glory : EndingType.Tragedy);
            return true;
        }

        return false;
    }

    private void StartNextPhaseTwoRound()
    {
        if (currentPhase != GamePhase.PhaseTwo || CheckPhaseTwoVictory())
            return;

        phaseTwoRoundIndex++;
        phaseTwoState = PhaseTwoState.RoundPrepare;
        phaseTwoStateElapsed = 0f;
        activeRoundWindowDuration = roundWindowDuration;
        lastPhaseTwoResult = string.Empty;
        currentRoundHasPerfectAdvantage = nextRoundHasPerfectAdvantage;
        nextRoundHasPerfectAdvantage = false;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;

        if (currentRoundHasPerfectAdvantage)
            activeRoundWindowDuration += perfectTelegraphBonus;

        if (timingRing != null)
        {
            timingRing.HideImmediate();
            timingRing.ResetTimingWindow();
        }

        playerController?.ClearInputBuffers();
        bullAI?.PlayPhaseTwoStanceConfirm();
    }

    private void BeginPhaseTwoAttackWindow()
    {
        phaseTwoState = PhaseTwoState.RoundWindow;
        phaseTwoStateElapsed = 0f;

        if (!phaseTwoPrepareTelegraphStarted)
        {
            phaseTwoPrepareTelegraphStarted = true;
            bullAI?.PlayPhaseTwoTelegraph();
        }

        if (timingRing == null)
            return;

        timingRing.ResetTimingWindow();
        if (currentRoundHasPerfectAdvantage)
        {
            float adjustedGood = timingRing.DefaultGoodProgress - perfectTimingEase;
            float adjustedPerfect = timingRing.DefaultPerfectProgress - perfectTimingEase;
            timingRing.SetTimingWindow(adjustedGood, adjustedPerfect);
        }

        timingRing.StartTiming(BullTimingRing.TimingMode.Estocada, HandlePhaseTwoStabResult);
        timingRing.SetTelegraphProgress(0f);
    }

    private void EnterPhaseTwo()
    {
        currentPhase = GamePhase.PhaseTwo;
        currentEnding = EndingType.None;
        phaseTwoState = PhaseTwoState.Intro;
        phaseTwoStateElapsed = 0f;
        calibrationHoldTimer = 0f;
        phaseTwoCalibrated = false;
        phaseTwoHasCommittedAttack = false;
        mercyTimer = 0f;
        phaseTwoRoundIndex = 0;
        bullHitCount = 0;
        playerHitCount = 0;
        activeRoundWindowDuration = roundWindowDuration;
        nextRoundHasPerfectAdvantage = false;
        currentRoundHasPerfectAdvantage = false;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;
        lastPhaseTwoResult = string.Empty;
        Time.timeScale = phaseTwoTimeScale;

        if (bullAI != null)
            bullAI.enabled = false;

        ResetBullForPhaseTwoRound();
        bullAI?.PlayPhaseTwoStandoffIdle();

        if (playerStats != null)
        {
            playerStats.SetHoldingCloth(false);
            playerStats.SetShooterGameplayEnabled(false);
        }

        playerController?.ClearInputBuffers();

        if (timingRing != null)
        {
            timingRing.HideImmediate();
            timingRing.ResetTimingWindow();
        }

        phaseTwoPresentation?.EnterPhaseTwo(phaseTwoAmbientIntensity, phaseTwoDirectionalLightMultiplier);

        Debug.Log("Phase 2 intro started. Press G to calibrate, then use E to stab.");
    }

    private void EnterCalibration()
    {
        phaseTwoState = PhaseTwoState.Calibration;
        phaseTwoStateElapsed = 0f;
        calibrationHoldTimer = 0f;
        phaseTwoHasCommittedAttack = false;
        currentRoundHasPerfectAdvantage = false;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;
        lastPhaseTwoResult = string.Empty;
        bullAI?.PlayPhaseTwoStandoffIdle();
    }

    private void EnterStandoff()
    {
        phaseTwoState = PhaseTwoState.Standoff;
        phaseTwoStateElapsed = 0f;
        mercyTimer = 0f;
        phaseTwoHasCommittedAttack = false;
        currentRoundHasPerfectAdvantage = false;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;
        lastPhaseTwoResult = string.Empty;
        ResetBullForPhaseTwoRound();
        bullAI?.PlayPhaseTwoStandoffIdle();

        if (timingRing != null)
        {
            timingRing.HideImmediate();
            timingRing.ResetTimingWindow();
        }

        playerController?.ClearInputBuffers();
    }

    private void HandleDebugShortcuts()
    {
        if (!enableDebugShortcuts)
            return;

        if (Input.GetKeyDown(debugPhaseTwoKey) && bullStats != null)
        {
            bullStats.ApplyDebugDamage(debugPhaseTwoDamage);
            Debug.Log($"Debug shortcut: bull damaged for {debugPhaseTwoDamage}. Current health: {bullStats.currentHealth}");
        }

        if (Input.GetKeyDown(debugRefillStaminaKey) && playerStats != null)
            playerStats.RefillStaminaForDebug();

        if (Input.GetKeyDown(debugKillBullKey) && bullStats != null)
        {
            bullStats.SetHealth(0f);
            Debug.Log("Debug shortcut: bull health forced to 0.");
        }

        if (Input.GetKeyDown(debugKillPlayerKey) && playerStats != null)
            playerStats.ForceDeathForDebug();
    }

    private void SetEnding(EndingType ending)
    {
        if (currentPhase == GamePhase.Ending)
            return;

        currentPhase = GamePhase.Ending;
        currentEnding = ending;
        phaseTwoState = PhaseTwoState.None;
        Time.timeScale = 1f;
        phaseTwoPresentation?.ExitPhaseTwo();

        if (timingRing != null)
        {
            timingRing.HideImmediate();
            timingRing.ResetTimingWindow();
        }

        Debug.Log($"Ending reached: {ending}");
    }

    private void ResetState()
    {
        currentPhase = GamePhase.PhaseOne;
        currentEnding = EndingType.None;
        phaseTwoState = PhaseTwoState.None;
        mercyTimer = 0f;
        bullDeathTimer = 0f;
        phaseTwoStateElapsed = 0f;
        calibrationHoldTimer = 0f;
        activeRoundWindowDuration = roundWindowDuration;
        phaseTwoRoundIndex = 0;
        bullHitCount = 0;
        playerHitCount = 0;
        phaseTwoCalibrated = false;
        phaseTwoHasCommittedAttack = false;
        nextRoundHasPerfectAdvantage = false;
        currentRoundHasPerfectAdvantage = false;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;
        lastPhaseTwoResult = string.Empty;
        phaseTwoPresentation?.ExitPhaseTwo();
        Time.timeScale = 1f;
    }

    private void AutoAssignReferences()
    {
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>(true);

        if (bullStats == null)
            bullStats = FindObjectOfType<BullStats>(true);

        if (bullAI == null)
            bullAI = FindObjectOfType<BullAI>(true);

        if (playerController == null)
            playerController = FindObjectOfType<BullfightPlayerController>(true);

        if (timingRing == null)
            timingRing = FindObjectOfType<BullTimingRing>(true);

        if (spawnManager == null && playerStats != null)
            spawnManager = playerStats.GetComponent<BullfightSpawnManager>();

        if (spawnManager == null)
            spawnManager = FindObjectOfType<BullfightSpawnManager>(true);

        if (phaseTwoPresentation == null)
            phaseTwoPresentation = GetComponent<BullfightPhaseTwoPresentation>() ?? FindObjectOfType<BullfightPhaseTwoPresentation>(true);

        if (phaseTwoPresentation == null)
            phaseTwoPresentation = gameObject.AddComponent<BullfightPhaseTwoPresentation>();
    }

    private void ResetBullForPhaseTwoRound()
    {
        spawnManager?.ResetBullForPhaseTwoRound(phaseTwoBullFrontDistance, phaseTwoBullSideOffset);
    }

    private float GetActiveRoundPrepareDuration()
    {
        return Mathf.Max(roundPrepareDuration, 0.95f) + GetRoundStanceDuration();
    }

    private float GetActiveRoundResolveDuration()
    {
        return Mathf.Max(roundResolveDuration, 0.9f) + Mathf.Max(0f, interRoundFaceoffDuration);
    }

    private float GetRoundResolveBullResetDelay()
    {
        return Mathf.Min(0.42f, GetActiveRoundResolveDuration() * 0.45f);
    }

    private float GetRoundStanceDuration()
    {
        return Mathf.Max(0f, roundStanceConfirmDuration);
    }

    private float GetRoundStanceProgress()
    {
        if (phaseTwoState != PhaseTwoState.RoundPrepare)
            return 0f;

        float stanceDuration = GetRoundStanceDuration();
        if (stanceDuration <= 0f)
            return 1f;

        return Mathf.Clamp01(phaseTwoStateElapsed / stanceDuration);
    }

    private string GetPhaseTwoReflectionLine()
    {
        if (phaseTwoReflectionLines == null || phaseTwoReflectionLines.Length == 0)
            return string.Empty;

        int clampedIndex = Mathf.Clamp(Mathf.Max(phaseTwoRoundIndex, 1) - 1, 0, phaseTwoReflectionLines.Length - 1);
        return phaseTwoReflectionLines[clampedIndex] ?? string.Empty;
    }

    private void OnDestroy()
    {
        phaseTwoPresentation?.ExitPhaseTwo();
        Time.timeScale = 1f;
    }
}
