using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Manager_Ch1 : MonoBehaviour
{
    public TMP_Text dialogText;      // Assign your UI Text component
    public AudioSource audioSource; // Assign your AudioSource component
    public AudioClip[] dialogClips; // Assign 4 audio clips in order
    public string[] dialogLines; // Assign 4 dialog lines in order
    public GameObject continueButton; // The button to show at the end

    private int currentDialog = 0;

    void Start()
    {
        if (dialogLines.Length != 4 || dialogClips.Length != 4)
        {
            Debug.LogError("You need exactly 4 dialogs and 4 audio clips.");
            return;
        }
        continueButton.SetActive(false);
        StartCoroutine(PlayDialogs());
    }

    public void Dialog() // This function can be called to restart the dialog sequence
    {
        currentDialog = 0;
        continueButton.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(PlayDialogs());
    }

    IEnumerator PlayDialogs()
    {
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(2f); // Pause between dialogs
            dialogText.text = dialogLines[i];
            audioSource.clip = dialogClips[i];
            audioSource.Play();
            yield return new WaitForSeconds(3f); // Audio duration
            audioSource.Stop();
            yield return new WaitForSeconds(1f); // Pause between dialogs
        }
        continueButton.SetActive(true); // Show button at the end
    }
}
