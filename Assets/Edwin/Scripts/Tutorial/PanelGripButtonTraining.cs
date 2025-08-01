using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;

public class PanelGripButtonTraining : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;

    [Header("Affordance Callout To Hide")]
    [Tooltip("The Affordance Callout GameObject to deactivate after completion.")]
    public GameObject affordanceCalloutToHide;

    [Header("Event Listening")]
    public string showEventName = "showPanelGripButtonTraining";
    [Tooltip("The event to listen for to hide this panel.")]
    public string hideEventName = "hidePanelGripButtonTraining";

    [Header("Settings")]
    public string completedEventName = "panelGripTrainingCompleted";

    [Header("Grabbable Objects")]
    [Tooltip("Assign the 3 grabbable objects here (must have XRGrabInteractable component).")]
    public List<GameObject> grabbableObjects = new List<GameObject>();

    // Private state
    private int grabbedCount = 0;
    private bool completed = false;

    // For tracking which objects have already been grabbed
    private HashSet<GameObject> grabbedSet = new HashSet<GameObject>();

    void Awake()
    {
        HidePanelAtStart();
    }

    void OnEnable()
    {
        HidePanelAtStart();

        if (!string.IsNullOrEmpty(showEventName))
            EventFunctions.ListenEvent(showEventName, Show);

        if (!string.IsNullOrEmpty(hideEventName))
            EventFunctions.ListenEvent(hideEventName, Hide);
    }

    void OnDisable()
    {
        if (!string.IsNullOrEmpty(showEventName))
            EventFunctions.RemoveListener(showEventName, Show);
        if (!string.IsNullOrEmpty(hideEventName))
            EventFunctions.RemoveListener(hideEventName, Hide);
        UnsubscribeFromGrabbables();
    }

    private void HidePanelAtStart()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show()
    {
        Debug.Log("EDWIN_DEBUG: PanelGripButtonTraining.Show() called");
        grabbedCount = 0;
        completed = false;
        grabbedSet.Clear();

        if (panelRoot != null)
            panelRoot.SetActive(true);

        SubscribeToGrabbables();
    }

    private void SubscribeToGrabbables()
    {
        foreach (var go in grabbableObjects)
        {
            if (go == null) continue;
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = go.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null)
                grab.selectEntered.AddListener(OnGrabbableGrabbed);
        }
    }

    private void UnsubscribeFromGrabbables()
    {
        foreach (var go in grabbableObjects)
        {
            if (go == null) continue;
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = go.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null)
                grab.selectEntered.RemoveListener(OnGrabbableGrabbed);
        }
    }

    private void OnGrabbableGrabbed(SelectEnterEventArgs args)
    {
        GameObject grabbedObject = args.interactableObject.transform.gameObject;
        if (!grabbedSet.Contains(grabbedObject))
        {
            grabbedSet.Add(grabbedObject);
            grabbedCount++;
            Debug.Log($"EDWIN_DEBUG: PanelGripButtonTraining.OnGrabbableGrabbed() grabbed {grabbedObject.name} ({grabbedCount}/{grabbableObjects.Count})");

            if (!completed && grabbedCount >= grabbableObjects.Count)
            {
                completed = true;
                Debug.Log("EDWIN_DEBUG: All grabbables taken! Sending completed event and hiding callout.");

                // Deactivate the Affordance Callout if assigned
                if (affordanceCalloutToHide != null)
                {
                    affordanceCalloutToHide.SetActive(false);
                    Debug.Log("EDWIN_DEBUG: Deactivated Affordance Callout Right: " + affordanceCalloutToHide.name);
                }

                EventFunctions.SendEvent(completedEventName);
                // No timer here: panel will hide only on hide event
            }
        }
    }

    public void Hide()
    {
        Debug.Log("EDWIN_DEBUG: PanelGripButtonTraining.Hide() called");
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}