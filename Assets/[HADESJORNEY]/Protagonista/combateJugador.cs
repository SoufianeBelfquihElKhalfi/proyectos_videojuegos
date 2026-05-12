using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CombateJugador : MonoBehaviour
{
    [Header("Combo")]
    [SerializeField] private int maxGolpesCombo = 3;
    [SerializeField] private float tiempoEntreGolpes = 2f;
    [SerializeField] private float tiempoResetCombo = 3f;

    [Header("Daño por golpe (medio corazón = 1)")]
    [SerializeField] private int[] danioPorGolpe = { 1, 1, 2 };

    [Header("Retroceso por golpe")]
    [SerializeField] private float[] retrocesoPorGolpe = { 1.5f, 1.5f, 4f };
    [SerializeField] private float duracionRetroceso = 0.25f;

    [Header("Hitbox")]
    [SerializeField] private Transform puntoAtaque;
    [SerializeField] private float rangoAtaque = 1.5f;
    [SerializeField] private LayerMask capaEnemigos;

    [Header("Efectos")]
    [SerializeField] private GameObject efectoGolpe;
    [SerializeField] private GameObject efectoDestello;
    [SerializeField] private GameObject objetoEstela;
    private TrailRenderer estela;


    [Header("Visual")]
    [SerializeField] private ArmaVisual armaVisual;


    private int golpeActual = 0;
    private float tiempoUltimoGolpe;
    private bool puedeAtacar = true;
    private bool combateHabilitado = true;
    private bool inputGuardado = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        ComprobarSiSePuedeAtacarEnLaEscena();
        if (animator == null)
            Debug.Log("Animator es null");
        else
            Debug.Log("Animator encontrado");

        if (objetoEstela != null)
        {
            estela = objetoEstela.GetComponent<TrailRenderer>();
            objetoEstela.SetActive(false);
        }
    }

    void Update()
    {
        if (Time.time - tiempoUltimoGolpe > tiempoResetCombo && golpeActual > 0)
        {
            golpeActual = 0;
        }

        if (Input.GetMouseButtonDown(0) && combateHabilitado)
        {
            if (puedeAtacar)
                Atacar();
            else
                inputGuardado = true;
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
        if (!puedeAtacar) return;
        if (Time.time - tiempoUltimoGolpe < tiempoEntreGolpes && golpeActual > 0)
            return;
        if (estela != null)
        {
            objetoEstela.SetActive(true);
            estela.Clear();
        }
        puedeAtacar = false;
        tiempoUltimoGolpe = Time.time;

        if (armaVisual != null) armaVisual.Mostrar();

        if (animator != null)
        {
            Debug.Log("Golpe: " + golpeActual);
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


        bool maniquiGolpeado = false;

        foreach (Collider enemigo in enemigos)
        {
            SistemaVida vida = enemigo.GetComponentInParent<SistemaVida>();
            if (vida != null)
            {
                vida.RecibirDanio(danio);
                if (efectoGolpe != null)
                {
                    GameObject efecto = Instantiate(efectoGolpe, enemigo.transform.position + Vector3.up, Quaternion.identity);
                    Destroy(efecto, 0.5f);
                }
                if (efectoDestello != null)
                {
                    GameObject destello = Instantiate(efectoDestello, enemigo.transform.position + Vector3.up, Quaternion.identity);
                    Destroy(destello, 0.3f);
                }

                if (enemigo.CompareTag("maniqui"))
                {
                    if (!maniquiGolpeado)
                    {
                        Maniqui maniqui = enemigo.GetComponentInParent<Maniqui>();
                        if (maniqui != null)
                        {
                            maniqui.RecibirDanio();
                            maniquiGolpeado = true;
                        }
                    }
                    continue;
                }

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

        Invoke(nameof(ResetAtaque), 1f);
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

        if (inputGuardado)
        {
            inputGuardado = false;
            Atacar();
        }
        else
        {
            if (estela != null)
                objetoEstela.SetActive(false);
        }

    }

    void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }
}