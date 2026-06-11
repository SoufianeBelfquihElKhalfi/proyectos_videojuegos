using UnityEngine;
using UnityEngine.Events;

public class Maniqui : MonoBehaviour
{
    [Header("Eventos")]
    [SerializeField] private UnityEvent alSerGolpeadoPorPrimeraVez = new UnityEvent();

    private Animator animator;
    private bool golpeando = false;
    private bool primerGolpeRegistrado = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void RecibirDanio()
    {
        if (golpeando) return;

        golpeando = true;

        if (!primerGolpeRegistrado)
        {
            primerGolpeRegistrado = true;
            alSerGolpeadoPorPrimeraVez.Invoke();
        }

        if (animator != null)
            animator.SetTrigger("Hit");

        Invoke(nameof(ResetGolpe), 0.5f);
    }

    private void ResetGolpe()
    {
        golpeando = false;
    }
}