using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using InfimaGames.LowPolyShooterPack;

public class BullfightPlayerController : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Transform bullTarget;

    [Header("Input")]
    public KeyCode holdClothKey = KeyCode.C;
    public KeyCode capaKey = KeyCode.Space;
    public KeyCode attackKey = KeyCode.F;
    public KeyCode evadeKey = KeyCode.LeftControl;
    public KeyCode phaseTwoCalibrationKey = KeyCode.G;
    public KeyCode phaseTwoStabKey = KeyCode.E;

    [Header("Action Assets")]
    [SerializeField] private InputActionAsset bullfightActionsAsset;

    [Header("Buffer")]
    public float capaBufferDuration = 0.35f;
    public float attackBufferDuration = 0.15f;
    public float phaseTwoStabBufferDuration = 0.2f;

    private float capaBufferedUntil = -1f;
    private float attackBufferedUntil = -1f;
    private float phaseTwoStabBufferedUntil = -1f;
    private bool dashSuppressed;
    private Character shooterCharacter;
    private FieldInfo axisMovementField;
    private FieldInfo holdingButtonRunField;
    private BullfightGameFlow gameFlow;
    private InputActionAsset runtimeBullfightActions;
    private InputAction holdAction;
    private InputAction swingAction;
    private InputAction attackAction;
    private InputAction dashAction;
    private InputAction phaseTwoCalibrationAction;
    private InputAction phaseTwoStabAction;
    private int sensorSwingFrame = -1;
    private int sensorPhaseTwoStabFrame = -1;
    private bool sensorPhaseTwoCalibrationHeld;

    private void Awake()
    {
        AutoAssignReferences();
        RefreshBullfightActions();
    }

    private void OnEnable()
    {
        RefreshBullfightActions();
    }

    private void OnDisable()
    {
        runtimeBullfightActions?.Disable();
    }

    private void OnDestroy()
    {
        if (runtimeBullfightActions != null)
            Destroy(runtimeBullfightActions);
    }

    private void Update()
    {
        AutoAssignReferences();

        if (IsPhaseTwoInputMode())
        {
            playerStats?.SetHoldingCloth(false);
            ForceStopMovement();
            UpdatePhaseTwoBufferedInputs();
            return;
        }

        UpdateHoldingCloth();
        if (playerStats != null && playerStats.isStunned)
            ForceStopMovement();
        FreezeMovementWhileHoldingCloth();
        UpdateBufferedInputs();
        HandleEvade();
    }

    public bool ConsumeCapaPressed()
    {
        if (playerStats == null || !playerStats.isHoldingCloth)
        {
            capaBufferedUntil = -1f;
            return false;
        }

        if (Time.time > capaBufferedUntil)
            return false;

        capaBufferedUntil = -1f;
        return true;
    }

    public bool ConsumeAttackPressed()
    {
        if (playerStats != null && playerStats.isHoldingCloth)
        {
            attackBufferedUntil = -1f;
            return false;
        }

        if (Time.time > attackBufferedUntil)
            return false;

        attackBufferedUntil = -1f;
        return true;
    }

    public Transform GetBullTarget()
    {
        AutoAssignReferences();
        return bullTarget != null ? bullTarget : transform;
    }

    public bool IsPhaseTwoCalibrationHeld()
    {
        return IsPhaseTwoInputMode() && IsPhaseTwoCalibrationPressed();
    }

    public bool ConsumePhaseTwoStabPressed()
    {
        if (!IsPhaseTwoInputMode())
        {
            phaseTwoStabBufferedUntil = -1f;
            return false;
        }

        if (Time.time > phaseTwoStabBufferedUntil)
            return false;

        phaseTwoStabBufferedUntil = -1f;
        return true;
    }

    public bool IsPhaseTwoStabPressedThisFrame()
    {
        return IsPhaseTwoInputMode() && WasPhaseTwoStabPressedThisFrame();
    }

    public void ClearInputBuffers()
    {
        capaBufferedUntil = -1f;
        attackBufferedUntil = -1f;
        phaseTwoStabBufferedUntil = -1f;
        sensorSwingFrame = -1;
        sensorPhaseTwoStabFrame = -1;
    }

    public void ConfigureInputActions(InputActionAsset asset)
    {
        bullfightActionsAsset = asset;
        RefreshBullfightActions();
    }

    public void SetDashSuppressed(bool suppressed)
    {
        dashSuppressed = suppressed;
    }

    public bool IsDashSuppressed() => dashSuppressed;

    public void TriggerSensorSwing()
    {
        sensorSwingFrame = Time.frameCount;
    }

    public void SetPhaseTwoCalibrationSensorHeld(bool held)
    {
        sensorPhaseTwoCalibrationHeld = held;
    }

    public void TriggerPhaseTwoStab()
    {
        sensorPhaseTwoStabFrame = Time.frameCount;
    }

    public void ForceStopMovement()
    {
        AutoAssignReferences();

        if (shooterCharacter == null)
            return;

        axisMovementField?.SetValue(shooterCharacter, Vector2.zero);
        holdingButtonRunField?.SetValue(shooterCharacter, false);
    }

    private void UpdateHoldingCloth()
    {
        if (playerStats == null)
            return;

        playerStats.SetHoldingCloth(IsHoldPressed());
    }

    private void UpdateBufferedInputs()
    {
        if (WasSwingPressedThisFrame() && playerStats != null && playerStats.isHoldingCloth)
            capaBufferedUntil = Time.time + capaBufferDuration;

        if (WasAttackPressedThisFrame() && (playerStats == null || !playerStats.isHoldingCloth))
            attackBufferedUntil = Time.time + attackBufferDuration;
    }

    private void HandleEvade()
    {
        if (playerStats == null)
            return;

        if (dashSuppressed)
            return;

        if (WasDashPressedThisFrame())
            playerStats.TryEvade();
    }

    private void UpdatePhaseTwoBufferedInputs()
    {
        if (WasPhaseTwoStabPressedThisFrame())
            phaseTwoStabBufferedUntil = Time.time + phaseTwoStabBufferDuration;
    }

    private bool IsHoldPressed()
    {
        return IsActionPressed(holdAction) || Input.GetKey(holdClothKey);
    }

    private bool WasSwingPressedThisFrame()
    {
        return WasSensorTriggeredThisFrame(sensorSwingFrame) || WasActionPressedThisFrame(swingAction) || Input.GetKeyDown(capaKey);
    }

    private bool WasAttackPressedThisFrame()
    {
        return WasActionPressedThisFrame(attackAction) || Input.GetKeyDown(attackKey);
    }

    private bool WasDashPressedThisFrame()
    {
        return WasActionPressedThisFrame(dashAction) || Input.GetKeyDown(evadeKey);
    }

    private bool IsPhaseTwoCalibrationPressed()
    {
        return sensorPhaseTwoCalibrationHeld || IsActionPressed(phaseTwoCalibrationAction) || Input.GetKey(phaseTwoCalibrationKey);
    }

    private bool WasPhaseTwoStabPressedThisFrame()
    {
        return WasSensorTriggeredThisFrame(sensorPhaseTwoStabFrame) || WasActionPressedThisFrame(phaseTwoStabAction) || Input.GetKeyDown(phaseTwoStabKey);
    }

    private bool WasSensorTriggeredThisFrame(int triggeredFrame)
    {
        return triggeredFrame == Time.frameCount;
    }

    private bool IsActionPressed(InputAction action)
    {
        return action != null && action.IsPressed();
    }

    private bool WasActionPressedThisFrame(InputAction action)
    {
        return action != null && action.WasPressedThisFrame();
    }

    private void AutoAssignReferences()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>() ?? FindObjectOfType<PlayerStats>(true);

        if (bullTarget == null)
            bullTarget = transform;

        if (shooterCharacter == null)
            shooterCharacter = GetComponent<Character>();

        if (axisMovementField == null)
            axisMovementField = typeof(Character).GetField("axisMovement", BindingFlags.Instance | BindingFlags.NonPublic);

        if (holdingButtonRunField == null)
            holdingButtonRunField = typeof(Character).GetField("holdingButtonRun", BindingFlags.Instance | BindingFlags.NonPublic);

        if (gameFlow == null)
            gameFlow = FindObjectOfType<BullfightGameFlow>(true);
    }

    private void RefreshBullfightActions()
    {
        if (!isActiveAndEnabled)
            return;

        if (bullfightActionsAsset == null)
            return;

        if (runtimeBullfightActions != null)
        {
            runtimeBullfightActions.Disable();
            Destroy(runtimeBullfightActions);
        }

        runtimeBullfightActions = Instantiate(bullfightActionsAsset);
        runtimeBullfightActions.Enable();

        holdAction = runtimeBullfightActions.FindAction("player/hold");
        swingAction = runtimeBullfightActions.FindAction("player/swing");
        attackAction = runtimeBullfightActions.FindAction("player/attack");
        dashAction = runtimeBullfightActions.FindAction("player/dash");
        phaseTwoCalibrationAction = runtimeBullfightActions.FindAction("player/phaseTwoCalibration");
        phaseTwoStabAction = runtimeBullfightActions.FindAction("player/phaseTwoStab");
    }

    private void FreezeMovementWhileHoldingCloth()
    {
        if (playerStats == null || !playerStats.isHoldingCloth)
            return;

        AutoAssignReferences();

        if (shooterCharacter == null)
            return;

        axisMovementField?.SetValue(shooterCharacter, Vector2.zero);
        holdingButtonRunField?.SetValue(shooterCharacter, false);
    }

    private bool IsPhaseTwoInputMode()
    {
        return gameFlow != null && gameFlow.currentPhase == BullfightGameFlow.GamePhase.PhaseTwo;
    }
}
