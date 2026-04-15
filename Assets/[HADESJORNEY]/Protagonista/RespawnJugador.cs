using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnJugador : MonoBehaviour
{
    [Header("Posición inicial si no hay checkpoint")]
    [SerializeField] private Transform posicionInicial;

    private void Start()
    {
        CheckpointData datos = CheckpointData.Instancia;

        if (datos == null || !datos.HayCheckpoint)
        {
            IrAPosicionInicial();
            return;
        }

        if (SceneManager.GetActiveScene().name != datos.EscenaCheckpoint)
        {
            IrAPosicionInicial();
            return;
        }

        ColocarJugador(datos.PosicionCheckpoint);
        RestaurarVida(datos);
        RestaurarInventario(datos);
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
        Rigidbody rb = GetComponent<Rigidbody>();

        if (cc != null) cc.enabled = false;

        transform.position = posicion;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cc != null) cc.enabled = true;
    }

    private void RestaurarVida(CheckpointData datos)
    {
        SistemaVida vida = GetComponent<SistemaVida>();
        if (vida == null) return;

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
        if (inventario == null) return;

        inventario.PerderTodasLasAlmas();
        inventario.AgregarAlmas(datos.AlmasGuardadas);

        int fragmentosActuales = inventario.Fragmentos;

        if (fragmentosActuales < datos.FragmentosGuardados)
        {
            inventario.AgregarFragmento(datos.FragmentosGuardados - fragmentosActuales);
        }
    }
}