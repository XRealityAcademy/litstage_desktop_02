using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer Reference")]
    public AudioMixer mainMixer; // Assign in Inspector

    [Header("Default Volume Settings")]
    [Range(-80, 0)]
    public float defaultVolumeDb = -10f; // Good default for most games

    private const string masterVolumeParam = "MasterVolume"; // Should match your exposed AudioMixer parameter name

    void Start()
    {
        // Set the mixer to the default volume at startup
        SetVolumeDb(defaultVolumeDb);
    }

    /// <summary>
    /// Set the master volume directly in decibels.
    /// </summary>
    public void SetVolumeDb(float dB)
    {
        if (mainMixer != null)
            mainMixer.SetFloat(masterVolumeParam, Mathf.Clamp(dB, -80f, 0f));
        else
            Debug.LogWarning("AudioManager: mainMixer not assigned!");
    }

    /// <summary>
    /// Set the volume using a slider value (0 = mute, 1 = max), mapped logarithmically for audio comfort.
    /// </summary>
    /// <param name="sliderValue">Slider value from 0.0 (mute) to 1.0 (full)</param>
    public void SetVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Lerp(0.0001f, 1f, sliderValue)) * 20f;
        SetVolumeDb(dB);
    }

    /// <summary>
    /// (Optional) Get the current volume as a slider value (0...1)
    /// </summary>
    public float GetSliderValue()
    {
        if (mainMixer != null && mainMixer.GetFloat(masterVolumeParam, out float dB))
        {
            float linear = Mathf.Pow(10f, dB / 20f);
            return Mathf.InverseLerp(0.0001f, 1f, linear);
        }
        return 1f;
    }
}