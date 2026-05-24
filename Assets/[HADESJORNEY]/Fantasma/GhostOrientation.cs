using UnityEngine;

public class GhostOrientation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GhostFollower follower;
    [SerializeField] private FantasmaCombate combate;

    [Header("Rotación")]
    [SerializeField] private float velocidadRotacionMovimiento = 360f;
    [SerializeField] private float velocidadRotacionReposo = 90f;
    [SerializeField] private float velocidadMinimaParaRotar = 0.05f;

    [Header("Mirada en reposo")]
    [SerializeField] private float tiempoMinimoCambioReposo = 1.5f;
    [SerializeField] private float tiempoMaximoCambioReposo = 3.5f;
    [SerializeField] private float gradosMaximosCambioReposo = 45f;

    private Vector3 direccionReposo;
    private float siguienteCambioReposo;

    private void Awake()
    {
        direccionReposo = transform.forward;
        direccionReposo.y = 0f;
        direccionReposo.Normalize();

        ProgramarSiguienteCambioReposo();
    }

    private void LateUpdate()
    {
        float velocidadRotacion;
        Vector3 direccion = ObtenerDireccionMirada(out velocidadRotacion);

        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            rotacionObjetivo,
            velocidadRotacion * Time.deltaTime
        );
    }

    private Vector3 ObtenerDireccionMirada(out float velocidadRotacion)
    {
        Transform objetivoCombate = combate.ObtenerObjetivoActual();

        if (objetivoCombate != null)
        {
            velocidadRotacion = velocidadRotacionMovimiento;
            return objetivoCombate.position - transform.position;
        }

        Vector3 velocidadMovimiento = follower.UltimaVelocidadMovimiento;

        if (velocidadMovimiento.magnitude > velocidadMinimaParaRotar)
        {
            direccionReposo = velocidadMovimiento.normalized;
            velocidadRotacion = velocidadRotacionMovimiento;
            return velocidadMovimiento;
        }

        ActualizarDireccionReposo();

        velocidadRotacion = velocidadRotacionReposo;
        return direccionReposo;
    }

    private void ActualizarDireccionReposo()
    {
        if (Time.time < siguienteCambioReposo)
        {
            return;
        }

        float angulo = Random.Range(-gradosMaximosCambioReposo, gradosMaximosCambioReposo);

        direccionReposo = Quaternion.Euler(0f, angulo, 0f) * direccionReposo;
        direccionReposo.Normalize();

        ProgramarSiguienteCambioReposo();
    }

    private void ProgramarSiguienteCambioReposo()
    {
        siguienteCambioReposo = Time.time + Random.Range(
            tiempoMinimoCambioReposo,
            tiempoMaximoCambioReposo
        );
    }
}