using UnityEngine;

// Controla la lógica de la sala de combate, abriendo la salida al derrotar a todos los enemigos.
public class SalaCombateController : MonoBehaviour
{
    [Header("Enemigos de la sala")]
    [SerializeField] private SistemaVida[] enemigosSala;

    [Header("Salida")]
    [SerializeField] private GameObject puertaVisual;
    [SerializeField] private Collider bloqueadorSalida;
    [SerializeField] private GameObject teleportSalida;

    private int enemigosRestantes;
    private bool salidaAbierta = false;

    private void Start()
    {
        enemigosRestantes = 0;

        foreach (SistemaVida enemigo in enemigosSala)
        {
            if (enemigo == null)
            {
                continue;
            }

            if (enemigo.EstaMuerto)
            {
                continue;
            }

            enemigosRestantes++;
        }

        CerrarSalida();
    }

    public void EnemigoDerrotado()
    {
        if (salidaAbierta)
        {
            return;
        }

        enemigosRestantes = Mathf.Max(0, enemigosRestantes - 1);

        if (enemigosRestantes == 0)
        {
            AbrirSalida();
        }
    }

    private void CerrarSalida()
    {
        if (puertaVisual != null)
        {
            puertaVisual.SetActive(true);
        }

        if (bloqueadorSalida != null)
        {
            bloqueadorSalida.enabled = true;
        }

        if (teleportSalida != null)
        {
            teleportSalida.SetActive(false);
        }
    }

    private void AbrirSalida()
    {
        salidaAbierta = true;

        if (puertaVisual != null)
        {
            puertaVisual.SetActive(false);
        }

        if (bloqueadorSalida != null)
        {
            bloqueadorSalida.enabled = false;
        }

        if (teleportSalida != null)
        {
            teleportSalida.SetActive(true);
        }
    }
}