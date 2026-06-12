using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using Dapasa.Audio;


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

    [Header("Part�culas (opcional)")]
    [SerializeField] private GameObject particulasCaida;
    [SerializeField] private GameObject particulasImpacto;

    [Header("Shake del impacto")]
    [SerializeField] private float duracionShake = 0.3f;
    [SerializeField] private float fuerzaShake = 0.2f;

    [Header("Audio del impacto")]
    [SerializeField] private string idSonidoCaida = "caida";
    [SerializeField] private string idSonidoGrunido = "caida_grunido";

    private const float ANTICIPO_CAIDA   = 0.25f;
    private const float RETRASO_GRUNIDO  = 1f;

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
        //  Bloquear jugador
        var movimiento = jugador.GetComponent<MovimientoAlastor>();
        if (movimiento != null) movimiento.movimientoHabilitado = false;

        var combate = jugador.GetComponent<CombateJugador>();
        if (combate != null) combate.enabled = false;

        //  Fade out 
        yield return Fade(0f, 1f);

        //  Cambiar camara mientras esta negro
        if (camaraPrincipal != null) camaraPrincipal.enabled = false;
        if (camaraEntradaJefe != null) camaraEntradaJefe.gameObject.SetActive(true);

        //  Posicionar jefe 
        DesactivarComportamientoJefe(jefeEnEscena, true);
        jefeEnEscena.SetActive(true);
        jefeEnEscena.transform.position = puntoSpawnJefe.position;

        
        Animator animJefe = jefeEnEscena.GetComponentInChildren<Animator>();
        if (animJefe != null) animJefe.enabled = true;

        //  Fade in 
        yield return Fade(1f, 0f);

        //  Particulas de caida
        GameObject particulas = null;
        if (particulasCaida != null)
        {
            particulas = Instantiate(particulasCaida, jefeEnEscena.transform.position, Quaternion.identity);
            particulas.transform.SetParent(jefeEnEscena.transform);
        }

        yield return new WaitForSeconds(esperaAntesDeCaer);

        if (animJefe != null)
        {
            animJefe.speed = 1f;
            float caerLength = ObtenerDuracionClip(animJefe, "caer");
            if (caerLength > 0.01f && duracionCaida > 0.05f)
                animJefe.speed = caerLength / duracionCaida;
            int hashCaer = Animator.StringToHash("Caer");
            animJefe.ResetTrigger("caer");
            animJefe.Play(hashCaer, 0, 0f);
            animJefe.SetTrigger("caer");
            animJefe.Update(0f);
        }

        //  Caer
        Vector3 inicio = jefeEnEscena.transform.position;
        Vector3 destino = puntoAterrizaje.position;
        float tiempo = 0f;
        bool caidaSonada = false;

        while (tiempo < duracionCaida)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionCaida;
            t = t * t;
            jefeEnEscena.transform.position = Vector3.Lerp(inicio, destino, t);

            if (!caidaSonada && tiempo >= duracionCaida - ANTICIPO_CAIDA)
            {
                caidaSonada = true;
                if (AudioManager.Instance != null && !string.IsNullOrEmpty(idSonidoCaida))
                    AudioManager.Instance.ReproducirSFX2D(idSonidoCaida);
            }

            yield return null;
        }

        jefeEnEscena.transform.position = destino;

        if (!caidaSonada && AudioManager.Instance != null && !string.IsNullOrEmpty(idSonidoCaida))
            AudioManager.Instance.ReproducirSFX2D(idSonidoCaida);

        if (animJefe != null) animJefe.speed = 1f;

        StartCoroutine(ReproducirGrunidoConRetraso());

        if (particulasImpacto != null)
            Instantiate(particulasImpacto, destino, Quaternion.identity);

        if (particulas != null) Destroy(particulas);

   
        yield return new WaitForSeconds(esperaTrasImpacto);

        //  Fade out para volver al juego
        yield return Fade(0f, 1f);

        if (camaraEntradaJefe != null) camaraEntradaJefe.gameObject.SetActive(false);
        if (camaraPrincipal != null) camaraPrincipal.enabled = true;

        yield return Fade(1f, 0f);

        
        var ataque = jefeEnEscena.GetComponent<AtaqueJefe>();
        if (ataque != null) ataque.MarcarIntroCompletada();

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

    private IEnumerator ReproducirGrunidoConRetraso()
    {
        if (RETRASO_GRUNIDO > 0f)
            yield return new WaitForSeconds(RETRASO_GRUNIDO);

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(idSonidoGrunido))
            AudioManager.Instance.ReproducirSFX2D(idSonidoGrunido);
    }

    private float ObtenerDuracionClip(Animator anim, string nombre)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return 0f;
        var clips = anim.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name == nombre) return clips[i].length;
        }
        return 0f;
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
