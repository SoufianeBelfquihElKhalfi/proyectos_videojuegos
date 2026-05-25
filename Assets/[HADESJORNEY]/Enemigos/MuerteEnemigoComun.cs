using UnityEngine;

public class MuerteEnemigoComun : MonoBehaviour
{
    [Header("Componentes que se apagan al morir")]
    [SerializeField] private Behaviour[] componentesADesactivar;

    [Header("Colliders que se apagan al morir")]
    [SerializeField] private Collider[] collidersADesactivar;

    public void PrepararMuerte()
    {
        DesactivarComponentes();
        DesactivarColliders();
    }

    private void DesactivarComponentes()
    {
        foreach (Behaviour componente in componentesADesactivar)
        {
            componente.enabled = false;
        }
    }

    private void DesactivarColliders()
    {
        foreach (Collider collider in collidersADesactivar)
        {
            collider.enabled = false;
        }
    }
}