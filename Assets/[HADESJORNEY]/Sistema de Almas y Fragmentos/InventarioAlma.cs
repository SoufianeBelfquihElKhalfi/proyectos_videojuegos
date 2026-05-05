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

    private void Awake()
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
        if (OnContadorCambio == null) OnContadorCambio = new UnityEvent<string, int>();
    }

    private void Start()
    {
        NotificarCambios();
    }

    public void AgregarAlmas(int cantidad)
    {
        if (cantidad <= 0) return;

        almas += cantidad;
        OnAlmasCambiaron?.Invoke(almas);
    }

    public void AgregarFragmento(int cantidad = 1)
    {
        if (cantidad <= 0) return;

        fragmentos += cantidad;
        OnFragmentosCambiaron?.Invoke(fragmentos);
        RegistrarContador("Fragmentos", cantidad);
    }

    public bool GastarAlmas(int cantidad)
    {
        if (cantidad <= 0) return false;

        if (almas < cantidad)
        {
            return false;
        }

        almas -= cantidad;
        OnAlmasCambiaron?.Invoke(almas);
        return true;
    }

    public void EstablecerInventario(int nuevasAlmas, int nuevosFragmentos)
    {
        almas = Mathf.Max(0, nuevasAlmas);
        fragmentos = Mathf.Max(0, nuevosFragmentos);

        NotificarCambios();
    }

    public void ResetearInventario()
    {
        EstablecerInventario(0, 0);
        contadores.Clear();
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

    private void RegistrarContador(string nombre, int cantidad)
    {
        if (!contadores.ContainsKey(nombre))
        {
            contadores[nombre] = 0;
        }

        contadores[nombre] += cantidad;
        OnContadorCambio?.Invoke(nombre, contadores[nombre]);
    }

    public int ObtenerContador(string nombre)
    {
        return contadores.ContainsKey(nombre) ? contadores[nombre] : 0;
    }

    private void NotificarCambios()
    {
        OnAlmasCambiaron?.Invoke(almas);
        OnFragmentosCambiaron?.Invoke(fragmentos);
    }
}