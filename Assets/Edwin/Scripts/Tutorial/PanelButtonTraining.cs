using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PanelButtonTraining : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public Button trainingButton;
    public TMP_Text counterText;

    [Header("Affordance Callout To Hide")]
    [Tooltip("The Affordance Callout GameObject to deactivate after completion.")]
    public GameObject affordanceCalloutToHide;

    [Header("Event Listening")]
    public string showEventName = "showPanelButtonTraining";

    [Header("Settings")]
    public string completedEventName = "panelButtonTrainingCompleted";
    public int clicksRequired = 5;
    public float hideDelaySeconds = 20f;

    private int clickCount = 0;
    private bool completed = false;
    private Coroutine hideCoroutine;

    void Awake()
    {
        HidePanelAtStart();
        if (trainingButton != null)
        {
            trainingButton.onClick.RemoveListener(OnButtonClicked);
            trainingButton.onClick.AddListener(OnButtonClicked);
        }
    }

    void OnEnable()
    {
        HidePanelAtStart();
        if (!string.IsNullOrEmpty(showEventName))
            EventFunctions.ListenEvent(showEventName, Show);

        if (trainingButton != null)
        {
            trainingButton.onClick.RemoveListener(OnButtonClicked);
            trainingButton.onClick.AddListener(OnButtonClicked);
        }
    }

    void OnDisable()
    {
        if (!string.IsNullOrEmpty(showEventName))
            EventFunctions.RemoveListener(showEventName, Show);
    }

    private void HidePanelAtStart()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show()
    {
        Debug.Log("EDWIN_DEBUG: PanelButtonTraining.Show() called");
        clickCount = 0;
        completed = false;
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        UpdateCounterText();
        if (panelRoot != null)
            panelRoot.SetActive(true);
        if (trainingButton != null)
            trainingButton.interactable = true;
    }

    public void Hide()
    {
        Debug.Log("EDWIN_DEBUG: PanelButtonTraining.Hide() called");
        if (panelRoot != null)
            panelRoot.SetActive(false);
        if (trainingButton != null)
            trainingButton.interactable = false;
    }

    void OnButtonClicked()
    {
        clickCount++;
        Debug.Log($"EDWIN_DEBUG: PanelButtonTraining.OnButtonClicked() clickCount={clickCount}");

        if (!completed && clickCount >= clicksRequired)
        {
            completed = true;
            counterText.text = "Well Done";
            Debug.Log("EDWIN_DEBUG: 5 clicks reached! Sending event: " + completedEventName);

            // Deactivate the Affordance Callout Right if assigned
            if (affordanceCalloutToHide != null)
            {
                affordanceCalloutToHide.SetActive(false);
                Debug.Log("EDWIN_DEBUG: Deactivated Affordance Callout Right: " + affordanceCalloutToHide.name);
            }

            EventFunctions.SendEvent(completedEventName);

            trainingButton.interactable = false;

            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
        else if (!completed)
        {
            UpdateCounterText();
        }
    }

    void UpdateCounterText()
    {
        if (counterText != null)
            counterText.text = $"N:{clickCount}";
    }

    IEnumerator HideAfterDelay()
    {
        Debug.Log("EDWIN_DEBUG: PanelButtonTraining.HideAfterDelay() waiting " + hideDelaySeconds + " seconds");
        yield return new WaitForSeconds(hideDelaySeconds);
        Hide();
    }
}