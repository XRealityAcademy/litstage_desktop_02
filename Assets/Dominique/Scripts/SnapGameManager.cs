using UnityEngine;
using UnityEngine.Events;

public class SnapGameManager : MonoBehaviour
{
    [SerializeField] private int totalSlots = 6;
    [SerializeField] private UnityEvent onAllSnapped;

    static SnapGameManager inst;
    int filled;

    void Awake() => inst = this;

    public static void FlagSlotFilled()
    {
        if (inst == null) return;
        if (++inst.filled >= inst.totalSlots)
            inst.onAllSnapped?.Invoke();
    }
}
