using UnityEngine;

public class Maniqui : MonoBehaviour
{
    private Animator animator;
    private bool golpeando = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void RecibirDanio()
    {
        if (golpeando) return;
        golpeando = true;

        Debug.Log("Animacion activada");

        if (animator != null)
            animator.SetTrigger("Hit");

        Invoke(nameof(ResetGolpe), 0.5f);
    }

    private void ResetGolpe()
    {
        golpeando = false;
    }
}