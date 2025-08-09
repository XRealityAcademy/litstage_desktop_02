using UnityEngine;
public class Spinner : MonoBehaviour
{
    public float speed = 180f;
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
    }
}