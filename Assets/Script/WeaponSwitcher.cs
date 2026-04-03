using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    private enum SwordVisibilityMode
    {
        PhaseTwoOnly,
        PhaseTwoStateDriven,
        HandAnimationStateDriven
    }

    public GameObject originalGun;
    public GameObject mySword;
    public BullfightGameFlow gameFlow;
    [SerializeField] private BullfightHandAnimatorController handAnimatorController;

    [Header("Visibility")]
    [SerializeField] private SwordVisibilityMode swordVisibilityMode = SwordVisibilityMode.HandAnimationStateDriven;
    [SerializeField] private bool showDuringPhaseTwoIntro;
    [SerializeField] private bool showDuringPhaseTwoCalibration;
    [SerializeField] private bool showDuringRoundPrepare = true;
    [SerializeField] private bool showDuringRoundWindow = true;
    [SerializeField] private bool showDuringRoundResolve = true;
    [SerializeField] private bool hideOriginalGunWhenSwordVisible = true;

    [Header("Sword Offset")]
    public Vector3 swordOffsetPos = Vector3.zero;
    public Vector3 swordOffsetRot = Vector3.zero;

    private Vector3 cachedOriginalGunScale = Vector3.one;
    private bool originalGunScaleCached;

    private void Awake()
    {
        CacheOriginalGunScale();
    }

    private void Update()
    {
        if (gameFlow == null)
            gameFlow = FindObjectOfType<BullfightGameFlow>(true);

        if (handAnimatorController == null)
            handAnimatorController = GetComponent<BullfightHandAnimatorController>() ?? FindObjectOfType<BullfightHandAnimatorController>(true);

        if (originalGun != null)
        {
            CacheOriginalGunScale();
            originalGun.transform.localScale = ShouldHideOriginalGun() ? Vector3.zero : cachedOriginalGunScale;
        }

        if (mySword == null)
            return;

        bool showSword = ShouldShowSword();
        mySword.SetActive(showSword);

        if (!showSword)
            return;

        mySword.transform.localPosition = swordOffsetPos;
        mySword.transform.localRotation = Quaternion.Euler(swordOffsetRot);
    }

    private bool ShouldHideOriginalGun()
    {
        return hideOriginalGunWhenSwordVisible && ShouldShowSword();
    }

    private bool ShouldShowSword()
    {
        if (swordVisibilityMode == SwordVisibilityMode.HandAnimationStateDriven)
            return handAnimatorController != null && handAnimatorController.ShouldShowSwordProp();

        if (gameFlow == null || gameFlow.currentPhase != BullfightGameFlow.GamePhase.PhaseTwo)
            return false;

        if (swordVisibilityMode == SwordVisibilityMode.PhaseTwoOnly)
            return true;

        return gameFlow.CurrentPhaseTwoState switch
        {
            BullfightGameFlow.PhaseTwoState.Intro => showDuringPhaseTwoIntro,
            BullfightGameFlow.PhaseTwoState.Calibration => showDuringPhaseTwoCalibration,
            BullfightGameFlow.PhaseTwoState.RoundPrepare => showDuringRoundPrepare,
            BullfightGameFlow.PhaseTwoState.RoundWindow => showDuringRoundWindow,
            BullfightGameFlow.PhaseTwoState.RoundResolve => showDuringRoundResolve,
            _ => false
        };
    }

    private void CacheOriginalGunScale()
    {
        if (originalGun == null || originalGunScaleCached)
            return;

        cachedOriginalGunScale = originalGun.transform.localScale;
        originalGunScaleCached = true;
    }
}
