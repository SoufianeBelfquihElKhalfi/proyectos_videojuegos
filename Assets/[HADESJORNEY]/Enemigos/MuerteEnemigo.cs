using UnityEngine;

public class MuerteEnemigo : MonoBehaviour
{
    public void Morir()
    {
        Destroy(gameObject);
    }
}