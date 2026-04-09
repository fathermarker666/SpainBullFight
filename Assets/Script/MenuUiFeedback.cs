using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public static class MenuUiFeedback
{
    public static void PlayOneShot(Component owner, ref AudioSource audioSource, AudioClip clip, float volume)
    {
        if (owner == null || clip == null)
            return;

        if (audioSource == null)
        {
            audioSource = owner.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = owner.gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public static void TriggerRumble(MonoBehaviour owner, ref Coroutine rumbleRoutine, float lowFrequency, float highFrequency, float duration)
    {
        if (owner == null || Gamepad.current == null)
            return;

        if (rumbleRoutine != null)
            owner.StopCoroutine(rumbleRoutine);

        rumbleRoutine = owner.StartCoroutine(RumbleRoutine(
            Mathf.Clamp01(lowFrequency),
            Mathf.Clamp01(highFrequency),
            Mathf.Max(0f, duration)));
    }

    private static IEnumerator RumbleRoutine(float lowFrequency, float highFrequency, float duration)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
            yield break;

        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        gamepad.ResetHaptics();
    }
}
