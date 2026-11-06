using UnityEngine;
using UnityEngine.InputSystem;
//Controller class for the player character, handling movement and looking around using the new Input System.
public class PlayerController : MonoBehaviour
{
    [SerializeField] InputActionReference moveAction, lookAction;
    [SerializeField] Transform cameraPivot;
    [SerializeField] float moveSpeed = 5f, lookSpeed = 0.1f;
    [SerializeField] float gravity = -20f, maxPitch = 85f;

    float pitch, verticalVelocity;
    CharacterController characterController;

    #region Unity Methods
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }     
    void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
    }
    void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
    }
    void Update()
    {
        HandleLook(lookAction.action);
        HandleMove(moveAction.action);
    }
    void HandleLook(InputAction lookAction)
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        pitch -= lookInput.y * lookSpeed;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * lookSpeed);
    }
    void HandleMove(InputAction moveAction)
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        move.x *= moveSpeed;
        move.z *= moveSpeed;           
        characterController.Move(move * Time.deltaTime);
        if (IsGrounded() && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // Small negative value to keep the player grounded
        }
    }
    bool IsGrounded()
    {
        return characterController.isGrounded;
    }
    #endregion
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
