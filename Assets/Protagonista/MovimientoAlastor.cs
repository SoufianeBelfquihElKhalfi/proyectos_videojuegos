using System.Collections.Specialized;
using System.Threading;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float speed = 6f;
    public float RotationSpeed = 10f;
    public Vector3 forward, right;
    void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);

        right = Camera.main.transform.right;
        right.y = 0;        
        right = Vector3.Normalize(right);
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction=horizontalInput * right + verticalInput * forward;
        if (direction.magnitude > 0.1f)
        {
            
            transform.position += direction * speed * Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed*Time.deltaTime);
        }
    }
}
