using System.Collections;
using UnityEngine;
using TMPro;

public class Manager_Ch1 : MonoBehaviour
{
    /*──────── Outline refs ───────*/
    [Header("Item Outlines")]
    public Outline potOutline;
    public Outline seedOutline;
    public Outline xOutline;
    public Outline rulerOutline;
    public Outline waterCanOutline;
    public Outline chatBotOutline;

    Outline[] outlines;
    int       currentItemIndex = -1;   // <- starts at “none”

    /*──────── Dialog UI / Audio ───*/
    [Header("UI & Audio")]
    public TMP_Text    dialogText;
    public AudioSource audioSource;

    [Header("Dialog Content (exactly 10)")]
    public AudioClip[] dialogClips = new AudioClip[10];
    public string[]    dialogLines = new string[10];

    public GameObject continueButton;
    readonly bool[] played = new bool[10];

    /*──────── Constants ───────────*/
    static readonly Color Pink = ParseHex("#FF0047");
    const float ActiveWidth = 10f, HiddenWidth = 0f;

    /*──────── Awake ───────────────*/
    void Awake()
    {
        outlines = new[] {
            potOutline, seedOutline, xOutline,
            rulerOutline, waterCanOutline, chatBotOutline
        };

        if (dialogClips.Length != 10 || dialogLines.Length != 10)
        {
            Debug.LogError("Manager_Ch1: need EXACTLY 10 clips + 10 lines.");
            enabled = false;
            return;
        }

        // Hide every outline; none are shown until Dialog 5 triggers
        foreach (var o in outlines) SetOutlineHidden(o);
    }

    void Start()
    {
        continueButton?.SetActive(false);
        StartCoroutine(AutoplayFirstFour());
    }

    /*──────── Public API ─────────*/
    public void PlayDialogByIndex(int index) => TryPlay(index);

    /*──────── Internals ──────────*/
    void TryPlay(int idx)
    {
        if (idx < 0 || idx >= played.Length || played[idx]) return;

        // Show corresponding item outline for Dialog 5-10
        if (idx >= 4 && idx <= 9) SwitchOutline(idx - 4);

        StartCoroutine(PlayLine(idx));
    }

    IEnumerator AutoplayFirstFour()
    {
        for (int i = 0; i < 4; i++) yield return PlayLine(i);
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

    /*──────── Outline helpers ────*/
    void SwitchOutline(int newIndex)
    {
        if (currentItemIndex >= 0 && currentItemIndex < outlines.Length)
            SetOutlineHidden(outlines[currentItemIndex]);

        currentItemIndex = newIndex;
        if (currentItemIndex >= 0 && currentItemIndex < outlines.Length)
            SetOutlineRed(outlines[currentItemIndex]);
    }

    void SetOutlineRed(Outline o)
    {
        if (!o) return;
        o.OutlineColor = Pink;
        o.OutlineWidth = ActiveWidth;
        o.OutlineMode  = Outline.Mode.OutlineAll;
        o.enabled      = true;
    }

    void SetOutlineHidden(Outline o)
    {
        if (!o) return;
        o.OutlineWidth = HiddenWidth;
        o.OutlineMode  = Outline.Mode.OutlineHidden;
        o.enabled      = true;
    }

    static Color ParseHex(string hex)
    {
        Color c; ColorUtility.TryParseHtmlString(hex, out c); return c;
    }
}
