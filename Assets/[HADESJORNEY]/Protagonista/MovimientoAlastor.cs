using UnityEngine;
using System.Collections;

/* Mejorado el input del dash, ya no está hardcodeado.
 * Se han cambiado los public a SerializeField.
 * RotationSpeed → rotationSpeed
 * null check para Camera.main
 */
public class MovimientoAlastor : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;

    private bool isDashing = false;
    private Vector3 forward;
    private Vector3 right;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (Camera.main == null)
        {
            Debug.LogError("MovimientoAlastor: no se ha encontrado Main Camera.");
            enabled = false;
            return;
        }

        forward = Camera.main.transform.forward;
        forward.y = 0f;
        forward = Vector3.Normalize(forward);

        right = Camera.main.transform.right;
        right.y = 0f;
        right = Vector3.Normalize(right);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Dash") && !isDashing)
        {
            StartCoroutine(Dash());
        }

        if (isDashing)
        {
            return;
        }

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
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            Vector3 movement = transform.forward * dashSpeed * Time.deltaTime;

            if (!Physics.Raycast(rb.position, transform.forward, movement.magnitude + 0.5f))
            {
                rb.MovePosition(rb.position + movement);
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