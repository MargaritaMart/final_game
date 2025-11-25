using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Configuracion de movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // Componentes
    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isGrounded;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            enabled = false;
            return;
        }

        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnJumpPerformed;

        AssignCamera();
    }

    public override void OnNetworkDespawn()
    {
        if (inputActions != null)
        {
            inputActions.Player.Jump.performed -= OnJumpPerformed;
            inputActions.Player.Disable();
            inputActions.Dispose();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
    
    private void AssignCamera()
    {
        var vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Target.TrackingTarget = transform;
            Debug.Log("Camera assigned to player");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}