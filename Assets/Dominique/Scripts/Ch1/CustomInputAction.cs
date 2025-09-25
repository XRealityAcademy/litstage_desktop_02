using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Wraps an InputActionReference so other scripts can poll <see cref="IsGrabPressed"/>.
/// </summary>
public class CustomInputAction : MonoBehaviour
{
    [Tooltip("Input Action Reference for the controller’s grab / trigger.")]
    public InputActionReference grabObjectCustomTrigger;

    /// <summary>True while the grab action is held down.</summary>
    public bool IsGrabPressed { get; private set; }

    void OnEnable()
    {
        if (grabObjectCustomTrigger == null)
        {
            Debug.LogError($"{name}: grabObjectCustomTrigger not assigned.");
            enabled = false;
            return;
        }

        grabObjectCustomTrigger.action.Enable();
        grabObjectCustomTrigger.action.started  += _ => IsGrabPressed = true;
        grabObjectCustomTrigger.action.canceled += _ => IsGrabPressed = false;
    }

    void OnDisable()
    {
        if (grabObjectCustomTrigger != null)
            grabObjectCustomTrigger.action.Disable();
    }
}
