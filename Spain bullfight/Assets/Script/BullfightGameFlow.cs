using InfimaGames.LowPolyShooterPack;
using UnityEngine;

public class BullfightGameFlow : MonoBehaviour
{
    public enum GamePhase
    {
        PhaseZeroTutorial,
        PhaseOne,
        PhaseTwo,
        Ending
    }

    public enum TutorialState
    {
        None,
        Intro,
        HoldCloth,
        Capa,
        Dash,
        Attack,
        Complete
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
        "The arena grows quieter each round.",
        "There has to be more than one ending.",
        "Steel settles arguments too quickly.",
        "Stand your ground before you strike.",
        "Only one of you leaves this ritual unchanged."
    };

    [Header("Tutorial")]
    public float tutorialIntroDuration = 2.25f;
    public float tutorialHoldDuration = 0.5f;
    public float tutorialTransitionDelay = 0.9f;
    public float tutorialCompleteDelay = 1.5f;
    public float tutorialCapaWindowDuration = 1.35f;
    public float tutorialChargeFrontDistance = 3.6f;
    public float tutorialDashFrontDistance = 3.4f;
    public float tutorialAttackFrontDistance = 3f;
    public float tutorialAttackResolveDelay = 1.1f;
    public float tutorialBullSideOffset = 0f;

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
    public TutorialState CurrentTutorialState => tutorialState;
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
    public bool IsPhaseTwoQuestionVisible => currentPhase == GamePhase.PhaseTwo &&
                                            phaseTwoState == PhaseTwoState.Intro &&
                                            phaseTwoStateElapsed >= introQuestionDelay;
    public bool IsTutorialActive => currentPhase == GamePhase.PhaseZeroTutorial;
    public bool IsTutorialCapaStepActive => currentPhase == GamePhase.PhaseZeroTutorial && tutorialState == TutorialState.Capa;
    public bool IsTutorialAttackStepActive => currentPhase == GamePhase.PhaseZeroTutorial && tutorialState == TutorialState.Attack;
    public string CurrentTutorialTitle => GetTutorialTitle();
    public string CurrentTutorialInstruction => GetTutorialInstruction();
    public string CurrentTutorialStatus => GetTutorialStatus();

    private PhaseTwoState phaseTwoState = PhaseTwoState.None;
    private TutorialState tutorialState = TutorialState.None;
    private TutorialState queuedTutorialState = TutorialState.None;
    private float mercyTimer;
    private float bullDeathTimer;
    private float phaseTwoStateElapsed;
    private float calibrationHoldTimer;
    private float activeRoundWindowDuration;
    private float tutorialStateElapsed;
    private float tutorialHoldTimer;
    private float tutorialTransitionTimer;
    private float tutorialCapaWindowTimer;
    private float tutorialDashStepStartedAt = -999f;
    private float tutorialAttackResolveAt = -1f;
    private float tutorialAttackHealthAtStepStart;
    private int bullHitCount;
    private int playerHitCount;
    private int phaseTwoRoundIndex;
    private bool phaseTwoCalibrated;
    private bool phaseTwoHasCommittedAttack;
    private bool nextRoundHasPerfectAdvantage;
    private bool currentRoundHasPerfectAdvantage;
    private bool phaseTwoPrepareTelegraphStarted;
    private bool phaseTwoResolveBullResetApplied;
    private bool tutorialChargeStarted;
    private bool tutorialCapaQteStarted;
    private bool tutorialCapaQteResolved;
    private bool tutorialAttackPerformed;
    private bool tutorialAttackCleanHitRegistered;
    private string lastPhaseTwoResult = string.Empty;
    private string tutorialFeedbackText = string.Empty;
    private PlayerStats subscribedTutorialPlayerStats;
    private BullAI subscribedTutorialBullAI;

    private void Awake()
    {
        ResetState();

        if (FindObjectOfType<BullfightStartMenu>(true) == null)
        {
            GameObject startMenuObject = new("BullfightStartMenu");
            _ = startMenuObject.AddComponent<BullfightStartMenu>();
        }

        ResolveReferencesIfNeeded();
        RefreshTutorialSubscriptions();

        if (bullAI != null)
            bullAI.enabled = true;
    }

