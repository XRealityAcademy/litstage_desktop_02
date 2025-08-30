using System.Collections;
using UnityEngine;
using TMPro;

public class Manager_Ch3 : MonoBehaviour
{
    /*──────── Config ────────*/
    private const int TOTAL = 25; // indices 0..24

    [Header("UI & Audio")]
    public TMP_Text dialogText;
    public AudioSource audioSource;

    [Header("Dialog Content (exactly 25)")]
    [Tooltip("Audio clips for each dialog index (0..24).")]
    public AudioClip[] dialogClips = new AudioClip[TOTAL];

    [TextArea(1, 3)]
    [Tooltip("Text lines for each dialog index (0..24).")]
    public string[] dialogLines = new string[TOTAL];

    [Header("Autoplay Timing")]
    [Tooltip("Default seconds to wait AFTER each line before auto-playing the next.")]
    public float defaultDelay = 5f;

    [Tooltip("Optional per-line overrides. If <= 0, the defaultDelay is used.")]
    public float[] perLineDelays = new float[TOTAL];

    [Header("UI")]
    [Tooltip("Shown when all lines have finished playing.")]
    public GameObject continueButton;

    /*──────── Internals ─────*/
    readonly bool[] played = new bool[TOTAL];
    Coroutine autoplayRoutine;

    /*──────── Unity ─────────*/
    void Awake()
    {
        // Basic validation
        if (dialogClips == null || dialogClips.Length != TOTAL ||
            dialogLines == null || dialogLines.Length != TOTAL)
        {
            Debug.LogError($"Manager_Ch3: Need EXACTLY {TOTAL} clips and {TOTAL} lines.");
            enabled = false;
            return;
        }

        if (perLineDelays == null || perLineDelays.Length != TOTAL)
            perLineDelays = new float[TOTAL];

        // Audio source sensible defaults
        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            if (audioSource.volume <= 0f) audioSource.volume = 1f;
        }
    }

    void Start()
    {
        if (continueButton) continueButton.SetActive(false);
        autoplayRoutine = StartCoroutine(AutoPlayAll());
    }

    /*──────── Public API ────*/
    /// <summary>Plays a specific index immediately (useful for testing or branching).</summary>
    public void PlayDialogByIndex(int index)
    {
        if (index < 0 || index >= TOTAL) return;
        StartCoroutine(PlayLine(index));
    }

    /// <summary>Stops any running autoplay.</summary>
    public void StopAutoplay()
    {
        if (autoplayRoutine != null)
        {
            StopCoroutine(autoplayRoutine);
            autoplayRoutine = null;
        }
        if (audioSource) audioSource.Stop();
    }

    /*──────── Core Flow ─────*/
    IEnumerator AutoPlayAll()
    {
        for (int i = 0; i < TOTAL; i++)
        {
            yield return PlayLine(i);

            // Wait AFTER each line (except you can still wait after last if you want)
            float delay = perLineDelays[i] > 0f ? perLineDelays[i] : defaultDelay;
            if (delay > 0f && i < TOTAL - 1)
                yield return new WaitForSeconds(delay);
        }

        if (continueButton) continueButton.SetActive(true);
    }

    IEnumerator PlayLine(int idx)
    {
        // If you don’t want repeats, uncomment next line:
        // if (played[idx]) yield break;
        played[idx] = true;

        // Set UI text
        if (dialogText) dialogText.text = dialogLines[idx];

        // Play audio (or fallback wait)
        if (audioSource && dialogClips[idx])
        {
            audioSource.Stop();
            audioSource.clip = dialogClips[idx];
            audioSource.time = 0f;
            audioSource.Play();

            float len = audioSource.clip.length;
            float t = 0f;
            while (t < len)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // Fallback wait if clip missing
            Debug.LogWarning($"Manager_Ch3: Missing clip at index {idx}. Using 3s fallback.");
            yield return new WaitForSeconds(3f);
        }
    }
}
