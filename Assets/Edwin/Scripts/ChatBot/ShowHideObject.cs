using UnityEngine;

public class ShowHideObject : MonoBehaviour
{
    [Header("If true, object is hidden on Start")]
    public bool hideOnStart = true;

    private void Start()
    {
        gameObject.SetActive(!hideOnStart);
    }

    // Call this function to show the object
    public void Show()
    {
        gameObject.SetActive(true);
    }

    // Call this function to hide the object
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}