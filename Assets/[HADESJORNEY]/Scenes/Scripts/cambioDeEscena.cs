using UnityEngine;

// Controlador para el cambio de escena al entrar en un trigger, verifica que el objeto que entra es el jugador y luego inicia la carga de la nueva escena con un mensaje personalizado
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

        isLoading = true;
        SceneLoader.Load(nombreEscena, mensajeCarga);
    }
}