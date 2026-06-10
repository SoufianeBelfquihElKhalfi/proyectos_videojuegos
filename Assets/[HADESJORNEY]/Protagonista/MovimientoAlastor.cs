using UnityEngine;
using System.Collections;
using Dapasa.Audio;
using Dapasa.Audio;

[RequireComponent(typeof(CharacterController))]
public class MovimientoAlastor : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float aceleracion = 25f;
    [SerializeField] private float deceleracion = 30f;
    [SerializeField] private float rotacionVelocidadGrados = 720f;
    [SerializeField] private float gravedad = 20f;
    [SerializeField] private float multiplicadorCaida = 1.5f;

    [Header("Dash")]
    [SerializeField] private string idSonidoDash = "dash";
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private ParticleSystem particulasPisada;

    [Header("Audio Pisadas")]
    [SerializeField] private string idSonidoPisada = "pisada_alastor";
    [SerializeField] private float pitchMinPisada = 0.92f;
    [SerializeField] private float pitchMaxPisada = 1.08f;
    [SerializeField] private float volumenMinPisada = 0.85f;
    [SerializeField] private float volumenMaxPisada = 1f;
    [SerializeField] private float tiempoMinimoEntrePisadas = 0.12f;

    [Header("Animator")]
    public Animator anim;

    // Estado público
    public bool movimientoHabilitado = true;
    public bool isDashing = false;
    public bool esInvulnerable = false;
    public float velocidadActual = 0f;

    // Componentes y referencias cacheadas
    private CharacterController cc;
    private Transform camTransform;
    private EfectoDash dash;

    // Estado interno
    private Vector3 velocidadHorizontal;
    private float velocidadVertical;
    private float tiempoUltimoDash = -999f;
    private bool isInKnockback = false;

    private Coroutine dashActivo;
    private Coroutine retrocesoActivo;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        dash = GetComponent<EfectoDash>();

        if (Camera.main == null)
        {
            Debug.LogError("MovimientoAlastor: no se ha encontrado Main Camera.");
            enabled = false;
            return;
        }

        camTransform = Camera.main.transform;
    }

    private void Update()
    {
        ProcesarInputDash();

        if (isDashing || isInKnockback)
            return;

        MoverJugador();
    }

    private void ProcesarInputDash()
    {
        if (!Input.GetButtonDown("Dash")) return;
        if (isDashing || isInKnockback) return;
        if (Time.time - tiempoUltimoDash < dashCooldown) return;

        tiempoUltimoDash = Time.time;

        if (dashActivo != null)
            StopCoroutine(dashActivo);

        dashActivo = StartCoroutine(Dash());
    }

    private void MoverJugador()
    {
        if (!movimientoHabilitado)
        {
            velocidadHorizontal = Vector3.zero;
            velocidadActual = 0f;

            if (anim != null)
                anim.SetBool("correr", false);

            return;
        }

        Vector3 direccionInput = ObtenerDireccionInput();

        ActualizarVelocidad(direccionInput);
        AplicarGravedad();
        AplicarMovimiento();
        ActualizarRotacion(direccionInput);
        ActualizarAnimacion(direccionInput);
    }

    private Vector3 ObtenerDireccionInput()
    {
        Vector3 forward = camTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = camTransform.right;
        right.y = 0f;
        right.Normalize();

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        return h * right + v * forward;
    }

    private void ActualizarVelocidad(Vector3 direccionInput)
    {
        Vector3 objetivo = direccionInput * speed;
        float factor = direccionInput.magnitude > 0.1f ? aceleracion : deceleracion;

        velocidadHorizontal = Vector3.MoveTowards(
            velocidadHorizontal,
            objetivo,
            factor * Time.deltaTime
        );

        if (velocidadHorizontal.magnitude < 0.05f)
            velocidadHorizontal = Vector3.zero;

        velocidadActual = velocidadHorizontal.magnitude / speed;
    }

    private void AplicarGravedad()
    {
        if (cc.isGrounded)
        {
            velocidadVertical = -1f;
        }
        else
        {
            float mult = velocidadVertical < 0 ? multiplicadorCaida : 1f;
            velocidadVertical -= gravedad * mult * Time.deltaTime;
        }
    }

    private void AplicarMovimiento()
    {
        Vector3 movimiento = velocidadHorizontal;
        movimiento.y = velocidadVertical;

        cc.Move(movimiento * Time.deltaTime);
    }

    private void ActualizarRotacion(Vector3 direccionInput)
    {
        if (direccionInput.magnitude < 0.1f) return;

        Quaternion objetivo = Quaternion.LookRotation(direccionInput);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            objetivo,
            rotacionVelocidadGrados * Time.deltaTime
        );
    }

    private void ActualizarAnimacion(Vector3 direccionInput)
    {
        bool moviendose = direccionInput.magnitude > 0.01f;

        if (anim != null)
            anim.SetBool("correr", moviendose);
    }

    // Esta función la llamas desde Animation Events en la animación de correr/caminar.
    public void EventoPisada()
    {
        if (!movimientoHabilitado) return;
        if (isDashing || isInKnockback) return;
        if (velocidadActual <= 0.1f) return;
        if (!cc.isGrounded) return;

        if (particulasPisada != null)
            particulasPisada.Play();

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("No hay AudioManager en la escena.");
            return;
        }

        AudioManager.Instance.ReproducirSFX2DConVariacion(
            idSonidoPisada,
            pitchMinPisada,
            pitchMaxPisada,
            volumenMinPisada,
            volumenMaxPisada
        );
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        esInvulnerable = true;

        if (anim != null)
            anim.SetBool("dash", true);

        if (dash != null)
            dash.Activar();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirSFX2D(idSonidoDash);
        }

        float tiempo = 0f;

        while (tiempo < dashDuration)
        {
            tiempo += Time.deltaTime;
            AplicarGravedad();

            Vector3 movimiento = transform.forward * dashSpeed;
            movimiento.y = velocidadVertical;

            cc.Move(movimiento * Time.deltaTime);

            yield return null;
        }

        isDashing = false;
        esInvulnerable = false;

        if (anim != null)
            anim.SetBool("dash", false);

        if (dash != null)
            dash.Desactivar();

        dashActivo = null;
    }

    public void AplicarRetroceso(Vector3 direccion, float distancia, float duracion)
    {
        if (!isActiveAndEnabled || cc == null) return;

        direccion.y = 0f;

        if (direccion.sqrMagnitude <= 0.001f) return;

        direccion.Normalize();

        if (dashActivo != null)
        {
            StopCoroutine(dashActivo);
            dashActivo = null;
            isDashing = false;
            esInvulnerable = false;

            if (anim != null)
                anim.SetBool("dash", false);

            if (dash != null)
                dash.Desactivar();
        }

        if (retrocesoActivo != null)
            StopCoroutine(retrocesoActivo);

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
                break;

            yield return null;
        }

        isInKnockback = false;
        retrocesoActivo = null;
    }

    public bool EstaMoviendose()
    {
        return velocidadActual > 0.1f;
    }
}