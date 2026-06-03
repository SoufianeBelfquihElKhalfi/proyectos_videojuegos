using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CambioDeEscena : MonoBehaviour
{
    [SerializeField] private string nombreEscena;
    [SerializeField] private string mensajeCarga = "Avanzando a la siguiente sala...";

    [Header("Checkpoint")]
    [SerializeField] private bool guardarCheckpointAntesDeCambiar = false;
    [SerializeField] private string escenaCheckpoint = "EscenaMercader";
    [SerializeField] private Transform puntoRespawnCheckpoint;
    [SerializeField] private bool guardarVidaCompletaEnCheckpoint = true;

    private bool isLoading = false;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        ValidarConfiguracion();
    }

    private void ValidarConfiguracion()
    {
        if (!triggerCollider.isTrigger)
        {
            throw new InvalidOperationException($"{name}: el Collider de CambioDeEscena debe tener Is Trigger activado.");
        }

        if (string.IsNullOrWhiteSpace(nombreEscena))
        {
            throw new InvalidOperationException($"{name}: nombreEscena está vacío.");
        }

        if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            throw new InvalidOperationException($"{name}: la escena '{nombreEscena}' no está en Build Settings o el nombre no coincide.");
        }

        if (!guardarCheckpointAntesDeCambiar)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(escenaCheckpoint))
        {
            throw new InvalidOperationException($"{name}: escenaCheckpoint está vacío.");
        }

        if (!Application.CanStreamedLevelBeLoaded(escenaCheckpoint))
        {
            throw new InvalidOperationException($"{name}: la escena de checkpoint '{escenaCheckpoint}' no está en Build Settings o el nombre no coincide.");
        }

        if (puntoRespawnCheckpoint == null)
        {
            throw new MissingReferenceException($"{name}: falta asignar puntoRespawnCheckpoint.");
        }
    }

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

        isLoading = true;

        SistemaVida vida = jugador.GetComponent<SistemaVida>();

        if (vida == null)
        {
            throw new MissingComponentException($"{jugador.name}: necesita SistemaVida para cambiar de escena.");
        }

        vida.GuardarVida();

        if (guardarCheckpointAntesDeCambiar)
        {
            GuardarCheckpoint(vida);
        }

        Time.timeScale = 1f;
        SceneLoader.Load(nombreEscena, mensajeCarga);
    }

    private void GuardarCheckpoint(SistemaVida vida)
    {
        if (CheckpointData.Instancia == null)
        {
            throw new MissingReferenceException("No existe CheckpointData en la escena inicial.");
        }

        InventarioAlmas inventario = InventarioAlmas.Instancia;

        if (inventario == null)
        {
            throw new MissingReferenceException("No existe InventarioAlmas.");
        }

        int vidaGuardada = guardarVidaCompletaEnCheckpoint
            ? vida.VidaMaxima
            : vida.VidaActual;

        CheckpointData.Instancia.Guardar(
            puntoRespawnCheckpoint.position,
            escenaCheckpoint,
            vidaGuardada,
            vida.VidaMaxima,
            inventario.Almas,
            inventario.Fragmentos
        );
    }
}