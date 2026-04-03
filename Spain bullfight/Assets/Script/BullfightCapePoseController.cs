using UnityEngine;

public class BullfightCapePoseController : MonoBehaviour
{
    public PlayerStats playerStats;

    [Header("Blend")]
    public float poseBlendSpeed = 10f;
    public float swingPoseDuration = 0.22f;

    [Header("Hold Pose")]
    public Vector3 holdUpperArmEuler = new Vector3(308f, 252f, 330f);
    public Vector3 holdLowerArmEuler = new Vector3(24f, 286f, 92f);
    public Vector3 holdHandEuler = new Vector3(14f, 94f, 248f);

    [Header("Swing Pose")]
    public Vector3 swingUpperArmEuler = new Vector3(286f, 248f, 338f);
    public Vector3 swingLowerArmEuler = new Vector3(352f, 274f, 116f);
    public Vector3 swingHandEuler = new Vector3(6f, 126f, 252f);

    private Transform upperArm;
    private Transform lowerArm;
    private Transform hand;

    private Quaternion defaultUpperArmRotation;
    private Quaternion defaultLowerArmRotation;
    private Quaternion defaultHandRotation;

    private float swingTimer;
    private bool poseCached;
    private BullfightHandAnimatorController handAnimatorController;
    private PlayerStats subscribedPlayerStats;

    private void Awake()
    {
        ResolveReferencesIfNeeded();
        EnsureBonesCachedIfNeeded();
    }

    private void OnEnable()
    {
        ResolveReferencesIfNeeded();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void LateUpdate()
    {
        if (HasMissingReferences())
            ResolveReferencesIfNeeded();

        if (subscribedPlayerStats != playerStats)
            Subscribe();

        if (handAnimatorController != null && handAnimatorController.enabled)
            return;

        EnsureBonesCachedIfNeeded();

        if (!poseCached)
            return;

        if (swingTimer > 0f)
            swingTimer -= Time.deltaTime;

        bool holdingCloth = playerStats != null && playerStats.isHoldingCloth;
        bool useHoldPose = holdingCloth || swingTimer > 0f;

        Quaternion targetUpper = useHoldPose ? Quaternion.Euler(swingTimer > 0f ? swingUpperArmEuler : holdUpperArmEuler) : defaultUpperArmRotation;
        Quaternion targetLower = useHoldPose ? Quaternion.Euler(swingTimer > 0f ? swingLowerArmEuler : holdLowerArmEuler) : defaultLowerArmRotation;
        Quaternion targetHand = useHoldPose ? Quaternion.Euler(swingTimer > 0f ? swingHandEuler : holdHandEuler) : defaultHandRotation;

        upperArm.localRotation = Quaternion.Slerp(upperArm.localRotation, targetUpper, Time.deltaTime * poseBlendSpeed);
        lowerArm.localRotation = Quaternion.Slerp(lowerArm.localRotation, targetLower, Time.deltaTime * poseBlendSpeed);
        hand.localRotation = Quaternion.Slerp(hand.localRotation, targetHand, Time.deltaTime * poseBlendSpeed);
    }

    private void HandleCapaPerformed()
    {
        swingTimer = swingPoseDuration;
    }

    private bool HasMissingReferences()
    {
        return playerStats == null || handAnimatorController == null;
    }

    private void ResolveReferencesIfNeeded()
    {
        if (playerStats == null)
            playerStats = BullfightSceneCache.GetLocalOrScene<PlayerStats>(this);

        if (handAnimatorController == null)
            handAnimatorController = GetComponent<BullfightHandAnimatorController>() ?? BullfightSceneCache.GetLocalOrScene<BullfightHandAnimatorController>(this);
    }

    private void Subscribe()
    {
        if (playerStats == null)
            return;

        if (subscribedPlayerStats == playerStats)
            return;

        Unsubscribe();
        playerStats.OnCapaPerformed += HandleCapaPerformed;
        subscribedPlayerStats = playerStats;
    }

    private void Unsubscribe()
    {
        if (subscribedPlayerStats == null)
            return;

        subscribedPlayerStats.OnCapaPerformed -= HandleCapaPerformed;
        subscribedPlayerStats = null;
    }

    private void EnsureBonesCachedIfNeeded()
    {
        if (poseCached && upperArm != null && lowerArm != null && hand != null)
            return;

        poseCached = false;
        upperArm = FindChildRecursive(transform, "upperarm_l");
        lowerArm = FindChildRecursive(transform, "lowerarm_l");
        hand = FindChildRecursive(transform, "hand_l");

        if (upperArm == null || lowerArm == null || hand == null)
            return;

        defaultUpperArmRotation = upperArm.localRotation;
        defaultLowerArmRotation = lowerArm.localRotation;
        defaultHandRotation = hand.localRotation;
        poseCached = true;
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
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
}
