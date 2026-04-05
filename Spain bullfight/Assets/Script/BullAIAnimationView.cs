using UnityEngine;

public sealed class BullAIAnimationView
{
    private readonly Animator animator;
    private string currentAnimationState;

    public BullAIAnimationView(Animator animator)
    {
        this.animator = animator;
    }

    public void Reset()
    {
        currentAnimationState = null;
    }

    public void Play(string clipName, float crossFadeDuration, bool forceRestart = false)
    {
        if (string.IsNullOrWhiteSpace(clipName))
            return;

        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        if (!forceRestart && currentAnimationState == clipName)
            return;

        animator.CrossFade(clipName, crossFadeDuration, 0, 0f);
        currentAnimationState = clipName;
    }

    public void PlayState(BullAI.BullState state, bool isMovingHorizontally)
    {
        string nextState = state switch
        {
            BullAI.BullState.Idle => "Arm_Bull|Idle_1",
            BullAI.BullState.Roaming => isMovingHorizontally ? "Arm_Bull|Trot_F_IP" : "Arm_Bull|Idle_1",
            BullAI.BullState.Engaging => isMovingHorizontally ? "Arm_Bull|Trot_F_IP" : "Arm_Bull|Idle_3",
            BullAI.BullState.Telegraphing => "Arm_Bull|Attack_F_IP",
            BullAI.BullState.Charging => isMovingHorizontally ? "Arm_Bull|Run_F_IP" : "Arm_Bull|Idle_3",
            BullAI.BullState.Impact => "Arm_Bull|Idle_3",
            BullAI.BullState.Hurt => "Arm_Bull|Turn180_L_IP",
            BullAI.BullState.Fatigued => "Arm_Bull|Idle_3",
            BullAI.BullState.CirclingReset => isMovingHorizontally ? "Arm_Bull|Trot_F_IP" : "Arm_Bull|Idle_1",
            BullAI.BullState.Dead => "Arm_Bull|Death_L",
            _ => "Arm_Bull|Idle_1"
        };

        Play(nextState, 0.1f);
    }
}
