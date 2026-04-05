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
        Move,
        Look,
        HoldCloth,
        Capa,
        Dash,
        Attack,
        Rules,
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
    public float phaseTwoRoundSideOffset = 1.2f;
    public float phaseTwoResolveWalkDelay = 0.45f;
    public float phaseTwoResolveWalkDuration = 1.2f;
    public float phaseTwoResolveWalkFrontDistance = 2.55f;
    public float phaseTwoShuttleRunSpeed = 2.7f;
    public float phaseTwoMissChargeDuration = 0.85f;
    public float phaseTwoMissPauseDuration = 3f;
    public float phaseTwoAmbientIntensity = 0.65f;
    public float phaseTwoDirectionalLightMultiplier = 0.8f;
    [TextArea(2, 3)] public string[] phaseTwoReflectionLines =
    {
        "\u4f60\u773c\u524d\u7684\uff0c\u771f\u7684\u662f\u6575\u4eba\u55ce\uff1f",
        "\u5982\u679c\u7260\u53ea\u662f\u60f3\u6d3b\u4e0b\u53bb\uff0c\u8ab0\u53c8\u5148\u8209\u8d77\u4e86\u6b66\u5668\uff1f",
        "\u89c0\u773e\u60f3\u770b\u7684\uff0c\u662f\u52dd\u5229\uff0c\u9084\u662f\u9bae\u8840\uff1f",
        "\u9019\u4e00\u64ca\u82e5\u6210\u529f\uff0c\u4f60\u771f\u7684\u6703\u6bd4\u8f03\u8f15\u9b06\u55ce\uff1f",
        "\u5982\u679c\u4f60\u5011\u90fd\u4e0d\u51fa\u624b\uff0c\u8ab0\u624d\u6709\u8cc7\u683c\u8aaa\u9019\u5834\u6c7a\u9b25\u5fc5\u9808\u7e7c\u7e8c\uff1f"
    };

    [Header("Tutorial")]
    public float tutorialIntroDuration = 2.25f;
    public float tutorialMoveDuration = 4.1f;
    public float tutorialLookDuration = 3.9f;
    public float tutorialHoldDuration = 0.5f;
    public float tutorialTransitionDelay = 0.9f;
    public float tutorialCompleteDelay = 1.5f;
    public float tutorialRulesMinReadDuration = 3f;
    public float tutorialCapaWindowDuration = 1.35f;
    public float tutorialChargeFrontDistance = 3.6f;
    public float tutorialDashFrontDistance = 3.4f;
    public float tutorialAttackFrontDistance = 3f;
    public float tutorialAttackResolveDelay = 1.1f;
    public float tutorialBullSideOffset = 0f;
    public int tutorialHoldRequiredCount = 3;
    public int tutorialDashRequiredCount = 3;
    public int tutorialAttackRequiredCount = 3;
    public float phaseOneGroundingSnapDuration = 1.2f;

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
    public bool IsPhaseTwoMissPauseActive => currentPhase == GamePhase.PhaseTwo &&
                                             phaseTwoState == PhaseTwoState.RoundResolve &&
                                             phaseTwoResolveWasMiss &&
                                             phaseTwoStateElapsed >= phaseTwoMissChargeDuration &&
                                             phaseTwoStateElapsed < phaseTwoMissChargeDuration + phaseTwoMissPauseDuration;
    public string CurrentPhaseTwoResolveNarration => IsPhaseTwoMissPauseActive ? phaseTwoResolveNarrationLine : string.Empty;
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
    public bool IsTutorialRulesStep => currentPhase == GamePhase.PhaseZeroTutorial && tutorialState == TutorialState.Rules;
    public string CurrentTutorialTitle => GetTutorialTitle();
    public string CurrentTutorialInstruction => GetTutorialInstruction();
    public string CurrentTutorialStatus => GetTutorialStatus();
    public string CurrentTutorialBody => GetTutorialBody();

    private PhaseTwoState phaseTwoState = PhaseTwoState.None;
    private TutorialState tutorialState = TutorialState.None;
    private TutorialState queuedTutorialState = TutorialState.None;
    private BullBleedVfx bullBleedVfx;
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
    private float tutorialMoveProgress;
    private float tutorialLookProgress;
    private float phaseOneGroundingSnapTimer;
    private int bullHitCount;
    private int playerHitCount;
    private int phaseTwoRoundIndex;
    private int tutorialHoldSuccessCount;
    private int tutorialDashSuccessCount;
    private int tutorialAttackSuccessCount;
    private bool phaseTwoCalibrated;
    private bool phaseTwoHasCommittedAttack;
    private bool nextRoundHasPerfectAdvantage;
    private bool currentRoundHasPerfectAdvantage;
    private int phaseTwoRoundSideSign = 1;
    private bool phaseTwoPrepareTelegraphStarted;
    private bool phaseTwoResolveBullResetApplied;
    private bool phaseTwoResolveWalkInitialized;
    private bool phaseTwoResolveWasMiss;
    private bool phaseTwoResolveMissImpactApplied;
    private bool phaseTwoResolveCompleted;
    private int phaseTwoResolveShuttleSegment;
    private bool tutorialChargeStarted;
    private bool tutorialCapaQteStarted;
    private bool tutorialCapaQteResolved;
    private bool tutorialHoldRegisteredThisAttempt;
    private bool tutorialAttackPerformed;
    private bool tutorialAttackCleanHitRegistered;
    private string lastPhaseTwoResult = string.Empty;
    private string tutorialFeedbackText = string.Empty;
    private string phaseTwoResolveNarrationLine = string.Empty;
    private Vector3 phaseTwoResolveCenterPoint;
    private Vector3 phaseTwoResolveLeftPoint;
    private Vector3 phaseTwoResolveRightPoint;
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
        bullBleedVfx?.ClearBleeds();
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
                    EnterTutorialState(TutorialState.Move);
                break;
            case TutorialState.Move:
                UpdateTutorialMove();
                break;
            case TutorialState.Look:
                UpdateTutorialLook();
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
            case TutorialState.Rules:
                UpdateTutorialRules();
                break;
            case TutorialState.Complete:
                if (tutorialStateElapsed >= tutorialCompleteDelay)
                    StartPhaseOneFromTutorial();
                break;
        }
    }

    private void UpdateTutorialMove()
    {
        if (playerController == null)
            return;

        tutorialMoveProgress += playerController.GetMovementInputMagnitude() >= 0.35f
            ? Time.unscaledDeltaTime
            : -Time.unscaledDeltaTime * 0.35f;
        tutorialMoveProgress = Mathf.Clamp(tutorialMoveProgress, 0f, tutorialMoveDuration);

        if (tutorialMoveProgress >= tutorialMoveDuration)
            QueueTutorialState(TutorialState.Look, 0.25f, "\u79fb\u52d5\u8a13\u7df4\u5b8c\u6210\uff0c\u63a5\u4e0b\u4f86\u8acb\u8f49\u52d5\u8996\u89d2\u89c0\u5bdf\u9b25\u725b\u5834\u3002");
    }

    private void UpdateTutorialLook()
    {
        if (playerController == null)
            return;

        tutorialLookProgress += playerController.GetLookInputMagnitude() >= 0.18f
            ? Time.unscaledDeltaTime
            : -Time.unscaledDeltaTime * 0.35f;
        tutorialLookProgress = Mathf.Clamp(tutorialLookProgress, 0f, tutorialLookDuration);

        if (tutorialLookProgress >= tutorialLookDuration)
            QueueTutorialState(TutorialState.HoldCloth, 0.25f, "\u8996\u89d2\u8a13\u7df4\u5b8c\u6210\uff0c\u73fe\u5728\u958b\u59cb\u6301\u5e03\u6559\u5b78\u3002");
    }

    private void UpdateTutorialHoldCloth()
    {
        if (playerStats == null)
            return;

        if (playerStats.isHoldingCloth)
        {
            tutorialHoldTimer += Time.unscaledDeltaTime;
            if (!tutorialHoldRegisteredThisAttempt && tutorialHoldTimer >= tutorialHoldDuration)
            {
                tutorialHoldRegisteredThisAttempt = true;
                tutorialHoldSuccessCount++;
            }
        }
        else
        {
            tutorialHoldTimer = 0f;
            tutorialHoldRegisteredThisAttempt = false;
        }

        if (tutorialHoldSuccessCount >= Mathf.Max(1, tutorialHoldRequiredCount))
            QueueTutorialState(TutorialState.Capa, 0.4f, "\u5f88\u597d\uff0c\u63a5\u4e0b\u4f86\u8981\u4ee5 Perfect \u5b8c\u6210\u4e00\u6b21\u63ee\u5e03\u3002");
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
            timingRing?.SetTimingWindow(0.5f, 0.75f);
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

        bool success = bullAI.TutorialChargeResult == "Perfect!";
        if (success)
        {
            QueueTutorialState(TutorialState.Dash, tutorialTransitionDelay, "\u63ee\u5e03 Perfect\uff0c\u73fe\u5728\u9023\u7e8c\u5b8c\u6210 3 \u6b21\u9583\u907f\u3002");
            return;
        }

        QueueTutorialState(TutorialState.Capa, tutorialTransitionDelay, "\u9019\u6b21\u4e0d\u662f Perfect\uff0c\u518d\u4f86\u4e00\u6b21\u3002");
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
            tutorialDashSuccessCount++;
            if (tutorialDashSuccessCount >= Mathf.Max(1, tutorialDashRequiredCount))
            {
                QueueTutorialState(TutorialState.Attack, tutorialTransitionDelay, "\u9583\u907f\u5b8c\u6210\uff0c\u73fe\u5728\u9023\u7e8c 3 \u6b21\u6210\u529f\u653b\u64ca\u3002");
                return;
            }

            QueueTutorialState(TutorialState.Dash, tutorialTransitionDelay, $"\u9583\u907f\u6210\u529f {tutorialDashSuccessCount}/{Mathf.Max(1, tutorialDashRequiredCount)}\uff0c\u518d\u4f86\u4e00\u6b21\u3002");
            return;
        }

        QueueTutorialState(TutorialState.Dash, tutorialTransitionDelay, "\u9583\u907f\u6642\u6a5f\u4e0d\u5c0d\uff0c\u518d\u4f86\u4e00\u6b21\u3002");
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
            tutorialAttackSuccessCount++;
            if (tutorialAttackSuccessCount >= Mathf.Max(1, tutorialAttackRequiredCount))
            {
                QueueTutorialState(TutorialState.Rules, tutorialTransitionDelay, "\u57fa\u790e\u64cd\u4f5c\u5b8c\u6210\uff0c\u4e0a\u5834\u524d\u8acb\u5148\u8b80\u5b8c\u6240\u6709\u898f\u5247\u3002");
                return;
            }

            QueueTutorialState(TutorialState.Attack, tutorialTransitionDelay, $"\u653b\u64ca\u6210\u529f {tutorialAttackSuccessCount}/{Mathf.Max(1, tutorialAttackRequiredCount)}\uff0c\u518d\u88dc\u4e00\u64ca\u3002");
            return;
        }

        if (!tutorialAttackPerformed &&
            tutorialAttackResolveAt < 0f &&
            ((playerController != null && playerController.IsAttackPressedThisFrame()) || Input.GetKeyDown(KeyCode.F)))
        {
            tutorialAttackResolveAt = Time.time + tutorialAttackResolveDelay;
        }

        if (tutorialAttackResolveAt >= 0f && Time.time >= tutorialAttackResolveAt)
            QueueTutorialState(TutorialState.Attack, tutorialTransitionDelay, "\u8981\u5728\u6709\u6548\u8ddd\u96e2\u5167\u6210\u529f\u547d\u4e2d\uff0c\u518d\u8a66\u4e00\u6b21\u3002");
    }

    private void UpdateTutorialRules()
    {
        if (tutorialStateElapsed < tutorialRulesMinReadDuration)
            return;

        if (playerController != null && playerController.WasTutorialAdvancePressedThisFrame())
            QueueTutorialState(TutorialState.Complete, 0.2f, "\u6e96\u5099\u9032\u5165\u6b63\u5f0f\u6230\u9b25\u3002");
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
        tutorialMoveProgress = 0f;
        tutorialLookProgress = 0f;
        tutorialChargeStarted = false;
        tutorialCapaQteStarted = false;
        tutorialCapaQteResolved = false;
        tutorialHoldRegisteredThisAttempt = false;
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
        bullBleedVfx?.ClearBleeds();
        currentPhase = GamePhase.PhaseOne;
        phaseOneGroundingSnapTimer = Mathf.Max(0f, phaseOneGroundingSnapDuration);
    }

    private string GetTutorialTitle()
    {
        return tutorialState switch
        {
            TutorialState.Intro => "\u7b2c0\u968e\u6bb5\uff1a\u65b0\u624b\u6559\u5b78",
            TutorialState.Move => "\u7b2c0\u968e\u6bb5\uff1a\u79fb\u52d5",
            TutorialState.Look => "\u7b2c0\u968e\u6bb5\uff1a\u8f49\u52d5\u8996\u89d2",
            TutorialState.HoldCloth => "\u7b2c0\u968e\u6bb5\uff1a\u6301\u5e03",
            TutorialState.Capa => "\u7b2c0\u968e\u6bb5\uff1a\u63ee\u5e03",
            TutorialState.Dash => "\u7b2c0\u968e\u6bb5\uff1a\u9583\u907f",
            TutorialState.Attack => "\u7b2c0\u968e\u6bb5\uff1a\u653b\u64ca",
            TutorialState.Rules => "\u4e0a\u5834\u524d\u898f\u5247\u8aaa\u660e",
            TutorialState.Complete => "\u6559\u5b78\u5b8c\u6210",
            _ => string.Empty
        };
    }

    private string GetTutorialInstruction()
    {
        return tutorialState switch
        {
            TutorialState.Intro => $"\u5148\u8a8d\u8b58\u57fa\u790e\u64cd\u4f5c\uff1a{GetMoveLabel()} \u79fb\u52d5\uff0c{GetLookLabel()} \u8f49\u8996\u89d2\uff0c{GetHoldLabel()} \u6301\u5e03\uff0c{GetSwingLabel()} \u63ee\u5e03\uff0c{GetDashLabel()} \u9583\u907f\uff0c{GetAttackLabel()} \u653b\u64ca\u3002",
            TutorialState.Move => $"\u8acb\u4f7f\u7528 {GetMoveLabel()} \u79fb\u52d5\u9b25\u725b\u58eb\u3002",
            TutorialState.Look => $"\u8acb\u4f7f\u7528 {GetLookLabel()} \u89c0\u5bdf\u9b25\u725b\u5834\u8207\u725b\u7684\u4f4d\u7f6e\u3002",
            TutorialState.HoldCloth => $"\u6309\u4f4f {GetHoldLabel()} \u8209\u8d77\u7d05\u5e03\uff0c\u9023\u7e8c\u5b8c\u6210 3 \u6b21\u3002",
            TutorialState.Capa => $"\u6309\u4f4f {GetHoldLabel()}\uff0c\u7b49 QTE \u74b0\u7e2e\u8fd1\u6642\u6309 {GetSwingLabel()} \uff0c\u4e26\u62ff\u5230 Perfect\u3002",
            TutorialState.Dash => $"\u7576\u725b\u76f4\u885d\u904e\u4f86\u6642\uff0c\u6309 {GetDashLabel()} \u9023\u7e8c\u5b8c\u6210 3 \u6b21\u6210\u529f\u9583\u907f\u3002",
            TutorialState.Attack => $"\u9760\u8fd1\u725b\u5f8c\u6309 {GetAttackLabel()} \u9032\u884c\u653b\u64ca\uff0c\u9023\u7e8c\u5b8c\u6210 3 \u6b21\u6709\u6548\u547d\u4e2d\u3002",
            TutorialState.Rules => $"\u8acb\u8b80\u5b8c\u4e0b\u65b9\u898f\u5247\uff0c\u4e4b\u5f8c\u6309 {GetAttackLabel()} \u7e7c\u7e8c\u3002",
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
            TutorialState.Move => $"{Mathf.RoundToInt(Mathf.Clamp01(tutorialMoveProgress / Mathf.Max(0.01f, tutorialMoveDuration)) * 100f)}%",
            TutorialState.Look => $"{Mathf.RoundToInt(Mathf.Clamp01(tutorialLookProgress / Mathf.Max(0.01f, tutorialLookDuration)) * 100f)}%",
            TutorialState.HoldCloth => $"{tutorialHoldSuccessCount}/{Mathf.Max(1, tutorialHoldRequiredCount)}",
            TutorialState.Capa => "\u6559\u5b78\u8981\u6c42\uff1aPerfect \u4e00\u6b21",
            TutorialState.Dash => $"{tutorialDashSuccessCount}/{Mathf.Max(1, tutorialDashRequiredCount)}",
            TutorialState.Attack => tutorialAttackPerformed
                ? "\u6b63\u5728\u78ba\u8a8d\u662f\u5426\u70ba CLEAN \u547d\u4e2d..."
                : $"{tutorialAttackSuccessCount}/{Mathf.Max(1, tutorialAttackRequiredCount)}",
            TutorialState.Rules => tutorialStateElapsed < tutorialRulesMinReadDuration
                ? "\u8acb\u5148\u8b80\u5b8c\u6240\u6709\u898f\u5247"
                : $"\u6309 {GetAttackLabel()} \u9032\u5165\u6b63\u5f0f\u6230\u9b25",
            TutorialState.Complete => "\u9032\u5165\u6b63\u5f0f\u6230\u9b25...",
            _ => string.Empty
        };
    }

    private string GetTutorialBody()
    {
        if (tutorialState != TutorialState.Rules)
            return string.Empty;

        return
            "\u9b25\u725b\u58eb\u60a8\u597d\uff0c\u6b61\u8fce\u53c3\u52a0\u672c\u6b21\u7684\u9b25\u725b\u7af6\u8cfd\u3002\u9019\u662f\u4e00\u4efd\u6975\u9ad8\u7684\u69ae\u8b7d\uff0c\u60a8\u5c07\u89aa\u8eab\u5c0d\u6c7a\u9019\u982d\u731b\u725b\uff0c\u4e26\u6c7a\u5b9a\u6700\u7d42\u7684\u7d50\u5c40\u3002\u8acb\u52d9\u5fc5\u719f\u8a18\u4ee5\u4e0b\u6240\u6709\u898f\u5247\uff0c\u9019\u5c07\u662f\u60a8\u5728\u5834\u4e0a\u751f\u5b58\u7684\u552f\u4e00\u4f9d\u64da\u3002\n\n" +
            "\u4e00\u3001\u9b25\u725b\u58eb\u72c0\u614b\u8207\u9650\u5236\n" +
            "\u9ad4\u529b\u8207\u6688\u7729\uff1a\n" +
            "\u57f7\u884c\u4efb\u4f55\u6fc0\u70c8\u52d5\u4f5c\uff08\u62ab\u80a9\u3001\u9583\u907f\u3001\u653b\u64ca\uff09\u7686\u6703\u6d88\u8017\u9ad4\u529b\u3002\n" +
            "\u898f\u5247\uff1a\u82e5\u9ad4\u529b\u6d88\u8017\u5149\u4fbf\u6703\u9032\u5165 2 \u79d2\u6688\u7729\u72c0\u614b\uff0c\u6b64\u671f\u9593\u73a9\u5bb6\u4e0d\u53ef\u88ab\u64cd\u7e31\uff0c\u4e14\u6975\u6613\u53d7\u5230\u653b\u64ca\u3002\n" +
            "\u6301\u5e03\u72c0\u614b\uff1a\n" +
            "\u898f\u5247\uff1a\u6301\u5e03\u72c0\u614b\u4e0b\u73a9\u5bb6\u4e0d\u53ef\u79fb\u52d5\u4e5f\u4e0d\u53ef\u653b\u64ca\u3002\n" +
            "\u88dc\u511f\uff1a\u6b64\u72c0\u614b\u4e0b\u9ad4\u529b\u6062\u5fa9\u901f\u5ea6\u6700\u5feb\uff08\u6bcf\u79d2\u6062\u5fa9 15%\uff09\uff0c\u662f\u6230\u9b25\u4e2d\u552f\u4e00\u7684\u5598\u606f\u6a5f\u6703\u3002\n" +
            "\u751f\u547d\u503c\uff1a\n" +
            "\u82e5\u8840\u91cf\u6b78\u96f6\u5247\u76f4\u63a5\u5224\u5b9a\u6b7b\u4ea1\uff08\u89f8\u767c\u60b2\u5287\u7d50\u5c40\uff09\u3002\n\n" +
            "\u4e8c\u3001\u4ec7\u6068\u8207\u7bc4\u570d\u5224\u5b9a\n" +
            "\u653b\u64ca\u7bc4\u570d\u898f\u5247\uff1a\n" +
            "* \u82e5\u73a9\u5bb6\u5728\u725b\u7684\u885d\u92d2\u7db2\u683c\uff08\u5730\u9762\u7d05\u8272\u77e9\u5f62\uff09\u5167\uff0c\u5247\u88ab\u8996\u70ba\u53ef\u653b\u64ca\u5c0d\u8c61\u3002\n" +
            "\u82e5\u6210\u529f\u9583\u907f\u6216\u9003\u51fa\u8a72\u7bc4\u570d\uff0c\u5247\u725b\u7684\u885d\u92d2\u5c07\u4e0d\u6703\u5c0d\u60a8\u9020\u6210\u50b7\u5bb3\u3002\n" +
            "\u6012\u6c23\uff1a\n" +
            "\u9b25\u725b\u53d7\u5230\u50b7\u5bb3\u6216\u88ab\u6311\u91c1\u6703\u589e\u52a0\u6012\u6c23\u3002\u6012\u6c23\u8d8a\u9ad8\uff0c\u725b\u7684\u885d\u92d2\u901f\u5ea6\u8d8a\u5feb\uff08\u6700\u9ad8 1.1x\uff09\uff0c\u4e14\u8f49\u5411\u8207\u653b\u64ca\u983b\u7387\u4e5f\u6703\u96a8\u4e4b\u63d0\u5347\u3002\n\n" +
            "\u4e09\u3001\u7cbe\u6e96\u5224\u5b9a\u7cfb\u7d71\n" +
            "\u7576\u9032\u884c\u6301\u5e03 \u6216 \u6700\u7d42\u523a\u6bba \u6642\uff0c\u756b\u9762\u4e0a\u6703\u51fa\u73fe\u7e2e\u5708\u5224\u5b9a\uff1a\n\n" +
            "Miss (\u7d05\u8272)\uff1a\u5224\u5b9a\u5931\u6557\u3002\u73a9\u5bb6\u53d7\u50b7\u3001\u9ad4\u529b\u7acb\u5373\u6b78\u96f6\u3001\u4e26\u76f4\u63a5\u9032\u5165\u6688\u7729\u72c0\u614b\u3002\n" +
            "Good (\u6a58\u8272)\uff1a\u5224\u5b9a\u6210\u529f\u3002\u53ef\u8b93\u725b\u505c\u4e0b\u7576\u524d\u7684\u653b\u64ca\uff0c\u96d9\u65b9\u91cd\u65b0\u62c9\u958b\u8ddd\u96e2\u3002\n" +
            "Perfect (\u7da0\u8272)\uff1a\u5b8c\u7f8e\u5224\u5b9a\u3002\u8b93\u725b\u505c\u6b62\u653b\u64ca\uff0c\u4e14\u73a9\u5bb6\u6703\u7372\u5f97\u9ad4\u529b\u5927\u5e45\u56de\u5347\u8207\u77ed\u66ab\u7684\u79fb\u52d5\u52a0\u901f Buff\u3002";
    }

    private void UpdatePhaseOne()
    {
        if (phaseOneGroundingSnapTimer > 0f)
        {
            phaseOneGroundingSnapTimer = Mathf.Max(0f, phaseOneGroundingSnapTimer - Time.unscaledDeltaTime);
            bullAI?.ForceSnapToGround();
        }

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

        if (!phaseTwoResolveBullResetApplied)
        {
            phaseTwoResolveBullResetApplied = true;
            phaseTwoResolveCompleted = false;
            phaseTwoResolveWalkInitialized = false;
        }

        if (phaseTwoResolveWasMiss)
            UpdatePhaseTwoMissResolve();
        else
            UpdatePhaseTwoResolveWalk();

        if (!phaseTwoResolveCompleted)
            return;

        if (CheckPhaseTwoVictory())
            return;

        StartNextPhaseTwoRound();
    }

    private void UpdatePhaseTwoResolveWalk()
    {
        if (bullAI == null || playerStats == null || phaseTwoState != PhaseTwoState.RoundResolve || !phaseTwoResolveBullResetApplied)
            return;

        if (!phaseTwoResolveWalkInitialized)
        {
            phaseTwoResolveWalkInitialized = true;
            phaseTwoResolveCenterPoint = bullAI.transform.position;
            ResolvePhaseTwoShuttlePoints(phaseTwoResolveCenterPoint);
            phaseTwoResolveShuttleSegment = 0;
            bullAI.PlayPhaseTwoWalkLoop();
        }

        Vector3 current = bullAI.transform.position;
        Vector3 target = phaseTwoResolveShuttleSegment switch
        {
            0 => phaseTwoRoundSideSign > 0 ? phaseTwoResolveRightPoint : phaseTwoResolveLeftPoint,
            1 => phaseTwoRoundSideSign > 0 ? phaseTwoResolveLeftPoint : phaseTwoResolveRightPoint,
            _ => phaseTwoResolveCenterPoint
        };

        Vector3 nextPosition = Vector3.MoveTowards(current, target, Mathf.Max(0.5f, phaseTwoShuttleRunSpeed) * Time.unscaledDeltaTime);
        Vector3 moveDirection = target - current;
        moveDirection.y = 0f;
        Quaternion nextRotation = moveDirection.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(moveDirection.normalized, Vector3.up)
            : bullAI.transform.rotation;

        bullAI.SetPhaseTwoPose(nextPosition, nextRotation);

        if (Vector3.Distance(nextPosition, target) > 0.04f)
            return;

        phaseTwoResolveShuttleSegment++;
        if (phaseTwoResolveShuttleSegment <= 2)
            return;

        phaseTwoResolveCompleted = true;
        bullAI.PlayPhaseTwoRoundResetIdle();
    }

    private void UpdatePhaseTwoMissResolve()
    {
        if (bullAI == null || playerStats == null)
            return;

        float chargeDuration = Mathf.Max(0.2f, phaseTwoMissChargeDuration);
        float pauseDuration = Mathf.Max(0f, phaseTwoMissPauseDuration);
        float totalDuration = chargeDuration + pauseDuration;

        if (phaseTwoStateElapsed <= chargeDuration)
        {
            Vector3 current = bullAI.transform.position;
            Vector3 toPlayer = playerStats.transform.position - current;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                Vector3 direction = toPlayer.normalized;
                Vector3 target = playerStats.transform.position - direction * 0.55f;
                target.y = current.y;
                Vector3 nextPosition = Vector3.MoveTowards(current, target, Mathf.Max(0.8f, phaseTwoShuttleRunSpeed * 1.4f) * Time.unscaledDeltaTime);
                Vector3 moveDirection = target - current;
                moveDirection.y = 0f;
                Quaternion nextRotation = moveDirection.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(moveDirection.normalized, Vector3.up)
                    : bullAI.transform.rotation;
                bullAI.SetPhaseTwoPose(nextPosition, nextRotation);
            }

            bullAI.PlayPhaseTwoAttackFollowThrough();

            if (!phaseTwoResolveMissImpactApplied && phaseTwoStateElapsed >= chargeDuration * 0.65f)
            {
                phaseTwoResolveMissImpactApplied = true;
                playerStats.TakeBullImpact(14f, bullAI.transform.position);
            }
        }
        else
        {
            bullAI.PlayPhaseTwoStandoffIdle();
        }

        if (phaseTwoStateElapsed >= totalDuration)
            phaseTwoResolveCompleted = true;
    }

    private void HandlePhaseTwoStabResult(string result)
    {
        if (currentPhase != GamePhase.PhaseTwo || phaseTwoState != PhaseTwoState.RoundWindow)
            return;

        lastPhaseTwoResult = result;
        mercyTimer = 0f;
        phaseTwoHasCommittedAttack = true;

        bool isSuccess = result == "Perfect!" || result == "Good";
        phaseTwoResolveWasMiss = !isSuccess;
        phaseTwoResolveMissImpactApplied = false;
        phaseTwoResolveCompleted = false;
        phaseTwoResolveShuttleSegment = 0;
        phaseTwoResolveNarrationLine = string.Empty;
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
            phaseTwoResolveNarrationLine = GetRandomPhaseTwoReflectionLine();
        }

        if (timingRing != null)
        {
            timingRing.HideRingKeepFeedback();
            timingRing.ResetTimingWindow();
        }

        if (CheckPhaseTwoVictory())
            return;

        phaseTwoState = PhaseTwoState.RoundResolve;
        phaseTwoStateElapsed = 0f;
        phaseTwoResolveBullResetApplied = false;
        phaseTwoResolveWalkInitialized = false;
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
        activeRoundWindowDuration = GetActiveRoundWindowDuration();
        lastPhaseTwoResult = string.Empty;
        currentRoundHasPerfectAdvantage = nextRoundHasPerfectAdvantage;
        nextRoundHasPerfectAdvantage = false;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;
        phaseTwoResolveWalkInitialized = false;
        phaseTwoResolveWasMiss = false;
        phaseTwoResolveMissImpactApplied = false;
        phaseTwoResolveCompleted = false;
        phaseTwoResolveShuttleSegment = 0;
        phaseTwoResolveNarrationLine = string.Empty;

        if (currentRoundHasPerfectAdvantage)
            activeRoundWindowDuration += perfectTelegraphBonus;

        // Keep each phase-two attack centered on screen; only the resolve sequence
        // should perform the side-to-side shuttle motion.
        ResetBullForPhaseTwoRound();
        phaseTwoRoundSideSign *= -1;

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
        activeRoundWindowDuration = GetActiveRoundWindowDuration();
        nextRoundHasPerfectAdvantage = false;
        currentRoundHasPerfectAdvantage = false;
        phaseTwoRoundSideSign = 1;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;
        phaseTwoResolveWalkInitialized = false;
        phaseTwoResolveWasMiss = false;
        phaseTwoResolveMissImpactApplied = false;
        phaseTwoResolveCompleted = false;
        phaseTwoResolveShuttleSegment = 0;
        phaseTwoResolveNarrationLine = string.Empty;
        lastPhaseTwoResult = string.Empty;
        Time.timeScale = phaseTwoTimeScale;

        if (bullAI != null)
            bullAI.enabled = false;

        bullBleedVfx?.ClearBleeds();
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
        phaseTwoResolveWalkInitialized = false;
        phaseTwoResolveWasMiss = false;
        phaseTwoResolveMissImpactApplied = false;
        phaseTwoResolveCompleted = false;
        phaseTwoResolveShuttleSegment = 0;
        phaseTwoResolveNarrationLine = string.Empty;
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
        phaseTwoResolveWalkInitialized = false;
        phaseTwoResolveWasMiss = false;
        phaseTwoResolveMissImpactApplied = false;
        phaseTwoResolveCompleted = false;
        phaseTwoResolveShuttleSegment = 0;
        phaseTwoResolveNarrationLine = string.Empty;
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

        if (ending == EndingType.Tragedy && playerStats != null)
            playerStats.ForceEndingDeathPresentation();

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
        tutorialMoveProgress = 0f;
        tutorialLookProgress = 0f;
        phaseTwoRoundIndex = 0;
        bullHitCount = 0;
        playerHitCount = 0;
        tutorialHoldSuccessCount = 0;
        tutorialDashSuccessCount = 0;
        tutorialAttackSuccessCount = 0;
        phaseTwoCalibrated = false;
        phaseTwoHasCommittedAttack = false;
        nextRoundHasPerfectAdvantage = false;
        currentRoundHasPerfectAdvantage = false;
        phaseTwoRoundSideSign = 1;
        phaseTwoPrepareTelegraphStarted = false;
        phaseTwoResolveBullResetApplied = false;
        phaseTwoResolveWalkInitialized = false;
        phaseTwoResolveWasMiss = false;
        phaseTwoResolveMissImpactApplied = false;
        phaseTwoResolveCompleted = false;
        phaseTwoResolveShuttleSegment = 0;
        tutorialChargeStarted = false;
        tutorialCapaQteStarted = false;
        tutorialCapaQteResolved = false;
        tutorialHoldRegisteredThisAttempt = false;
        tutorialAttackPerformed = false;
        tutorialAttackCleanHitRegistered = false;
        lastPhaseTwoResult = string.Empty;
        tutorialFeedbackText = string.Empty;
        phaseOneGroundingSnapTimer = 0f;
        phaseTwoResolveNarrationLine = string.Empty;
        phaseTwoResolveCenterPoint = Vector3.zero;
        phaseTwoResolveLeftPoint = Vector3.zero;
        phaseTwoResolveRightPoint = Vector3.zero;
        phaseTwoPresentation?.ExitPhaseTwo();
        bullAI?.SetTutorialControl(false);
        Time.timeScale = 1f;
    }

    private string GetMoveLabel()
    {
        return playerController != null ? playerController.GetMoveDisplayLabel() : "\u5de6\u6416\u687f";
    }

    private string GetLookLabel()
    {
        return playerController != null ? playerController.GetLookDisplayLabel() : "\u53f3\u6416\u687f";
    }

    private string GetHoldLabel()
    {
        return playerController != null ? playerController.GetHoldDisplayLabel() : "ZL + ZR";
    }

    private string GetSwingLabel()
    {
        return playerController != null ? playerController.GetSwingDisplayLabel() : "X";
    }

    private string GetDashLabel()
    {
        return playerController != null ? playerController.GetDashDisplayLabel() : "Y";
    }

    private string GetAttackLabel()
    {
        return playerController != null ? playerController.GetAttackDisplayLabel() : "B";
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

        if (bullBleedVfx == null && bullAI != null)
            bullBleedVfx = bullAI.GetComponent<BullBleedVfx>();

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

    private void ResetBullForPhaseTwoRound(int sideSign = 0)
    {
        float sideMagnitude = Mathf.Max(Mathf.Abs(phaseTwoBullSideOffset), Mathf.Abs(phaseTwoRoundSideOffset));
        float resolvedSideOffset = sideSign == 0 ? phaseTwoBullSideOffset : sideMagnitude * sideSign;
        spawnManager?.ResetBullForPhaseTwoRound(phaseTwoBullFrontDistance, resolvedSideOffset);
        bullAI?.ForceSnapToGround();
    }

    private float GetActiveRoundPrepareDuration()
    {
        return Mathf.Max(roundPrepareDuration, 1.8f) + Mathf.Max(0.9f, GetRoundStanceDuration());
    }

    private float GetActiveRoundWindowDuration()
    {
        return Mathf.Max(roundWindowDuration, 1.7f);
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

    private string GetRandomPhaseTwoReflectionLine()
    {
        if (phaseTwoReflectionLines == null || phaseTwoReflectionLines.Length == 0)
            return string.Empty;

        int randomIndex = Random.Range(0, phaseTwoReflectionLines.Length);
        return phaseTwoReflectionLines[randomIndex] ?? string.Empty;
    }

    private void ResolvePhaseTwoShuttlePoints(Vector3 centerPoint)
    {
        if (playerStats == null)
        {
            phaseTwoResolveCenterPoint = centerPoint;
            phaseTwoResolveLeftPoint = centerPoint + Vector3.left * Mathf.Max(0.45f, phaseTwoRoundSideOffset);
            phaseTwoResolveRightPoint = centerPoint + Vector3.right * Mathf.Max(0.45f, phaseTwoRoundSideOffset);
            return;
        }

        Transform playerTransform = playerStats.transform;
        Vector3 forward = playerTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude <= 0.0001f)
            forward = Vector3.forward;

        forward.Normalize();
        Vector3 right = new Vector3(forward.z, 0f, -forward.x).normalized;
        float sideDistance = Mathf.Max(0.45f, phaseTwoRoundSideOffset);

        phaseTwoResolveCenterPoint = centerPoint;
        phaseTwoResolveLeftPoint = centerPoint - right * sideDistance;
        phaseTwoResolveRightPoint = centerPoint + right * sideDistance;
        phaseTwoResolveLeftPoint.y = centerPoint.y;
        phaseTwoResolveRightPoint.y = centerPoint.y;
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
