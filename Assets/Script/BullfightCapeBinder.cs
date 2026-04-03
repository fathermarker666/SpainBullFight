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

    public Transform LeftHandAnchor => leftHandAnchor;
    public Transform CapeInstance => capeInstance;

    private void Awake()
    {
        AutoAssignReferences();
        EnsureCapeInstance();
        ApplyCapeTransform();
        UpdateCapeVisibility(forceHide: true);
    }

    private void Update()
    {
        AutoAssignReferences();
        EnsureCapeInstance();
        ApplyCapeTransform();
        UpdateCapeVisibility(forceHide: false);
    }

    private void AutoAssignReferences()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (handAnimatorController == null)
            handAnimatorController = GetComponent<BullfightHandAnimatorController>();

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
        if (capeInstance.gameObject.activeSelf != visible)
            capeInstance.gameObject.SetActive(visible);
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
