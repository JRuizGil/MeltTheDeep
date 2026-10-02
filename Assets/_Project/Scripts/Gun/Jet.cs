// <<hecho por IA>> 2026-10-02 · prototipo
using UnityEngine;
using UnityEngine.InputSystem;

namespace MeltTheDeep
{
    /// <summary>
    /// Chorro de agua caliente (clic izquierdo). Lanza un rayo desde el centro de la cámara y, si da en
    /// hielo, le pide al IceVolume que funda un poco alrededor del impacto en cada frame.
    /// </summary>
    public class Jet : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Desde dónde se apunta: la cámara, para que el rayo salga del centro de la pantalla.")]
        [SerializeField] Transform aim;
        [Tooltip("Línea que dibuja el chorro desde la boquilla (este objeto) hasta el impacto.")]
        [SerializeField] LineRenderer beam;
        [Tooltip("Pool al que se pasan los litros fundidos para que suelte bolitas.")]
        [SerializeField] WaterBallPool waterPool;

        [Header("Entrada")]
        [SerializeField] InputActionReference jetAction;

        [Header("Chorro")]
        [SerializeField] float range = 7f;
        [Tooltip("Radio de la zona que se funde alrededor del impacto, en metros.")]
        [SerializeField] float meltRadius = 0.5f;
        [Tooltip("Densidad que se quita por segundo en el centro del impacto (1 = un punto de hielo entero).")]
        [SerializeField] float meltRate = 0.6f;
        [Tooltip("Capas que paran el chorro. Solo funde si lo que toca es hielo.")]
        [SerializeField] LayerMask hitLayers = ~0;

        bool firing;
        Vector3 beamEnd;

        void OnEnable()
        {
            jetAction.action.Enable();
        }

        void OnDisable()
        {
            jetAction.action.Disable();
            beam.enabled = false;
        }

        void Update()
        {
            // Solo dispara con el ratón capturado, para no fundir con el clic que vuelve a capturarlo.
            firing = jetAction.action.IsPressed() && Cursor.lockState == CursorLockMode.Locked;
            if (!firing) return;

            var ray = new Ray(aim.position, aim.forward);
            beamEnd = ray.GetPoint(range);

            if (Physics.Raycast(ray, out RaycastHit hit, range, hitLayers, QueryTriggerInteraction.Ignore))
            {
                beamEnd = hit.point;
                // Los colliders son de los chunks; el IceVolume es su padre.
                IceVolume ice = hit.collider.GetComponentInParent<IceVolume>();
                if (ice != null)
                {
                    // Por deltaTime: funde lo mismo por segundo vaya el juego a 30 o a 144 FPS.
                    float liters = ice.Melt(hit.point, meltRadius, meltRate * Time.deltaTime);
                    waterPool.AddMeltedIce(liters, hit.point, hit.normal);
                }
            }
        }

        // El chorro se dibuja en LateUpdate, cuando la cámara ya se ha movido en este frame, para que no tiemble.
        void LateUpdate()
        {
            beam.enabled = firing;
            if (!firing) return;

            beam.SetPosition(0, transform.position);
            beam.SetPosition(1, beamEnd);
        }
    }
}
