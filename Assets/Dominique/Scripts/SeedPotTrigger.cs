using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SeedPotTrigger : MonoBehaviour
{
    [Tooltip("Must match your seed prefab tag (e.g., 'Seed').")]
    public string seedTag = "Seed";

    Manager_Ch1 manager;
    bool hasSeed = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    public void SetManager(Manager_Ch1 m) => manager = m;

    void OnTriggerEnter(Collider other)
    {
        if (hasSeed) return;
        if (!other.CompareTag(seedTag)) return;

        // Require Rigidbody to ensure it's a real seed physics object
        if (!other.attachedRigidbody) return;

        hasSeed = true;
        if (manager) manager.NotifySeedPlaced(this);
    }
}
