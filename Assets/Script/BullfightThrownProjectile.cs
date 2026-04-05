using UnityEngine;

[DisallowMultipleComponent]
public class BullfightThrownProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private bool alignToVelocity = true;
    [SerializeField] private bool destroyOnCollision = true;

    private Rigidbody cachedRigidbody;
    private float remainingLifetime;
    private BullAI bullAI;
    private BullStats bullStats;
    private float damage;
    private bool damageEnabled = true;
    private bool hitResolved;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        remainingLifetime = lifetime;
    }

    public void Configure(BullAI targetBullAI, BullStats targetBullStats, float projectileDamage, bool enableDamage = true)
    {
        bullAI = targetBullAI;
        bullStats = targetBullStats;
        damage = Mathf.Max(0f, projectileDamage);
        damageEnabled = enableDamage;
    }

    private void Update()
    {
        remainingLifetime -= Time.deltaTime;
        if (remainingLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (!alignToVelocity || cachedRigidbody == null)
            return;

        Vector3 velocity = cachedRigidbody.velocity;
        if (velocity.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (damageEnabled && !hitResolved && collision.collider != null)
        {
            BullAI hitBullAI = collision.collider.GetComponentInParent<BullAI>();
            BullStats hitBullStats = collision.collider.GetComponentInParent<BullStats>();

            bool hitBull = (bullAI != null && hitBullAI == bullAI) || (bullStats != null && hitBullStats == bullStats);
            if (hitBull)
            {
                if (bullAI != null)
                    bullAI.RegisterBanderillasHit(damage);
                else if (bullStats != null)
                    bullStats.TakeDamage(damage);

                hitResolved = true;
            }
        }

        if (!destroyOnCollision)
            return;

        Destroy(gameObject);
    }
}
