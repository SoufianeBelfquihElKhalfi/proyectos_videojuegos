using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfazCorazones : MonoBehaviour
{
    [Header("Referencia a la vida")]
    public SistemaVida sistemaVida;

    [Header("UI")]
    public GameObject prefabCorazon;
    public Transform contenedorCorazones;

    [Header("Sprites")]
    public Sprite corazonLleno;
    public Sprite medioCorazon;
    public Sprite corazonVacio;

    private List<Image> imagenesCorazones = new List<Image>();

    private void Start()
    {
        if (sistemaVida == null) return;

        CrearCorazones();
        ActualizarCorazones(sistemaVida.VidaActual, sistemaVida.VidaMaxima);
        sistemaVida.alCambiarVida.AddListener(ActualizarCorazones);
    }

    private void OnDestroy()
    {
        if (sistemaVida != null)
            sistemaVida.alCambiarVida.RemoveListener(ActualizarCorazones);
    }

    private void CrearCorazones()
    {
        foreach (Transform hijo in contenedorCorazones)
        {
            Destroy(hijo.gameObject);
        }

        imagenesCorazones.Clear();

        int totalCorazones = sistemaVida.VidaMaxima / 2;

        for (int i = 0; i < totalCorazones; i++)
        {
            GameObject obj = Instantiate(prefabCorazon, contenedorCorazones);
            Image img = obj.GetComponent<Image>();
            imagenesCorazones.Add(img);
        }
    }

    public void ActualizarCorazones(int vidaActual, int vidaMaxima)
    {
        int totalCorazones = vidaMaxima / 2;

        if (imagenesCorazones.Count != totalCorazones)
        {
            CrearCorazones();
        }

        for (int i = 0; i < totalCorazones; i++)
        {
            int vidaPorCorazon = 2;
            int vidaRestante = vidaActual - (i * 2);

            if (vidaRestante >= vidaPorCorazon)
            {
                imagenesCorazones[i].sprite = corazonLleno;
            }
            else if (vidaRestante == 1)
            {
                imagenesCorazones[i].sprite = medioCorazon;
            }
            else
            {
                imagenesCorazones[i].sprite = corazonVacio;
            }
        }
    }
}