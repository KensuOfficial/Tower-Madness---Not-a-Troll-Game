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

    [Header("Audio Channels")]
    [SerializeField] private AudioSource footstepAudioSource; // Channel 1: Dedicated to footsteps
    [SerializeField] private AudioSource actionsAudioSource;   // Channel 2: Dedicated to jumps and lands

    [Header("Audio Customization")]
    [SerializeField] private float footstepCutoffDelay = 0.2f; // Time before footstep cuts off

    [Header("Movement Clips")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip landSound;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private float moveX;
    private int extraJumpsLeft;
    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private float stopTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Save the previous frame's grounding state
        wasGroundedLastFrame = isGrounded;

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

        // LANDING AUDIO FIX: Plays on the actions channel so it never conflicts
        if (isGrounded && !wasGroundedLastFrame)
        {
            PlayActionSound(landSound);
        }

        // 3. Jump Input
        bool jump = Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);

        if (jump)
        {
            if (isGrounded)
            {
                Jump();
                PlayActionSound(jumpSound);
            }
            else if (extraJumpsLeft > 0)
            {
                Jump();
                extraJumpsLeft--;
                anim.SetTrigger("DoubleJump");
                PlayActionSound(doubleJumpSound);
            }
        }

        // 4. Update Animator
        anim.SetFloat("Speed", Mathf.Abs(moveX));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);

        // ROCK-SOLID FOOTSTEP CUTOFF LOGIC
        if (moveX == 0f && isGrounded)
        {
            stopTimer += Time.deltaTime;

            if (stopTimer >= footstepCutoffDelay)
            {
                if (footstepAudioSource != null && footstepAudioSource.isPlaying)
                {
                    footstepAudioSource.Stop(); // Safely stop ONLY the footsteps channel
                }
            }
        }
        else
        {
            stopTimer = 0f;
        }
    }

    void FixedUpdate() => rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);

    void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

    // Called via Animation Window Event flags during running cycles
    public void PlayFootstepSound()
    {
        if (moveX == 0f || !isGrounded) return;

        if (footstepAudioSource != null && footstepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);

            // Keeps footsteps continuous but allows snappy cuts
            footstepAudioSource.Stop();
            footstepAudioSource.PlayOneShot(footstepSounds[randomIndex]);
        }
    }

    // Handles playing actions on a separate channel where it can NEVER be accidentally stopped
    private void PlayActionSound(AudioClip clip)
    {
        if (actionsAudioSource != null && clip != null)
        {
            actionsAudioSource.PlayOneShot(clip);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck) Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}