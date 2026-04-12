using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlechaFantasma : MonoBehaviour
{
    [HideInInspector] public Transform objetivo;
    public float velocidad = 15f;
    public float fuerzaGiro = 5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        if (objetivo == null)
        {
            rb.linearVelocity = transform.forward * velocidad;
            return;
        }

        Vector3 dirObjetivo = (objetivo.position - transform.position).normalized;

        Vector3 nuevaDireccion = Vector3.RotateTowards(
            transform.forward,
            dirObjetivo,
            fuerzaGiro * Time.fixedDeltaTime,
            0f
        );

        rb.linearVelocity = nuevaDireccion * velocidad;
        transform.rotation = Quaternion.LookRotation(nuevaDireccion);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ghost") )
            return;

        Destroy(gameObject); // se destruye aquí directamente
    }
}