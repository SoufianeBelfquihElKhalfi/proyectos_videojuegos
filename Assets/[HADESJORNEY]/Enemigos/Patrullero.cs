using System.Collections;
using Enemy.FSM;
using UnityEngine;
using Dapasa.Audio;

public class Patrullero : MaquinaFSM
{
    [SerializeField] GameString patrulla;
    [SerializeField] GameString ataque;
    [SerializeField] private GameString deteccion;
    [SerializeField] private float rangoDeteccion = 10f;

    [Header("Aviso de detección")]
    [SerializeField] private AvisoDeteccionEnemigo avisoDeteccion;

    [Header("Música")]
    [SerializeField] private string idMusicaDeteccion = "musica_combate";

    Transform player;
    private Animator animator;
    private bool jugadorDetectado = false;
    private bool deteccionEnCurso = false;

    void Start()
    {
        MovimientoAlastor jugador = FindFirstObjectByType<MovimientoAlastor>();
        player = jugador.transform;

        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (deteccionEnCurso)
        {
            return;
        }

        float distanciaAlPlayer = Vector3.Distance(transform.position, player.position);

        if (distanciaAlPlayer < rangoDeteccion)
        {
            if (!jugadorDetectado)
            {
                StartCoroutine(SecuenciaDeteccion());
                return;
            }

            SetEstado(ataque.Value);
        }
        else
        {
            jugadorDetectado = false;
            SetEstado(patrulla.Value);
        }
    }

    private IEnumerator SecuenciaDeteccion()
    {
        deteccionEnCurso = true;
        jugadorDetectado = true;

        SetEstado(deteccion.Value);
        animator.SetTrigger("Deteccion");

        AudioManager.Instance.ReproducirMusica(idMusicaDeteccion);

        yield return avisoDeteccion.MostrarYEsperar();

        deteccionEnCurso = false;
        SetEstado(ataque.Value);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}