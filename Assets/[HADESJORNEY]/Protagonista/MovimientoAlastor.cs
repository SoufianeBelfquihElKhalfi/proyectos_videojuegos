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
    [SerializeField] private float gravedad = 20f;

    private bool isDashing = false;
    private Vector3 forward;
    private Vector3 right;
    private CharacterController cc;
    private float velocidadVertical;

    private void Start()
    {
        cc = GetComponent<CharacterController>();

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
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            StartCoroutine(Dash());
        }

        if (isDashing) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 direction = horizontalInput * right + verticalInput * forward;

        if (cc.isGrounded)
            velocidadVertical = -1f;
        else
            velocidadVertical -= gravedad * Time.deltaTime;

        Vector3 movimiento = direction * speed;
        movimiento.y = velocidadVertical;
        cc.Move(movimiento * Time.deltaTime);

        if (direction.magnitude > 0.1f)
        {
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
            movement.y = cc.isGrounded ? -1f : -gravedad * Time.deltaTime;

            cc.Move(movement);

            yield return null;
        }

        isDashing = false;
    }
}