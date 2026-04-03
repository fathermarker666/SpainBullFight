using UnityEngine;
using UnityEngine.Serialization;

public class BullfightSpawnManager : MonoBehaviour
{
    [Header("Names")]
    public string arenaRootName = "";
    public string playerSpawnName = "PlayerSpawnPoint";
    public string bullSpawnName = "BullSpawnPoint";

    [Header("Spawn")]
    public float defaultBullSpawnDistance = 4.2f;
    public float defaultBullSpawnSideOffset = 0f;
    public float groundProbeHeight = 12f;
    public float fallThresholdY = -2f;
    public float resetHeightOffset = 0.1f;

    [Header("Arena Safety")]
    public float floorThicknessExtra = 4f;
    public float wallPaddingExtra = 0.5f;
    public float wallThicknessExtra = 1.5f;
    [FormerlySerializedAs("arenaRadius")]
    public float bullArenaRadius = 4.1f;
    public float arenaEdgePadding = 0.25f;
    public float spawnEdgePadding = 0.25f;

    public PlayerStats playerStats;
    public BullAI bullAI;

    private Rigidbody playerRigidbody;
    private Rigidbody bullRigidbody;
    private Transform playerSpawnPoint;
    private Transform bullSpawnPoint;
    private BoxCollider arenaFloorCollider;
    private Vector3 arenaWorldCenter;
    private bool arenaAdjusted;
    private bool initialSpawnApplied;

    public Vector3 ArenaCenter => arenaFloorCollider != null ? arenaWorldCenter : (playerSpawnPoint != null ? playerSpawnPoint.position : transform.position);
    public float ArenaRadius => bullArenaRadius;
    public Transform BullSpawnPoint => bullSpawnPoint;

    private void Awake()
    {
        AutoAssignReferences();
        playerRigidbody = GetComponent<Rigidbody>();
        AdjustArenaColliders();
        CacheArenaBounds();
        EnsureSpawnPoints();
        CacheArenaBounds();
    }

    private void Start()
    {
        AutoAssignReferences();
        EnsureSpawnPoints();
        CacheArenaBounds();
        ApplyInitialSpawn();
    }

    private void LateUpdate()
    {
        AutoAssignReferences();

        if (!initialSpawnApplied)
            ApplyInitialSpawn();

        if (transform.position.y < fallThresholdY)
            ResetPlayerToSpawn();

        if (bullAI != null && bullAI.transform.position.y < fallThresholdY)
            ResetBullToSpawn();
    }

