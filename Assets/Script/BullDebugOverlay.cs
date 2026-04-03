using UnityEngine;

public class BullDebugOverlay : MonoBehaviour
{
    [SerializeField] private Rect panelRect = new Rect(12f, 12f, 260f, 172f);
    [SerializeField] private int fontSize = 11;

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
        playerStats = FindObjectOfType<PlayerStats>(true);
        timingRing = FindObjectOfType<BullTimingRing>(true);
        handAnimatorController = FindObjectOfType<BullfightHandAnimatorController>(true);
    }

    private void OnGUI()
    {
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

        string debugText =
            $"Bull State: {bullAI.currentState}\n" +
            $"Target: {targetName}\n" +
            $"Distance: {distance:F2}\n" +
            $"Bull HP: {(bullStats != null ? bullStats.currentHealth : 0f):F0}\n" +
            $"Bull Rage: {(bullStats != null ? bullStats.currentRage : 0f):F2}\n" +
            $"QTE Progress: {(timingRing != null ? timingRing.CurrentProgress : 0f):F2}\n" +
            $"Player HP: {(playerStats != null ? playerStats.currentHealth : 0f):F0}\n" +
            $"Player Stamina: {(playerStats != null ? playerStats.currentStamina : 0f):F0}\n" +
            $"Holding Cloth: {(playerStats != null && playerStats.isHoldingCloth)}\n" +
            $"Stunned: {(playerStats != null && playerStats.isStunned)}\n" +
            $"Invulnerable: {(playerStats != null && playerStats.IsInvulnerable)}\n" +
            $"Can Damage Charge: {bullAI.CanDamagePlayerThisCharge}\n" +
            $"Phase: {(bullAI.gameFlow != null ? bullAI.gameFlow.currentPhase.ToString() : "None")}";

        if (handAnimatorController == null)
            handAnimatorController = FindObjectOfType<BullfightHandAnimatorController>(true);

        if (handAnimatorController != null)
            debugText += "\n" + handAnimatorController.GetDebugSummary();

        float requiredHeight = Mathf.Max(panelRect.height, boxStyle.CalcHeight(new GUIContent(debugText), panelRect.width));
        Rect resolvedRect = new Rect(panelRect.x, panelRect.y, panelRect.width, requiredHeight);

        GUI.color = Color.white;
        GUI.Box(resolvedRect, debugText, boxStyle);
    }
}
