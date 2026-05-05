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

        if (cc != null)
        {
            cc.enabled = false;
        }

        transform.position = posicion;

        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    private void RestaurarVida(CheckpointData datos)
    {
        SistemaVida vida = GetComponent<SistemaVida>();

        if (vida == null)
        {
            return;
        }

        int corazones = datos.VidaMaximaGuardada / 2;
        vida.CambiarCorazonesMaximos(corazones, false);

        int diferencia = datos.VidaMaximaGuardada - datos.VidaGuardada;

        if (diferencia > 0)
        {
            vida.RecibirDanio(diferencia);
        }
    }

    private void RestaurarInventario(CheckpointData datos)
    {
        InventarioAlmas inventario = InventarioAlmas.Instancia;

        if (inventario == null)
        {
            Debug.LogWarning("RespawnJugador: InventarioAlmas.Instancia es null.");
            return;
        }

        inventario.EstablecerInventario(
            datos.AlmasGuardadas,
            datos.FragmentosGuardados
        );
    }
}