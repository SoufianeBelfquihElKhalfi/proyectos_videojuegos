using UnityEngine;

public class Exclamacion : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam != null)
            transform.rotation = cam.transform.rotation;
    }
}