    public void ResetPlayerToSpawn()
    {
        if (playerSpawnPoint == null)
            EnsureSpawnPoints();

        if (playerSpawnPoint == null)
            return;

        Vector3 targetPosition = playerSpawnPoint.position + Vector3.up * resetHeightOffset;
        Quaternion targetRotation = playerSpawnPoint.rotation;

        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.position = targetPosition;
            playerRigidbody.rotation = targetRotation;
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);
    }

    public void ResetBullToSpawn()
    {
        if (bullAI == null)
            return;

        if (bullSpawnPoint == null)
            EnsureSpawnPoints();

        if (bullSpawnPoint == null)
            return;

        if (bullRigidbody == null)
            bullRigidbody = bullAI.GetComponent<Rigidbody>();

        Vector3 targetPosition = bullSpawnPoint.position + Vector3.up * resetHeightOffset;
        Quaternion targetRotation = bullSpawnPoint.rotation;

        if (bullRigidbody != null)
        {
            if (!bullRigidbody.isKinematic)
            {
                bullRigidbody.velocity = Vector3.zero;
                bullRigidbody.angularVelocity = Vector3.zero;
            }

            bullRigidbody.position = targetPosition;
            bullRigidbody.rotation = targetRotation;
        }

        bullAI.transform.SetPositionAndRotation(targetPosition, targetRotation);
        bullAI.ResetCombatState();
    }

    public void ResetBullForPhaseTwoRound(float frontDistance = 2.2f, float sideOffset = 0f)
    {
        if (bullAI == null)
            return;

        EnsureSpawnPoints();
        if (bullSpawnPoint == null)
            return;

        Vector3 forward = GetViewForward();
        Vector3 right = new Vector3(forward.z, 0f, -forward.x);
        Vector3 playerOrigin = transform.position;
        float clampedDistance = Mathf.Max(1.25f, frontDistance);
        Vector3 candidate = playerOrigin + forward * clampedDistance + right * sideOffset;
        candidate = ClampToArena(candidate, spawnEdgePadding);
        candidate = SampleGround(candidate, bullAI.transform);

        Vector3 lookDirection = playerOrigin - candidate;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude < 0.001f)
            lookDirection = -forward;

        bullSpawnPoint.position = candidate;
        bullSpawnPoint.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        ResetBullToSpawn();
    }

    private void ApplyInitialSpawn()
    {
        EnsureSpawnPoints();
        ResetPlayerToSpawn();
        if (bullAI != null)
            ResetBullToSpawn();

        initialSpawnApplied = bullAI != null && playerSpawnPoint != null && bullSpawnPoint != null;
    }

    private void EnsureSpawnPoints()
    {
        if (playerSpawnPoint == null)
            playerSpawnPoint = GameObject.Find(playerSpawnName)?.transform;

        if (bullSpawnPoint == null)
            bullSpawnPoint = GameObject.Find(bullSpawnName)?.transform;

        if (playerSpawnPoint == null)
        {
            GameObject anchor = new GameObject(playerSpawnName);
            anchor.transform.position = SampleGround(transform.position, transform);
            anchor.transform.rotation = transform.rotation;
            playerSpawnPoint = anchor.transform;
        }

        if (bullSpawnPoint == null)
        {
            GameObject anchor = new GameObject(bullSpawnName);
            Vector3 forward = GetViewForward();
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            float spawnDistance = Mathf.Min(defaultBullSpawnDistance, Mathf.Max(2f, bullArenaRadius - spawnEdgePadding));
            Vector3 candidate = playerSpawnPoint.position + forward * spawnDistance + right * defaultBullSpawnSideOffset;
            candidate = ClampToArena(candidate, spawnEdgePadding);
            anchor.transform.position = SampleGround(candidate, bullAI != null ? bullAI.transform : null);
            anchor.transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
            bullSpawnPoint = anchor.transform;
        }
        else
        {
            Vector3 clampedBullSpawn = ClampToArena(bullSpawnPoint.position, spawnEdgePadding);
            bullSpawnPoint.position = SampleGround(clampedBullSpawn, bullAI != null ? bullAI.transform : null);
        }
    }

    private void CacheArenaBounds()
    {
        BoxCollider bestFloor = FindLargestArenaFloorCollider();
        if (bestFloor == null)
            return;

        arenaFloorCollider = bestFloor;
        Bounds bounds = bestFloor.bounds;
        arenaWorldCenter = bounds.center;
        arenaWorldCenter.y = playerSpawnPoint != null ? playerSpawnPoint.position.y : transform.position.y;

        float derivedRadius = Mathf.Min(bounds.extents.x, bounds.extents.z) - arenaEdgePadding;
        bullArenaRadius = Mathf.Max(1.5f, Mathf.Min(bullArenaRadius, derivedRadius));
    }

    private void AdjustArenaColliders()
    {
        if (arenaAdjusted)
            return;

        GameObject arenaRoot = FindArenaRoot();
        if (arenaRoot == null)
            return;

        BoxCollider[] colliders = arenaRoot.GetComponentsInChildren<BoxCollider>(true);
        foreach (BoxCollider box in colliders)
        {
            Vector3 size = box.size;
            Vector3 center = box.center;

            if (size.y <= 1.25f)
            {
                size.y += floorThicknessExtra;
                center.y -= floorThicknessExtra * 0.5f;
            }
            else
            {
                size.x += wallPaddingExtra;
                size.z += wallPaddingExtra;
                size.y += wallThicknessExtra;
            }

            box.size = size;
            box.center = center;
        }

        arenaAdjusted = true;
    }

    private GameObject FindArenaRoot()
    {
        GameObject arenaRoot = GameObject.Find(arenaRootName);
        if (arenaRoot != null)
            return arenaRoot;

        GameObject best = null;
        int bestScore = 0;
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            int score = root.GetComponentsInChildren<BoxCollider>(true).Length;
            if (score > bestScore)
            {
                best = root;
                bestScore = score;
            }
        }

        return best;
    }

    private BoxCollider FindLargestArenaFloorCollider()
    {
        GameObject arenaRoot = FindArenaRoot();
        if (arenaRoot == null)
            return null;

        BoxCollider best = null;
        float bestArea = -1f;

        foreach (BoxCollider box in arenaRoot.GetComponentsInChildren<BoxCollider>(true))
        {
            if (box == null || !box.enabled)
                continue;

            Bounds bounds = box.bounds;
            float area = bounds.size.x * bounds.size.z;
            if (area <= bestArea)
                continue;

            best = box;
            bestArea = area;
        }

        return best;
    }

    private Vector3 SampleGround(Vector3 preferredPosition, Transform ignoredRoot)
    {
        Vector3 origin = preferredPosition + Vector3.up * groundProbeHeight;
        float maxDistance = groundProbeHeight * 2f;
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        float closestDistance = float.MaxValue;
        Vector3 bestPoint = preferredPosition;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            Transform hitTransform = hit.collider.transform;
            if (ignoredRoot != null && (hitTransform == ignoredRoot || hitTransform.IsChildOf(ignoredRoot)))
                continue;

            if (hit.normal.y < 0.2f)
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                bestPoint = hit.point;
                found = true;
            }
        }

        if (!found)
            bestPoint.y = preferredPosition.y;

        return bestPoint;
    }

    private Vector3 ClampToArena(Vector3 position, float padding = 0f)
    {
        Vector3 center = ArenaCenter;
        Vector3 flatOffset = new Vector3(position.x - center.x, 0f, position.z - center.z);
        float clampedRadius = Mathf.Max(1f, bullArenaRadius - padding);
        if (flatOffset.magnitude > clampedRadius)
            flatOffset = flatOffset.normalized * clampedRadius;

        return new Vector3(center.x + flatOffset.x, position.y, center.z + flatOffset.z);
    }

    private Vector3 GetViewForward()
    {
        Transform reference = Camera.main != null ? Camera.main.transform : transform;
        Vector3 forward = reference.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = transform.forward;

        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();
        return forward;
    }

    private void AutoAssignReferences()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>() ?? FindObjectOfType<PlayerStats>(true);

        if (bullAI == null)
            bullAI = FindObjectOfType<BullAI>(true);

        if (bullRigidbody == null && bullAI != null)
            bullRigidbody = bullAI.GetComponent<Rigidbody>();
    }
}
