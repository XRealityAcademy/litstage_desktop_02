using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDialog : MonoBehaviour
{
    [Tooltip("Dialog index to play.")]
    public int dialogIndex = 4;

    public Manager_Ch1 manager;

    [Header("Ray & Grab (optional)")]
    public Transform         rightRayOrigin;   // controller transform
    public CustomInputAction rightHandInput;   // exposes IsGrabPressed
    [SerializeField] float   maxRayDistance = 10f;

    LayerMask dialogMask;
    bool fired;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        dialogMask = LayerMask.GetMask("DialogTrigger");
        if (!manager) manager = FindObjectOfType<Manager_Ch1>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!fired && other.CompareTag("Player")) Activate();
    }

    void Update()
    {
        if (fired || !rightRayOrigin || !rightHandInput || !rightHandInput.IsGrabPressed) return;

        if (Physics.Raycast(rightRayOrigin.position, rightRayOrigin.forward,
                            out var hit, maxRayDistance, dialogMask,
                            QueryTriggerInteraction.Collide) &&
            hit.collider == GetComponent<Collider>())
        {
            Activate();
        }
    }

    void Activate()
    {
        if (fired || !manager) return;
        fired = true;
        manager.PlayDialogByIndex(dialogIndex);
    }
}
