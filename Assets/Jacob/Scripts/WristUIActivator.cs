using UnityEngine;

public class WristUIActivator : MonoBehaviour
{
    public GameObject wristUI;

    [Header("Activation Settings")]
    public float showThreshold = 0.2f;
    public float hideThreshold = 0.1f;

    private bool isVisible = false;

    void Update()
    {
        float backHandUpDot = Vector3.Dot(-transform.up, Vector3.up);

        if (!isVisible && backHandUpDot > showThreshold)
        {
            wristUI.SetActive(true);
            isVisible = true;
        }
        else if (isVisible && backHandUpDot < hideThreshold)
        {
            wristUI.SetActive(false);
            isVisible = false;
        }
    }
}