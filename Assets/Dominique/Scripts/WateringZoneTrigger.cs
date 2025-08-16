using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WateringZoneTrigger : MonoBehaviour
{
    Manager_Ch1 manager;

    [Header("Detection (Mesh/Collider)")]
    [Tooltip("Require this tag on the watering can *tip* (e.g., a small cube).")]
    public string acceptedTag = "WaterCanTip";

    bool fired = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true; // zone is trigger; tip is non-trigger
    }

    public void SetManager(Manager_Ch1 m) => manager = m;

    void OnTriggerEnter(Collider other)
    {
        if (fired) return;
        if (!string.IsNullOrEmpty(acceptedTag) && !other.CompareTag(acceptedTag)) return;

        fired = true;
        if (manager) manager.NotifyWateringZoneHit();
    }
}
