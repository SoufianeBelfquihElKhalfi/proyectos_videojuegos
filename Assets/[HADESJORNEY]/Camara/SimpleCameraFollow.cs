using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    [SerializeField] private float smoothTime = 0.1f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 posicionDeseada = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, posicionDeseada, ref velocity, smoothTime);
    }
}
