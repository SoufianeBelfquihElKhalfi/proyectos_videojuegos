using UnityEngine;

public class ControladorSala : MonoBehaviour
{
    [SerializeField] private Animator animatorPuerta;
    [SerializeField] private SistemaVida[] enemigos;
    private int enemigosVivos = 0;

    void Start()
    {
        enemigosVivos = enemigos.Length;
    }

    public void EnemigoMuerto()
    {
        enemigosVivos--;
        if (enemigosVivos <= 0)
        {
            AbrirPuerta();
        }
    }

    void AbrirPuerta()
    {
        if (animatorPuerta != null)
            animatorPuerta.SetTrigger("Abrir");
    }
}