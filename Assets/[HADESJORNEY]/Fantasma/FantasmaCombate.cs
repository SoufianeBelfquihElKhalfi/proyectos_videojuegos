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
    public float radioDeteccion = 10f;

    [Header("Estado (solo lectura)")]
    [SerializeField] private bool enModoCombate = false;
    [SerializeField] private float tiempoRestanteCombate = 0f;
    [SerializeField] private float tiempoRestanteCooldown = 0f;

    private float timerDisparo = 0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !enModoCombate && tiempoRestanteCooldown <= 0f)
            EntrarModoCombate();

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
                SalirModoCombate();
        }

        if (tiempoRestanteCooldown > 0f)
            tiempoRestanteCooldown -= Time.deltaTime;
    }

    void EntrarModoCombate()
    {
        enModoCombate = true;
        tiempoRestanteCombate = duracionCombate;
        timerDisparo = 0f;
    }

    void SalirModoCombate()
    {
        enModoCombate = false;
        tiempoRestanteCooldown = cooldown;
    }

    void DisparrarFlecha()
    {
        if (prefabFlecha == null || puntoDisparo == null) return;

        Transform objetivo = BuscarEnemigoMasCercano();
        if (objetivo == null) return; // no hay enemigos en el radio, no dispara

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
        Transform masCercano = null;
        float distanciaMin = Mathf.Infinity;

        foreach (GameObject e in enemigos)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d > radioDeteccion) continue;

            if (d < distanciaMin)
            {
                distanciaMin = d;
                masCercano = e.transform;
            }
        }

        return masCercano;
    }

    // Visualiza el radio en la Scene
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