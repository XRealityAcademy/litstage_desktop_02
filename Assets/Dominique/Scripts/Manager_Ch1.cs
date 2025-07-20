using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Central controller for a 10‑line dialog sequence.
/// ─────────────────────────────────────────────────────
/// • Dialogs 0‑3 autoplay at Start().
/// • Dialogs 4‑9 are triggered by external callers
///   (e.g., TriggerDialog.cs) via <see cref="PlayDialogByIndex"/>.
/// • After dialog 9 (player‑visible “Dialog 10”) the Continue button appears.
/// </summary>
public class Manager_Ch1 : MonoBehaviour
{
    /*────────── Inspector Hooks ──────────*/
    [Header("UI & Audio")]
    public TMP_Text    dialogText;
    public AudioSource audioSource;

    [Header("Dialog Content (exactly 10)")]
    [Tooltip("Size must be 10: indices 0‑9.")]
    public AudioClip[] dialogClips = new AudioClip[10];
    [Tooltip("Size must be 10: indices 0‑9.")]
    public string[]    dialogLines = new string[10];

    public GameObject  continueButton;   // hidden until final line

    /*────────── Internals ──────────*/
    private readonly bool[] played = new bool[10];

    /*────────── Validation ─────────*/
    void Awake()
    {
        if (dialogClips.Length != 10 || dialogLines.Length != 10)
        {
            Debug.LogError("Manager_Ch1: provide EXACTLY 10 clips and 10 lines.");
            enabled = false;
        }
    }

    /*────────── Boot ───────────────*/
    void Start()
    {
        continueButton?.SetActive(false);
        StartCoroutine(AutoplayFirstFour());
    }

    IEnumerator AutoplayFirstFour()
    {
        for (int i = 0; i < 4; i++)
            yield return PlayLine(i);
    }

    /*────────── Public API ─────────*/
    /// <summary>Play a specific dialog index (0‑9). Safe to call multiple times.</summary>
    public void PlayDialogByIndex(int index)
    {
        if (index < 0 || index >= played.Length) return;
        if (played[index]) return;
        StartCoroutine(PlayLine(index));
    }

    /*────────── Core Coroutine ─────*/
    IEnumerator PlayLine(int idx)
    {
        played[idx] = true;

        if (dialogText) dialogText.text = dialogLines[idx];

        if (audioSource && dialogClips[idx])
        {
            audioSource.clip = dialogClips[idx];
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(3f); // fallback duration
        }

        if (idx == 9 && continueButton) continueButton.SetActive(true);
    }
}
