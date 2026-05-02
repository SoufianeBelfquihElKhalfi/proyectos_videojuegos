using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CombateJugador : MonoBehaviour
{
    [Header("Combo")]
    [SerializeField] private int maxGolpesCombo = 3;
    [SerializeField] private float tiempoEntreGolpes = 0.8f;
    [SerializeField] private float tiempoResetCombo = 1.2f;

    [Header("Daño por golpe (medio corazón = 1)")]
    [SerializeField] private int[] danioPorGolpe = { 1, 1, 2 };

    [Header("Retroceso por golpe")]
    [SerializeField] private float[] retrocesoPorGolpe = { 1.5f, 1.5f, 4f };
    [SerializeField] private float duracionRetroceso = 0.25f;

    [Header("Hitbox")]
    [SerializeField] private Transform puntoAtaque;
    [SerializeField] private float rangoAtaque = 1.5f;
    [SerializeField] private LayerMask capaEnemigos;

    [Header("Visual")]
    [SerializeField] private ArmaVisual armaVisual;

    private int golpeActual = 0;
    private float tiempoUltimoGolpe;
    private bool puedeAtacar = true;
    private bool combateHabilitado = true;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        ComprobarSiSePuedeAtacarEnLaEscena();
    }

    void Update()
    {
        if (Time.time - tiempoUltimoGolpe > tiempoResetCombo && golpeActual > 0)
        {
            golpeActual = 0;
        }

        if (Input.GetMouseButtonDown(0) && puedeAtacar && combateHabilitado)
        {
            Atacar();
        }
    }

    void ComprobarSiSePuedeAtacarEnLaEscena()
    {
        string nombreEscena = SceneManager.GetActiveScene().name;

        if (nombreEscena == "SalaMercader")
        {
            combateHabilitado = false;
        }
        else
        {
            combateHabilitado = true;
        }
    }

    void Atacar()
    {
        if (Time.time - tiempoUltimoGolpe < tiempoEntreGolpes && golpeActual > 0)
            return;

        puedeAtacar = false;
        tiempoUltimoGolpe = Time.time;

        if (armaVisual != null) armaVisual.Mostrar();

        if (animator != null)
        {
            animator.SetTrigger("Ataque");
            animator.SetInteger("GolpeCombo", golpeActual);
        }

        Collider[] enemigos = Physics.OverlapSphere(puntoAtaque.position, rangoAtaque, capaEnemigos);

        int danio = danioPorGolpe[golpeActual];

        if (EstadisticasJugador.Instancia != null)
        {
            danio = Mathf.CeilToInt(danio * EstadisticasJugador.Instancia.multiplicadorDanio);
        }
        float retroceso = retrocesoPorGolpe[golpeActual];

        foreach (Collider enemigo in enemigos)
        {
            SistemaVida vida = enemigo.GetComponentInParent<SistemaVida>();
            if (vida != null)
            {
                vida.RecibirDanio(danio);

                Transform enemigoRoot = vida.transform;
                Vector3 direccion = (enemigoRoot.position - transform.position).normalized;
                direccion.y = 0;

                StartCoroutine(RetrocesoSuave(enemigoRoot, direccion, retroceso));
            }
        }

        golpeActual++;
        if (golpeActual >= maxGolpesCombo)
        {
            golpeActual = 0;
        }

        Invoke(nameof(ResetAtaque), tiempoEntreGolpes);
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

    void ResetAtaque()
    {
        puedeAtacar = true; 
    }

    void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }
}