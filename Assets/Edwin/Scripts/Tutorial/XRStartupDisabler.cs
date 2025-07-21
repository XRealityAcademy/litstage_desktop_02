using UnityEngine;


/// <summary>
/// Disables specified Affordance Callout GameObjects and disables Move and Teleport Providers on game start.
/// Attach this to a central manager GameObject. Assign all callouts and providers to disable in the inspector.
/// </summary>
public class XRStartupDisabler : MonoBehaviour
{
    [Tooltip("Assign all Affordance Callout GameObjects (left and right) to disable on game start.")]
    public GameObject[] calloutsToDisable;

    [Header("XR Locomotion Providers")]

    [Tooltip("Assign the DynamicMoveProvider (or ActionBasedContinuousMoveProvider) for joystick locomotion.")]
    public MonoBehaviour moveProvider; // Accepts DynamicMoveProvider or ActionBasedContinuousMoveProvider

    [Tooltip("Assign the TeleportationProvider for teleport movement.")]
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;

    void Start()
    {
        // Disable all affordance callouts
        foreach (var go in calloutsToDisable)
        {
            if (go != null && go.activeSelf)
            {
                go.SetActive(false);
                Debug.Log($"EDWIN_DEBUG: Disabled callout: {go.name}");
            }
        }

        // Disable the move provider (DynamicMoveProvider or ActionBasedContinuousMoveProvider)
        if (moveProvider != null && moveProvider.enabled)
        {
            moveProvider.enabled = false;
            Debug.Log($"EDWIN_DEBUG: Disabled joystick movement provider: {moveProvider.name}");
        }

        // Disable the teleportation provider
        if (teleportationProvider != null && teleportationProvider.enabled)
        {
            teleportationProvider.enabled = false;
            Debug.Log($"EDWIN_DEBUG: Disabled teleportation (TeleportationProvider): {teleportationProvider.name}");
        }
    }
}