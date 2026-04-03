#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Text;
using UnityEngine;

public class BullDebugOverlay : MonoBehaviour
{
    [SerializeField] private Rect panelRect = new Rect(12f, 12f, 260f, 172f);
    [SerializeField] private int fontSize = 11;

    private readonly StringBuilder debugBuilder = new StringBuilder(320);
    private BullAI bullAI;
    private PlayerStats playerStats;
    private BullStats bullStats;
    private BullTimingRing timingRing;
    private BullfightHandAnimatorController handAnimatorController;
    private GUIStyle boxStyle;

    private void Awake()
    {
        bullAI = GetComponent<BullAI>();
        bullStats = GetComponent<BullStats>();
        playerStats = BullfightSceneCache.FindObject<PlayerStats>();
        timingRing = BullfightSceneCache.FindObject<BullTimingRing>();
        handAnimatorController = BullfightSceneCache.FindObject<BullfightHandAnimatorController>();
    }

    private void OnGUI()
    {
        if (Event.current == null || Event.current.type != EventType.Repaint)
            return;

        if (bullAI == null)
            return;

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = fontSize,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                clipping = TextClipping.Clip,
                padding = new RectOffset(8, 8, 8, 8)
            };
        }

        float distance = -1f;
        string targetName = "None";
        if (bullAI.player != null)
        {
            distance = Vector3.Distance(bullAI.transform.position, bullAI.player.position);
            targetName = bullAI.player.name;
        }

        debugBuilder.Clear();
        debugBuilder.Append("Bull State: ").Append(bullAI.currentState);
        debugBuilder.Append("\nTarget: ").Append(targetName);
        debugBuilder.Append("\nDistance: ").Append(distance.ToString("F2"));
        debugBuilder.Append("\nBull HP: ").Append((bullStats != null ? bullStats.currentHealth : 0f).ToString("F0"));
        debugBuilder.Append("\nBull Rage: ").Append((bullStats != null ? bullStats.currentRage : 0f).ToString("F2"));
        debugBuilder.Append("\nQTE Progress: ").Append((timingRing != null ? timingRing.CurrentProgress : 0f).ToString("F2"));
        debugBuilder.Append("\nPlayer HP: ").Append((playerStats != null ? playerStats.currentHealth : 0f).ToString("F0"));
        debugBuilder.Append("\nPlayer Stamina: ").Append((playerStats != null ? playerStats.currentStamina : 0f).ToString("F0"));
        debugBuilder.Append("\nHolding Cloth: ").Append(playerStats != null && playerStats.isHoldingCloth);
        debugBuilder.Append("\nStunned: ").Append(playerStats != null && playerStats.isStunned);
        debugBuilder.Append("\nInvulnerable: ").Append(playerStats != null && playerStats.IsInvulnerable);
        debugBuilder.Append("\nCan Damage Charge: ").Append(bullAI.CanDamagePlayerThisCharge);
        debugBuilder.Append("\nPhase: ").Append(bullAI.gameFlow != null ? bullAI.gameFlow.currentPhase.ToString() : "None");

        if (handAnimatorController != null)
            debugBuilder.Append('\n').Append(handAnimatorController.GetDebugSummary());

        string debugText = debugBuilder.ToString();
        float requiredHeight = Mathf.Max(panelRect.height, boxStyle.CalcHeight(new GUIContent(debugText), panelRect.width));
        Rect resolvedRect = new Rect(panelRect.x, panelRect.y, panelRect.width, requiredHeight);

        GUI.color = Color.white;
        GUI.Box(resolvedRect, debugText, boxStyle);
    }
}
#else
using UnityEngine;

public class BullDebugOverlay : MonoBehaviour
{
}
#endif
