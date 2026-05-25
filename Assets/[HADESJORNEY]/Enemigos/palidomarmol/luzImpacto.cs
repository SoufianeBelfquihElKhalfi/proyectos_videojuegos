using UnityEngine;
using System.Collections;
public class luzImpacto : MonoBehaviour
{
    [SerializeField] private Light luz;
    [SerializeField] private float duracion = 0.15f;
    [SerializeField] private float intensidadInicial = 10f;

    void Start()
    {
        if (luz != null) luz.intensity = intensidadInicial;
        StartCoroutine(Apagar());
    }

    IEnumerator Apagar()
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            luz.intensity = Mathf.Lerp(intensidadInicial, 0f, tiempo / duracion);
            yield return null;
        }
        Destroy(gameObject);
    }
}
