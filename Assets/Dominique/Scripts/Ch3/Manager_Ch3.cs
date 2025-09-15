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

    [Header("Chapter 3 Props")]
    [Tooltip("Chart visual that appears at index 7 and hides at index 9.")]
    public GameObject chart;

    [Tooltip("Number UI that appears at index 9.")]
    public GameObject numberUI;

    [Header("UI")]
    [Tooltip("Shown when all lines have finished playing.")]
    public GameObject continueButton;
    
    [Tooltip("Shown at index 11.")]
    public GameObject continueButtonIndex11;

    /*──────── Internals ─────*/
    readonly bool[] played = new bool[TOTAL];
    Coroutine autoplayRoutine;

    /*──────── Unity ─────────*/
    void Awake()
    {
        // Validation
        if (dialogClips == null || dialogClips.Length != TOTAL ||
            dialogLines == null || dialogLines.Length != TOTAL)
        {
            Debug.LogError($"Manager_Ch3: Need EXACTLY {TOTAL} clips and {TOTAL} lines.");
            enabled = false;
            return;
        }

        if (perLineDelays == null || perLineDelays.Length != TOTAL)
            perLineDelays = new float[TOTAL];

        // Audio defaults
        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            if (audioSource.volume <= 0f) audioSource.volume = 1f;
        }

        // Ensure initial prop visibility (safe defaults)
        if (chart) chart.SetActive(false);
        if (numberUI) numberUI.SetActive(false);
        if (continueButtonIndex11) continueButtonIndex11.SetActive(false);
    }

    void Start()
    {
        if (continueButton) continueButton.SetActive(false);
        autoplayRoutine = StartCoroutine(AutoPlayAll());
    }

    /*──────── Public API ────*/
    public void PlayDialogByIndex(int index)
    {
        if (index < 0 || index >= TOTAL) return;
        StartCoroutine(PlayLine(index));
    }

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

            float delay = perLineDelays[i] > 0f ? perLineDelays[i] : defaultDelay;
            if (delay > 0f && i < TOTAL - 1)
                yield return new WaitForSeconds(delay);
        }

        if (continueButton) continueButton.SetActive(true);
    }

    IEnumerator PlayLine(int idx)
    {
        played[idx] = true;

        // Text
        if (dialogText) dialogText.text = dialogLines[idx];

        // Audio (or fallback)
        if (audioSource && dialogClips[idx])
        {
            audioSource.Stop();
            audioSource.clip = dialogClips[idx];
            audioSource.time = 0f;
            audioSource.Play();

            float len = audioSource.clip.length;
            float t = 0f;
            while (t < len) { t += Time.deltaTime; yield return null; }
        }
        else
        {
            Debug.LogWarning($"Manager_Ch3: Missing clip at index {idx}. Using 3s fallback.");
            yield return new WaitForSeconds(3f);
        }

        // Chapter-3 specific post-line actions
        PostLineActions(idx);
    }

    void PostLineActions(int idx)
    {
        // 0–6 remain untouched by design

        if (idx == 7)
        {
            // Show the chart
            if (chart) chart.SetActive(true);
        }
        else if (idx == 9)
        {
            // Hide chart, show number UI
            if (chart) chart.SetActive(false);
            if (numberUI) numberUI.SetActive(true);
        }
        else if (idx == 11)
        {
            // Show continue button at index 11
            if (continueButtonIndex11) continueButtonIndex11.SetActive(true);
        }
        else if (idx == 12)
        {
            // Hide NumberUI and Chart at index 12
            if (numberUI) numberUI.SetActive(false);
            if (chart) chart.SetActive(false);
        }
    }
}
