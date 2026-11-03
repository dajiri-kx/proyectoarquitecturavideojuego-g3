using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class TheoController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float runSpeed = 7.5f;
    [SerializeField] private float jumpForce = 11.5f;
    [SerializeField] private bool allowRun = true;

    [Header("Top-Down / 4-Way Free Movement Option")]
    [Tooltip("If enabled, player moves freely in all 4 directions with vertical velocity. If disabled, operates in platformer mode (jump + gravity).")]
    [SerializeField] private bool topDownMode = false;
    [SerializeField] private float verticalMoveSpeed = 4.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.22f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.65f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackCooldown = 0.35f;

    [Header("Respawn")]
    [SerializeField] private float fallThreshold = -7f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    // State
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isRunning;
    private float lastAttackTime = -99f;
    private float defaultGravity;
    private Vector3 spawnPosition;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int JumpAttackHash = Animator.StringToHash("JumpAttack");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        defaultGravity = rb.gravityScale;
        spawnPosition = transform.position;

        // Auto-create GroundCheck if not assigned
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0, -0.7f, 0);
            groundCheck = gc.transform;
        }

        // Auto-create AttackPoint if not assigned
        if (attackPoint == null)
        {
            GameObject ap = new GameObject("AttackPoint");
            ap.transform.parent = transform;
            ap.transform.localPosition = new Vector3(0.6f, 0, 0);
            attackPoint = ap.transform;
        }

        // Default groundLayer if not set
        if (groundLayer.value == 0)
        {
            groundLayer = ~LayerMask.GetMask("Ignore Raycast");
        }
    }

    private void Update()
    {
        // Toggle Top-Down mode with 'T' key for testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            topDownMode = !topDownMode;
            if (topDownMode)
            {
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
            }
            else
            {
                rb.gravityScale = defaultGravity;
            }
        }

        // Read 4-direction input
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Up/Down

        isRunning = allowRun && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        // Jump input: Space, W, or Up Arrow in platformer mode
        if (!topDownMode)
        {
            if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded)
            {
                Jump();
            }
        }

        // Attack input: Left Click, J, X, or Left Control
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftControl)) 
            && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
        }

        // Handle sprite flipping
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Respawn if fell
        if (transform.position.y < fallThreshold)
        {
            transform.position = spawnPosition;
            rb.velocity = Vector2.zero;
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        CheckGround();

        if (topDownMode)
        {
            // Free 4-direction movement (top-down style, no gravity)
            rb.gravityScale = 0f;
            Vector2 dir = new Vector2(horizontalInput, verticalInput).normalized;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            rb.velocity = dir * currentSpeed;
        }
        else
        {
            // Standard 2D platformer movement with gravity
            rb.gravityScale = defaultGravity;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            rb.velocity = new Vector2(horizontalInput * currentSpeed, rb.velocity.y);

            // Fast fall on Down Arrow / S while airborne
            if (verticalInput < -0.5f && !isGrounded && rb.velocity.y < 0)
            {
                rb.velocity += Vector2.down * 15f * Time.fixedDeltaTime;
            }
        }
    }

    private void CheckGround()
    {
        if (topDownMode)
        {
            isGrounded = true;
            return;
        }

        // Cast overlap circle at feet excluding player collider
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = false;
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject && !col.isTrigger)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        if (animator != null)
        {
            if (isGrounded)
            {
                animator.SetTrigger(AttackHash);
            }
            else
            {
                animator.SetTrigger(JumpAttackHash);
            }
        }

        // Deal damage in front of Theo
        if (attackPoint != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer.value != 0 ? enemyLayer : ~0);
            foreach (var hit in hits)
            {
                if (hit.gameObject != gameObject && !hit.isTrigger)
                {
                    var target = hit.GetComponent<IDamageable>();
                    if (target != null)
                    {
                        target.TakeDamage(1, transform.position);
                    }
                }
            }
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = Mathf.Abs(rb.velocity.x);
        if (topDownMode)
        {
            speed = rb.velocity.magnitude;
        }
        animator.SetFloat(SpeedHash, speed);
        animator.SetBool(IsGroundedHash, isGrounded);
    }



    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
