using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[AddComponentMenu("XR/Dialog/Trigger Dialog")]
public class TriggerDialog : MonoBehaviour
{
    [Tooltip("Dialog index to play on activation.")]
    public int dialogIndex = 4;

    [Tooltip("Reference to the Manager_Ch1 that plays dialog lines. If null, will auto-find one in the scene.")]
    public Manager_Ch1 manager;

    [Header("Collision Activation")]
    [Tooltip("Only activate when this tag enters the trigger.")]
    public string playerTag = "Player";

    [Tooltip("Allow activation only once.")]
    public bool oneShot = true;

    [Header("Ray & Grab Activation (optional)")]
    [Tooltip("Controller or hand transform used as the ray origin.")]
    public Transform rightRayOrigin;            // controller/hand

    [Tooltip("Custom input wrapper exposing IsGrabPressed.")]
    public CustomInputAction rightHandInput;    // must expose bool IsGrabPressed

    [SerializeField, Tooltip("Max distance for the dialog raycast.")]
    private float maxRayDistance = 10f;

    [SerializeField, Tooltip("Layers the dialog raycast will consider. If left empty, defaults to a layer named 'DialogTrigger' if it exists.")]
    private LayerMask raycastMask;

    // ───────── Internals ─────────
    Collider _col;
    bool _fired;

    void Awake()
    {
        // Ensure trigger collider
        _col = GetComponent<Collider>();
        _col.isTrigger = true;

        // Fallback: if mask not set in Inspector, try 'DialogTrigger' to mirror your old setup
        if (raycastMask.value == 0)
            raycastMask = LayerMask.GetMask("DialogTrigger");

        // Soft auto-wiring for convenience
        if (!manager)
            manager = FindObjectOfType<Manager_Ch1>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_fired && oneShot) return;
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
            Activate();
    }

    void Update()
    {
        if (_fired && oneShot) return;
        if (!rightRayOrigin || !rightHandInput || !rightHandInput.IsGrabPressed) return;

        // Raycast from the controller/hand forward
        if (Physics.Raycast(rightRayOrigin.position,
                            rightRayOrigin.forward,
                            out var hit,
                            maxRayDistance,
                            raycastMask.value == 0 ? ~0 : raycastMask,
                            QueryTriggerInteraction.Collide))
        {
            if (hit.collider == _col)
                Activate();
        }
    }

    void Activate()
    {
        if (_fired && oneShot) return;
        if (!manager)
        {
            Debug.LogWarning($"{nameof(TriggerDialog)}: No Manager_Ch1 found; cannot play dialog index {dialogIndex}.");
            return;
        }

        _fired = true;
        manager.PlayDialogByIndex(dialogIndex);
    }

    // Optional helper if you ever need to re-arm this trigger at runtime
    public void ResetTrigger()
    {
        _fired = false;
    }
}
