using UnityEngine;

/* CAMBIOS REALIZADOS:
 * Distrobuye correctamente las almas entre los drops.
 */
public class DropAlSpawnner : MonoBehaviour
{
    [SerializeField] private GameObject prefabAlma;
    [SerializeField] private int cantidadAlmas = 20;
    [SerializeField] private int numeroDePrefabs = 3;

    [SerializeField] private bool dropFragmento = false;
    [SerializeField] private GameObject prefabFragmento;

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded)
        {
            return;
        }

        if (prefabAlma == null || cantidadAlmas <= 0 || numeroDePrefabs <= 0)
        {
            return;
        }

        int cantidadDrops = Mathf.Min(numeroDePrefabs, cantidadAlmas);
        int almasPorDrop = cantidadAlmas / cantidadDrops;
        int resto = cantidadAlmas % cantidadDrops;

        for (int i = 0; i < cantidadDrops; i++)
        {
            GameObject drop = Instantiate(
                prefabAlma,
                transform.position + Vector3.up * 0.5f,
                Quaternion.identity
            );

            DropAlma dropAlma = drop.GetComponent<DropAlma>();

            if (dropAlma != null)
            {
                dropAlma.tipo = DropAlma.TipoDrop.Alma;
                dropAlma.cantidad = almasPorDrop + (i < resto ? 1 : 0);
            }
        }

        if (dropFragmento && prefabFragmento != null)
        {
            GameObject frag = Instantiate(
                prefabFragmento,
                transform.position + Vector3.up * 0.5f,
                Quaternion.identity
            );

            DropAlma dropFrag = frag.GetComponent<DropAlma>();

            if (dropFrag != null)
            {
                dropFrag.tipo = DropAlma.TipoDrop.FragmentoDeAlma;
                dropFrag.cantidad = 1;
            }
        }
    }
}