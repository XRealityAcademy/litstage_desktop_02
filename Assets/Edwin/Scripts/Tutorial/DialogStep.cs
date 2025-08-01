using UnityEngine;

[System.Serializable]
public class DialogStep
{
    [Tooltip("Text to display for this dialog step.")]
    public string dialogText;

    [Tooltip("Audio clip to play for this step (optional). Leave empty for no audio.")]
    public AudioClip audioClip;

    [Tooltip("If true, user must confirm before advancing (shows Next button).")]
    public bool requiresConfirmation;

    [Tooltip("If true, user can go back to the previous step (shows Back button).")]
    public bool canGoBack;

    [Tooltip("Event name to trigger when this dialog step appears (optional).")]
    public string triggerEvent;

    [Tooltip("If set, dialog waits for this event before allowing advance.")]
    public string dependingOnEvent;

    [Tooltip("Extra seconds to wait after this step before advancing (added after audio if present).")]
    public float timeAfterStep = 1.0f;
}