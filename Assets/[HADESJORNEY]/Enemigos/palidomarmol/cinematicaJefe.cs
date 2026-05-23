using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public class cinematicaJefe : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject jefeEnEscena;
    [SerializeField] private Transform puntoSpawnJefe;
    [SerializeField] private Transform puntoAterrizaje;
    [SerializeField] private Camera camaraEntradaJefe;
    [SerializeField] private Camera camaraPrincipal;

    [Header("Partículas (opcional)")]
    [SerializeField] private GameObject particulasCaida;
    [SerializeField] private GameObject particulasImpacto;

    [Header("Tiempos")]
    [SerializeField] private float esperaAntesDeCaer = 1f;
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

        // 2. Cambiar cámara
        if (camaraPrincipal != null) camaraPrincipal.enabled = false;
        if (camaraEntradaJefe != null) camaraEntradaJefe.gameObject.SetActive(true);

        // 3. Activar jefe y posicionarlo en el aire
        jefeEnEscena.transform.position = puntoSpawnJefe.position;
        jefeEnEscena.SetActive(true);
        DesactivarComportamientoJefe(jefeEnEscena, true);

        // 4. Partículas caída
        GameObject particulas = null;
        if (particulasCaida != null)
        {
            particulas = Instantiate(particulasCaida, jefeEnEscena.transform.position, Quaternion.identity);
            particulas.transform.SetParent(jefeEnEscena.transform);
        }

        yield return new WaitForSeconds(esperaAntesDeCaer);

        // 5. Caer
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

        // 6. Impacto
        if (particulasImpacto != null)
            Instantiate(particulasImpacto, destino, Quaternion.identity);

        if (particulas != null) Destroy(particulas);

        yield return new WaitForSeconds(esperaTrasImpacto);

        // 7. Volver cámara principal
        if (camaraEntradaJefe != null) camaraEntradaJefe.gameObject.SetActive(false);
        if (camaraPrincipal != null) camaraPrincipal.enabled = true;

        // 8. Reactivar comportamiento del jefe y jugador
        DesactivarComportamientoJefe(jefeEnEscena, false);
        if (movimiento != null) movimiento.movimientoHabilitado = true;
        if (combate != null) combate.enabled = true;
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
