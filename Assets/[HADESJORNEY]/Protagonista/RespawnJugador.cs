using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnJugador : MonoBehaviour
{
    [Header("Posici�n inicial si no hay checkpoint")]
    [SerializeField] private Transform posicionInicial;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // En la primera escena tambien hay que correr la logica
        Respawnear();
    }

    private void OnSceneLoaded(Scene escena, LoadSceneMode modo)
    {
        Respawnear();
    }

    private void Respawnear()
    {
        Debug.Log($"[Respawn] Respawnear() en escena {SceneManager.GetActiveScene().name}");

        CheckpointData datos = CheckpointData.Instancia;

        if (datos == null || !datos.HayCheckpoint)
        {
            Debug.Log($"[Respawn] Sin checkpoint (datos null={datos == null}). Reseteando inventario.");
            IrAPosicionInicial();
            ResetearInventario();
            return;
        }

        Debug.Log($"[Respawn] Checkpoint encontrado en {datos.EscenaCheckpoint} con {datos.AlmasGuardadas} almas");

        if (SceneManager.GetActiveScene().name != datos.EscenaCheckpoint)
        {
            Debug.Log($"[Respawn] Escena actual ({SceneManager.GetActiveScene().name}) != checkpoint ({datos.EscenaCheckpoint}). Reseteando.");
            IrAPosicionInicial();
            ResetearInventario();
            return;
        }

        Debug.Log($"[Respawn] Restaurando desde checkpoint...");
        RestaurarVida(datos);
        RestaurarInventario(datos);
        ColocarJugador(datos.PosicionCheckpoint);
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

        if (cc != null) cc.enabled = false;

        transform.position = posicion;

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

    private void ResetearInventario()
    {
        InventarioAlmas inventario = InventarioAlmas.Instancia;
        if (inventario == null) return;

        inventario.PerderTodasLasAlmas();
        inventario.PerderTodosLosFragmentos();
    }

    private void RestaurarInventario(CheckpointData datos)
    {
        InventarioAlmas inventario = InventarioAlmas.Instancia;
        if (inventario == null)
        {
            Debug.LogWarning("RespawnJugador: InventarioAlmas.Instancia es null");
            return;
        }

        Debug.Log($"[Respawn] Antes de reset: almas={inventario.Almas}, fragmentos={inventario.Fragmentos}");
        Debug.Log($"[Respawn] Datos del checkpoint: almas={datos.AlmasGuardadas}, fragmentos={datos.FragmentosGuardados}");

        inventario.PerderTodasLasAlmas();
        inventario.PerderTodosLosFragmentos();

        Debug.Log($"[Respawn] Tras reset: almas={inventario.Almas}, fragmentos={inventario.Fragmentos}");

        inventario.AgregarAlmas(datos.AlmasGuardadas);
        inventario.AgregarFragmento(datos.FragmentosGuardados);

        Debug.Log($"[Respawn] Tras restaurar: almas={inventario.Almas}, fragmentos={inventario.Fragmentos}");
    }
}