using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SnapSlot : MonoBehaviour
{
    [Header("Identification")]
    [SerializeField] private int slotID = 0;          // which box belongs here?

    [Header("Snapping")]
    [SerializeField] private float snapDuration = 0.25f;
    [SerializeField] private Vector3 localOffset = Vector3.zero;

    public bool Occupied { get; private set; }

    void Awake()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;                         // socket is trigger‑only
    }

    void OnTriggerEnter(Collider other)
    {
        if (Occupied) return;
        if (!other.TryGetComponent(out SnappableBox box)) return;
        if (box.BoxID != slotID) return;

        StartCoroutine(MagneticSnap(box));
    }

    IEnumerator MagneticSnap(SnappableBox box)
    {
        Vector3 startPos = box.transform.position;
        Quaternion startRot = box.transform.rotation;
        Vector3 endPos = transform.TransformPoint(localOffset);
        Quaternion endRot = transform.rotation;

        float t = 0;
        while (t < snapDuration)
        {
            t += Time.deltaTime;
            float u = t / snapDuration;
            box.transform.position = Vector3.Lerp(startPos, endPos, u);
            box.transform.rotation = Quaternion.Slerp(startRot, endRot, u);
            yield return null;
        }

        box.transform.SetPositionAndRotation(endPos, endRot);
        box.Freeze();                 // lock in place

        Occupied = true;
        box.MarkSnapped();
        SnapGameManager.FlagSlotFilled();  // optional win counter
    }

    /// <summary>Called if you ever decide to unsnap.</summary>
    public void Vacate() => Occupied = false;
}
