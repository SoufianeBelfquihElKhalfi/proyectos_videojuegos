using UnityEngine;

[RequireComponent(typeof(Light))]
public class luzDinamicaAntorcha : MonoBehaviour
{
    [Header("Intensidad")]
    [SerializeField] private float intensidadMin = 1.5f;
    [SerializeField] private float intensidadMax = 2.8f;

    [Header("Velocidad")]
    [Tooltip("Cuántos parpadeos por segundo aproximadamente")]
    [SerializeField] private float velocidad = 3f;

    private Light luz;
    private float offset;

    void Start()
    {
        luz = GetComponent<Light>();
        // Offset aleatorio para que cada antorcha parpadee distinto
        offset = Random.value * 100f;
    }

    void Update()
    {
        float t = Mathf.PerlinNoise(Time.time * velocidad + offset, 0f);
        luz.intensity = Mathf.Lerp(intensidadMin, intensidadMax, t);
    }
}