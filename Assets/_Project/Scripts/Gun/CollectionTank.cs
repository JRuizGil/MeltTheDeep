// <<hecho por IA>> 2026-10-02 · prototipo
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MeltTheDeep
{
    /// <summary>
    /// Tanque donde va el agua aspirada. Tiene una capacidad máxima en litros, se vacía con la tecla E
    /// y avisa con un evento cada vez que cambia, para que la UI no tenga que consultarlo en cada frame.
    /// </summary>
    public class CollectionTank : MonoBehaviour
    {
        [Tooltip("Litros que caben en el tanque.")]
        [SerializeField, Min(1f)] float capacity = 100f;

        [Header("Entrada")]
        [SerializeField] InputActionReference emptyAction;

        /// <summary>Se lanza cada vez que cambian los litros, al llenar o al vaciar.</summary>
        public event Action<CollectionTank> Changed;

        public float Liters { get; private set; }
        public float Capacity => capacity;
        public float Fill => Liters / capacity; // de 0 (vacío) a 1 (lleno)
        public bool IsFull => Liters >= capacity;

        void OnEnable()
        {
            emptyAction.action.Enable();
            emptyAction.action.performed += OnEmptyPerformed;
        }

        void OnDisable()
        {
            emptyAction.action.performed -= OnEmptyPerformed;
            emptyAction.action.Disable();
        }

        /// <summary>Suma agua. Si no cabe entera, llena hasta el tope y el resto se pierde.</summary>
        /// <returns>false si el tanque ya estaba lleno y no ha entrado nada.</returns>
        public bool TryAdd(float liters)
        {
            if (IsFull) return false;

            Liters = Mathf.Min(capacity, Liters + liters);
            Changed?.Invoke(this);
            return true;
        }

        public void Empty()
        {
            Liters = 0f;
            Changed?.Invoke(this);
        }

        // EmptyTank es una acción de tipo botón: "performed" se lanza una vez por pulsación.
        void OnEmptyPerformed(InputAction.CallbackContext context) => Empty();
    }
}
