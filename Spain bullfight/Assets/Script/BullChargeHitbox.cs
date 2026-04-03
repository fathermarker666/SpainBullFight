using UnityEngine;

public class BullChargeHitbox : MonoBehaviour
{
    private BullAI owner;

    public void Initialize(BullAI bullAI)
    {
        owner = bullAI;
    }

    private void OnTriggerEnter(Collider other)
    {
        owner?.TryHitPlayerFromCharge(other);
    }

    private void OnTriggerStay(Collider other)
    {
        owner?.TryHitPlayerFromCharge(other);
    }
}
