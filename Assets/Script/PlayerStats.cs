using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using InfimaGames.LowPolyShooterPack;

public class PlayerStats : MonoBehaviour
{
    public event Action OnCapaPerformed;
    public event Action OnDashPerformed;
    public event Action OnEvadePerformed;
    public event Action OnBanderillasPerformed;
    public event Action<float> OnDamaged;
    public event Action<bool> OnHoldingClothChanged;
    public event Action<bool> OnStunStateChanged;
    public event Action<bool> OnPerfectDodgeBuffStateChanged;

    [Header("Health")]
    public float maxHealth = 500f;
    public float currentHealth;
    public Slider healthBar;
    public Slider staminaBar;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float holdClothRecoveryPerSecond = 15f;
    public float capaCost = 15f;
    [FormerlySerializedAs("evadeCost")] public float dashCost = 25f;
    public float banderillasCost = 60f;
    public float stunDuration = 2f;

    [Header("Dash")]
    public float dashDistance = 2.25f;
    public float dashDuration = 0.18f;
    [SerializeField] private Animator dashAnimator;
    [SerializeField] private string dashAnimationStateName = "Dash";

    [Header("Hit Reaction")]
    public float knockbackDistance = 0.1f;
    public float knockbackDuration = 0.08f;

    [Header("Perfect Reward")]
    public float perfectDodgeDuration = 2f;
    public float perfectDodgeSpeedMultiplier = 1.25f;
    public float perfectDodgeStaminaRestore = 40f;

    [Header("Debug")]
    public bool isStunned;
    public bool isActing;
    public bool isTaunting;
    public bool isHoldingCloth;

    [Header("Input")]
    [SerializeField] private InputActionAsset bullfightActionsAsset;

    private Rigidbody rigidBody;
    private float stunTimer;
    private float invulnerabilityTimer;
    private float dashTimer;
    private float knockbackTimer;
    private float perfectDodgeBuffTimer;
    private Vector3 dashVelocity;
    private Vector3 knockbackVelocity;
    private Transform firstPersonRoot;
    private Transform firstPersonCamera;
    private Camera gameplayCamera;
    private Character shooterCharacter;
    private BullfightPlayerController playerController;
    private BullfightStunVfx stunVfx;
    private BullAI bullAI;
    private CameraLook cameraLook;
    private Movement movementComponent;
    private bool shooterGameplayEnabled = true;
    private bool deathPresentationApplied;
    private bool cachedGameplayNearClip;
    private Quaternion cachedCameraLocalRotation = Quaternion.identity;
    private float cachedGameplayNearClipPlane;
    private bool perfectDodgeBuffActive;

    [Header("Death Presentation")]
    [SerializeField] private Vector3 deathCameraEuler = new Vector3(-70f, 0f, 0f);
    [SerializeField] private float gameplayNearClipPlane = 0.1f;

