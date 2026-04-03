using UnityEngine;

[DisallowMultipleComponent]
public class BullfightProjectileThrower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BullfightPlayerController playerController;
    [SerializeField] private BullAI bullAI;
    [SerializeField] private BullStats bullStats;
    [SerializeField] private BullfightGameFlow gameFlow;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnAnchor;

    [Header("Detection")]
    [SerializeField] private string projectileSpawnAnchorNameHint = "hand_r";

    [Header("Throw Timing")]
    [SerializeField] private float throwSpawnNormalizedTime = 0.55f;
    [SerializeField] private float phaseOneDamageDelay = 0.25f;

    [Header("Throw Tuning")]
    [SerializeField] private Vector3 spawnLocalOffset = new Vector3(0.12f, 0f, 0.02f);
    [SerializeField] private float projectileSpeed = 16f;
    [SerializeField] private float upwardBias = 0.05f;
    [SerializeField] private float projectileDamage = 25f;
    [SerializeField] private bool usePlaceholderWhenMissing = true;

    private bool pendingThrow;
    private bool pendingPhaseOneDamage;
    private float phaseOneDamageResolveAt = -1f;

    public float ThrowSpawnNormalizedTime => throwSpawnNormalizedTime;

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void Update()
    {
        if (!pendingPhaseOneDamage || Time.time < phaseOneDamageResolveAt)
            return;

        pendingPhaseOneDamage = false;
        phaseOneDamageResolveAt = -1f;
        ResolvePhaseOneDamage();
    }

    private void OnEnable()
    {
        AutoAssignReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnBanderillasPerformed -= HandleBanderillasPerformed;

        pendingPhaseOneDamage = false;
        phaseOneDamageResolveAt = -1f;
    }

    public void NotifyThrowAnimationReachedFrame()
    {
        if (!pendingThrow)
            return;

        AutoAssignReferences();
        SpawnProjectile();
        pendingThrow = false;
    }

    private void HandleBanderillasPerformed()
    {
        pendingThrow = true;
        if (IsPhaseOneDirectDamageActive())
        {
            pendingPhaseOneDamage = true;
            phaseOneDamageResolveAt = Time.time + phaseOneDamageDelay;
        }
    }

    private void Subscribe()
    {
        if (playerStats == null)
            return;

        playerStats.OnBanderillasPerformed -= HandleBanderillasPerformed;
        playerStats.OnBanderillasPerformed += HandleBanderillasPerformed;
    }

    private void AutoAssignReferences()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>() ?? FindObjectOfType<PlayerStats>(true);

        if (playerController == null)
            playerController = GetComponent<BullfightPlayerController>() ?? FindObjectOfType<BullfightPlayerController>(true);

        if (bullAI == null)
            bullAI = FindObjectOfType<BullAI>(true);

        if (bullStats == null)
            bullStats = bullAI != null ? bullAI.bullStats : FindObjectOfType<BullStats>(true);

        if (gameFlow == null)
            gameFlow = FindObjectOfType<BullfightGameFlow>(true);

        if (bullAI != null && bullAI.banderillasDamage > 0f)
            projectileDamage = bullAI.banderillasDamage;

        if (projectileSpawnAnchor == null)
            projectileSpawnAnchor = FindChildRecursive(transform, projectileSpawnAnchorNameHint) ?? FindChildRecursive(transform, "hand_l");
    }

    private void SpawnProjectile()
    {
        if (projectileSpawnAnchor == null)
            return;

        Vector3 spawnPosition = projectileSpawnAnchor.TransformPoint(spawnLocalOffset);
        Quaternion spawnRotation = projectileSpawnAnchor.rotation;
        GameObject projectile = projectilePrefab != null
            ? Instantiate(projectilePrefab, spawnPosition, spawnRotation)
            : (usePlaceholderWhenMissing ? CreatePlaceholderProjectile(spawnPosition, spawnRotation) : null);

        if (projectile == null)
            return;

        projectile.name = "BullfightThrownProjectile";

        Rigidbody rigidbody = projectile.GetComponent<Rigidbody>();
        if (rigidbody == null)
            rigidbody = projectile.AddComponent<Rigidbody>();

        rigidbody.useGravity = false;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        Vector3 direction = ResolveThrowDirection(spawnPosition);
        rigidbody.velocity = direction * projectileSpeed;

        IgnoreOwnerCollisions(projectile);

        BullfightThrownProjectile thrownProjectile = projectile.GetComponent<BullfightThrownProjectile>();
        if (thrownProjectile == null)
            thrownProjectile = projectile.AddComponent<BullfightThrownProjectile>();

        bool enableProjectileDamage = !IsPhaseOneDirectDamageActive();
        thrownProjectile.Configure(bullAI, bullStats, projectileDamage, enableProjectileDamage);
    }

    private void ResolvePhaseOneDamage()
    {
        AutoAssignReferences();
        if (!IsPhaseOneDirectDamageActive() || bullAI == null || bullStats == null)
            return;

        if (bullStats.currentHealth <= 0f)
            return;

        if (bullAI.CurrentDistanceToPlayer > bullAI.banderillasRange)
            return;

        bullAI.RegisterBanderillasHit(projectileDamage);
    }

    private bool IsPhaseOneDirectDamageActive()
    {
        return gameFlow == null || gameFlow.currentPhase == BullfightGameFlow.GamePhase.PhaseOne;
    }

    private Vector3 ResolveThrowDirection(Vector3 spawnPosition)
    {
        Transform bullTarget = playerController != null ? playerController.GetBullTarget() : null;
        if (bullTarget != null)
        {
            Vector3 toTarget = bullTarget.position - spawnPosition;
            toTarget.y += upwardBias;
            if (toTarget.sqrMagnitude > 0.0001f)
                return toTarget.normalized;
        }

        Transform reference = Camera.main != null ? Camera.main.transform : projectileSpawnAnchor;
        Vector3 fallback = reference != null ? reference.forward : transform.forward;
        fallback.y += upwardBias;
        return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.forward;
    }

    private static GameObject CreatePlaceholderProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        placeholder.transform.SetPositionAndRotation(position, rotation * Quaternion.Euler(90f, 0f, 0f));
        placeholder.transform.localScale = new Vector3(0.03f, 0.2f, 0.03f);

        Collider collider = placeholder.GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = false;

        Renderer renderer = placeholder.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            material.color = new Color(0.76f, 0.54f, 0.2f, 1f);
        }

        return placeholder;
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
            return null;

        foreach (Transform child in root)
        {
            if (child.name == childName)
                return child;

            Transform nested = FindChildRecursive(child, childName);
            if (nested != null)
                return nested;
        }

        return null;
    }

    private void IgnoreOwnerCollisions(GameObject projectile)
    {
        Collider[] projectileColliders = projectile.GetComponentsInChildren<Collider>(true);
        Collider[] ownerColliders = GetComponentsInChildren<Collider>(true);

        foreach (Collider projectileCollider in projectileColliders)
        {
            if (projectileCollider == null)
                continue;

            foreach (Collider ownerCollider in ownerColliders)
            {
                if (ownerCollider == null)
                    continue;

                Physics.IgnoreCollision(projectileCollider, ownerCollider, true);
            }
        }
    }
}
