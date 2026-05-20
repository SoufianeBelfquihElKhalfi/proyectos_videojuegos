using UnityEngine;
using UnityEngine.SceneManagement;

public class FantasmaCombate : MonoBehaviour
{
    [Header("Configuración")]
    public float duracionCombate = 7f;
    public float cooldown = 10f;
    public Transform puntoDisparo;
    public GameObject prefabFlecha;
    public float velocidadFlecha = 15f;
    public float intervaloDisparo = 1f;
    public float radioDeteccion = 10f;

    [Header("Visual Combate")]
    public Material materialHijoDelRayo;

    [Header("Estado (solo lectura)")]
    [SerializeField] private bool enModoCombate = false;
    [SerializeField] private float tiempoRestanteCombate = 0f;
    [SerializeField] private float tiempoRestanteCooldown = 0f;

    private float timerDisparo = 0f;
    private bool habilidadEHabilitada = true;
    private Renderer[] renderers;
    private Animator animator;

    // Guardamos los materiales originales de cada renderer
    private Material[][] materialesOriginales;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();

        // Guardamos una copia de los materiales originales antes de tocar nada
        materialesOriginales = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            materialesOriginales[i] = renderers[i].materials;
        }

        ComprobarSiLaHabilidadFantasmaSePuedeUsarEnLaEscena();

        if (animator != null)
        {
            animator.SetBool("transformado", false);
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("HijoDelRayo") && habilidadEHabilitada && !enModoCombate && tiempoRestanteCooldown <= 0f)
        {
            EntrarModoCombate();
        }

        if (enModoCombate)
        {
            tiempoRestanteCombate -= Time.deltaTime;
            timerDisparo -= Time.deltaTime;

            if (timerDisparo <= 0f)
            {
                DisparrarFlecha();
                timerDisparo = intervaloDisparo;
            }

            if (tiempoRestanteCombate <= 0f)
            {
                SalirModoCombate();
            }
        }

        if (tiempoRestanteCooldown > 0f)
        {
            tiempoRestanteCooldown -= Time.deltaTime;
        }
    }

    void ComprobarSiLaHabilidadFantasmaSePuedeUsarEnLaEscena()
    {
        string nombreEscena = SceneManager.GetActiveScene().name;
        habilidadEHabilitada = nombreEscena != "SalaMercader";
    }

    void EntrarModoCombate()
    {
        enModoCombate = true;
        tiempoRestanteCombate = duracionCombate;
        timerDisparo = 0f;

        if (animator != null)
        {
            animator.SetBool("transformado", true);
        }

        AplicarMaterialCombate();
    }

    void SalirModoCombate()
    {
        enModoCombate = false;
        tiempoRestanteCooldown = cooldown;

        if (animator != null)
        {
            animator.SetBool("transformado", false);
        }

        RestaurarMaterialesOriginales();
    }

    void AplicarMaterialCombate()
    {
        if (materialHijoDelRayo == null) return;

        foreach (Renderer rend in renderers)
        {
            // Creamos un array del mismo tamaño que los slots originales, todo con el material de combate
            Material[] nuevosMateriales = new Material[rend.materials.Length];
            for (int i = 0; i < nuevosMateriales.Length; i++)
            {
                nuevosMateriales[i] = materialHijoDelRayo;
            }
            rend.materials = nuevosMateriales;
        }
    }

    void RestaurarMaterialesOriginales()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].materials = materialesOriginales[i];
        }
    }

    void DisparrarFlecha()
    {
        if (prefabFlecha == null || puntoDisparo == null) return;

        Transform objetivo = BuscarEnemigoMasCercano();
        if (objetivo == null) return;

        Vector3 direccionInicial = (objetivo.position - puntoDisparo.position).normalized;

        GameObject flecha = Instantiate(
            prefabFlecha,
            puntoDisparo.position,
            Quaternion.LookRotation(direccionInicial)
        );

        FlechaFantasma scriptFlecha = flecha.GetComponent<FlechaFantasma>();

        if (scriptFlecha != null)
        {
            scriptFlecha.objetivo = objetivo;
            scriptFlecha.velocidad = velocidadFlecha;
        }

        Destroy(flecha, 5f);
    }

    Transform BuscarEnemigoMasCercano()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");

        Debug.Log("[FANTASMA] Enemigos encontrados con tag Enemy: " + enemigos.Length);

        Transform masCercano = null;
        float distanciaMin = Mathf.Infinity;

        foreach (GameObject e in enemigos)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);

            Debug.Log("[FANTASMA] Revisando enemigo: " + e.name + " | Distancia: " + d);

            if (d > radioDeteccion)
            {
                Debug.Log("[FANTASMA] Enemigo fuera del radio: " + e.name);
                continue;
            }

            if (d < distanciaMin)
            {
                distanciaMin = d;
                masCercano = e.transform;
                Debug.Log("[FANTASMA] Nuevo enemigo más cercano: " + e.name);
            }
        }

        if (masCercano == null)
        {
            Debug.LogWarning("[FANTASMA] No hay enemigos válidos dentro del radio.");
        }
        else
        {
            Debug.Log("[FANTASMA] Enemigo final seleccionado: " + masCercano.name);
        }

        return masCercano;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }

    public bool EnModoCombate => enModoCombate;
    public float TiempoRestanteCombate => tiempoRestanteCombate;
    public float TiempoRestanteCooldown => tiempoRestanteCooldown;
    public float DuracionCombate => duracionCombate;
    public float Cooldown => cooldown;
}