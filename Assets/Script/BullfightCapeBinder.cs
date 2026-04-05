using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BullfightCapeBinder : MonoBehaviour
{
    public PlayerStats playerStats;
    public GameObject capePrefab;
    public Transform leftHandAnchor;

    [Header("Cape Offset")]
    public Vector3 localPosition = new Vector3(-0.34f, -0.05f, 0.18f);
    public Vector3 localEulerAngles = new Vector3(10f, -22f, -34f);
    public Vector3 localScale = new Vector3(0.38f, 0.38f, 0.38f);

    private Transform capeInstance;
    private BullfightHandAnimatorController handAnimatorController;
    private Transform attachedCapeAnchor;
    private bool lastCapeVisible;
    private bool hasLastCapeVisible;

    public Transform LeftHandAnchor => leftHandAnchor;
    public Transform CapeInstance => capeInstance;

    private void Awake()
    {
        ResolveReferencesIfNeeded();
        EnsureCapeInstance();
        EnsureCapeAttachment();
        UpdateCapeVisibility(forceHide: true);
    }

    private void OnEnable()
    {
        ResolveReferencesIfNeeded();
        EnsureCapeInstance();
        EnsureCapeAttachment();
    }

    private void Update()
    {
        if (HasMissingReferences())
            ResolveReferencesIfNeeded();

        EnsureCapeInstance();
        EnsureCapeAttachment();
        UpdateCapeVisibility(forceHide: false);
    }

    private bool HasMissingReferences()
    {
        return playerStats == null ||
               handAnimatorController == null ||
               leftHandAnchor == null ||
               capePrefab == null;
    }

    private void ResolveReferencesIfNeeded()
    {
        if (playerStats == null)
            playerStats = BullfightSceneCache.GetLocalOrScene<PlayerStats>(this);

        if (handAnimatorController == null)
            handAnimatorController = GetComponent<BullfightHandAnimatorController>() ?? BullfightSceneCache.GetLocalOrScene<BullfightHandAnimatorController>(this);

        if (leftHandAnchor == null)
            leftHandAnchor = FindChildRecursive(transform, "hand_l_holder") ?? FindChildRecursive(transform, "hand_l");

#if UNITY_EDITOR
        if (capePrefab == null)
        {
            string[] guids = AssetDatabase.FindAssets("Cape t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("Cape.prefab"))
                    continue;

                capePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (capePrefab != null)
                    break;
            }
        }
#endif
    }

    private void EnsureCapeInstance()
    {
        if (capeInstance != null || leftHandAnchor == null || capePrefab == null)
            return;

        GameObject instantiated = Instantiate(capePrefab, leftHandAnchor);
        instantiated.name = "BullfightCape";
        capeInstance = instantiated.transform;
        ApplyCapeTransform();
        attachedCapeAnchor = leftHandAnchor;
        hasLastCapeVisible = false;
    }

    private void EnsureCapeAttachment()
    {
        if (capeInstance == null || leftHandAnchor == null)
            return;

        if (capeInstance.parent != leftHandAnchor)
            capeInstance.SetParent(leftHandAnchor, false);

        if (attachedCapeAnchor != leftHandAnchor)
        {
            attachedCapeAnchor = leftHandAnchor;
            ApplyCapeTransform();
        }
    }

    private void ApplyCapeTransform()
    {
        if (capeInstance == null)
            return;

        capeInstance.localPosition = localPosition;
        capeInstance.localRotation = Quaternion.Euler(localEulerAngles);
        capeInstance.localScale = localScale;
    }

    private void UpdateCapeVisibility(bool forceHide)
    {
        if (capeInstance == null)
            return;

        bool visible = !forceHide &&
                       ((handAnimatorController != null && handAnimatorController.ShouldShowClothProp()) ||
                        (handAnimatorController == null && playerStats != null && playerStats.isHoldingCloth));
        if (!hasLastCapeVisible || lastCapeVisible != visible)
        {
            lastCapeVisible = visible;
            hasLastCapeVisible = true;
            if (capeInstance.gameObject.activeSelf != visible)
                capeInstance.gameObject.SetActive(visible);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ResolveReferencesIfNeeded();
        if (capeInstance != null)
            ApplyCapeTransform();
    }
#endif

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
