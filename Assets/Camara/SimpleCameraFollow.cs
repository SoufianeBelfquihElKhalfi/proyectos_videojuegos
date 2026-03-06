using System.Collections.Specialized;
using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset;
    private void LateUpdate()
    {
        transform.position= target.position + offset;
    }
}
