using UnityEngine;

public class CambioDeEscena : MonoBehaviour
{
    [SerializeField] private string nombreEscena;
    [SerializeField] private string mensajeCarga = "Avanzando a la siguiente sala...";

    private bool isLoading = false;

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
            Debug.LogError("CambioDeEscena: nombreEscena está vacío.");
            return;
        }

        SistemaVida vida = jugador.GetComponent<SistemaVida>();

        if (vida != null)
        {
            vida.GuardarVida();
        }

        isLoading = true;
        SceneLoader.Load(nombreEscena, mensajeCarga);
    }
}