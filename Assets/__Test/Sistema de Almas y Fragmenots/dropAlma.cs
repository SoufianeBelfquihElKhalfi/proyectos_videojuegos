using UnityEngine;

public class DropAlma : MonoBehaviour
{
    public enum TipoDrop { Alma, FragmentoDeAlma }

    [Header("Configuración")]
    public TipoDrop tipo = TipoDrop.Alma;
    public int cantidad = 1;

    [Header("Movimiento")]
    [SerializeField] private float fuerzaSalida = 3f;
    [SerializeField] private float radioRecoleccion = 2f;
    [SerializeField] private float velocidadAtraccion = 8f;

    [Header("Flotación visual")]
    [SerializeField] private float alturaFlotacion = 0.5f;
    [SerializeField] private float floatAmplitud = 0.15f;
    [SerializeField] private float floatFrecuencia = 2f;

    private Rigidbody rb;
    private Transform jugador;
    private bool enSuelo = false;
    private Vector3 posBase;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Vector3 direccionAleatoria = new Vector3(
            Random.Range(-1f, 1f),
            0.5f,
            Random.Range(-1f, 1f)
        ).normalized;

        if (rb != null)
        {
            rb.AddForce(direccionAleatoria * fuerzaSalida, ForceMode.Impulse);
        }

        var jugadorObj = FindFirstObjectByType<MovimientoAlastor>();
        if (jugadorObj != null)
            jugador = jugadorObj.transform;

        Invoke(nameof(Aterrizar), 0.8f);
    }

    void Aterrizar()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        posBase = new Vector3(transform.position.x, alturaFlotacion, transform.position.z);
        transform.position = posBase;
        enSuelo = true;
    }


    void Update()
    {
        if (!enSuelo || jugador == null) return;

        float offsetY = Mathf.Sin(Time.time * floatFrecuencia) * floatAmplitud;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= radioRecoleccion)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                jugador.position + Vector3.up * 0.5f,
                velocidadAtraccion * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, jugador.position) < 0.3f)
            {
                Recoger();
            }
        }
        else
        {
            transform.position = posBase + new Vector3(0f, offsetY, 0f);
        }
    }

    void Recoger()
    {
        if (InventarioAlmas.Instancia == null) return;

        switch (tipo)
        {
            case TipoDrop.Alma:
                InventarioAlmas.Instancia.AgregarAlmas(cantidad);
                break;
            case TipoDrop.FragmentoDeAlma:
                InventarioAlmas.Instancia.AgregarFragmento(cantidad);
                break;
        }

        Destroy(gameObject);
    }
}
