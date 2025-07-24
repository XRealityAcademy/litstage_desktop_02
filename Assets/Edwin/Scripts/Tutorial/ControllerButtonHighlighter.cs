using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public enum VRButton
{
    Trigger,
    Grip,
    PrimaryButton,   // A/X
    SecondaryButton, // B/Y
}

public class ControllerButtonHighlighter : MonoBehaviour
{
    [Header("Root of the full Affordance Callout (Set to root GameObject)")]
    [Tooltip("Assign the top-level Affordance Callout (parent of the tooltip canvas) to enable/disable the whole callout when highlighting.")]
    public GameObject affordanceCalloutRoot;

    [Header("Tooltip Canvas (Animated, usually child of callout root)")]
    [Tooltip("Assign the RectTransform you want to pulse (usually the Tooltip Canvas or similar).")]
    public RectTransform tooltipCanvas;

    [Header("Tooltip Background (optional)")]
    [Tooltip("Assign the background image to pulse color (optional).")]
    public Image tooltipBackground;

    [Header("Tooltip Text (optional, overrides auto-find)")]
    [Tooltip("Assign a specific TMP_Text for the tooltip label, or leave empty to auto-find.")]
    public TMP_Text customTooltipText;

    [Header("Tooltip Message")]
    [Tooltip("If set, this text will be used for the tooltip when highlighted.")]
    [TextArea]
    public string customText;

    [Header("Other Callouts to Hide (Optional)")]
    [Tooltip("Assign all other callout roots to hide when this one is active.")]
    public GameObject[] calloutsToHide;

    [Header("Pulse Effect Settings")]
    [Tooltip("Scale multiplier for pulse effect.")]
    public float pulseScale = 1.5f;
    [Tooltip("Speed of pulse animation.")]
    public float pulseSpeed = 2f;
    [Tooltip("Color of background when pulsing.")]
    public Color pulseColor = Color.yellow;
    [Tooltip("Color of text when pulsing.")]
    public Color pulseTextColor = Color.red;
    [Tooltip("Default background color.")]
    public Color defaultBackgroundColor = Color.white;
    [Tooltip("Default text color.")]
    public Color defaultTextColor = Color.white;

    [Header("Button Settings")]
    [Tooltip("Which VR button to wait for to trigger completion event.")]
    public VRButton expectedButton = VRButton.Trigger;

    [Header("Event Settings")]
    [Tooltip("Event to listen for to start the highlight.")]
    public string triggerEventName = "showTriggerEffect";
    [Tooltip("Event to send when the button is pressed.")]
    public string completionEventName = "triggerClicked";

    private TMP_Text tooltipText;
    private Vector3 originalScale;
    private bool pulsing = false;
    private Coroutine pulseCoroutine;

    void Awake()
    {
        if (tooltipCanvas == null)
            tooltipCanvas = GetComponent<RectTransform>();

        // If the user assigned a custom TMP_Text, use it. Otherwise auto-find one in children.
        tooltipText = customTooltipText != null ? customTooltipText : GetComponentInChildren<TMP_Text>();

        if (tooltipCanvas != null)
            originalScale = tooltipCanvas.localScale;
        else
            originalScale = Vector3.one;

        Debug.Log("EDWIN_DEBUG: ControllerButtonHighlighter.Awake() - tooltipCanvas, tooltipText initialized");
    }

    void OnEnable()
    {
        Debug.Log($"EDWIN_DEBUG: ControllerButtonHighlighter.OnEnable() - Listening for {triggerEventName}");
        EventFunctions.ListenEvent(triggerEventName, StartPulse);
    }

    void OnDisable()
    {
        Debug.Log($"EDWIN_DEBUG: ControllerButtonHighlighter.OnDisable() - Removing listener for {triggerEventName}");
        EventFunctions.RemoveListener(triggerEventName, StartPulse);
    }

