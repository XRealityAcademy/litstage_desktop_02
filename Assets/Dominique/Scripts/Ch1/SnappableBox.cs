using UnityEngine;

/// <summary>
/// Box that snaps to its matching <see cref="SnapSlot"/> and always preserves
/// the exact scale it had at start‑up, no matter what happens later.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SnappableBox : MonoBehaviour
{
    /*────────── Identification ──────────*/
    [SerializeField] private int boxID = 0;
    public  int BoxID => boxID;

    /*────────── Optional Mesh Lock ──────*/
    [Header("Optional Appearance Lock")]
    [Tooltip("Drop a mesh here if you want every box to revert to this mesh at runtime.")]
    [SerializeField] private Mesh sharedMesh;          // leave null to ignore

    /*────────── Internals ──────────*/
    Vector3   initialScale;
    Rigidbody rb;

    /*────────── Unity Lifecycle ──────────*/
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialScale = transform.localScale;           // remember designer’s size
        ResetMesh();
    }

    void LateUpdate()
    {
        // If *anything* has resized us, force‑revert.
        if (transform.localScale != initialScale)
            transform.localScale = initialScale;
    }

    /*────────── Called by SnapSlot ──────────*/
    public void Freeze()
    {
        rb.linearVelocity = rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        RestoreAppearance();
    }

    public void MarkSnapped() => RestoreAppearance();

    /*────────── Helpers ──────────*/
    void RestoreAppearance()
    {
        // ensure scale and (optional) mesh are intact
        if (transform.localScale != initialScale)
            transform.localScale = initialScale;
        ResetMesh();
    }

    void ResetMesh()
    {
        if (!sharedMesh) return;
        var mf = GetComponent<MeshFilter>();
        if (mf && mf.sharedMesh != sharedMesh)
            mf.sharedMesh = sharedMesh;
    }
}
