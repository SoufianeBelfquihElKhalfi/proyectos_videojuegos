using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnJugador : MonoBehaviour
{
    [Header("Posicion inicial si no hay checkpoint en esta escena")]
    [SerializeField] private Transform posicionInicial;

    private void Start()
    {
        Respawnear();
    }

    private void Respawnear()
    {
        CheckpointData datos = CheckpointData.Instancia;

        if (!HayCheckpointValidoEnEstaEscena(datos))
        {
            IrAPosicionInicial();
            return;
        }

        RestaurarVida(datos);
        RestaurarInventario(datos);
        ColocarJugador(datos.PosicionCheckpoint);
    }

    private bool HayCheckpointValidoEnEstaEscena(CheckpointData datos)
    {
        if (datos == null) return false;
        if (!datos.HayCheckpoint) return false;
        if (string.IsNullOrEmpty(datos.EscenaCheckpoint)) return false;

        return SceneManager.GetActiveScene().name == datos.EscenaCheckpoint;
    }

    private void IrAPosicionInicial()
    {
        if (posicionInicial != null)
        {
            ColocarJugador(posicionInicial.position);
        }
    }

    private void ColocarJugador(Vector3 posicion)
    {
        CharacterController cc = GetComponent<CharacterController>();

        if (cc == null)
        {
            throw new MissingComponentException($"{name}: RespawnJugador necesita CharacterController para recolocar al jugador.");
        }

        cc.enabled = false;
        transform.position = posicion;
        cc.enabled = true;
    }

    private void RestaurarVida(CheckpointData datos)
    {
        SistemaVida vida = GetComponent<SistemaVida>();

        if (vida == null)
        {
            throw new MissingComponentException($"{name}: RespawnJugador necesita SistemaVida en el mismo GameObject.");
        }

        vida.EstablecerVidaDesdeCheckpoint(
            datos.VidaGuardada,
            datos.VidaMaximaGuardada
        );

        vida.GuardarVida();
    }

    private void RestaurarInventario(CheckpointData datos)
    {
        InventarioAlmas inventario = InventarioAlmas.Instancia;

        if (inventario == null)
        {
            throw new MissingReferenceException("No existe InventarioAlmas. Debe existir antes de restaurar un checkpoint.");
        }

        inventario.EstablecerInventario(
            datos.AlmasGuardadas,
            datos.FragmentosGuardados
        );
    }
}