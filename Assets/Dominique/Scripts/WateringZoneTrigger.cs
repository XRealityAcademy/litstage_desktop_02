using UnityEngine;

public class WateringZoneTrigger : MonoBehaviour
{
    public Manager_Ch1 manager;

    // Option A: tag the "water stream" or watering can tip
    public string acceptedTag = "WaterStream";

    // Option B: leave tag blank and just accept any entry once
    public bool acceptAnyOnce = false;

    bool done = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (done) return;
        if (!acceptAnyOnce && !string.IsNullOrEmpty(acceptedTag) && !other.CompareTag(acceptedTag)) return;

        done = true;
        if (manager) manager.NotifyWateringDone();
    }
}
