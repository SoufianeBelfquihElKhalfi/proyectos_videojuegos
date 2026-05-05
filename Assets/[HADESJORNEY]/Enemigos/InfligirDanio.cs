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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaGolpe);
    }
}