using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform puntoRespawn;

    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado) return;
        if (!other.CompareTag("Player")) return;

        SistemaVida vida = other.GetComponentInParent<SistemaVida>();
        InventarioAlmas inventario = InventarioAlmas.Instancia;

        if (vida == null || inventario == null)
        {
            Debug.LogWarning("Checkpoint: faltan componentes en el jugador.");
            return;
        }

        if (CheckpointData.Instancia == null)
        {
            Debug.LogWarning("CheckpointData no existe. Ponlo en la primera escena del juego.");
            return;
        }

        activado = true;

        Vector3 posicionGuardada = puntoRespawn != null ? puntoRespawn.position : vida.transform.position;

        Debug.Log($"[Checkpoint] DISPARADO en escena {SceneManager.GetActiveScene().name} | almas a guardar: {inventario.Almas} | fragmentos: {inventario.Fragmentos}");

        CheckpointData.Instancia.Guardar(
            posicionGuardada,
            SceneManager.GetActiveScene().name,
            vida.VidaActual,
            vida.VidaMaxima,
            inventario.Almas,
            inventario.Fragmentos
        );
    }
}