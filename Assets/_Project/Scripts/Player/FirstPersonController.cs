// <<hecho por IA>> 2026-10-02 · prototipo
using UnityEngine;
using UnityEngine.InputSystem;

namespace MeltTheDeep
{
    /// <summary>
    /// Jugador en primera persona: WASD para moverse, ratón para mirar y espacio para saltar.
    /// El cuerpo gira en horizontal (yaw) y la cámara en vertical (pitch).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] Transform cameraPivot;

        [Header("Entrada (acciones de MeltControls)")]
        [SerializeField] InputActionReference moveAction;
        [SerializeField] InputActionReference lookAction;
        [SerializeField] InputActionReference jumpAction;
        [SerializeField] InputActionReference releaseCursorAction;

        [Header("Movimiento")]
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float jumpHeight = 1.2f;
        [SerializeField] float gravity = -20f;

        [Header("Vista")]
        [Tooltip("Grados que gira la vista por cada píxel que se mueve el ratón.")]
        [SerializeField] float lookSensitivity = 0.1f;
        [SerializeField] float maxPitch = 85f;

        CharacterController controller;
        float pitch;
        float verticalVelocity;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            jumpAction.action.Enable();
            releaseCursorAction.action.Enable();
            SetCursorLocked(true);
        }

        void OnDisable()
        {
            moveAction.action.Disable();
            lookAction.action.Disable();
            jumpAction.action.Disable();
            releaseCursorAction.action.Disable();
            SetCursorLocked(false);
        }

        void Update()
        {
            // Esc suelta el ratón; un clic en la ventana lo vuelve a bloquear.
            if (releaseCursorAction.action.WasPressedThisFrame())
                SetCursorLocked(false);
            else if (Cursor.lockState != CursorLockMode.Locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                SetCursorLocked(true);

            if (Cursor.lockState == CursorLockMode.Locked)
                Look();

            Move();
        }

        void Look()
        {
            // El delta del ratón ya es "lo que se ha movido este frame", así que no se multiplica por deltaTime.
            Vector2 delta = lookAction.action.ReadValue<Vector2>() * lookSensitivity;

            transform.Rotate(0f, delta.x, 0f);
            pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);
            cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        void Move()
        {
            Vector2 input = moveAction.action.ReadValue<Vector2>();
            Vector3 horizontal = (transform.right * input.x + transform.forward * input.y) * moveSpeed;

            if (controller.isGrounded)
            {
                // Un pequeño valor negativo mantiene al controlador pegado al suelo en las bajadas.
                if (verticalVelocity < 0f) verticalVelocity = -2f;
                // v = raíz(2·g·h): la velocidad inicial necesaria para subir jumpHeight metros.
                if (jumpAction.action.WasPressedThisFrame()) verticalVelocity = Mathf.Sqrt(2f * -gravity * jumpHeight);
            }
            verticalVelocity += gravity * Time.deltaTime;

            controller.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
