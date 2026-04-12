using UnityEngine;

public class DropAlSpawnner : MonoBehaviour
{
    public GameObject prefabAlma;
    public int cantidadAlmas = 20;
    public int numeroDePrefabs = 3;

    public bool dropFragmento = false;
    public GameObject prefabFragmento;

    void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return; // Evita drops al cerrar escena

        int almasPorPrefab = cantidadAlmas / numeroDePrefabs;
        int resto = cantidadAlmas % numeroDePrefabs;

        for (int i = 0; i < numeroDePrefabs; i++)
        {
            GameObject drop = Instantiate(prefabAlma, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            DropAlma dropAlma = drop.GetComponent<DropAlma>();

            if (dropAlma != null)
            {
                dropAlma.tipo = DropAlma.TipoDrop.Alma;
                dropAlma.cantidad = 1;
            }
        }

        if (dropFragmento && prefabFragmento != null)
        {
            GameObject frag = Instantiate(prefabFragmento, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            DropAlma dropFrag = frag.GetComponent<DropAlma>();

            if (dropFrag != null)
            {
                dropFrag.tipo = DropAlma.TipoDrop.FragmentoDeAlma;
                dropFrag.cantidad = 1;
            }
        }
    }
}
