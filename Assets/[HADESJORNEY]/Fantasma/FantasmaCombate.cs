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
    public Color colorNormal = Color.white;
    public Color colorCombate = Color.yellow;

    [Header("Estado (solo lectura)")]
    [SerializeField] private bool enModoCombate = false;
    [SerializeField] private float tiempoRestanteCombate = 0f;
    [SerializeField] private float tiempoRestanteCooldown = 0f;

    private float timerDisparo = 0f;
    private bool habilidadEHabilitada = true;
    private Renderer[] renderers;

    void Start()
    {
        // Obtener todos los Renderers del fantasma y sus hijos
        renderers = GetComponentsInChildren<Renderer>();
        ComprobarSiLaHabilidadFantasmaSePuedeUsarEnLaEscena();
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
        AplicarColor(colorCombate);
    }

    void SalirModoCombate()
    {
        enModoCombate = false;
        tiempoRestanteCooldown = cooldown;
        AplicarColor(colorNormal);
    }

    void AplicarColor(Color color)
    {
        foreach (Renderer rend in renderers)
        {
            // Compatibe con materiales que usen _Color o _BaseColor (URP/HDRP)
            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", color);
            }
        }
    }

    void DisparrarFlecha()
    {
        if (prefabFlecha == null || puntoDisparo == null) return;

        Transform objetivo = BuscarEnemigoMasCercano();
        if (objetivo == null) return;

        Vector3 direccionInicial = (objetivo.position - puntoDisparo.position).normalized;
        GameObject flecha = Instantiate(prefabFlecha, puntoDisparo.position, Quaternion.LookRotation(direccionInicial));

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