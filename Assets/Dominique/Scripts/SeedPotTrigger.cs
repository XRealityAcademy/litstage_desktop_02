using UnityEngine;

public class SeedPotTrigger : MonoBehaviour
{
    public Manager_Ch1 manager;
    [Tooltip("Must match Manager_Ch1.seedTag")]
    public string seedTag = "Seed";

    bool hasSeed = false;

    void Reset()
    {
        // Helpful defaults
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasSeed) return;
        if (!other.CompareTag(seedTag)) return;

        // Require a Rigidbody on the seed to ensure it's a real physics object
        if (!other.attachedRigidbody) return;

        hasSeed = true;
        if (manager) manager.NotifySeedPlaced();
    }
}
