using System.Threading;
using UnityEngine;

public class VidaEnemigo : MonoBehaviour
{
    public float vida = 100f;
    public float fuerzaRetroceso = 8f;

    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void RecibirDanio(float danio, Vector3 posicionAtacante)
    {
        
        vida -= danio;

        Vector3 direccion = (transform.position - posicionAtacante).normalized;
        direccion.y = 0f;
        rb.AddForce(direccion * fuerzaRetroceso, ForceMode.Impulse);

        if (vida <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
