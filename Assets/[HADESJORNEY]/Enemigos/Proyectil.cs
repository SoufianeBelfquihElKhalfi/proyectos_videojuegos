using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float velocidad = 10f;
    [SerializeField] private int danio = 1;
    [SerializeField] private float tiempoVida = 5f;
    [SerializeField] private float gravedad = 2f;

    [Header("Retroceso")]
    [SerializeField] private float fuerzaRetroceso = 2f;
    [SerializeField] private float duracionRetroceso = 0.2f;

    private Vector3 velocidadActual;
    private bool inicializado = false;
    private Rigidbody rb;

    public void Inicializar(Vector3 dir)
    {
        Debug.Log("Dirección inicial: " + dir);
        Vector3 dirConArco = (dir.normalized + Vector3.up * 0.2f).normalized;
        Debug.Log("Dirección con arco: " + dirConArco);
        velocidadActual = dirConArco * velocidad;
        inicializado = true;

        Destroy(gameObject, tiempoVida);
    }

    private void Start()
    {
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!inicializado || rb == null)
        {
            return;
        }

        velocidadActual.y -= gravedad * Time.fixedDeltaTime;
        rb.linearVelocity = velocidadActual;

        if (velocidadActual != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocidadActual);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        SistemaVida vida = other.collider.GetComponentInParent<SistemaVida>();

        if (vida != null)
        {
            vida.RecibirDanio(danio);

            MovimientoAlastor movimientoJugador = vida.GetComponent<MovimientoAlastor>();

            if (movimientoJugador != null)
            {
                Vector3 direccion = vida.transform.position - transform.position;
                direccion.y = 0f;

                movimientoJugador.AplicarRetroceso(
                    direccion,
                    fuerzaRetroceso,
                    duracionRetroceso
                );
            }

            Destroy(gameObject);
            return;
        }

        if (!other.collider.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}