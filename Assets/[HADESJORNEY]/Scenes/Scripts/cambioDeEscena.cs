using UnityEngine;

/**
 * SE HA CAMBIADO TODO EL SISTEMA DE CAMBIO DE ESCENA, AHORA SE UTILIZA UNA PANTALLA DE CARGA INTERMEDIA.
 * También se han cambiado los colliders de los portales para un mejor funcionamiento.
 * Al cambiar esta clase:
 * - el portal deja de usar colisión física normal,
 * - pasa a usar trigger,
 * - detecta al jugador a través de su componente MovimientoAlastor,
 * - carga la escena a través de SceneLoader.
 */
public class CambioDeEscena : MonoBehaviour
{
    // Nombre de la escena a cargar al entrar en el portal y mensaje de carga.
    [SerializeField] private string nombreEscena;
    [SerializeField] private string mensajeCarga = "Avanzando a la siguiente sala...";

    private bool isLoading = false;

    // Método que se llama al entrar en el trigger del portal.
    private void OnTriggerEnter(Collider other)
    {
        // Si ya se está cargando una escena, no hacer nada.
        if (isLoading)
        {
            return;
        }

        // Intentar obtener el componente MovimientoAlastor del objeto que entró en el trigger.
        MovimientoAlastor jugador = other.GetComponentInParent<MovimientoAlastor>();

        // Si no se encontró el componente, no hacer nada.
        if (jugador == null)
        {
            return;
        }

        // Si el nombre de la escena está vacío, mostrar un error y no hacer nada.
        if (string.IsNullOrWhiteSpace(nombreEscena))
        {
            Debug.LogError("CambioDeEscena: nombreEscena está vacío");
            return;
        }

        // Si todo está correcto, marcar que se está cargando una escena y llamar a SceneLoader para cargar la nueva escena.
        isLoading = true;
        SceneLoader.Load(nombreEscena, mensajeCarga);
    }
}