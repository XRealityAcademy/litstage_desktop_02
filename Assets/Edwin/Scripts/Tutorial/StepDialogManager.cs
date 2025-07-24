using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StepDialogManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text dialogText;
    public AudioSource audioSource;
    public GameObject nextButton;
    public GameObject backButton;

    [Header("Dialog Sequence")]
    public List<DialogStep> dialogSteps;

    private int currentStepIndex = 0;
    private bool waitingForEvent = false;

    void Start()
    {
        if (dialogSteps == null || dialogSteps.Count == 0)
        {
            Debug.LogError("Dialog steps not set!");
            return;
        }
        currentStepIndex = 0;
        Debug.Log("EDWIN_DEBUG: DialogManager.Start() - starting dialog sequence");

        // Wait one frame to allow other scripts to register event listeners before sending any events
        StartCoroutine(ShowCurrentStepNextFrame());
    }

    private System.Collections.IEnumerator ShowCurrentStepNextFrame()
    {
        yield return null; // Wait one frame
        ShowCurrentStep();
    }

    void ShowCurrentStep()
    {
        CancelInvoke(nameof(AdvanceToNextStep));
        DialogStep step = dialogSteps[currentStepIndex];
        Debug.Log($"EDWIN_DEBUG: ShowCurrentStep() - step index: {currentStepIndex}, text: \"{step.dialogText}\", triggerEvent: {step.triggerEvent}, dependingOnEvent: {step.dependingOnEvent}");

        dialogText.text = step.dialogText;

        float autoAdvanceDelay = step.timeAfterStep; // Start with per-step value

        // Handle audio if present
        if (step.audioClip != null)
        {
            audioSource.clip = step.audioClip;
            audioSource.Play();
            autoAdvanceDelay = audioSource.clip.length + step.timeAfterStep;
            Debug.Log($"EDWIN_DEBUG: Playing audio (duration={audioSource.clip.length}s) + timeAfterStep ({step.timeAfterStep}s)");
        }
        else
        {
            audioSource.Stop();
            audioSource.clip = null;
            Debug.Log($"EDWIN_DEBUG: No audio. Will wait timeAfterStep={step.timeAfterStep}s.");
        }

        // Fire triggerEvent if set
        if (!string.IsNullOrEmpty(step.triggerEvent))
        {
            Debug.Log($"EDWIN_DEBUG: Sending trigger event: {step.triggerEvent}");
            EventFunctions.SendEvent(step.triggerEvent);
        }

        // Listen for dependency event if set
        if (!string.IsNullOrEmpty(step.dependingOnEvent))
        {
            Debug.Log($"EDWIN_DEBUG: Listening for dependency event: {step.dependingOnEvent}");
            waitingForEvent = true;
            nextButton.SetActive(false);
            backButton.SetActive(false);
            EventFunctions.ListenEvent(step.dependingOnEvent, OnEventDependencyMet);
        }
        else
        {
            Debug.Log("EDWIN_DEBUG: No dependency event, setting buttons based on requiresConfirmation and canGoBack");
            waitingForEvent = false;
            nextButton.SetActive(step.requiresConfirmation);
            backButton.SetActive(step.canGoBack && step.requiresConfirmation);

            // Auto-advance if no confirmation required and not waiting for event
            if (!step.requiresConfirmation)
            {
                Debug.Log($"EDWIN_DEBUG: Auto-advancing after audio or fallback delay ({autoAdvanceDelay}s)");
                Invoke(nameof(AdvanceToNextStep), autoAdvanceDelay);
            }
        }
    }

    void OnEventDependencyMet()
    {
        Debug.Log("EDWIN_DEBUG: OnEventDependencyMet() - dependency event received");
        waitingForEvent = false;

        var step = dialogSteps[currentStepIndex];

        // Remove listener so it doesn't persist into future steps
        EventFunctions.RemoveListener(step.dependingOnEvent, OnEventDependencyMet);

        if (step.requiresConfirmation)
        {
            Debug.Log("EDWIN_DEBUG: Event received, showing Next and Back if allowed");
            nextButton.SetActive(true);
            backButton.SetActive(step.canGoBack);
        }
        else
        {
            Debug.Log("EDWIN_DEBUG: Event received, auto-advancing dialog");
            AdvanceToNextStep();
        }
    }

    public void OnNextClicked()
    {
        Debug.Log("EDWIN_DEBUG: OnNextClicked()");
        if (currentStepIndex < dialogSteps.Count - 1 && !waitingForEvent)
        {
            currentStepIndex++;
            ShowCurrentStep();
        }
        else
        {
            Debug.Log("EDWIN_DEBUG: OnNextClicked() - waitingForEvent or at end of dialog");
        }
    }

    public void OnBackClicked()
    {
        Debug.Log("EDWIN_DEBUG: OnBackClicked()");
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            ShowCurrentStep();
        }
    }

    private void AdvanceToNextStep()
    {
        Debug.Log("EDWIN_DEBUG: AdvanceToNextStep()");
        if (currentStepIndex < dialogSteps.Count - 1 && !waitingForEvent)
        {
            currentStepIndex++;
            ShowCurrentStep();
        }
        else
        {
            Debug.Log("EDWIN_DEBUG: AdvanceToNextStep() - waitingForEvent or at end of dialog");
        }
    }

    private void OnDisable()
    {
        // Clean up any listeners to avoid memory leaks
        if (currentStepIndex < dialogSteps.Count && !string.IsNullOrEmpty(dialogSteps[currentStepIndex].dependingOnEvent))
        {
            Debug.Log($"EDWIN_DEBUG: OnDisable() - removing listener for {dialogSteps[currentStepIndex].dependingOnEvent}");
            EventFunctions.RemoveListener(dialogSteps[currentStepIndex].dependingOnEvent, OnEventDependencyMet);
        }
    }
}