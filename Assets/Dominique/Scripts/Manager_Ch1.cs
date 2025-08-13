using System.Collections;
using UnityEngine;
using TMPro;

public class Manager_Ch1 : MonoBehaviour
{
    /*──────── Tunables ───────────*/
    private const int TOTAL = 26;     // <- ✨ Updated: total dialog/clip count
    [Range(1, 10)] public int firstAutoCount = 4; // how many to autoplay at start

    /*──────── Outline refs ───────*/
    [Header("Item Outlines")]
    public Outline potOutline;
    public Outline seedOutline;
    public Outline xOutline;
    public Outline rulerOutline;
    public Outline waterCanOutline;
    public Outline chatBotOutline;

    Outline[] outlines;
    int       currentItemIndex = -1;   // starts at “none”

    /*──────── Dialog UI / Audio ───*/
    [Header("UI & Audio")]
    public TMP_Text    dialogText;
    public AudioSource audioSource;

    [Header("Dialog Content (exactly 26)")]
    public AudioClip[] dialogClips = new AudioClip[TOTAL];
    public string[]    dialogLines = new string[TOTAL];

    public GameObject continueButton;
    readonly bool[] played = new bool[TOTAL];

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

        if (dialogClips.Length != TOTAL || dialogLines.Length != TOTAL)
        {
            Debug.LogError($"Manager_Ch1: need EXACTLY {TOTAL} clips + {TOTAL} lines.");
            enabled = false;
            return;
        }

        foreach (var o in outlines) SetOutlineHidden(o);

        if (audioSource)
        {
            audioSource.playOnAwake = false;
            audioSource.loop        = false;
            audioSource.enabled     = true;

            // Start narration in 2D so distance/FOV can’t mute it at scene start.
            audioSource.spatialBlend = 0f;
            if (audioSource.volume <= 0f) audioSource.volume = 1f;
        }
    }

    void Start()
    {
        continueButton?.SetActive(false);
        StartCoroutine(AutoplayFirstN(firstAutoCount));
    }

    /*──────── Public API ─────────*/
    public void PlayDialogByIndex(int index) => TryPlay(index);

    /*──────── Internals ──────────*/
    void TryPlay(int idx)
    {
        if (idx < 0 || idx >= played.Length || played[idx]) return;

        // Show corresponding item outline for Dialog 5–10 (indices 4..9)
        if (idx >= 4 && idx <= 9) SwitchOutline(idx - 4);

        StartCoroutine(PlayLine(idx));
    }

    IEnumerator AutoplayFirstN(int count)
    {
        // one-frame + tiny delay: lets Quest get audio focus
        yield return null;
        yield return new WaitForSeconds(0.05f);

        if (Time.timeScale == 0f) Time.timeScale = 1f;
        int safe = Mathf.Clamp(count, 0, TOTAL);

        for (int i = 0; i < safe; i++)
            yield return PlayLine(i);
    }

    IEnumerator PlayLine(int idx)
    {
        played[idx] = true;

        if (dialogText) dialogText.text = dialogLines[idx];

        if (audioSource && dialogClips[idx])
        {
            audioSource.Stop();
            audioSource.clip = dialogClips[idx];
            audioSource.time = 0f;
            audioSource.Play();

            float len = audioSource.clip.length;
            float t = 0f;
            while (t < len) { t += Time.unscaledDeltaTime; yield return null; }
        }
        else
        {
            Debug.LogWarning($"Manager_Ch1: Missing clip at index {idx}. Using 3s fallback.");
            yield return new WaitForSeconds(3f);
        }

        if (idx == TOTAL - 1 && continueButton) continueButton.SetActive(true);
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
