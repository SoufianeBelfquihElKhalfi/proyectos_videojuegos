using UnityEngine;
using Dapasa.Audio;

public class InfligirDanioFlecha : MonoBehaviour
{
    [Tooltip("1 = medio corazón, 2 = un corazón, 3 = corazón y medio...")]
    public int danioMitadCorazones = 1;

    public bool destruirAlImpactar = true;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;

        if (other.CompareTag("Ghost"))
            return;

        SistemaVida vida = other.GetComponent<SistemaVida>();

        if (vida == null)
        {
            vida = other.GetComponentInParent<SistemaVida>();
        }

        if (vida == null)
            return;

        vida.RecibirDanio(danioMitadCorazones);

       

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirSFX3D("EnemigoElectrificado", vida.transform.position);
        }

        if (destruirAlImpactar)
        {
            Destroy(gameObject);
        }
    }
}