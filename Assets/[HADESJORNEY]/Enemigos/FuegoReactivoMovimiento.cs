using UnityEngine;

public class FuegoReactivoMovimiento : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform objetivoMovimiento;
    [SerializeField] private Transform pivoteFuego;

    [Header("Inclinación")]
    [SerializeField] private float velocidadParaInclinacionMaxima = 3.5f;
    [SerializeField] private float inclinacionMaxima = 28f;
    [SerializeField] private float suavizado = 12f;
    [SerializeField] private float umbralVelocidad = 0.05f;

    [Header("Seguridad")]
    [SerializeField] private float distanciaMaximaSinTeleport = 2f;

    private Vector3 posicionAnterior;
    private Quaternion rotacionInicialLocal;
    private Quaternion rotacionObjetivoLocal;

    private void Awake()
    {
        if (pivoteFuego == null)
        {
            pivoteFuego = transform;
        }

        if (objetivoMovimiento == null)
        {
            objetivoMovimiento = transform.root;
        }

        posicionAnterior = objetivoMovimiento.position;
        rotacionInicialLocal = pivoteFuego.localRotation;
        rotacionObjetivoLocal = rotacionInicialLocal;
    }

    private void LateUpdate()
    {
        if (objetivoMovimiento == null || pivoteFuego == null || Time.deltaTime <= 0f)
        {
            return;
        }

        Vector3 desplazamiento = objetivoMovimiento.position - posicionAnterior;
        posicionAnterior = objetivoMovimiento.position;

        if (desplazamiento.magnitude > distanciaMaximaSinTeleport)
        {
            rotacionObjetivoLocal = rotacionInicialLocal;
            AplicarRotacionSuavizada();
            return;
        }

        Vector3 velocidad = desplazamiento / Time.deltaTime;
        velocidad.y = 0f;

        if (velocidad.sqrMagnitude < umbralVelocidad * umbralVelocidad)
        {
            rotacionObjetivoLocal = rotacionInicialLocal;
            AplicarRotacionSuavizada();
            return;
        }

        Transform referenciaLocal = pivoteFuego.parent != null ? pivoteFuego.parent : pivoteFuego;

        Vector3 velocidadLocal = referenciaLocal.InverseTransformDirection(velocidad);
        velocidadLocal.y = 0f;

        Vector3 direccionLocal = velocidadLocal.normalized;

        float factorVelocidad = Mathf.Clamp01(velocidadLocal.magnitude / velocidadParaInclinacionMaxima);
        float angulo = inclinacionMaxima * factorVelocidad;

        Quaternion inclinacionContraria = Quaternion.Euler(
            -direccionLocal.z * angulo,
            0f,
            direccionLocal.x * angulo
        );

        rotacionObjetivoLocal = rotacionInicialLocal * inclinacionContraria;

        AplicarRotacionSuavizada();
    }

    private void AplicarRotacionSuavizada()
    {
        float t = 1f - Mathf.Exp(-suavizado * Time.deltaTime);

        pivoteFuego.localRotation = Quaternion.Slerp(
            pivoteFuego.localRotation,
            rotacionObjetivoLocal,
            t
        );
    }
}