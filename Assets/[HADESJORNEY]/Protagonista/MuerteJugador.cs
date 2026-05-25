using UnityEngine;

using UnityEngine.SceneManagement;



/* Se ha cambiado por completo este script.

 * ANTES: el jugador perd�a toda la vida, se llamaba a MuerteJugador.Morir() y se cargaba MAIN directamente.

 * AHORA:

 * - evita que Morir() se ejecute varias veces,

 * - desactiva scripts del jugador,

 * - pausa el juego con Time.timeScale = 0,

 * - muestra el panel de muerte si existe, si no existe, usa fallback al men�

 * - permite reintentar cargando la escena actual (cambiar para checkpoints)

 * - permite volver al men�

 * - usa SceneLoader.

 */

public class MuerteJugador : MonoBehaviour

{

    [Header("UI")]

    [SerializeField] private GameObject panelGameOver;



    [Header("Control")]

    [SerializeField] private MonoBehaviour[] componentesADesactivar;



    [Header("Escenas")]

    [SerializeField] private string escenaMenu = "MAIN";

    [SerializeField] private bool usarLoadingScreen = true;



    private bool haMuerto = false;



    public void Morir()

    {

        if (haMuerto)

        {

            return;

        }



        haMuerto = true;



        foreach (MonoBehaviour componente in componentesADesactivar)

        {

            if (componente != null)

            {

                componente.enabled = false;

            }

        }



        Time.timeScale = 0f;



        if (panelGameOver != null)

        {

            panelGameOver.SetActive(true);

            return;

        }



        Debug.LogWarning("MuerteJugador: no hay panel de Game Over asignado. Volviendo al men�.");

        VolverAlMenu();

    }



    public void Reintentar()
    {
        RestaurarEstado();
        RestaurarDesdeCheckpoint();

        string escenaDestino = "EscenaInicial";
        string mensajeCarga = "Reiniciando partida...";

        bool hayCheckpointValido =
            CheckpointData.Instancia != null &&
            CheckpointData.Instancia.HayCheckpoint &&
            !string.IsNullOrEmpty(CheckpointData.Instancia.EscenaCheckpoint);

        if (hayCheckpointValido)
        {
            escenaDestino = CheckpointData.Instancia.EscenaCheckpoint;
            mensajeCarga = "Volviendo al checkpoint...";
        }

        if (usarLoadingScreen)
        {
            SceneLoader.Load(escenaDestino, mensajeCarga);
        }
        else
        {
            SceneManager.LoadScene(escenaDestino);
        }
    }

    private void RestaurarDesdeCheckpoint()
    {
        InventarioAlmas inventario = InventarioAlmas.Instancia;
        if (inventario == null) return;

        inventario.PerderTodasLasAlmas();
        inventario.PerderTodosLosFragmentos();

        CheckpointData datos = CheckpointData.Instancia;
        if (datos != null && datos.HayCheckpoint)
        {
            inventario.AgregarAlmas(datos.AlmasGuardadas);
            inventario.AgregarFragmento(datos.FragmentosGuardados);

            SistemaVida vida = GetComponent<SistemaVida>();
            if (vida != null)
            {
                int corazones = datos.VidaMaximaGuardada / 2;
                vida.CambiarCorazonesMaximos(corazones, true);
            }
        }
    }


    public void VolverAlMenu()

    {

        RestaurarEstado();



        if (usarLoadingScreen)

        {

            SceneLoader.Load(escenaMenu, "Volviendo al men�...");

        }

        else

        {

            SceneManager.LoadScene(escenaMenu);

        }

    }



    private void RestaurarEstado()

    {
        Time.timeScale = 1f;

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        foreach (MonoBehaviour componente in componentesADesactivar)
        {
            if (componente != null)
            {
                componente.enabled = true;
            }
        }

        haMuerto = false;
    }
}