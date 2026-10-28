using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private int maxExtraJumps = 1;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private float moveX;
    private int extraJumpsLeft;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Read Movement Input
        moveX = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
        }

        if (moveX != 0) sr.flipX = moveX < 0;

        // 2. Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGrounded) extraJumpsLeft = maxExtraJumps;

        // 3. Jump Input
        bool jump = Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);

        if (jump)
        {
            if (isGrounded) Jump();
            else if (extraJumpsLeft > 0) { Jump(); extraJumpsLeft--; anim.SetTrigger("DoubleJump"); }
        }

        // 4. Update Animator
        anim.SetFloat("Speed", Mathf.Abs(moveX));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    void FixedUpdate() => rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);

    void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

    void OnDrawGizmosSelected()
    {
        if (groundCheck) Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}