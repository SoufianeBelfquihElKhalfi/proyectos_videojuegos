using UnityEngine;
using TMPro;
using System.Collections;

public class runaMensaje : MonoBehaviour
{
    [Header("Mensaje")]
    [TextArea(2, 4)]
    [SerializeField] private string mensaje;

    [Header("Referencias UI")]
    [SerializeField] private GameObject textoFlotante;
    [SerializeField] private GameObject panelTexto;
    [SerializeField] private TextMeshProUGUI texto;

    [Header("Comportamiento")]
    [SerializeField] private float duracionEnPantalla = 5f;
    [SerializeField] private KeyCode tecla = KeyCode.E;

    private bool jugadorCerca = false;
    private Coroutine activa;

    void Start()
    {
        if (textoFlotante != null) textoFlotante.SetActive(false);
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(tecla))
        {
            Mostrar();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorCerca = true;
        if (textoFlotante != null) textoFlotante.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorCerca = false;
        if (textoFlotante != null) textoFlotante.SetActive(false);
    }

    private void Mostrar()
    {
        if (activa != null) StopCoroutine(activa);
        activa = StartCoroutine(Rutina());
    }

    private IEnumerator Rutina()
    {
        texto.text = mensaje;
        panelTexto.SetActive(true);

        // Forzar alpha a 1 por si quedó a 0 del fade anterior
        CanvasGroup grupo = panelTexto.GetComponent<CanvasGroup>();
        if (grupo != null) grupo.alpha = 1f;

        yield return new WaitForSeconds(duracionEnPantalla);
        panelTexto.SetActive(false);
        activa = null;
    }
}