using UnityEngine;
using System.Collections;

public class InfligirDanio : MonoBehaviour
{
    [Tooltip("1 = medio corazón, 2 = un corazón")]
    public int danioMitadCorazones = 1;

    [Header("Configuración")]
    [SerializeField] private float distanciaGolpe = 2f;
    [SerializeField] private float cooldown = 1.5f;

    [Header("Retroceso")]
    [SerializeField] private float fuerzaRetroceso = 3f;
    [SerializeField] private float duracionRetroceso = 0.2f;

    [Header("Efecto visual")]
    [SerializeField] private float duracionParpadeo = 0.3f;
    [SerializeField] private int cantidadParpadeos = 3;

    private float ultimoGolpe = -999f;
    private Transform jugador;

    void Start()
    {
        var jugadorObj = FindFirstObjectByType<MovimientoAlastor>();
        if (jugadorObj != null)
            jugador = jugadorObj.transform;
    }

    void Update()
    {
        if (jugador == null) return;
        if (Time.time - ultimoGolpe < cooldown) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaGolpe)
        {
            SistemaVida vida = jugador.GetComponent<SistemaVida>();
            if (vida != null)
            {
                vida.RecibirDanio(danioMitadCorazones);
                ultimoGolpe = Time.time;

                // Retroceso suave
                Vector3 direccion = (jugador.position - transform.position).normalized;
                direccion.y = 0;
                StartCoroutine(RetrocesoSuave(jugador, direccion, fuerzaRetroceso));

                // Parpadeo rojo
                StartCoroutine(ParpadeoGolpe(jugador));
            }
        }
    }

    IEnumerator RetrocesoSuave(Transform objetivo, Vector3 direccion, float distancia)
    {
        if (objetivo == null) yield break;

        Vector3 inicio = objetivo.position;
        Vector3 destino = inicio + direccion * distancia;
        float tiempo = 0f;

        while (tiempo < duracionRetroceso)
        {
            if (objetivo == null) yield break;
            tiempo += Time.deltaTime;
            float t = tiempo / duracionRetroceso;
            float curva = 1f - Mathf.Pow(1f - t, 3f);
            objetivo.position = Vector3.Lerp(inicio, destino, curva);
            yield return null;
        }
    }

    IEnumerator ParpadeoGolpe(Transform objetivo)
    {
        if (objetivo == null) yield break;

        Renderer[] renderers = objetivo.GetComponentsInChildren<Renderer>();
        Color colorOriginal = Color.white;
        Color colorGolpe = Color.red;

        float tiempoPorParpadeo = duracionParpadeo / cantidadParpadeos;

        for (int i = 0; i < cantidadParpadeos; i++)
        {
            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                    r.material.color = colorGolpe;
            }
            yield return new WaitForSeconds(tiempoPorParpadeo / 2f);

            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                    r.material.color = colorOriginal;
            }
            yield return new WaitForSeconds(tiempoPorParpadeo / 2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaGolpe);
    }
}