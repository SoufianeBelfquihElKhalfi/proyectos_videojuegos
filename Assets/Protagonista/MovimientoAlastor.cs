using UnityEngine;
using System.Collections;

public class MovimientoAlastor : MonoBehaviour
{
    public float speed = 6f;
    public float RotationSpeed = 10f;

    public float dashSpeed = 80f;
    public float dashDuration = 0.2f;
    private bool isDashing = false;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        if (isDashing) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = horizontalInput * right + verticalInput * forward;

        if (direction.magnitude > 0.1f)
        {
            transform.position += direction * speed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                RotationSpeed * Time.deltaTime
            );
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            transform.position += transform.forward * dashSpeed * Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}