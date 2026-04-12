using UnityEngine;
using System.Collections;

public class MovimientoAlastor : MonoBehaviour
{
    public float speed = 6f;
    public float RotationSpeed = 10f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    private bool isDashing = false;
    public Vector3 forward, right;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Camera.main.transform.right;
        right.y = 0;
        right = Vector3.Normalize(right);
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        if (isDashing) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 direction = horizontalInput * right + verticalInput * forward;

        if (direction.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + direction * speed * Time.deltaTime);

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
            Vector3 movimiento = transform.forward * dashSpeed * Time.deltaTime;

            if (!Physics.Raycast(rb.position, transform.forward, movimiento.magnitude + 0.5f))
            {
                rb.MovePosition(rb.position + movimiento);
            }
            else
            {
                break;
            }

            yield return null;
        }
        isDashing = false;
    }
}