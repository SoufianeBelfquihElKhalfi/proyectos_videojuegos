using UnityEngine;

public class CheckpointData : MonoBehaviour
{
    public static CheckpointData Instancia { get; private set; }

    public Vector3 PosicionCheckpoint { get; private set; }
    public string EscenaCheckpoint { get; private set; }
    public bool HayCheckpoint { get; private set; }

    public int VidaGuardada { get; private set; }
    public int VidaMaximaGuardada { get; private set; }
    public int AlmasGuardadas { get; private set; }
    public int FragmentosGuardados { get; private set; }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Guardar(Vector3 posicion, string escena, int vida, int vidaMaxima, int almas, int fragmentos)
    {
        PosicionCheckpoint = posicion;
        EscenaCheckpoint = escena;
        VidaGuardada = vida;
        VidaMaximaGuardada = vidaMaxima;
        AlmasGuardadas = almas;
        FragmentosGuardados = fragmentos;
        HayCheckpoint = true;

        Debug.Log($"Checkpoint guardado en {escena} -> Pos: {posicion} | Vida: {vida}/{vidaMaxima} | Almas: {almas} | Fragmentos: {fragmentos}");
    }

    public void BorrarCheckpoint()
    {
        PosicionCheckpoint = Vector3.zero;
        EscenaCheckpoint = string.Empty;
        VidaGuardada = 0;
        VidaMaximaGuardada = 0;
        AlmasGuardadas = 0;
        FragmentosGuardados = 0;
        HayCheckpoint = false;

        Debug.Log("Checkpoint borrado.");
    }
}