using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages a 10-line dialog + a red-outline baton that hops across six items.
/// ─────────────────────────────────────────────────────────────────────────
/// • Dialog 0-3 autoplay at Start().
/// • Dialog 4-9 (indices 4–9) are triggered by TriggerDialog.cs
///   which simply calls <see cref="PlayDialogByIndex(int)"/>.
/// • As each dialog fires, the matching item’s outline turns red and the
///   previous item’s outline is set to OutlineHidden.
/// • After dialog 9 (player-visible “Dialog 10”) the Continue button appears.
/// </summary>
public class Manager_Ch1 : MonoBehaviour
{
    /*──────────────────── Outline Enum & Map ────────────────────*/
    public enum Item { Pot, Seed, X, Ruler, WaterCan, ChatBot }

    [Header("Item Outlines (assign in Inspector)")]
    public Outline potOutline;
    public Outline seedOutline;
    public Outline xOutline;
    public Outline rulerOutline;
    public Outline waterCanOutline;
    public Outline chatBotOutline;

    private Outline[] outlines; // length 6
    private int       currentItemIndex = -1; // none yet

    /*──────────────────── Dialog Fields ─────────────────────────*/
    [Header("UI & Audio")]
    public TMP_Text    dialogText;
    public AudioSource audioSource;

    [Header("Dialog Content (exactly 10)")]
    public AudioClip[] dialogClips = new AudioClip[10];
    public string[]    dialogLines = new string[10];

    public GameObject continueButton;

    readonly bool[] played = new bool[10];

    /*──────────────────── Setup ─────────────────────────────────*/
    void Awake()
    {
        // pack outline array for easy indexing
        outlines = new[]
        {
            potOutline, seedOutline, xOutline,
            rulerOutline, waterCanOutline, chatBotOutline
        };

        // validate
        if (dialogClips.Length != 10 || dialogLines.Length != 10)
        {
            Debug.LogError("Manager_Ch1: need EXACTLY 10 clips and 10 lines.");
            enabled = false;
        }

        // ensure all outlines start hidden
        foreach (var o in outlines)
            SetOutlineHidden(o);
    }

    void Start()
    {
        continueButton?.SetActive(false);
        StartCoroutine(AutoplayFirstFour());
    }

    /*──────────────────── Public API ────────────────────────────*/
    public void PlayDialogByIndex(int index) => TryPlay(index);

    /*──────────────────── Internals ─────────────────────────────*/
    void TryPlay(int idx)
    {
        if (idx < 0 || idx >= played.Length) return;
        if (played[idx])                    return;

        // swap outlines if idx maps to an item (4-9)
        if (idx >= 4 && idx <= 9)
            SwitchOutline(idx - 4);         // map 4→0, 5→1, …

        StartCoroutine(PlayLine(idx));
    }

    IEnumerator AutoplayFirstFour()
    {
        for (int i = 0; i < 4; i++)
            yield return PlayLine(i);
    }

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
            yield return new WaitForSeconds(3f);

        if (idx == 9 && continueButton) continueButton.SetActive(true);
    }

    /*──────────────────── Outline Helpers ──────────────────────*/
    void SwitchOutline(int newIndex)
    {
        // hide previous
        if (currentItemIndex >= 0 && currentItemIndex < outlines.Length)
            SetOutlineHidden(outlines[currentItemIndex]);

        // show new
        currentItemIndex = newIndex;
        if (currentItemIndex >= 0 && currentItemIndex < outlines.Length)
            SetOutlineRed(outlines[currentItemIndex]);
    }

    static void SetOutlineRed(Outline o)
    {
        if (!o) return;
        o.OutlineColor = Color.red;
        o.OutlineMode  = Outline.Mode.OutlineAll;
        o.enabled      = true;
    }

    static void SetOutlineHidden(Outline o)
    {
        if (!o) return;
        o.OutlineMode = Outline.Mode.OutlineHidden;
        // optionally disable component entirely:
        // o.enabled = false;
    }
}
