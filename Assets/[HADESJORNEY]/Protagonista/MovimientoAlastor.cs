using UnityEngine;
using System.Collections;

public class MovimientoAlastor : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravedad = 20f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private ParticleSystem particulasPisada;

    private bool isDashing = false;
    private bool isInKnockback = false;

    private Vector3 forward;
    private Vector3 right;

    private CharacterController cc;
    private float velocidadVertical;

    private Coroutine dashActivo;
    private Coroutine retrocesoActivo;

    public Animator anim;

    private float tiempoPisada = 0f;
    private float intervaloPisada = 0.3f;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (cc == null)
        {
            Debug.LogError("MovimientoAlastor: falta CharacterController en el jugador.");
            enabled = false;
        }
    }

    private void Start()
    {
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
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && !isInKnockback)
        {
            if (dashActivo != null) StopCoroutine(dashActivo);
            dashActivo = StartCoroutine(Dash());

        }

        if (isDashing || isInKnockback)
        {
            return;
        }
        MoverJugador();
    }

    private void MoverJugador()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = horizontalInput * right + verticalInput * forward;

        AplicarGravedad();

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
        if(direction.magnitude > 0.01f)
        {
            anim.SetBool("correr", true);
            tiempoPisada += Time.deltaTime;
            if (tiempoPisada >= intervaloPisada)
            {
                particulasPisada.Play();
                tiempoPisada = 0f;
            }

        }
        else
        {
            tiempoPisada = 0f;
            anim.SetBool("correr", false);
        }
    }

    private void AplicarGravedad()
    {
        if (cc.isGrounded)
        {
            velocidadVertical = -1f;
        }
        else
        {
            velocidadVertical -= gravedad * Time.deltaTime;
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        anim.SetBool("dash", true);
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            AplicarGravedad();

            Vector3 movimiento = transform.forward * dashSpeed;
            movimiento.y = velocidadVertical;

            cc.Move(movimiento * Time.deltaTime);

            yield return null;
        }

        isDashing = false;
        dashActivo = null;
        anim.SetBool("dash", false);
    }

    public void AplicarRetroceso(Vector3 direccion, float distancia, float duracion)
    {
        if (!isActiveAndEnabled || cc == null)
        {
            return;
        }

        direccion.y = 0f;

        if (direccion.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direccion.Normalize();

        if (dashActivo != null)
        {
            StopCoroutine(dashActivo);
            dashActivo = null;
            isDashing = false;
        }

        if (retrocesoActivo != null)
        {
            StopCoroutine(retrocesoActivo);
        }

        retrocesoActivo = StartCoroutine(RetrocesoConColisiones(direccion, distancia, duracion));
    }

    private IEnumerator RetrocesoConColisiones(Vector3 direccion, float distancia, float duracion)
    {
        isInKnockback = true;

        float tiempo = 0f;
        float distanciaAnterior = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);
            float curva = 1f - Mathf.Pow(1f - t, 3f);

            float distanciaActual = distancia * curva;
            float deltaDistancia = distanciaActual - distanciaAnterior;
            distanciaAnterior = distanciaActual;

            AplicarGravedad();

            Vector3 desplazamiento = direccion * deltaDistancia;
            desplazamiento.y = velocidadVertical * Time.deltaTime;

            CollisionFlags colisiones = cc.Move(desplazamiento);

            if ((colisiones & CollisionFlags.Sides) != 0)
            {
                break;
            }

            yield return null;
        }

        isInKnockback = false;
        retrocesoActivo = null;
    }
}