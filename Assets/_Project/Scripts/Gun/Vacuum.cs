// <<hecho por IA>> 2026-10-02 · prototipo
using UnityEngine;
using UnityEngine.InputSystem;

namespace MeltTheDeep
{
    /// <summary>
    /// Aspiradora (clic derecho). Atrae las bolitas que están dentro de un cono delante de la boca
    /// (este objeto) y, cuando llegan a la boca, suma su agua al tanque y las devuelve al pool.
    /// Con el tanque lleno no aspira.
    /// </summary>
    public class Vacuum : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] CollectionTank tank;

        [Header("Entrada")]
        [SerializeField] InputActionReference vacuumAction;

        [Header("Aspiración")]
        [Tooltip("Distancia máxima a la que atrae, en metros.")]
        [SerializeField] float range = 4f;
        [Tooltip("Medio ángulo del cono, en grados: 30 = un cono de 60° de apertura.")]
        [SerializeField, Range(1f, 89f)] float coneAngle = 30f;
        [Tooltip("Fuerza de atracción. La aceleración máxima es fuerza / distancia: más fuerte cuanto más cerca.")]
        [SerializeField] float suctionStrength = 40f;
        [Tooltip("Velocidad máxima a la que viaja una bolita hacia la boca, en m/s.")]
        [SerializeField] float pullSpeed = 6f;
        [Tooltip("Distancia a la boca a la que la bolita entra en el tanque.")]
        [SerializeField] float captureRadius = 0.4f;
        [Tooltip("Capas en las que se buscan bolitas.")]
        [SerializeField] LayerMask waterLayers;

        // Buffer reutilizado: OverlapSphereNonAlloc rellena este array en vez de crear uno nuevo cada vez.
        readonly Collider[] hits = new Collider[128];

        void OnEnable() => vacuumAction.action.Enable();

        void OnDisable() => vacuumAction.action.Disable();

        // Las fuerzas se aplican en FixedUpdate, que va al ritmo de la física.
        void FixedUpdate()
        {
            bool sucking = vacuumAction.action.IsPressed() && Cursor.lockState == CursorLockMode.Locked;
            if (!sucking || tank.IsFull) return;

            Vector3 mouth = transform.position;
            int count = Physics.OverlapSphereNonAlloc(mouth, range, hits, waterLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                if (!hits[i].TryGetComponent(out WaterBall ball) || !ball.isActiveAndEnabled) continue;

                Vector3 toMouth = mouth - ball.Body.position;
                float distance = toMouth.magnitude;

                // En la boca: el agua entra al tanque y la bolita vuelve al pool.
                if (distance <= captureRadius)
                {
                    if (!tank.TryAdd(ball.Volume)) return; // el tanque se acaba de llenar
                    ball.Release();
                    continue;
                }

                // Fuera del cono no atrae: ángulo entre hacia dónde mira la boca y dónde está la bolita.
                if (Vector3.Angle(transform.forward, -toMouth) > coneAngle) continue;

                // Velocidad que queremos: derecha hacia la boca, a pullSpeed.
                Vector3 desiredVelocity = toMouth / distance * pullSpeed;
                // Cuánto puede cambiar la velocidad en este paso: más cuanto más cerca (inverso de la distancia, TDD).
                float maxChange = suctionStrength / distance * Time.fixedDeltaTime;
                // La velocidad actual se acerca a la deseada sin pasar de ese cambio. Así también se frena el
                // movimiento lateral y la bolita va derecha a la boca en vez de pasar de largo y salir despedida.
                // VelocityChange ignora la masa: las grandes se mueven igual que las pequeñas.
                Vector3 change = Vector3.ClampMagnitude(desiredVelocity - ball.Body.linearVelocity, maxChange);
                ball.Body.AddForce(change, ForceMode.VelocityChange);
            }
        }
    }
}
