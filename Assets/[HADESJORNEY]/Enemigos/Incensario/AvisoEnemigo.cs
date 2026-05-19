using UnityEngine;

public class AvisoEnemigo : MonoBehaviour
{
    private EnemigoDistancia enemigo;

    void Awake()
    {
        enemigo = GetComponentInParent<EnemigoDistancia>();
    }

    public void LanzarProyectil()
    {
        enemigo?.LanzarProyectil();
    }
}