    public void StartPulse()
    {
        // Activate the root callout immediately
        if (affordanceCalloutRoot != null)
            affordanceCalloutRoot.SetActive(true);

        if (tooltipCanvas != null)
            tooltipCanvas.gameObject.SetActive(true);

        if (pulsing)
        {
            Debug.Log("EDWIN_DEBUG: StartPulse() - Already pulsing, returning.");
            return;
        }

        Debug.Log("EDWIN_DEBUG: StartPulse() - Starting pulse effect.");
        pulsing = true;

        // Show custom text if assigned
        if (tooltipText != null && !string.IsNullOrEmpty(customText))
            tooltipText.text = customText;

        // Hide other callouts
        if (calloutsToHide != null)
            foreach (var go in calloutsToHide)
                if (go != null) go.SetActive(false);

        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    public void StopPulse()
    {
        Debug.Log("EDWIN_DEBUG: StopPulse() - Stopping pulse effect.");
        pulsing = false;
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        if (tooltipCanvas != null)
            tooltipCanvas.localScale = originalScale;
        if (tooltipBackground != null)
            tooltipBackground.color = defaultBackgroundColor;
        if (tooltipText != null)
            tooltipText.color = defaultTextColor;

        if (affordanceCalloutRoot != null)
            affordanceCalloutRoot.SetActive(false);
    }

    private System.Collections.IEnumerator PulseRoutine()
    {
        Debug.Log("EDWIN_DEBUG: PulseRoutine() - Pulse started.");
        while (pulsing)
        {
            if (tooltipCanvas != null)
            {
                float scale = Mathf.Lerp(1f, pulseScale, 0.5f * (1 + Mathf.Sin(Time.time * pulseSpeed)));
                tooltipCanvas.localScale = originalScale * scale;
            }
            if (tooltipBackground != null)
                tooltipBackground.color = Color.Lerp(defaultBackgroundColor, pulseColor, 0.5f * (1 + Mathf.Sin(Time.time * pulseSpeed)));
            if (tooltipText != null)
                tooltipText.color = Color.Lerp(defaultTextColor, pulseTextColor, 0.5f * (1 + Mathf.Sin(Time.time * pulseSpeed)));
            yield return null;
        }
        Debug.Log("EDWIN_DEBUG: PulseRoutine() - Pulse ended.");
    }

    public void SetTooltipText(string text)
    {
        Debug.Log($"EDWIN_DEBUG: SetTooltipText() - Setting tooltip text to: {text}");
        if (tooltipText != null)
            tooltipText.text = text;
    }

    void Update()
    {
        if (!pulsing) return;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("EDWIN_DEBUG: UI Press (KeyCode.T) detected in Editor.");
            StopPulse();
            EventFunctions.SendEvent(completionEventName);
            return;
        }
#endif

        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (!rightHand.isValid) return;

        bool pressed = false;
        switch (expectedButton)
        {
            case VRButton.Trigger:
                if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out pressed) && pressed)
                {
                    Debug.Log("EDWIN_DEBUG: Trigger pressed.");
                    StopPulse();
                    EventFunctions.SendEvent(completionEventName);
                }
                break;
            case VRButton.Grip:
                if (rightHand.TryGetFeatureValue(CommonUsages.gripButton, out pressed) && pressed)
                {
                    Debug.Log("EDWIN_DEBUG: Grip pressed.");
                    StopPulse();
                    EventFunctions.SendEvent(completionEventName);
                }
                break;
            case VRButton.PrimaryButton:
                if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out pressed) && pressed)
                {
                    Debug.Log("EDWIN_DEBUG: Primary (A/X) pressed.");
                    StopPulse();
                    EventFunctions.SendEvent(completionEventName);
                }
                break;
            case VRButton.SecondaryButton:
                if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed) && pressed)
                {
                    Debug.Log("EDWIN_DEBUG: Secondary (B/Y) pressed.");
                    StopPulse();
                    EventFunctions.SendEvent(completionEventName);
                }
                break;
        }
    }
}