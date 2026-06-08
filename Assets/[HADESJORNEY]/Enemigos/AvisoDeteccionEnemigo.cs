using System.Collections;
using UnityEngine;

public class AvisoDeteccionEnemigo : MonoBehaviour
{
    [Header("Exclamación")]
    [SerializeField] private GameObject prefabExclamacion;
    [SerializeField] private Transform puntoAparicion;
    [SerializeField] private float duracion = 1f;

    private GameObject avisoActual;

    private SonidosEnemigos sonidosEnemigo;

    private void Awake()
    {
        sonidosEnemigo = GetComponentInParent<SonidosEnemigos>();

        if (sonidosEnemigo == null)
        {
            throw new MissingComponentException($"{name}: falta SonidosEnemigo en el enemigo padre.");
        }
    }

    public void Mostrar()
    {
        StartCoroutine(MostrarDuranteDuracion());
    }

    public IEnumerator MostrarYEsperar()
    {
        CrearAviso();

        yield return new WaitForSeconds(duracion);

        OcultarAviso();
    }

    private IEnumerator MostrarDuranteDuracion()
    {
        CrearAviso();

        yield return new WaitForSeconds(duracion);

        OcultarAviso();
    }

    private void CrearAviso()
    {
        OcultarAviso();

        avisoActual = Instantiate(
            prefabExclamacion,
            puntoAparicion.position,
            Quaternion.identity,
            puntoAparicion
        );

        avisoActual.transform.localPosition = Vector3.zero;
        avisoActual.transform.localRotation = Quaternion.identity;

        sonidosEnemigo.ReproducirAlerta();
    }

    private void OcultarAviso()
    {
        if (avisoActual != null)
        {
            Destroy(avisoActual);
            avisoActual = null;
        }
    }

    private void OnDisable()
    {
        OcultarAviso();
    }
}