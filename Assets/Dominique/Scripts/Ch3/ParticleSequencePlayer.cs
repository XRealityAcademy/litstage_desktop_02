using System.Collections;
using UnityEngine;

public class ParticleSequencePlayer : MonoBehaviour
{
    [Header("VFX (order matters)")]
    [Tooltip("Assign 12 ParticleSystem roots (their GameObjects will be toggled active/inactive).")]
    public ParticleSystem[] effects = new ParticleSystem[24];

    [Header("Background Music")]
    [Tooltip("AudioSource that will play the background music.")]
    public AudioSource bgmSource;

    [Tooltip("Music clip that will play once at the very start.")]
    public AudioClip bgmClip;

    [Header("Behavior")]
    [Tooltip("Auto-run the full sequence on Start().")]
    public bool playOnStart = true;

    [Tooltip("Wait until each effect finishes (IsAlive) before continuing.")]
    public bool waitForEffectToFinish = true;

    [Tooltip("Used if 'waitForEffectToFinish' is false.")]
    public float delayBetween = 1f;

    [Tooltip("Safety timeout to avoid hanging if an effect never reports IsAlive == false.")]
    public float maxWaitSeconds = 6f;

    private Coroutine sequenceRoutine;
    private int currentIndex = -1;

    void Awake()
    {
        // Enforce: no looping and no play-on-awake (we control playback)
        NormalizeAll(loop: false, playOnAwake: false);

        // Start state: ALL inactive, stopped, and cleared
        SetAllActive(false, alsoStopAndClear: true);

        // Setup BGM source if provided
        if (bgmSource)
        {
            bgmSource.playOnAwake = false;
            bgmSource.loop = false;
        }
    }

    void Start()
    {
        // Play BGM once at the very beginning
        if (bgmSource && bgmClip)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }

        if (playOnStart)
            sequenceRoutine = StartCoroutine(PlaySequence());
    }

    public void PlayFromStart()
    {
        if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
        SetAllActive(false, alsoStopAndClear: true);
        currentIndex = -1;

        // Restart BGM only if you want it to replay on restart
        if (bgmSource && bgmClip)
        {
            bgmSource.Stop();
            bgmSource.Play();
        }

        sequenceRoutine = StartCoroutine(PlaySequence());
    }

    public void StopSequence()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }
        SetAllActive(false, alsoStopAndClear: true);
        currentIndex = -1;

        // Stop BGM too
        if (bgmSource) bgmSource.Stop();
    }

    private IEnumerator PlaySequence()
    {
        if (effects == null || effects.Length == 0) yield break;

        for (int i = 0; i < effects.Length; i++)
        {
            yield return PlayAtIndex(i);
        }

        sequenceRoutine = null;
    }

    private IEnumerator PlayAtIndex(int index)
    {
        // Stop & deactivate previous
        if (currentIndex >= 0 && currentIndex < effects.Length && effects[currentIndex])
        {
            var prev = effects[currentIndex];
            prev.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (prev.gameObject.activeSelf) prev.gameObject.SetActive(false);
        }

        currentIndex = index;

        if (index < 0 || index >= effects.Length) yield break;
        var ps = effects[index];
        if (!ps) yield break;

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;

        // Activate & play
        ps.gameObject.SetActive(true);
        ps.Clear(true);
        ps.Play(true);

        if (waitForEffectToFinish)
        {
            float t = 0f;
            float heuristic = main.duration + Mathf.Max(0.5f, main.startLifetime.constantMax) + 0.5f;
            float timeout = Mathf.Max(0.5f, Mathf.Min(maxWaitSeconds, heuristic));

            while (t < timeout)
            {
                if (!ps.IsAlive(true)) break;
                t += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delayBetween));
        }

        // Ensure stopped & hidden before moving on
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
    }

    private void SetAllActive(bool active, bool alsoStopAndClear)
    {
        if (effects == null) return;
        foreach (var ps in effects)
        {
            if (!ps) continue;
            if (alsoStopAndClear)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear(true);
            }
            if (ps.gameObject.activeSelf != active)
                ps.gameObject.SetActive(active);
        }
    }

    private void NormalizeAll(bool loop, bool playOnAwake)
    {
        if (effects == null) return;
        foreach (var ps in effects)
        {
            if (!ps) continue;
            var main = ps.main;
            main.loop = loop;
            main.playOnAwake = playOnAwake;
        }
    }
}
