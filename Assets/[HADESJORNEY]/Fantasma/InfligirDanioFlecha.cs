using UnityEngine;

public class InfligirDanioFlecha : MonoBehaviour
{
    [Tooltip("1 = medio corazón, 2 = un corazón, 3 = corazón y medio...")]
    public int danioMitadCorazones = 1;

    public bool destruirAlImpactar = true;

    private void Awake()
    {
        Debug.Log("[DAÑO FLECHA] Awake en: " + gameObject.name);

        Collider col = GetComponent<Collider>();

        if (col == null)
        {
            Debug.LogError("[DAÑO FLECHA] La flecha NO tiene Collider.");
        }
        else
        {
            Debug.Log("[DAÑO FLECHA] Collider encontrado. IsTrigger antes = " + col.isTrigger);
            col.isTrigger = true;
            Debug.Log("[DAÑO FLECHA] Collider configurado como Trigger.");
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("[DAÑO FLECHA] La flecha no tenía Rigidbody. Se va a añadir automáticamente.");
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        Debug.Log("[DAÑO FLECHA] Rigidbody configurado. Gravity = " + rb.useGravity + " | IsKinematic = " + rb.isKinematic);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[DAÑO FLECHA] OnTriggerEnter contra: " + other.name + " | Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("[DAÑO FLECHA] Ignorado porque es Player.");
            return;
        }

        if (other.CompareTag("Ghost"))
        {
            Debug.Log("[DAÑO FLECHA] Ignorado porque es Ghost.");
            return;
        }

        SistemaVida vida = other.GetComponent<SistemaVida>();

        if (vida == null)
        {
            Debug.Log("[DAÑO FLECHA] No hay SistemaVida en " + other.name + ". Buscando en padre...");
            vida = other.GetComponentInParent<SistemaVida>();
        }

        if (vida == null)
        {
            Debug.LogWarning("[DAÑO FLECHA] El objeto golpeado NO tiene SistemaVida ni en sí mismo ni en su padre: " + other.name);
            return;
        }

        Debug.Log("[DAÑO FLECHA] SistemaVida encontrado en: " + vida.gameObject.name);
        Debug.Log("[DAÑO FLECHA] Aplicando daño: " + danioMitadCorazones);

        vida.RecibirDanio(danioMitadCorazones);

        if (destruirAlImpactar)
        {
            Debug.Log("[DAÑO FLECHA] Flecha destruida tras impactar.");
            Destroy(gameObject);
        }
    }
}