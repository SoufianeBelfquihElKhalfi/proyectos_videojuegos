using UnityEngine;
using System.Collections;

public class ArmaVisual : MonoBehaviour
{
    [SerializeField] private GameObject arma;
    [SerializeField] private float tiempoMostrar = 0.2f;
    [SerializeField] private float anguloSwing = 120f;

    private Vector3 rotacionInicial;

    void Start()
    {
        if (arma != null)
        {
            rotacionInicial = arma.transform.localEulerAngles;
            arma.SetActive(false);
        }
    }

    public void Mostrar()
    {
        if (arma == null) return;
        arma.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        float tiempo = 0f;

        // Empieza arriba a la derecha (posición de preparación)
        arma.transform.localEulerAngles = rotacionInicial + new Vector3(-30, anguloSwing / 2f, 45);

        // Corte diagonal: de arriba-derecha a abajo-izquierda
        while (tiempo < tiempoMostrar)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoMostrar;
            float curva = 1f - Mathf.Pow(1f - t, 3f);

            float anguloY = Mathf.Lerp(anguloSwing / 2f, -anguloSwing / 2f, curva);
            float anguloX = Mathf.Lerp(-30f, 30f, curva);
            float anguloZ = Mathf.Lerp(45f, -45f, curva);

            arma.transform.localEulerAngles = rotacionInicial + new Vector3(anguloX, anguloY, anguloZ);
            yield return null;
        }

        arma.SetActive(false);
        arma.transform.localEulerAngles = rotacionInicial;
    }
}