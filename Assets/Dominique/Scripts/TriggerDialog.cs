// TriggerDialog.cs  (drop this in Assets/Scripts)
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDialog : MonoBehaviour
{
    [Tooltip("Which dialog index? 0‑9. 4 = first optional waypoint (Dialog 5).")]
    public int dialogIndex = 4;

    [Tooltip("If left blank, we auto‑find Manager_Ch1 at runtime.")]
    public Manager_Ch1 manager;

    bool fired;

    void Awake()
    {
        // Ensure this collider is a trigger
        GetComponent<Collider>().isTrigger = true;

        // Find the manager automatically if not set
        if (manager == null) manager = FindObjectOfType<Manager_Ch1>();
        if (manager == null)
            Debug.LogError($"{name}: Manager_Ch1 not found in scene!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (fired) return;                 // only once
        if (!other.CompareTag("Player")) return;
        if (manager == null) return;

        manager.PlayDialogByIndex(dialogIndex);
        fired = true;                      // lock out repeats
    }
}
