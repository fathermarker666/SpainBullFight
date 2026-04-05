using UnityEngine;
using UnityEngine.UI;

public class BullStats : MonoBehaviour
{
    public event System.Action<float> OnDamaged;
    public event System.Action OnDefeated;

    [Header("Health")]
    public float maxHealth = 300f;
    public float currentHealth;
    public Slider healthBar;

    [Header("Rage")]
    public float baseRage = 0.2f;
    public float rageFromDamage = 0.15f;
    public float rageFromTauntPerSecond = 0.12f;
    public float currentRage;
    public float chargeSpeedMultiplierMax = 1.1f;
    public float approachSpeedMultiplierMax = 1.08f;

    public bool InFinalPhase => currentHealth <= maxHealth / 3f;
    public float HealthNormalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;

    private void Awake()
    {
        chargeSpeedMultiplierMax = 1.1f;
        approachSpeedMultiplierMax = 1.08f;
        currentHealth = maxHealth;
        currentRage = baseRage;
        if (healthBar == null)
            healthBar = GetComponentInChildren<Slider>(true);

        UpdateHealthBar();
    }

    private void Update()
    {
        currentRage = Mathf.Clamp01(currentRage);
    }

    public void TakeDamage(float amount)
    {
        ApplyHealthChange(currentHealth - Mathf.Max(0f, amount), $"Bull health: {{0}}/{maxHealth}", true);
    }

    public void SetHealth(float value)
    {
        ApplyHealthChange(value, $"Bull health set: {{0}}/{maxHealth}", false);
    }

    public void ApplyDebugDamage(float amount)
    {
        SetHealth(currentHealth - Mathf.Max(0f, amount));
    }

    public void ResetCombatState()
    {
        currentHealth = maxHealth;
        currentRage = baseRage;
        UpdateHealthBar();
    }

    public void AddTauntRage(float deltaTime)
    {
        currentRage = Mathf.Clamp01(currentRage + rageFromTauntPerSecond * deltaTime);
    }

    public float GetChargeSpeedMultiplier()
    {
        return Mathf.Lerp(1f, chargeSpeedMultiplierMax, currentRage);
    }

    public float GetApproachSpeedMultiplier()
    {
        return Mathf.Lerp(1f, approachSpeedMultiplierMax, currentRage);
    }

    public float GetChargeDelayMultiplier()
    {
        return Mathf.Lerp(1f, 0.55f, currentRage);
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = HealthNormalized;
    }

    private void ApplyHealthChange(float targetHealth, string logFormat, bool addDamageRage)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(targetHealth, 0f, maxHealth);

        if (addDamageRage && currentHealth < previousHealth)
            currentRage = Mathf.Clamp01(currentRage + rageFromDamage);

        UpdateHealthBar();
        Debug.Log(string.Format(logFormat, currentHealth));

        float damageTaken = Mathf.Max(0f, previousHealth - currentHealth);
        if (damageTaken > 0f)
            OnDamaged?.Invoke(damageTaken);

        if (previousHealth > 0f && currentHealth <= 0f)
            OnDefeated?.Invoke();
    }
}
