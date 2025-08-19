using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WateringZoneTrigger : MonoBehaviour
{
    [Tooltip("Hook up your scene's Manager_Ch1 here.")]
    public Manager_Ch1 manager;

    [Header("Detection")]
    [Tooltip("Tag on the watering can's tip mesh/cube.")]
    public string acceptedTag = "WaterCanTip";

    bool fired = false;

    void Reset()
    {
        // Make this a trigger so a *non-trigger* WaterCanTip collider can enter it
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (!manager) manager = FindObjectOfType<Manager_Ch1>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Only respond to the water can tip mesh/cube
        if (fired) return;
        if (!other.CompareTag(acceptedTag)) return;

        fired = true;

        // Tell your existing manager: watering is done → jump to 13
        // (This uses the method name from your current Manager_Ch1.)
        if (manager) manager.NotifyWateringDone();
    }
}
