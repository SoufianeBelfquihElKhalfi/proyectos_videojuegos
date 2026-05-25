using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;


public class cinematicaJefe : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject jefeEnEscena;
    [SerializeField] private Transform puntoSpawnJefe;
    [SerializeField] private Transform puntoAterrizaje;
    [SerializeField] private Camera camaraEntradaJefe;
    [SerializeField] private Camera camaraPrincipal;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeNegro;
    [SerializeField] private float duracionFade = 0.4f;

    [Header("Partículas (opcional)")]
    [SerializeField] private GameObject particulasCaida;
    [SerializeField] private GameObject particulasImpacto;

    [Header("Shake del impacto")]
    [SerializeField] private float duracionShake = 0.3f;
    [SerializeField] private float fuerzaShake = 0.2f;

    [Header("Tiempos")]
    [SerializeField] private float esperaAntesDeCaer = 0f;
    [SerializeField] private float duracionCaida = 1.5f;
    [SerializeField] private float esperaTrasImpacto = 1f;

    private bool activada = false;

    void OnTriggerEnter(Collider other)
    {
        if (activada) return;
        if (!other.CompareTag("Player")) return;
        activada = true;
        StartCoroutine(SecuenciaEntrada(other.gameObject));
    }

    private IEnumerator SecuenciaEntrada(GameObject jugador)
    {
        // 1. Bloquear jugador
        var movimiento = jugador.GetComponent<MovimientoAlastor>();
        if (movimiento != null) movimiento.movimientoHabilitado = false;

        var combate = jugador.GetComponent<CombateJugador>();
        if (combate != null) combate.enabled = false;

        // 2. Fade out (a negro)
        yield return Fade(0f, 1f);

        // 3. Cambiar cámara mientras está negro
        if (camaraPrincipal != null) camaraPrincipal.enabled = false;
        if (camaraEntradaJefe != null) camaraEntradaJefe.gameObject.SetActive(true);

        // 4. Posicionar jefe
        jefeEnEscena.transform.position = puntoSpawnJefe.position;
        jefeEnEscena.SetActive(true);
        DesactivarComportamientoJefe(jefeEnEscena, true);

        // 5. Fade in (de negro a normal)
        yield return Fade(1f, 0f);

        // 6. Partículas de caída
        GameObject particulas = null;
        if (particulasCaida != null)
        {
            particulas = Instantiate(particulasCaida, jefeEnEscena.transform.position, Quaternion.identity);
            particulas.transform.SetParent(jefeEnEscena.transform);
        }

        yield return new WaitForSeconds(esperaAntesDeCaer);

        // 7. Caer
        Vector3 inicio = jefeEnEscena.transform.position;
        Vector3 destino = puntoAterrizaje.position;
        float tiempo = 0f;

        while (tiempo < duracionCaida)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionCaida;
            t = t * t;
            jefeEnEscena.transform.position = Vector3.Lerp(inicio, destino, t);
            yield return null;
        }

        jefeEnEscena.transform.position = destino;

        // 8. Impacto: partículas + shake
        if (particulasImpacto != null)
            Instantiate(particulasImpacto, destino, Quaternion.identity);

        if (particulas != null) Destroy(particulas);

        if (shakeCamara.Instancia != null)
        {
            Debug.Log("Llamando al shake");
            shakeCamara.Instancia.Shake(duracionShake, fuerzaShake);
        }
        else
        {
            Debug.Log("ShakeCamara.Instancia es null");
        }

        yield return new WaitForSeconds(esperaTrasImpacto);

        // 9. Fade out de nuevo para volver al juego
        yield return Fade(0f, 1f);

        if (camaraEntradaJefe != null) camaraEntradaJefe.gameObject.SetActive(false);
        if (camaraPrincipal != null) camaraPrincipal.enabled = true;

        yield return Fade(1f, 0f);

        // 10. Reactivar
        DesactivarComportamientoJefe(jefeEnEscena, false);
        if (movimiento != null) movimiento.movimientoHabilitado = true;
        if (combate != null) combate.enabled = true;
    }

    private IEnumerator Fade(float desde, float hasta)
    {
        if (fadeNegro == null) yield break;

        float tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            fadeNegro.alpha = Mathf.Lerp(desde, hasta, tiempo / duracionFade);
            yield return null;
        }
        fadeNegro.alpha = hasta;
    }

    private void DesactivarComportamientoJefe(GameObject jefe, bool desactivar)
    {
        var nav = jefe.GetComponent<NavMeshAgent>();
        if (nav != null) nav.enabled = !desactivar;

        var patrulla = jefe.GetComponent<Patrulla>();
        if (patrulla != null) patrulla.enabled = !desactivar;

        var ataque = jefe.GetComponent<AtaqueJefe>();
        if (ataque != null) ataque.enabled = !desactivar;
    }
}
