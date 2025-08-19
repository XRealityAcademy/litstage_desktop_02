using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PanelJoystickButtonTraining : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;

    [Header("Affordance Callout To Hide")]
    [Tooltip("The Affordance Callout GameObject to deactivate after completion.")]
    public GameObject affordanceCalloutToHide;

    [Header("Event Listening")]
    public string showEventName = "showPanelJoystickButtonTraining";
    [Tooltip("The event to listen for to hide this panel.")]
    public string hideEventName = "hidePanelJoystickButtonTraining";

    [Header("Settings")]
    public string completedEventName = "panelJoystickTrainingCompleted";
    [Tooltip("Event PREFIX to send EACH time the player enters a spot (will be playerVisitSpot0, playerVisitSpot1, etc).")]
    public string spotEventPrefix = "playerVisitSpot";

    [Header("Teleport Anchors To Visit (one will be active at a time)")]
    [Tooltip("Assign the TeleportAnchor GameObjects you want the user to visit, in order.")]
    public List<GameObject> requiredAnchors = new List<GameObject>();

    [Header("Player Transform (XR Rig)")]
    [Tooltip("Assign your XR Rig here to check proximity after teleport.")]
    public Transform playerTransform;

    [Header("Proximity Threshold (meters)")]
    public float anchorProximityThreshold = 0.5f;

    [Header("Teleportation Provider (to enable on panel show)")]
    [Tooltip("Reference to the TeleportationProvider. Will be enabled when this panel is shown.")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;

    // Private state
    private int currentAnchorIndex = 0;
    private bool completed = false;

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
    }

    private void HidePanelAtStart()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        // Deactivate all anchors at startup
        foreach (var anchor in requiredAnchors)
            if (anchor != null)
                anchor.SetActive(false);
    }

    public void Show()
    {
        Debug.Log("EDWIN_DEBUG: PanelJoystickButtonTraining.Show() called");
        completed = false;
        currentAnchorIndex = 0;

        // Enable teleportation provider if assigned (critical for this step)
        if (teleportationProvider != null && !teleportationProvider.enabled)
        {
            teleportationProvider.enabled = true;
            Debug.Log("EDWIN_DEBUG: TeleportationProvider enabled by PanelJoystickButtonTraining.Show()");
        }

        // Deactivate all anchors first
        foreach (var anchor in requiredAnchors)
            if (anchor != null)
                anchor.SetActive(false);

        // Activate only the first anchor
        if (requiredAnchors.Count > 0 && requiredAnchors[0] != null)
            requiredAnchors[0].SetActive(true);

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void Hide()
    {
        Debug.Log("EDWIN_DEBUG: PanelJoystickButtonTraining.Hide() called");
        if (panelRoot != null)
            panelRoot.SetActive(false);
        // Deactivate all anchors when hiding
        foreach (var anchor in requiredAnchors)
            if (anchor != null)
                anchor.SetActive(false);

        // (Optional) Disable teleportationProvider again if you want to lock it outside this panel
        // if (teleportationProvider != null && teleportationProvider.enabled)
        // {
        //     teleportationProvider.enabled = false;
        //     Debug.Log("EDWIN_DEBUG: TeleportationProvider disabled by PanelJoystickButtonTraining.Hide()");
        // }
    }

    void Update()
    {
        if (!panelRoot || !panelRoot.activeInHierarchy || completed) return;
        if (playerTransform == null || requiredAnchors == null || requiredAnchors.Count == 0) return;
        if (currentAnchorIndex >= requiredAnchors.Count) return;

        var currentAnchor = requiredAnchors[currentAnchorIndex];
        if (currentAnchor == null || !currentAnchor.activeSelf) return;

        float dist = Vector3.Distance(playerTransform.position, currentAnchor.transform.position);
        if (dist <= anchorProximityThreshold)
        {
            Debug.Log($"EDWIN_DEBUG: Visited anchor {currentAnchor.name} ({currentAnchorIndex + 1}/{requiredAnchors.Count})");
            currentAnchor.SetActive(false); // Hide this anchor

            // Fire the event for this spot!
            string thisSpotEvent = spotEventPrefix + currentAnchorIndex;
            if (!string.IsNullOrEmpty(spotEventPrefix))
            {
                EventFunctions.SendEvent(thisSpotEvent);
                Debug.Log($"EDWIN_DEBUG: Sent spot event: {thisSpotEvent}");
            }

            currentAnchorIndex++;

            // Activate next anchor (if any)
            if (currentAnchorIndex < requiredAnchors.Count)
            {
                var nextAnchor = requiredAnchors[currentAnchorIndex];
                if (nextAnchor != null)
                {
                    nextAnchor.SetActive(true);
                    Debug.Log($"EDWIN_DEBUG: Activated next anchor {nextAnchor.name}");
                }
            }
            else
            {
                // All anchors visited!
                completed = true;
                Debug.Log("EDWIN_DEBUG: All anchors visited! Sending completed event and hiding callout.");
                if (affordanceCalloutToHide != null)
                {
                    affordanceCalloutToHide.SetActive(false);
                    Debug.Log("EDWIN_DEBUG: Deactivated Affordance Callout: " + affordanceCalloutToHide.name);
                }
                EventFunctions.SendEvent(completedEventName);
            }
        }
    }
}