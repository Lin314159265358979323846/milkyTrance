using UnityEngine;
using UnityEngine.InputSystem; // Important to include this!

// This script will handle all player movement and input.
// We'll build on this script for the entire game.
public class PlayerController : MonoBehaviour
{
    // --- Player Movement ---
    [Header("Movement Settings")]
    [Tooltip("The speed at which the player moves left and right.")]
    public float moveSpeed = 7f;

    [Header("Jumping Settings")]
    [Tooltip("The force applied when the player jumps.")]
    public float jumpForce = 16f;

    [Header("Better Jumping Settings")]
    [Tooltip("The gravity scale applied when falling.")]
    public float fallGravityMultiplier = 2.5f;
    [Tooltip("The gravity scale applied when the jump button is released early.")]
    public float lowJumpGravityMultiplier = 2f;
    private float defaultGravityScale;


    // --- Ground Check ---
    [Header("Ground Check Settings")]
    [Tooltip("The transform representing the point where we check for ground.")]
    public Transform groundCheck;
    [Tooltip("The radius of the circle used to check for ground.")]
    public float groundCheckRadius = 0.2f;
    [Tooltip("A LayerMask indicating what layers are considered ground.")]
    public LayerMask whatIsGround;
    private bool isGrounded; // Is the player currently touching the ground?


    // --- Component References ---
    private Rigidbody2D rb; // A reference to the player's Rigidbody2D component.
    private PlayerInput playerInput; // A reference to the PlayerInput component.
    private Vector2 moveInput; // A variable to store the input from the new system.

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        defaultGravityScale = rb.gravityScale; // Store the default gravity scale

        if (rb == null) Debug.LogError("PlayerController is missing a Rigidbody2D component!");
        if (playerInput == null) Debug.LogError("PlayerController is missing a PlayerInput component!");
    }

    private void OnEnable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Move"].canceled += OnMove;
            playerInput.actions["Jump"].performed += OnJump;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions["Move"].performed -= OnMove;
            playerInput.actions["Move"].canceled -= OnMove;
            playerInput.actions["Jump"].performed -= OnJump;
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        // Apply horizontal movement.
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // --- Better Jumping Physics ---
        if (rb.linearVelocity.y < 0)
        {
            // We are falling, so apply the fall multiplier.
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && !playerInput.actions["Jump"].IsPressed())
        {
            // We are moving up but the player has released the jump button, so apply the low jump multiplier.
            rb.gravityScale = defaultGravityScale * lowJumpGravityMultiplier;
        }
        else
        {
            // Not falling or actively jumping, so use the default gravity.
            rb.gravityScale = defaultGravityScale;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            // Apply an instant upward force.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}