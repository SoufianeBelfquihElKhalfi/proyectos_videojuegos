using UnityEngine;

public class FantasmaCombate : MonoBehaviour
{
    [Header("Configuración")]
    public float duracionCombate = 7f;
    public float cooldown = 10f;
    public Transform puntoDisparo;
    public GameObject prefabFlecha;
    public float velocidadFlecha = 15f;
    public float intervaloDisparo = 1f;

    [Header("Estado (solo lectura)")]
    [SerializeField] private bool enModoCombate = false;
    [SerializeField] private float tiempoRestanteCombate = 0f;
    [SerializeField] private float tiempoRestanteCooldown = 0f;

    private float timerDisparo = 0f;
   
    void Update()
    {
        // Input del jugador
        if (Input.GetKeyDown(KeyCode.E) && !enModoCombate && tiempoRestanteCooldown <= 0f)
        {
            EntrarModoCombate();
        }

        // Tick modo combate
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

        // Tick cooldown
        if (tiempoRestanteCooldown > 0f)
        {
            tiempoRestanteCooldown -= Time.deltaTime;
        }
    }

    void EntrarModoCombate()
    {
        enModoCombate = true;
        tiempoRestanteCombate = duracionCombate;
        timerDisparo = 0f; // dispara inmediatamente al activar

        // Aquí puedes activar animaciones, VFX, etc.
        Debug.Log("Fantasma: MODO COMBATE activado");
    }

    void SalirModoCombate()
    {
        enModoCombate = false;
        tiempoRestanteCooldown = cooldown;

        Debug.Log("Fantasma: modo combate terminado. Cooldown iniciado.");
    }

    void DisparrarFlecha()
    {
        if (prefabFlecha == null || puntoDisparo == null) return;

        Transform objetivo = BuscarEnemigoMasCercano();
        Vector3 direccionInicial = objetivo != null
            ? (objetivo.position - puntoDisparo.position).normalized
            : puntoDisparo.forward;

        GameObject flecha = Instantiate(prefabFlecha, puntoDisparo.position, Quaternion.LookRotation(direccionInicial));

        // Pasarle el objetivo para que lo persiga
        FlechaFantasma scriptFlecha = flecha.GetComponent<FlechaFantasma>();
        if (scriptFlecha != null && objetivo != null)
        {
            scriptFlecha.objetivo = objetivo;
            scriptFlecha.velocidad = velocidadFlecha;
        }

        Destroy(flecha, 5f);
    }

    Transform BuscarEnemigoMasCercano()
    {
        // Ajusta el tag "Enemy" al que uses en tu proyecto
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        Transform masCercano = null;
        float distanciaMin = Mathf.Infinity;

        foreach (GameObject e in enemigos)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < distanciaMin)
            {
                distanciaMin = d;
                masCercano = e.transform;
            }
        }
        return masCercano;
    }

    // Propiedades públicas para la UI
    public bool EnModoCombate => enModoCombate;
    public float TiempoRestanteCombate => tiempoRestanteCombate;
    public float TiempoRestanteCooldown => tiempoRestanteCooldown;
    public float DuracionCombate => duracionCombate;
    public float Cooldown => cooldown;
}