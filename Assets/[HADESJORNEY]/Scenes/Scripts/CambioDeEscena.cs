using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CambioDeEscena : MonoBehaviour
{
    [SerializeField] private string nombreEscena;
    [SerializeField] private string mensajeCarga = "Avanzando a la siguiente sala...";

    private bool isLoading = false;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading)
        {
            return;
        }

        MovimientoAlastor jugador = other.GetComponentInParent<MovimientoAlastor>();

        if (jugador == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nombreEscena))
        {
            Debug.LogError("CambioDeEscena: nombreEscena está vacío.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            Debug.LogError($"CambioDeEscena: la escena '{nombreEscena}' no está en Build Settings o el nombre no coincide.", this);
            return;
        }

        SistemaVida vida = jugador.GetComponent<SistemaVida>();

        if (vida != null)
        {
            vida.GuardarVida();
        }

        isLoading = true;
        Time.timeScale = 1f;
        SceneLoader.Load(nombreEscena, mensajeCarga);
    }
}