using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("The force applied to the player on the ground to make them accelerate.")]
    public float groundAcceleration = 120f;
    [Tooltip("The force applied to the player in the air to change direction.")]
    public float airAcceleration = 90f;
    [Tooltip("The maximum horizontal speed the player can reach.")]
    public float maxSpeed = 12f;
    [Tooltip("The drag applied on the ground when there is no horizontal input.")]
    public float groundLinearDrag = 8f;
    [Tooltip("The drag applied in the air when there is no horizontal input.")]
    public float airLinearDrag = 2f;


    [Header("Jumping")]
    [Tooltip("The desired peak height of the jump.")]
    public float jumpHeight = 5f;
    [Tooltip("The gravity multiplier applied during the upward arc of the jump.")]
    public float jumpGravityMultiplier = 1f;
    [Tooltip("The gravity multiplier applied when falling to make the jump feel less floaty.")]
    public float fallGravityMultiplier = 3.5f;
    [Tooltip("The gravity multiplier applied when the jump button is released early, allowing for variable jump height.")]
    public float lowJumpGravityMultiplier = 2.5f;
    [Tooltip("How long (in seconds) the player can still jump after walking off a ledge.")]
    public float coyoteTime = 0.15f;
    [Tooltip("How long (in seconds) a jump input is 'remembered' before the player lands.")]
    public float jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;

    // --- Private State ---
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float defaultGravityScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        defaultGravityScale = rb.gravityScale;
    }

    private void OnEnable()
    {
        if (playerInput?.actions != null)
        {
            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Move"].canceled += OnMove;
            playerInput.actions["Jump"].performed += OnJump;
        }
    }

    private void OnDisable()
    {
        if (playerInput?.actions != null)
        {
            playerInput.actions["Move"].performed -= OnMove;
            playerInput.actions["Move"].canceled -= OnMove;
            playerInput.actions["Jump"].performed -= OnJump;
        }
    }

    void Update()
    {
        // --- Timer Management ---
        if (coyoteTimeCounter > 0) coyoteTimeCounter -= Time.deltaTime;
        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;

        // --- Ground Check & Coyote Time ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
    }

    void FixedUpdate()
    {
        // --- Horizontal Movement ---
        float currentAcceleration = isGrounded ? groundAcceleration : airAcceleration;
        rb.AddForce(new Vector2(moveInput.x * currentAcceleration, 0f));

        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
        }

        // --- Deceleration / Linear Drag ---
        if (Mathf.Abs(moveInput.x) < 0.01f)
        {
            float currentDrag = isGrounded ? groundLinearDrag : airLinearDrag;
            float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x), currentDrag);
            amount *= Mathf.Sign(rb.linearVelocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }

        // --- Jump Execution ---
        // The check is now performed here, every physics frame.
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * (Physics2D.gravity.y * defaultGravityScale));
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

            // Reset timers to prevent multiple jumps
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // --- Better Jumping Physics ---
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && !playerInput.actions["Jump"].IsPressed())
        {
            rb.gravityScale = defaultGravityScale * lowJumpGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0)
        {
            rb.gravityScale = defaultGravityScale * jumpGravityMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // This function's only job now is to start the buffer timer.
        jumpBufferCounter = jumpBufferTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}