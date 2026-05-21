using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class AnimacionEnemigoMelee : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Parametros del Animator")]
    [SerializeField] private string parametroVelocidad = "Velocidad";
    [SerializeField] private string triggerAtaqueEstocada = "AtaqueEstocada";
    [SerializeField] private string triggerAtaqueCorte = "AtaqueCorte";

    [Header("Ajustes")]
    [SerializeField] private float velocidadMinima = 0.05f;
    [SerializeField] private float suavizadoVelocidad = 0.1f;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (animator == null || agent == null)
        {
            return;
        }

        float velocidadActual = agent.velocity.magnitude;

        if (velocidadActual < velocidadMinima)
        {
            velocidadActual = 0f;
        }

        animator.SetFloat(
            parametroVelocidad,
            velocidadActual,
            suavizadoVelocidad,
            Time.deltaTime
        );
    }

    public void ReproducirAtaque(bool usarCorte)
    {
        if (animator == null)
        {
            return;
        }

        if (usarCorte)
        {
            animator.SetTrigger(triggerAtaqueCorte);
        }
        else
        {
            animator.SetTrigger(triggerAtaqueEstocada);
        }
    }
}