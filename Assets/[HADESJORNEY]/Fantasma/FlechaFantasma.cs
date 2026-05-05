using UnityEngine;

public class FlechaFantasma : MonoBehaviour
{
    public Transform objetivo;
    public float velocidad = 15f;
    public float rotacionSuavizada = 10f;

    private Rigidbody rb;

    private void Awake()
    {
        Debug.Log("[FLECHA MOVIMIENTO] Awake en: " + gameObject.name);

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("[FLECHA MOVIMIENTO] No había Rigidbody. Se añade automáticamente.");
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        Debug.Log("[FLECHA MOVIMIENTO] Rigidbody configurado correctamente.");
    }

    private void Start()
    {
        if (objetivo == null)
        {
            Debug.LogError("[FLECHA MOVIMIENTO] La flecha NO tiene objetivo asignado en Start.");
        }
        else
        {
            Debug.Log("[FLECHA MOVIMIENTO] Objetivo recibido en Start: " + objetivo.name);
        }
    }

    private void Update()
    {
        if (objetivo == null)
        {
            Debug.LogWarning("[FLECHA MOVIMIENTO] Objetivo perdido. Destruyendo flecha.");
            Destroy(gameObject);
            return;
        }

        Vector3 direccion = objetivo.position - transform.position;

        Debug.Log("[FLECHA MOVIMIENTO] Moviendo hacia: " + objetivo.name +
                  " | Distancia: " + direccion.magnitude);

        if (direccion.magnitude < 0.2f)
        {
            Debug.Log("[FLECHA MOVIMIENTO] Flecha llegó al objetivo.");
            return;
        }

        direccion.Normalize();

        transform.position += direccion * velocidad * Time.deltaTime;

        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                rotacionSuavizada * Time.deltaTime
            );
        }
    }
}