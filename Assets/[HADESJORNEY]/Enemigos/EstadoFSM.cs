using UnityEngine;

namespace Enemy.FSM
{
    public abstract class EstadoFSM : MonoBehaviour
    {
        [SerializeField] GameString nombre;
        [SerializeField] bool inicial = false;

        protected MaquinaFSM maquina;

        public string Nombre => nombre != null ? nombre.Value : "";
        public bool Inicial { get => inicial; set => inicial = value; }

        void Awake()
        {
            maquina = GetComponent<MaquinaFSM>();
            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}