using UnityEngine;

public class combate : MonoBehaviour
{
    public float danio = 20f;
    public float rangoAtaque = 1.5f;
    public float cooldownAtaque = 0.4f;
    public Transform puntoAtaque;
    public LayerMask capaEnemigos;

    float tiempoUltimoAtaque = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= tiempoUltimoAtaque + cooldownAtaque)
        {
            Atacar();
            tiempoUltimoAtaque = Time.time;
        }
    }

    void Atacar()
    {
        Collider[] enemigos = Physics.OverlapSphere(puntoAtaque.position, rangoAtaque, capaEnemigos);

        foreach (Collider enemigo in enemigos)
        {
            VidaEnemigo vida = enemigo.GetComponent<VidaEnemigo>();

            if (vida != null)
            {
                vida.RecibirDanio(danio, transform.position);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }
}
