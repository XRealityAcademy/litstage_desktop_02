using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Plays the first four dialogs automatically at Start, then waits for trigger
/// events to play dialogs 5‒10 one by one. Each trigger (e.g. a workplace
/// collider) should call <see cref="TriggerNextDialog"/>—either directly via a
/// UnityEvent or through the example <see cref="OnTriggerEnter"/> below.
/// </summary>
public class Manager_Ch1 : MonoBehaviour
{
    [Header("UI & Audio References")]
    public TMP_Text dialogText;          // Assign the on‑screen text component
    public AudioSource audioSource;      // Assign the AudioSource that plays clips

    [Header("Dialog Content (exactly 10)")]
    public AudioClip[] dialogClips;      // Size = 10, indices 0‑9
    public string[]    dialogLines;      // Size = 10, indices 0‑9

    [Header("UI Elements")]
    public GameObject continueButton;    // Shown after the 10th dialog

    /* ───────────────────────────── Internals ───────────────────────────── */
    private int  currentIndex;           // Next dialog index to play
    private bool waitingForTrigger;      // True once we need an external trigger
    private bool clipIsPlaying;          // Prevent double‑fire while audio runs

    /* ─────────────────────────── Unity Lifecycle ────────────────────────── */
    private void Start()
    {
        // Safety check
        if (dialogLines.Length != 10 || dialogClips.Length != 10)
        {
            Debug.LogError("Manager_Ch1: You must supply EXACTLY 10 dialog lines and 10 audio clips.");
            enabled = false;
            return;
        }

        continueButton.SetActive(false);
        currentIndex = 0;
        StartCoroutine(PlayInitialDialogs());       // Play 0‑3 automatically
    }

    /* ───────────────────────────── Public API ───────────────────────────── */
    /// <summary>
    /// Call this from an external trigger (e.g. OnTriggerEnter event) to
    /// advance the dialog after the fourth line has finished.
    /// </summary>
    public void TriggerNextDialog()
    {
        // Only react when we are in the trigger‑waiting phase and nothing is playing
        if (!waitingForTrigger || clipIsPlaying) return;
        StartCoroutine(PlayTriggeredDialog());
    }

    /* ───────────────────────────── Coroutines ───────────────────────────── */
    private IEnumerator PlayInitialDialogs()
    {
        yield return PlayDialogBlock(4);            // Play indices 0‑3
        waitingForTrigger = true;                   // Now rely on external triggers
    }

    private IEnumerator PlayTriggeredDialog()
    {
        waitingForTrigger = false;                  // Consume the trigger
        yield return PlayDialogBlock(1);            // Play ONE line (currentIndex)

        // Decide what happens next
        if (currentIndex < dialogLines.Length)
        {
            waitingForTrigger = true;               // Wait for the next trigger
        }
        else
        {
            continueButton.SetActive(true);         // All 10 dialogs done
        }
    }

    /// <summary>
    /// Plays <paramref name="count"/> consecutive dialogs starting at
    /// <see cref="currentIndex"/>. Updates <c>currentIndex</c> accordingly.
    /// </summary>
    private IEnumerator PlayDialogBlock(int count)
    {
        for (int i = 0; i < count && currentIndex < dialogLines.Length; i++)
        {
            // Short buffer before each dialog (optional)
            yield return new WaitForSeconds(0.5f);

            clipIsPlaying = true;
            dialogText.text = dialogLines[currentIndex];
            audioSource.clip = dialogClips[currentIndex];
            audioSource.Play();

            // Wait for the clip (fallback 4 seconds if clip length is 0)
            float waitTime = audioSource.clip ? audioSource.clip.length : 4f;
            yield return new WaitForSeconds(waitTime);

            audioSource.Stop();
            clipIsPlaying = false;
            currentIndex++;
        }
    }

    /* ─────────────────────────── Example Trigger ────────────────────────── */
    // If you prefer to keep trigger logic inside this script, uncomment the
    // method below and adjust the tag/conditions as needed. Otherwise, remove
    // it and call TriggerNextDialog() from the appropriate trigger scripts.

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WorkPlaceTrigger"))   // Example tag
        {
            TriggerNextDialog();                    // Plays dialog 5,6,7...
        }
    }

}
