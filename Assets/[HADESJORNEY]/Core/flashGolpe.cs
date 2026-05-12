using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class flashGolpe : MonoBehaviour
{
    public static flashGolpe Instancia;

    [SerializeField] private Image imagen;
    [SerializeField] private float duracion = 0.3f;
    [SerializeField] private float alphaMaximo = 0.4f;

    private void Awake()
    {
        Instancia = this;
    }

    public IEnumerator MostrarFlash()
    {
        float tiempo = 0f;
        Color color = imagen.color;

        // Fade in
        while (tiempo < duracion / 2f)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Lerp(0, alphaMaximo, tiempo / (duracion / 2f));
            imagen.color = color;
            yield return null;
        }

        // Fade out
        tiempo = 0f;
        while (tiempo < duracion / 2f)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Lerp(alphaMaximo, 0, tiempo / (duracion / 2f));
            imagen.color = color;
            yield return null;
        }

        color.a = 0;
        imagen.color = color;
    }
}
