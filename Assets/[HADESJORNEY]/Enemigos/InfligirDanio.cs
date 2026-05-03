using UnityEngine;
using System.Collections;
public class InfligirDanio : MonoBehaviour
{
    [Tooltip("1 = medio coraz�n, 2 = un coraz�n")]
    [SerializeField] private int danioMitadCorazones = 1;

    [Header("Golpe")]
    [SerializeField] private float distanciaGolpe = 2f;

    [Header("Retroceso")]
    [SerializeField] private float fuerzaRetroceso = 2f;
    [SerializeField] private float duracionRetroceso = 0.15f;

    [Header("Efecto visual")]
    [SerializeField] private float duracionParpadeo = 0.3f;
    [SerializeField] private int cantidadParpadeos = 3;

    public bool IntentarGolpear(Transform jugador)
    {
        if (jugador == null)
        {
            return false;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);
        if (distancia > distanciaGolpe)
        {
            return false;
        }

        SistemaVida vida = jugador.GetComponentInParent<SistemaVida>();
        if (vida == null)
        {
            return false;
        }

        vida.RecibirDanio(danioMitadCorazones);

        Vector3 direccion = (jugador.position - transform.position).normalized;
        direccion.y = 0f;

        StartCoroutine(RetrocesoSuave(jugador, direccion, fuerzaRetroceso));
        StartCoroutine(ParpadeoGolpe(jugador));

        return true;
    }

    private IEnumerator RetrocesoSuave(Transform objetivo, Vector3 direccion, float distancia)
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

    private IEnumerator ParpadeoGolpe(Transform objetivo)
    {
        if (objetivo == null) yield break;

        Renderer[] renderers = objetivo.GetComponentsInChildren<Renderer>();
        Color colorGolpe = Color.red;

        Color[] coloresOriginales = new Color[renderers.Length];
        for (int j = 0; j < renderers.Length; j++)
        {
            if (renderers[j].material.HasProperty("_Color"))
                coloresOriginales[j] = renderers[j].material.color;
        }

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

            yield return new WaitForSeconds(tiempoPorParpadeo * 0.5f);

            for (int j = 0; j < renderers.Length; j++)
            {
                if (renderers[j].material.HasProperty("_Color"))
                {
                    renderers[j].material.color = coloresOriginales[j];
                }
            }

            yield return new WaitForSeconds(tiempoPorParpadeo * 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaGolpe);
    }
}