    public bool IsDead => currentHealth <= 0f;
    public bool IsDashing => dashTimer > 0f;
    public bool IsInvulnerable => invulnerabilityTimer > 0f;
    public bool IsPerfectDodgeBuffActive => perfectDodgeBuffActive;
    public float LastDashTime { get; private set; } = -999f;
    public float HealthNormalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
    public float StaminaNormalized => maxStamina <= 0f ? 0f : currentStamina / maxStamina;
    public float evadeCost => dashCost;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        isStunned = false;
        isActing = false;
        isTaunting = false;
        isHoldingCloth = false;
        rigidBody = GetComponent<Rigidbody>();
        _ = GetComponent<BullfightPlayerController>() ?? gameObject.AddComponent<BullfightPlayerController>();
        _ = GetComponent<BullfightCapeBinder>() ?? gameObject.AddComponent<BullfightCapeBinder>();
        _ = GetComponent<BullfightSpawnManager>() ?? gameObject.AddComponent<BullfightSpawnManager>();
        _ = GetComponent<BullfightHandAnimatorController>() ?? gameObject.AddComponent<BullfightHandAnimatorController>();
        _ = GetComponent<BullfightProjectileThrower>() ?? gameObject.AddComponent<BullfightProjectileThrower>();
        _ = GetComponent<BullfightStaminaPreviewUI>() ?? gameObject.AddComponent<BullfightStaminaPreviewUI>();
        _ = GetComponent<BullfightStunVfx>() ?? gameObject.AddComponent<BullfightStunVfx>();
        _ = GetComponent<BullfightAudioController>() ?? gameObject.AddComponent<BullfightAudioController>();
        _ = GetComponent<BullfightPerfectDodgeVfx>() ?? gameObject.AddComponent<BullfightPerfectDodgeVfx>();
        stunVfx = GetComponent<BullfightStunVfx>();
        shooterCharacter = GetComponent<Character>();
        movementComponent = GetComponent<Movement>();
        playerController = GetComponent<BullfightPlayerController>();
        playerController?.ConfigureInputActions(bullfightActionsAsset);
        bullAI = FindObjectOfType<BullAI>(true);
        CachePresentationReferences();
        StabilizeFirstPersonPresentation();
        UpdateHealthBar();
    }

    private void Start()
    {
        // 強制再次校準數值
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // 立即更新一次 UI
        UpdateUI();

    }

    private void Update()
    {
        UpdateStun();
        UpdateInvulnerability();
        UpdateDash();
        UpdateKnockback();
        UpdatePerfectDodgeBuff();
        UpdateStamina();
        StabilizeFirstPersonPresentation();
        UpdateUI();
    }

    public bool CanAct()
    {
        return !IsDead && !isStunned && !isActing;
    }

    public bool TrySpendStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina -= amount;
        if (currentStamina <= 0f)
            TriggerStun();

        return true;
    }

    public bool HasEnoughStamina(float amount)
    {
        return currentStamina >= Mathf.Max(0f, amount);
    }

    public float GetProjectedStamina(float spendAmount)
    {
        return Mathf.Clamp(currentStamina - Mathf.Max(0f, spendAmount), 0f, maxStamina);
    }

    public float GetProjectedStaminaNormalized(float spendAmount)
    {
        return maxStamina <= 0f ? 0f : GetProjectedStamina(spendAmount) / maxStamina;
    }

    public bool TryStartBanderillas()
    {
        if (!CanAct())
            return false;

        if (!TrySpendStamina(banderillasCost))
            return false;

        isActing = true;
        Invoke(nameof(EndActionLock), 0.6f);
        OnBanderillasPerformed?.Invoke();
        return true;
    }

    public bool TryDoCapa()
    {
        if (!CanAct() || !isHoldingCloth)
            return false;

        if (!TrySpendStamina(capaCost))
            return false;

        isActing = true;
        Invoke(nameof(EndActionLock), 0.35f);
        OnCapaPerformed?.Invoke();
        return true;
    }

    public bool TryDash()
    {
        if (!CanAct())
            return false;

        if (!TrySpendStamina(dashCost))
            return false;

        isActing = true;
        dashVelocity = ResolveDashDirection() * (dashDistance / Mathf.Max(0.01f, dashDuration));
        dashTimer = dashDuration;
        LastDashTime = Time.time;
        if (dashAnimator != null && dashAnimator.runtimeAnimatorController != null && !string.IsNullOrWhiteSpace(dashAnimationStateName))
            dashAnimator.Play(dashAnimationStateName, 0, 0f);
        OnDashPerformed?.Invoke();
        OnEvadePerformed?.Invoke();
        return true;
    }

    public bool TryEvade() => TryDash();

    public void SetHoldingCloth(bool value)
    {
        bool nextValue = !IsDead && !isStunned && value;
        if (isHoldingCloth == nextValue)
            return;

        isHoldingCloth = nextValue;
        OnHoldingClothChanged?.Invoke(isHoldingCloth);
    }

    public void ForceStun()
    {
        TriggerStun();
    }

    public void GrantInvulnerability(float duration)
    {
        invulnerabilityTimer = Mathf.Max(invulnerabilityTimer, duration);
    }

    public void TakeDamage(float amount)
    {
        // 只要有撞到，這行一定會印出來
        Debug.Log("<color=orange>【碰撞偵測】牛撞過來了！</color>");

        if (IsDead) { Debug.Log("扣血失敗：玩家已死亡"); return; }

        if (IsInvulnerable)
        {
            Debug.Log($"扣血失敗：玩家處於無敵狀態 (剩餘時間: {invulnerabilityTimer})");
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"<color=red>【受傷成功】</color> HP 剩餘: {currentHealth}");
        stunVfx?.TriggerDamageFlash();
        OnDamaged?.Invoke(amount);

        if (IsDead)
            ApplyDeathPresentation();
    }

    public void TakeBullImpact(float amount, Vector3 impactSource)
    {
        float previousHealth = currentHealth;
        TakeDamage(amount);
        if (currentHealth >= previousHealth) return;
        ApplyKnockback(impactSource, knockbackDistance, knockbackDuration);
        ForceStun();
    }

    public void ForceDeathForDebug()
    {
        if (IsDead)
            return;

        invulnerabilityTimer = 0f;
        currentHealth = 0f;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        isActing = false;
        isStunned = false;
        SetHoldingCloth(false);
        StopMovementImmediate();
        SetShooterControlEnabled(false);
        stunVfx?.TriggerDamageFlash();
        OnDamaged?.Invoke(maxHealth);
        ApplyDeathPresentation();
        UpdateUI();
        Debug.Log("Debug shortcut: player health forced to 0.");
    }

    public void RefillStaminaForDebug()
    {
        if (IsDead)
            return;

        currentStamina = Mathf.Clamp(maxStamina, 0f, maxStamina);
        UpdateUI();
        Debug.Log($"Debug shortcut: player stamina restored to {currentStamina}.");
    }

    public void AddStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0f, maxStamina);
    }

    public void SetShooterGameplayEnabled(bool enabledState)
    {
        shooterGameplayEnabled = enabledState;
        SetShooterControlEnabled(enabledState && !isStunned);
    }

    public void RewardPerfectDodge()
    {
        AddStamina(perfectDodgeStaminaRestore);
        perfectDodgeBuffTimer = Mathf.Max(perfectDodgeBuffTimer, perfectDodgeDuration);
        ApplyPerfectDodgeBuffState(perfectDodgeBuffTimer > 0f);
    }

    private void UpdateStun()
    {
        if (!isStunned)
            return;

        StopMovementImmediate();
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            isStunned = false;
            SetShooterControlEnabled(shooterGameplayEnabled);
            OnStunStateChanged?.Invoke(false);
        }
    }

    private void UpdateDash()
    {
        if (dashTimer <= 0f)
            return;

        Vector3 delta = dashVelocity * Time.deltaTime;
        if (rigidBody != null && !rigidBody.isKinematic)
            rigidBody.MovePosition(rigidBody.position + delta);
        else
            transform.position += delta;

        dashTimer -= Time.deltaTime;
        if (dashTimer > 0f)
            return;

        dashTimer = 0f;
        dashVelocity = Vector3.zero;
        isActing = false;
    }

    private void UpdateStamina()
    {
        if (IsDead)
            return;

        if (isHoldingCloth && !isActing)
            AddStamina(holdClothRecoveryPerSecond * Time.deltaTime);
    }

    private void UpdatePerfectDodgeBuff()
    {
        if (perfectDodgeBuffTimer <= 0f)
        {
            if (perfectDodgeBuffActive)
                ApplyPerfectDodgeBuffState(false);
            return;
        }

        perfectDodgeBuffTimer = Mathf.Max(0f, perfectDodgeBuffTimer - Time.deltaTime);
        ApplyPerfectDodgeBuffState(perfectDodgeBuffTimer > 0f);
    }

    private void UpdateInvulnerability()
    {
        if (invulnerabilityTimer <= 0f)
            return;

        invulnerabilityTimer -= Time.deltaTime;
    }

    private void UpdateKnockback()
    {
        if (knockbackTimer <= 0f)
            return;

        Vector3 delta = knockbackVelocity * Time.deltaTime;
        if (rigidBody != null && !rigidBody.isKinematic)
            rigidBody.MovePosition(rigidBody.position + delta);
        else
            transform.position += delta;

        knockbackTimer -= Time.deltaTime;
    }

    private void ApplyKnockback(Vector3 sourcePosition, float distance, float duration)
    {
        Vector3 direction = transform.position - sourcePosition;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            direction = transform.forward.sqrMagnitude > 0.001f ? transform.forward : Vector3.back;

        direction.Normalize();
        knockbackVelocity = direction * (distance / Mathf.Max(0.01f, duration));
        knockbackTimer = duration;
    }

    private void TriggerStun()
    {
        currentStamina = 0f;
        isStunned = true;
        isActing = false;
        dashTimer = 0f;
        dashVelocity = Vector3.zero;
        SetHoldingCloth(false);
        stunTimer = stunDuration;
        StopMovementImmediate();
        SetShooterControlEnabled(false);
        OnStunStateChanged?.Invoke(true);
        Debug.Log("Player is stunned.");
    }

    private void ApplyPerfectDodgeBuffState(bool active)
    {
        if (movementComponent == null)
            movementComponent = GetComponent<Movement>();

        if (movementComponent != null)
            movementComponent.SetExternalSpeedMultiplier(active ? perfectDodgeSpeedMultiplier : 1f);

        if (perfectDodgeBuffActive == active)
            return;

        perfectDodgeBuffActive = active;
        OnPerfectDodgeBuffStateChanged?.Invoke(active);
    }

    private void EndActionLock()
    {
        isActing = false;
    }
    private void UpdateUI()
    {
        // 更新血條百分比 (0.0 ~ 1.0)
        if (healthBar != null)
            healthBar.value = HealthNormalized;

        // 更新體力條百分比 (0.0 ~ 1.0)
        if (staminaBar != null)
            staminaBar.value = StaminaNormalized;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = HealthNormalized;
    }

    private void CachePresentationReferences()
    {
        if (firstPersonRoot == null)
            firstPersonRoot = transform.Find("SK_FP_CH_Default_Root");

        if (firstPersonCamera == null && firstPersonRoot != null)
        {
            Transform socketCamera = firstPersonRoot.Find("Armature/root/pelvis/spine_01/spine_02/spine_03/neck_01/head/SOCKET_Camera/Camera");
            if (socketCamera != null)
            {
                firstPersonCamera = socketCamera;
                cachedCameraLocalRotation = firstPersonCamera.localRotation;
            }
        }

        if (gameplayCamera == null)
            gameplayCamera = shooterCharacter != null ? shooterCharacter.GetCameraWorld() : null;

        if (gameplayCamera == null && firstPersonCamera != null)
            gameplayCamera = firstPersonCamera.GetComponent<Camera>();

        if (cameraLook == null && firstPersonRoot != null)
            cameraLook = firstPersonRoot.GetComponent<CameraLook>();
    }

    private void StabilizeFirstPersonPresentation()
    {
        if (firstPersonRoot != null)
        {
            firstPersonRoot.localPosition = new Vector3(0f, 1.8f, 0f);
            firstPersonRoot.localRotation = Quaternion.identity;
        }

        if (gameplayCamera != null)
        {
            if (!cachedGameplayNearClip)
            {
                cachedGameplayNearClipPlane = gameplayCamera.nearClipPlane;
                cachedGameplayNearClip = true;
            }

            gameplayCamera.nearClipPlane = Mathf.Max(gameplayCamera.nearClipPlane, gameplayNearClipPlane);
        }

        if (IsDead)
            ApplyDeathPresentation();
    }

    private void ApplyDeathPresentation()
    {
        if (deathPresentationApplied)
            return;

        CachePresentationReferences();

        if (cameraLook != null)
            cameraLook.enabled = false;

        if (firstPersonCamera != null)
            firstPersonCamera.localRotation = cachedCameraLocalRotation * Quaternion.Euler(deathCameraEuler);

        deathPresentationApplied = true;
    }

    private void SetShooterControlEnabled(bool enabledState)
    {
        if (shooterCharacter != null)
            shooterCharacter.enabled = enabledState;
    }

    private void StopMovementImmediate()
    {
        if (playerController == null)
            playerController = GetComponent<BullfightPlayerController>();

        playerController?.ForceStopMovement();

        if (rigidBody != null)
        {
            Vector3 velocity = rigidBody.velocity;
            velocity.x = 0f;
            velocity.z = 0f;
            rigidBody.velocity = velocity;
        }
    }

    public bool TryGetFirstPersonCameraPoint(out Vector3 cameraPoint)
    {
        CachePresentationReferences();

        Transform reference = gameplayCamera != null ? gameplayCamera.transform : firstPersonCamera;
        if (reference == null)
        {
            cameraPoint = Vector3.zero;
            return false;
        }

        cameraPoint = reference.position;
        return true;
    }

    private Vector3 ResolveDashDirection()
    {
        Vector2 movementInput = shooterCharacter != null
            ? shooterCharacter.GetInputMovement()
            : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Transform reference = gameplayCamera != null ? gameplayCamera.transform : transform;
        Vector3 forward = reference.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude <= 0.0001f)
            forward = transform.forward;
        forward.Normalize();

        Vector3 right = reference.right;
        right.y = 0f;
        if (right.sqrMagnitude <= 0.0001f)
            right = transform.right;
        right.Normalize();

        Vector3 dashDirection = (forward * movementInput.y) + (right * movementInput.x);
        dashDirection.y = 0f;
        if (dashDirection.sqrMagnitude > 0.0001f)
            return dashDirection.normalized;

        if (bullAI == null)
            bullAI = FindObjectOfType<BullAI>(true);

        if (bullAI != null)
        {
            Vector3 awayFromBull = transform.position - bullAI.transform.position;
            awayFromBull.y = 0f;
            if (awayFromBull.sqrMagnitude > 0.0001f)
                return awayFromBull.normalized;
        }

        return forward.sqrMagnitude > 0.0001f ? forward : Vector3.forward;
    }
}
