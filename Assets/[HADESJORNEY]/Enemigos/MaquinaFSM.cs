
using UnityEngine;
using UnityEngine.Events;

namespace Enemy.FSM
{
    [DefaultExecutionOrder(-100)]
    public abstract class MaquinaFSM : MonoBehaviour
    {
        public UnityEvent<string> OnStateChanged;

        EstadoFSM[] estados;
        EstadoFSM _estado;

        public EstadoFSM Estado
        {
            get => _estado;
            set
            {
                if (_estado == value) return;

                _estado = value;
                //Debug.Log("Nuevo estado: " + _estado.Nombre);

                foreach (EstadoFSM estado in estados)
                {
                    estado.enabled = (estado == _estado);
                }

                OnStateChanged?.Invoke(_estado.Nombre);
            }
        }

        void Awake()
        {
            estados = GetComponents<EstadoFSM>();

            foreach (EstadoFSM estado in estados)
            {
                if (estado.Inicial)
                    Estado = estado;
            }

            if (Estado == null && estados.Length > 0)
                Estado = estados[0];

            OnAwake();
        }

        protected virtual void OnAwake() { }

        public void SetEstado(string nombre)
        {
            foreach (EstadoFSM estado in estados)
            {
                if (estado.Nombre == nombre)
                {
                    Estado = estado;
                    return;
                }
            }

            //Debug.LogError($"No hay un estado llamado: {nombre}");
        }
    }
}