    private void Start()
    {
        ResetState();
        ResolveReferencesIfNeeded();
        RefreshTutorialSubscriptions();
    }

    private void Update()
    {
        if (HasMissingReferences())
        {
            ResolveReferencesIfNeeded();
            RefreshTutorialSubscriptions();
        }

        HandleDebugShortcuts();

        if (currentPhase == GamePhase.PhaseZeroTutorial)
        {
            UpdateTutorial();
            return;
        }

        if (currentPhase == GamePhase.Ending)
            return;

        if (playerStats != null && playerStats.IsDead)
        {
            SetEnding(EndingType.Tragedy);
            return;
        }

        if (bullStats != null && bullStats.currentHealth <= 0f)
        {
            bullAI?.ForceDeathState();
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

    public void BeginTutorial()
    {
        ResolveReferencesIfNeeded();
        ResetState();
        PrepareEncounterForTutorial(tutorialChargeFrontDistance);
        tutorialFeedbackText = string.Empty;
        EnterTutorialState(TutorialState.Intro);
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

    public bool ShouldShowTutorialOverlay()
    {
        return currentPhase == GamePhase.PhaseZeroTutorial && currentEnding == EndingType.None;
    }

    private void UpdateTutorial()
    {
        if (tutorialTransitionTimer > 0f)
        {
            tutorialTransitionTimer = Mathf.Max(0f, tutorialTransitionTimer - Time.unscaledDeltaTime);
            if (tutorialTransitionTimer <= 0f)
                EnterTutorialState(queuedTutorialState);
            return;
        }

        tutorialStateElapsed += Time.unscaledDeltaTime;

        switch (tutorialState)
        {
            case TutorialState.Intro:
                if (tutorialStateElapsed >= tutorialIntroDuration)
                    EnterTutorialState(TutorialState.HoldCloth);
                break;
            case TutorialState.HoldCloth:
                UpdateTutorialHoldCloth();
                break;
            case TutorialState.Capa:
                UpdateTutorialCapa();
                break;
            case TutorialState.Dash:
                UpdateTutorialDash();
                break;
            case TutorialState.Attack:
                UpdateTutorialAttack();
                break;
            case TutorialState.Complete:
                if (tutorialStateElapsed >= tutorialCompleteDelay)
                    StartPhaseOneFromTutorial();
                break;
        }
    }

    private void UpdateTutorialHoldCloth()
    {
        if (playerStats == null)
            return;

        tutorialHoldTimer = playerStats.isHoldingCloth
            ? tutorialHoldTimer + Time.unscaledDeltaTime
            : 0f;

        if (tutorialHoldTimer >= tutorialHoldDuration)
            QueueTutorialState(TutorialState.Capa, 0.4f, "\u5f88\u597d\uff0c\u63a5\u4e0b\u4f86\u7528\u7d05\u5e03\u628a\u725b\u7684\u885d\u649e\u5e36\u958b\u3002");
    }

    private void UpdateTutorialCapa()
    {
        if (!tutorialChargeStarted)
        {
            PrepareEncounterForTutorial(tutorialChargeFrontDistance);
            tutorialChargeStarted = true;
            bullAI?.StartTutorialCharge(true, false);
        }

        if (!tutorialCapaQteStarted &&
            bullAI != null &&
            bullAI.currentState == BullAI.BullState.Charging)
        {
            tutorialCapaQteStarted = true;
            tutorialCapaQteResolved = false;
            tutorialCapaWindowTimer = 0f;
            timingRing?.HideImmediate();
            timingRing?.ResetTimingWindow();
            timingRing?.StartTiming(BullTimingRing.TimingMode.Capa, HandleTutorialCapaTimingResult);
            timingRing?.SetTelegraphProgress(0f);
        }

        if (tutorialCapaQteStarted && !tutorialCapaQteResolved)
        {
            tutorialCapaWindowTimer += Time.unscaledDeltaTime;
            float progress = tutorialCapaWindowDuration <= 0.0001f
                ? 1f
                : Mathf.Clamp01(tutorialCapaWindowTimer / tutorialCapaWindowDuration);
            timingRing?.SetTelegraphProgress(progress);

            if (tutorialCapaWindowTimer >= tutorialCapaWindowDuration && timingRing != null && timingRing.IsActive)
                timingRing.ResolveExternal("Miss");
        }

        if (bullAI == null || !bullAI.IsTutorialChargeSequenceComplete)
            return;

        bool success = bullAI.TutorialChargeResult == "Good" || bullAI.TutorialChargeResult == "Perfect!";
        if (success)
        {
            string message = bullAI.TutorialChargeResult == "Perfect!"
                ? "\u63ee\u5e03\u5b8c\u6210\uff0c\u9019\u6b21\u5f88\u4f86\u5f97\u5e72\u6de8\u3002"
                : "\u63ee\u5e03\u6210\u529f\uff0c\u4f60\u5df2\u7d93\u628a\u885d\u649e\u5e36\u958b\u4e86\u3002";
            QueueTutorialState(TutorialState.Dash, tutorialTransitionDelay, message);
            return;
        }

        QueueTutorialState(TutorialState.Capa, tutorialTransitionDelay, "\u6c92\u63ee\u4e2d\u6642\u6a5f\uff0c\u518d\u6309\u4e00\u6b21 Space \u8a66\u8a66\u770b\u3002");
    }

    private void UpdateTutorialDash()
    {
        if (!tutorialChargeStarted)
        {
            PrepareEncounterForTutorial(tutorialDashFrontDistance);
            tutorialChargeStarted = true;
            tutorialDashStepStartedAt = Time.time;
            bullAI?.StartTutorialCharge(false, false);
        }

        if (bullAI == null || !bullAI.IsTutorialChargeSequenceComplete)
            return;

        bool dashedThisAttempt = playerStats != null && playerStats.LastDashTime > tutorialDashStepStartedAt;
        bool success = dashedThisAttempt && !bullAI.DidTutorialChargeHitPlayer;
        if (success)
        {
            QueueTutorialState(TutorialState.Attack, tutorialTransitionDelay, "\u9583\u5f97\u5f88\u597d\uff0c\u73fe\u5728\u8fd1\u8eab\u653b\u64ca\u5427\u3002");
            return;
        }

        QueueTutorialState(TutorialState.Dash, tutorialTransitionDelay, "\u9583\u907f\u6642\u6a5f\u4e0d\u5c0d\uff0c\u518d\u7528 Ctrl \u8a66\u4e00\u6b21\u3002");
    }

    private void UpdateTutorialAttack()
    {
        if (!tutorialChargeStarted)
        {
            PrepareEncounterForTutorial(tutorialAttackFrontDistance);
            tutorialChargeStarted = true;
            tutorialAttackHealthAtStepStart = bullStats != null ? bullStats.currentHealth : 0f;
            tutorialAttackResolveAt = -1f;
            tutorialAttackPerformed = false;
            tutorialAttackCleanHitRegistered = false;
            bullAI?.EnterTutorialIdle();
        }

        if (tutorialAttackCleanHitRegistered ||
            (bullStats != null && bullStats.currentHealth < tutorialAttackHealthAtStepStart - 0.01f))
        {
            QueueTutorialState(TutorialState.Complete, tutorialTransitionDelay, "\u547d\u4e2d\u78ba\u8a8d\uff0c\u6e96\u5099\u9032\u5165\u6b63\u5f0f\u6230\u9b25\u3002");
            return;
        }

        if (!tutorialAttackPerformed &&
            tutorialAttackResolveAt < 0f &&
            ((playerController != null && playerController.IsAttackPressedThisFrame()) || Input.GetKeyDown(KeyCode.F)))
        {
            tutorialAttackResolveAt = Time.time + tutorialAttackResolveDelay;
        }

        if (tutorialAttackResolveAt >= 0f && Time.time >= tutorialAttackResolveAt)
            QueueTutorialState(TutorialState.Attack, tutorialTransitionDelay, "\u8981\u96e2\u725b\u66f4\u8fd1\uff0c\u4e26\u78ba\u5be6\u547d\u4e2d\uff0c\u9019\u6a23\u624d\u7b97 CLEAN\u3002");
    }

    private void QueueTutorialState(TutorialState nextState, float delay, string feedback)
    {
        queuedTutorialState = nextState;
        tutorialTransitionTimer = Mathf.Max(0f, delay);
        tutorialFeedbackText = feedback ?? string.Empty;

        if (tutorialTransitionTimer <= 0f)
            EnterTutorialState(nextState);
    }

    private void EnterTutorialState(TutorialState nextState)
    {
        queuedTutorialState = TutorialState.None;
        tutorialTransitionTimer = 0f;
        tutorialState = nextState;
        tutorialStateElapsed = 0f;
        tutorialHoldTimer = 0f;
        tutorialCapaWindowTimer = 0f;
        tutorialChargeStarted = false;
        tutorialCapaQteStarted = false;
        tutorialCapaQteResolved = false;
        tutorialDashStepStartedAt = -999f;
        tutorialAttackResolveAt = -1f;
        tutorialAttackPerformed = false;
        tutorialAttackCleanHitRegistered = false;
        currentPhase = GamePhase.PhaseZeroTutorial;
        currentEnding = EndingType.None;

        bullAI?.SetTutorialControl(true);

        if (playerStats != null)
        {
            playerStats.SetHoldingCloth(false);
            playerStats.SetShooterGameplayEnabled(true);
        }

        playerController?.ClearInputBuffers();
        timingRing?.HideImmediate();
        timingRing?.ResetTimingWindow();

        if (nextState == TutorialState.Complete)
            PrepareEncounterForTutorial(tutorialAttackFrontDistance);
    }

    private void PrepareEncounterForTutorial(float bullDistance)
    {
        ResolveReferencesIfNeeded();
        Time.timeScale = 1f;
        phaseTwoPresentation?.ExitPhaseTwo();
        playerStats?.ResetCombatState();
        playerStats?.SetShooterGameplayEnabled(true);
        bullStats?.ResetCombatState();
        playerController?.ClearInputBuffers();
        timingRing?.HideImmediate();
        timingRing?.ResetTimingWindow();
        spawnManager?.ResetPlayerAndBullForTutorial(bullDistance, tutorialBullSideOffset);
        bullAI?.ResetCombatState();
        bullAI?.SetTutorialControl(true);
        bullAI?.EnterTutorialIdle();
    }

    private void StartPhaseOneFromTutorial()
    {
        ResolveReferencesIfNeeded();
        tutorialFeedbackText = string.Empty;
        currentEnding = EndingType.None;
        tutorialState = TutorialState.None;
        queuedTutorialState = TutorialState.None;
        tutorialTransitionTimer = 0f;
        tutorialStateElapsed = 0f;
        tutorialHoldTimer = 0f;
        tutorialCapaWindowTimer = 0f;
        tutorialChargeStarted = false;
        tutorialCapaQteStarted = false;
        tutorialCapaQteResolved = false;
        tutorialAttackPerformed = false;
        tutorialAttackCleanHitRegistered = false;
        Time.timeScale = 1f;
        phaseTwoPresentation?.ExitPhaseTwo();
        playerStats?.ResetCombatState();
        playerStats?.SetShooterGameplayEnabled(true);
        bullStats?.ResetCombatState();
        playerController?.ClearInputBuffers();
        timingRing?.HideImmediate();
        timingRing?.ResetTimingWindow();
        bullAI?.SetTutorialControl(false);
        spawnManager?.ResetPlayerToSpawn();
        spawnManager?.ResetBullToSpawn();
        bullAI?.ResetCombatState();
        currentPhase = GamePhase.PhaseOne;
    }

    private string GetTutorialTitle()
    {
        return tutorialState switch
        {
            TutorialState.Intro => "\u7b2c0\u968e\u6bb5\uff1a\u65b0\u624b\u6559\u5b78",
            TutorialState.HoldCloth => "\u7b2c0\u968e\u6bb5\uff1a\u6301\u5e03",
            TutorialState.Capa => "\u7b2c0\u968e\u6bb5\uff1a\u63ee\u5e03",
            TutorialState.Dash => "\u7b2c0\u968e\u6bb5\uff1a\u9583\u907f",
            TutorialState.Attack => "\u7b2c0\u968e\u6bb5\uff1a\u653b\u64ca",
            TutorialState.Complete => "\u6559\u5b78\u5b8c\u6210",
            _ => string.Empty
        };
    }

    private string GetTutorialInstruction()
    {
        return tutorialState switch
        {
            TutorialState.Intro => "\u5148\u8a8d\u8b58\u64cd\u4f5c\uff1a\u79fb\u52d5\u8207\u8996\u89d2\uff0cC \u6301\u7d05\u5e03\uff0cSpace \u63ee\u5e03\uff0cCtrl \u9583\u907f\uff0cF \u653b\u64ca\u3002",
            TutorialState.HoldCloth => "\u6309\u4f4f C \u628a\u7d05\u5e03\u8209\u8d77\u4f86\u3002",
            TutorialState.Capa => "\u6309\u4f4f C\uff0c\u7b49 QTE \u74b0\u7e2e\u8fd1\u6642\u6309 Space \u5b8c\u6210\u63ee\u5e03\u3002",
            TutorialState.Dash => "\u7576\u725b\u76f4\u885d\u904e\u4f86\u6642\uff0c\u6309 Ctrl \u9583\u907f\u3002",
            TutorialState.Attack => "\u9760\u8fd1\u725b\u4e4b\u5f8c\u6309 F\uff0c\u78ba\u5be6\u547d\u4e2d\u624d\u7b97 CLEAN\u3002",
            TutorialState.Complete => "\u5373\u5c07\u9032\u5165\u6b63\u5f0f\u7684\u7b2c\u4e00\u968e\u6bb5\u3002",
            _ => string.Empty
        };
    }

    private string GetTutorialStatus()
    {
        if (!string.IsNullOrWhiteSpace(tutorialFeedbackText) && tutorialTransitionTimer > 0f)
            return tutorialFeedbackText;

        return tutorialState switch
        {
            TutorialState.Intro => "\u6e96\u5099\u958b\u59cb",
            TutorialState.HoldCloth => $"{Mathf.RoundToInt(Mathf.Clamp01(tutorialHoldTimer / Mathf.Max(0.01f, tutorialHoldDuration)) * 100f)}%",
            TutorialState.Capa => "\u7528\u7d05\u5e03\u628a\u725b\u7684\u885d\u649e\u5e36\u958b\u3002",
            TutorialState.Dash => "\u5f9e\u725b\u7684\u885d\u649e\u8def\u7dda\u4e2d\u9583\u958b\u3002",
            TutorialState.Attack => tutorialAttackPerformed
                ? "\u6b63\u5728\u78ba\u8a8d\u662f\u5426\u70ba CLEAN \u547d\u4e2d..."
                : "\u4e00\u6b21\u6709\u6548\u547d\u4e2d\u5c31\u80fd\u5b8c\u6210\u9019\u500b\u6b65\u9a5f\u3002",
            TutorialState.Complete => "\u9032\u5165\u6b63\u5f0f\u6230\u9b25...",
            _ => string.Empty
        };
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
        bullAI?.SetTutorialControl(false);
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

        Debug.Log("Phase 2 intro started. Hold G to calibrate, then use E to stab.");
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
        tutorialState = TutorialState.None;
        queuedTutorialState = TutorialState.None;
        tutorialTransitionTimer = 0f;
        tutorialFeedbackText = string.Empty;
        Time.timeScale = 1f;
        phaseTwoPresentation?.ExitPhaseTwo();
        bullAI?.SetTutorialControl(false);

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
        tutorialState = TutorialState.None;
        queuedTutorialState = TutorialState.None;
        mercyTimer = 0f;
        bullDeathTimer = 0f;
        phaseTwoStateElapsed = 0f;
        calibrationHoldTimer = 0f;
        activeRoundWindowDuration = roundWindowDuration;
        tutorialStateElapsed = 0f;
        tutorialHoldTimer = 0f;
        tutorialTransitionTimer = 0f;
        tutorialCapaWindowTimer = 0f;
        tutorialDashStepStartedAt = -999f;
        tutorialAttackResolveAt = -1f;
        tutorialAttackHealthAtStepStart = 0f;
        phaseTwoRoundIndex = 0;
        bullHitCount = 0;
        playerHitCount = 0;
        phaseTwoCalibrated = false;
        phaseTwoHasCommittedAttack = false;
        nextRoundHasPerfectAdvantage = false;
        currentRoundHasPerfectAdvantage = false;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;
        tutorialChargeStarted = false;
        tutorialCapaQteStarted = false;
        tutorialCapaQteResolved = false;
        tutorialAttackPerformed = false;
        tutorialAttackCleanHitRegistered = false;
        lastPhaseTwoResult = string.Empty;
        tutorialFeedbackText = string.Empty;
        phaseTwoPresentation?.ExitPhaseTwo();
        bullAI?.SetTutorialControl(false);
        Time.timeScale = 1f;
    }

    private bool HasMissingReferences()
    {
        return playerStats == null ||
               bullStats == null ||
               bullAI == null ||
               playerController == null ||
               timingRing == null ||
               spawnManager == null ||
               phaseTwoPresentation == null;
    }

    private void ResolveReferencesIfNeeded()
    {
        if (playerStats == null)
            playerStats = BullfightSceneCache.FindObject<PlayerStats>();

        if (bullStats == null)
            bullStats = BullfightSceneCache.FindObject<BullStats>();

        if (bullAI == null)
            bullAI = BullfightSceneCache.FindObject<BullAI>();

        if (playerController == null)
            playerController = BullfightSceneCache.FindObject<BullfightPlayerController>();

        if (timingRing == null)
            timingRing = BullfightSceneCache.FindObject<BullTimingRing>();

        if (spawnManager == null && playerStats != null)
            spawnManager = playerStats.GetComponent<BullfightSpawnManager>();

        if (spawnManager == null)
            spawnManager = BullfightSceneCache.FindObject<BullfightSpawnManager>();

        if (phaseTwoPresentation == null)
            phaseTwoPresentation = GetComponent<BullfightPhaseTwoPresentation>() ?? BullfightSceneCache.FindObject<BullfightPhaseTwoPresentation>();

        if (phaseTwoPresentation == null)
            phaseTwoPresentation = gameObject.AddComponent<BullfightPhaseTwoPresentation>();

        RefreshTutorialSubscriptions();
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
        UnsubscribeTutorialSignals();
        phaseTwoPresentation?.ExitPhaseTwo();
        Time.timeScale = 1f;
    }

    private void HandleTutorialCapaTimingResult(string result)
    {
        tutorialCapaQteResolved = true;
        bullAI?.ResolveExternalChargeTimingResult(result);
    }

    private void HandleTutorialBanderillasPerformed()
    {
        if (currentPhase != GamePhase.PhaseZeroTutorial || tutorialState != TutorialState.Attack)
            return;

        tutorialAttackPerformed = true;
        tutorialAttackResolveAt = Time.time + tutorialAttackResolveDelay;
    }

    private void HandleTutorialBanderillasHit(float damage)
    {
        if (currentPhase != GamePhase.PhaseZeroTutorial || tutorialState != TutorialState.Attack)
            return;

        if (damage <= 0f)
            return;

        tutorialAttackCleanHitRegistered = true;
        tutorialAttackResolveAt = -1f;
    }

    private void RefreshTutorialSubscriptions()
    {
        if (subscribedTutorialPlayerStats != playerStats)
        {
            if (subscribedTutorialPlayerStats != null)
                subscribedTutorialPlayerStats.OnBanderillasPerformed -= HandleTutorialBanderillasPerformed;

            subscribedTutorialPlayerStats = playerStats;
            if (subscribedTutorialPlayerStats != null)
                subscribedTutorialPlayerStats.OnBanderillasPerformed += HandleTutorialBanderillasPerformed;
        }

        if (subscribedTutorialBullAI != bullAI)
        {
            if (subscribedTutorialBullAI != null)
                subscribedTutorialBullAI.OnBanderillasHit -= HandleTutorialBanderillasHit;

            subscribedTutorialBullAI = bullAI;
            if (subscribedTutorialBullAI != null)
                subscribedTutorialBullAI.OnBanderillasHit += HandleTutorialBanderillasHit;
        }
    }

    private void UnsubscribeTutorialSignals()
    {
        if (subscribedTutorialPlayerStats != null)
            subscribedTutorialPlayerStats.OnBanderillasPerformed -= HandleTutorialBanderillasPerformed;

        if (subscribedTutorialBullAI != null)
            subscribedTutorialBullAI.OnBanderillasHit -= HandleTutorialBanderillasHit;

        subscribedTutorialPlayerStats = null;
        subscribedTutorialBullAI = null;
    }
}
