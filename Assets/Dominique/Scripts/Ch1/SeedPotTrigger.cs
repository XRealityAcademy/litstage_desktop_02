using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SeedPotTrigger : MonoBehaviour
{
    [Tooltip("Must match your seed prefab tag (e.g., 'Seed').")]
    public string seedTag = "Seed";

    Manager_Ch1 manager;
    bool hasSeed = false;
    public bool IsSeeded => hasSeed;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true; // trigger volume over the soil
    }

    public void SetManager(Manager_Ch1 m) => manager = m;

    void OnTriggerEnter(Collider other)
    {
        if (hasSeed) return;                     // only the first seed counts
        if (!other.CompareTag(seedTag)) return;  // must be a Seed
        if (!other.attachedRigidbody) return;    // ensure it's a physical object

        hasSeed = true;
        if (manager) manager.NotifySeedPlaced(this);
    }
}
