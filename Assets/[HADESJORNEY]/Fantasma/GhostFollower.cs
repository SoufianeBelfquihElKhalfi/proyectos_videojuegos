using UnityEngine;

public class GhostFollower : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Transform playerCenter;

    [Header("Distancia al jugador")]
    [SerializeField] private float distanciaMinima = 0.9f;
    [SerializeField] private float distanciaComoda = 1.35f;
    [SerializeField] private float distanciaMaxima = 1.9f;

    [Header("Movimiento")]
    [SerializeField] private float maxSnapDistance = 6f;

    [Header("Seguimiento progresivo")]
    [SerializeField] private float margenAceleracionLejos = 1.2f;
    [SerializeField] private float margenAceleracionCerca = 0.45f;

    [SerializeField] private float suavizadoSeguimientoSuave = 0.55f;
    [SerializeField] private float suavizadoSeguimientoFuerte = 0.24f;

    [SerializeField] private float velocidadSeguimientoSuave = 2.5f;
    [SerializeField] private float velocidadSeguimientoFuerte = 10f;

    [Header("Deriva en reposo")]
    [SerializeField] private float radioDeriva = 0.25f;
    [SerializeField] private float frecuenciaDeriva = 0.45f;

    private float faseDerivaX;
    private float faseDerivaZ;

    private Vector3 velocity;
    public Vector3 UltimaDireccionMovimiento { get; private set; }
    public Vector3 UltimaVelocidadMovimiento { get; private set; }

    private void Start()
    {
        transform.position = CalcularPosicionComoda();
        velocity = Vector3.zero;

        UltimaDireccionMovimiento = Vector3.zero;
        UltimaVelocidadMovimiento = Vector3.zero;

        faseDerivaX = Random.Range(0f, 100f);
        faseDerivaZ = Random.Range(0f, 100f);
    }

    private void LateUpdate()
    {
        Vector3 posicionAnterior = transform.position;
        Vector3 posicionObjetivo = CalcularSiguientePosicion();

        float distanciaAlObjetivo = Vector3.Distance(transform.position, posicionObjetivo);

        if (distanciaAlObjetivo > maxSnapDistance)
        {
            transform.position = posicionObjetivo;
            velocity = Vector3.zero;
        }
        else
        {
            float intensidadSeguimiento = CalcularIntensidadSeguimiento();

            float suavizadoActual = Mathf.Lerp(
                suavizadoSeguimientoSuave,
                suavizadoSeguimientoFuerte,
                intensidadSeguimiento
            );

            float velocidadActual = Mathf.Lerp(
                velocidadSeguimientoSuave,
                velocidadSeguimientoFuerte,
                intensidadSeguimiento
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                posicionObjetivo,
                ref velocity,
                suavizadoActual,
                velocidadActual
            );
        }

        Vector3 desplazamiento = transform.position - posicionAnterior;
        desplazamiento.y = 0f;

        UltimaDireccionMovimiento = desplazamiento;
        UltimaVelocidadMovimiento = desplazamiento / Time.deltaTime;
    }

    private Vector3 CalcularSiguientePosicion()
    {
        Vector3 posicionJugador = playerCenter.position;
        Vector3 offsetActual = transform.position - posicionJugador;
        offsetActual.y = 0f;

        float distanciaActual = offsetActual.magnitude;

        bool estaDemasiadoLejos = distanciaActual > distanciaMaxima;
        bool estaDemasiadoCerca = distanciaActual < distanciaMinima;

        if (estaDemasiadoLejos || estaDemasiadoCerca)
        {
            return CalcularPosicionComoda();
        }

        Vector3 posicionMantenida = transform.position + CalcularDeriva() * Time.deltaTime;
        posicionMantenida.y = followTarget.position.y;
        return posicionMantenida;
    }

    private Vector3 CalcularPosicionComoda()
    {
        Vector3 direccionDeseada = followTarget.position - playerCenter.position;
        direccionDeseada.y = 0f;

        if (direccionDeseada.sqrMagnitude < 0.001f)
        {
            direccionDeseada = -playerCenter.forward;
            direccionDeseada.y = 0f;
        }

        direccionDeseada.Normalize();

        Vector3 posicion = playerCenter.position + direccionDeseada * distanciaComoda;
        posicion.y = followTarget.position.y;

        return posicion;
    }

    private Vector3 CalcularDeriva()
    {
        float offsetX = Mathf.Sin((Time.time + faseDerivaX) * frecuenciaDeriva) * radioDeriva;
        float offsetZ = Mathf.Cos((Time.time + faseDerivaZ) * frecuenciaDeriva) * radioDeriva;

        return new Vector3(offsetX, 0f, offsetZ);
    }

    private float CalcularIntensidadSeguimiento()
    {
        Vector3 offset = transform.position - playerCenter.position;
        offset.y = 0f;

        float distancia = offset.magnitude;

        if (distancia > distanciaMaxima)
        {
            float excesoDistancia = distancia - distanciaMaxima;
            return Mathf.Clamp01(excesoDistancia / margenAceleracionLejos);
        }

        if (distancia < distanciaMinima)
        {
            float excesoCercania = distanciaMinima - distancia;
            return Mathf.Clamp01(excesoCercania / margenAceleracionCerca);
        }

        return 0f;
    }
}