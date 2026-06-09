using UnityEngine;

public class efectoCuracion : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfxCuracion;

    public void ReproducirCuracion()
    {
        if (vfxCuracion != null)
            vfxCuracion.Play();
    }
}