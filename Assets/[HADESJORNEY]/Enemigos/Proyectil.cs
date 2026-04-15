using UnityEngine;
using System.Collections;


/* CAMBIOS REALIZADOS:
 * - Quitar el uso de jugador.GetComponent<MonoBehaviour>().StartCoroutine(...).
 * - Lanzar las corrutinas desde el propio script Proyectil.
 * - No depender de other.collider.CompareTag("Player") para aplicar daño; comprobar mejor la jerarquía / SistemaVida.
 */
public class Proyectil : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float velocidad = 10f;
    [SerializeField] private int danio = 1;
    [SerializeField] private float tiempoVida = 5f;
    [SerializeField] private float gravedad = 9.8f;

    [Header("Retroceso")]
    [SerializeField] private float fuerzaRetroceso = 2f;
    [SerializeField] private float duracionRetroceso = 0.2f;

    [Header("Efecto visual")]
    [SerializeField] private float duracionParpadeo = 0.3f;
    [SerializeField] private int cantidadParpadeos = 3;

    private Vector3 velocidadActual;
    private bool inicializado = false;
    private Rigidbody rb;

    public void Inicializar(Vector3 dir)
    {
        Vector3 dirConArco = (dir.normalized + Vector3.up * 0.4f).normalized;
        velocidadActual = dirConArco * velocidad;
        inicializado = true;
        Destroy(gameObject, tiempoVida);
    }

    private void Start()
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
            Transform objetivo = vida.transform;

            vida.RecibirDanio(danio);

            Vector3 direccion = (objetivo.position - transform.position).normalized;
            direccion.y = 0f;

            StartCoroutine(RetrocesoSuave(objetivo, direccion, fuerzaRetroceso));
            StartCoroutine(ParpadeoGolpe(objetivo));

            Destroy(gameObject);
            return;
        }

        if (!other.collider.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator RetrocesoSuave(Transform objetivo, Vector3 direccion, float distancia)
    {
        if (objetivo == null)
        {
            yield break;
        }

        Vector3 inicio = objetivo.position;
        Vector3 destino = inicio + direccion * distancia;
        float tiempo = 0f;

        while (tiempo < duracionRetroceso)
        {
            if (objetivo == null)
            {
                yield break;
            }

            tiempo += Time.deltaTime;
            float t = tiempo / duracionRetroceso;
            float curva = 1f - Mathf.Pow(1f - t, 3f);
            objetivo.position = Vector3.Lerp(inicio, destino, curva);

            yield return null;
        }
    }

    private IEnumerator ParpadeoGolpe(Transform objetivo)
    {
        if (objetivo == null)
        {
            yield break;
        }

        Renderer[] renderers = objetivo.GetComponentsInChildren<Renderer>();
        Color colorGolpe = Color.red;
        Color colorOriginal = Color.white;

        float tiempoPorParpadeo = duracionParpadeo / cantidadParpadeos;

        for (int i = 0; i < cantidadParpadeos; i++)
        {
            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    r.material.color = colorGolpe;
                }
            }

            yield return new WaitForSeconds(tiempoPorParpadeo / 2f);

            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    r.material.color = colorOriginal;
                }
            }

            yield return new WaitForSeconds(tiempoPorParpadeo / 2f);
        }
    }
}