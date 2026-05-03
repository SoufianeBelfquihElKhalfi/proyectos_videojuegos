using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class InventarioAlmas : MonoBehaviour
{
    public static InventarioAlmas Instancia { get; private set; }

    [SerializeField] private int almas = 0;
    [SerializeField] private int fragmentos = 0;

    private Dictionary<string, int> contadores = new Dictionary<string, int>();

    public UnityEvent<int> OnAlmasCambiaron;
    public UnityEvent<int> OnFragmentosCambiaron;
    public UnityEvent<string, int> OnContadorCambio;

    public int Almas => almas;
    public int Fragmentos => fragmentos;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        if (OnAlmasCambiaron == null) OnAlmasCambiaron = new UnityEvent<int>();
        if (OnFragmentosCambiaron == null) OnFragmentosCambiaron = new UnityEvent<int>();
    }

    public void AgregarAlmas(int cantidad)
    {
        almas += cantidad;
        Debug.Log("AgregarAlmas llamado. Total ahora: " + almas + " | Listeners: " + OnAlmasCambiaron.GetPersistentEventCount());
        OnAlmasCambiaron?.Invoke(almas);
    }

    public void AgregarFragmento(int cantidad = 1)
    {
        fragmentos += cantidad;
        OnFragmentosCambiaron?.Invoke(fragmentos);
        RegistrarContador("Fragmentos", cantidad);
    }

    private void RegistrarContador(string nombre, int cantidad)
    {
        if (!contadores.ContainsKey(nombre))
            contadores[nombre] = 0;
        contadores[nombre] += cantidad;
        OnContadorCambio?.Invoke(nombre, contadores[nombre]);
    }

    public int ObtenerContador(string nombre)
    {
        return contadores.ContainsKey(nombre) ? contadores[nombre] : 0;
    }

    public bool GastarAlmas(int cantidad)
    {
        if (Almas < cantidad)
        {
            return false;
        }

        almas -= cantidad;
        OnAlmasCambiaron.Invoke(Almas);
        return true;
    }

    public void PerderTodasLasAlmas()
    {
        almas = 0;
        OnAlmasCambiaron?.Invoke(almas);
    }

    public void PerderTodosLosFragmentos()
    {
        fragmentos = 0;
        OnFragmentosCambiaron?.Invoke(fragmentos);
    }
}