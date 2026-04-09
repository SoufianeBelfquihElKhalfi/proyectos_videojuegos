using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlechaFantasma : MonoBehaviour
{
    [HideInInspector] public Transform objetivo;
    public float velocidad = 15f;
    public float fuerzaGiro = 5f; // qué tan rápido gira hacia el objetivo

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (objetivo == null)
        {
            // Si el objetivo murió, sigue recto
            rb.linearVelocity = transform.forward * velocidad;
            return;
        }

        // Dirección actual hacia el objetivo
        Vector3 dirObjetivo = (objetivo.position - transform.position).normalized;

        // Gira suavemente hacia él
        Vector3 nuevaDireccion = Vector3.RotateTowards(
            transform.forward,
            dirObjetivo,
            fuerzaGiro * Time.fixedDeltaTime, // radianes por segundo
            0f
        );

        // Aplica movimiento y rotación
        rb.linearVelocity = nuevaDireccion * velocidad;
        transform.rotation = Quaternion.LookRotation(nuevaDireccion);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Ghost"))
        {
            Destroy(gameObject);
        }
    }
}