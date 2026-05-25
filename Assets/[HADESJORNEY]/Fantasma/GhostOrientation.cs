using UnityEngine;

public class GhostOrientation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GhostFollower follower;
    [SerializeField] private FantasmaCombate combate;

    [Header("Rotación")]
    [SerializeField] private float suavizadoRotacionMovimiento = 0.12f;
    [SerializeField] private float suavizadoRotacionReposo = 0.75f;
    [SerializeField] private float velocidadMaximaRotacionMovimiento = 720f;
    [SerializeField] private float velocidadMaximaRotacionReposo = 120f;
    [SerializeField] private float velocidadMinimaParaRotar = 0.05f;

    [Header("Mirada en reposo")]
    [SerializeField] private float tiempoMinimoCambioReposo = 1.5f;
    [SerializeField] private float tiempoMaximoCambioReposo = 3.5f;
    [SerializeField] private float gradosMaximosCambioReposo = 45f;

    private Vector3 direccionReposo;
    private float siguienteCambioReposo;
    private float velocidadAngularActual;

    private void Awake()
    {
        direccionReposo = transform.forward;
        direccionReposo.y = 0f;
        direccionReposo.Normalize();

        ProgramarSiguienteCambioReposo();
    }

    private void LateUpdate()
    {
        bool estaEnReposo;
        Vector3 direccion = ObtenerDireccionMirada(out estaEnReposo);
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.001f)
        {
            return;
        }

        float yawObjetivo = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg;
        float yawActual = transform.eulerAngles.y;

        float suavizado = estaEnReposo ? suavizadoRotacionReposo : suavizadoRotacionMovimiento;
        float velocidadMaxima = estaEnReposo ? velocidadMaximaRotacionReposo : velocidadMaximaRotacionMovimiento;

        float yawSuavizado = Mathf.SmoothDampAngle(
            yawActual,
            yawObjetivo,
            ref velocidadAngularActual,
            suavizado,
            velocidadMaxima
        );

        transform.rotation = Quaternion.Euler(0f, yawSuavizado, 0f);
    }

    private Vector3 ObtenerDireccionMirada(out bool estaEnReposo)
    {
        Transform objetivoCombate = combate.ObtenerObjetivoActual();

        if (objetivoCombate != null)
        {
            estaEnReposo = false;
            return objetivoCombate.position - transform.position;
        }

        Vector3 velocidadMovimiento = follower.UltimaVelocidadMovimiento;

        if (velocidadMovimiento.magnitude > velocidadMinimaParaRotar)
        {
            direccionReposo = velocidadMovimiento.normalized;
            estaEnReposo = false;
            return velocidadMovimiento;
        }

        ActualizarDireccionReposo();

        estaEnReposo = true;
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