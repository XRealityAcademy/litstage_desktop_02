using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class TriggerDialog : MonoBehaviour
{
    [Tooltip("Dialog index 0-9 (4 = Dialog 5).")]
    public int dialogIndex = 4;

    [Tooltip("Assign Manager_Ch1 or leave empty to auto-find.")]
    public Manager_Ch1 manager;

    [Header("Ray & Grab")]
    public Transform         rightRayOrigin;   // controller transform
    public CustomInputAction rightHandInput;   // exposes IsGrabPressed
    [SerializeField] float   maxRayDistance = 10f;

    /*───────── Internals ─────────*/
    LayerMask dialogMask;          // DialogTrigger layer
    bool      fired;

    void Awake()
    {
        // Ensure collider is Trigger
        GetComponent<Collider>().isTrigger = true;

        dialogMask = LayerMask.GetMask("DialogTrigger");
        if (manager == null) manager = FindObjectOfType<Manager_Ch1>();
    }

    /*── Body walk-through ─*/
    void OnTriggerEnter(Collider other)
    {
        if (!fired && other.CompareTag("Player")) Activate();
    }

    /*── Ray + grab combo ─*/
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

    /*── Fire dialog, let Manager swap outline ─*/
    void Activate()
    {
        if (fired || manager == null) return;
        fired = true;
        manager.PlayDialogByIndex(dialogIndex);
    }
}
