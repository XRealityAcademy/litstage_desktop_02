using UnityEngine;

public class WaterCanGrabRelay : MonoBehaviour
{
    Manager_Ch1 manager;

    public void SetManager(Manager_Ch1 m) => manager = m;

    // Hook this to XR Interaction Toolkit's "Select Entered" event (or call manually)
    public void OnGrabbed()
    {
        if (manager) manager.NotifyWaterCanGrabbed();
    }
}
