using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    public Animator anim;
    public Rigidbody2D rb;
    public AudioSource audioSource;
    public AudioClip swordSlash;
    public SpriteRenderer spriteRenderer;

    // Footstep audio
    public AudioClip[] footstepSounds;
    private float footstepTimer = 0f;
    private float footstepInterval = 0.4f;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 12f;

    // Ground detection
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private bool grounded = false;
    private float move = 0f;
    private float lastMoveDirection = 0f; // tracks previous direction for turnaround detection
    private bool jumpQueued = false;

    void Update()
    {
        Keyboard k = Keyboard.current;

        // MOVEMENT INPUT
        move = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed)  move = -1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) move =  1f;

        // TURNAROUND — only triggers when flipping from one direction to the opposite
        if (move != 0f && lastMoveDirection != 0f && move != lastMoveDirection)
            anim.SetTrigger("TurnAround");

        // Track last active direction (ignore 0 so releasing a key doesn't reset it)
        if (move != 0f)
            lastMoveDirection = move;

        // FLIP SPRITE
        if (move < 0) spriteRenderer.flipX = true;
        else if (move > 0) spriteRenderer.flipX = false;

        // ANIMATOR
        bool isRunning = (Mathf.Abs(move) > 0.1f);
        bool isFalling = !grounded && rb.linearVelocity.y < 0f;

        anim.SetBool("isRunning", isRunning);
        anim.SetBool("Grounded", grounded);
        anim.SetBool("isFalling", isFalling);

        // FOOTSTEPS
        if (isRunning && grounded)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                PlayRandomFootstep();
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        // JUMP
        if (k.spaceKey.wasPressedThisFrame && grounded)
        {
            jumpQueued = true;
            anim.SetTrigger("Jump");
        }

        // ATTACK
        if (k.jKey.wasPressedThisFrame && grounded)
        {
            anim.SetTrigger("Attack");
            audioSource.PlayOneShot(swordSlash);
        }
    }

    void FixedUpdate()
    {
        // GROUND CHECK
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // MOVEMENT
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // JUMP
        if (jumpQueued)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpQueued = false;
        }
        Debug.Log("Grounded: " + grounded);

    }

    void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;
        int randomIndex = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[randomIndex]);
    }
}