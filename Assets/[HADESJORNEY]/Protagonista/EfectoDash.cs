using UnityEngine;
using System.Collections;

public class EfectoDash : MonoBehaviour
{
    [SerializeField] private float intervaloClones = 0.05f;
    [SerializeField] private float duracionClon = 0.3f;
    [SerializeField] private Color colorClon = new Color(0.5f, 0.8f, 1f, 0.5f);

    private bool activo = false;

    public void Activar()
    {
        activo = true;
        StartCoroutine(GenerarClones());
    }

    public void Desactivar()
    {
        activo = false;
    }

    private IEnumerator GenerarClones()
    {
        while (activo)
        {
            CrearClon();
            yield return new WaitForSeconds(intervaloClones);
        }
    }

    private void CrearClon()
    {
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer smr in renderers)
        {
            Mesh mesh = new Mesh();
            smr.BakeMesh(mesh);

            GameObject clon = new GameObject("Ghost");
            clon.transform.position = smr.transform.position;
            clon.transform.rotation = transform.rotation;
            clon.transform.localScale = smr.transform.lossyScale * 0.5f;

            MeshFilter mf = clon.AddComponent<MeshFilter>();
            mf.mesh = mesh;

            MeshRenderer mr = clon.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = colorClon;
            mr.material = mat;

            StartCoroutine(DesvanecerClon(clon, duracionClon));
        }
    }

    private IEnumerator DesvanecerClon(GameObject clon, float duracion)
    {
        MeshRenderer mr = clon.GetComponent<MeshRenderer>();
        Color color = colorClon;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            color.a = Mathf.Lerp(colorClon.a, 0f, tiempo / duracion);
            if (mr != null) mr.material.color = color;
            yield return null;
        }

        Destroy(clon);
    }
}
