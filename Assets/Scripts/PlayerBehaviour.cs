using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    public Animator anim;
    public Rigidbody2D rb;
    public AudioSource audioSource;
    public AudioClip swordSlash;
    public SpriteRenderer spriteRenderer; // Add this
    
    // Footstep audio
    public AudioClip[] footstepSounds;
    private float footstepTimer = 0f;
    private float footstepInterval = 0.6f;

    private float speed = 5f;
    private float jumpForce = 8f;

    bool grounded = true;

    void Update()
    {
        Keyboard k = Keyboard.current;

        // MOVEMENT
        float move = 0f;

        if (k.aKey.isPressed || k.leftArrowKey.isPressed)
            move = -1f;

        if (k.dKey.isPressed || k.rightArrowKey.isPressed)
            move = 1f;

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        // FLIP SPRITE
        if (move < 0)
            spriteRenderer.flipX = true;  // Facing left
        else if (move > 0)
            spriteRenderer.flipX = false; // Facing right

        // Set animator parameters
        bool isRunning = (Mathf.Abs(move) > 0.1f);
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("Grounded", grounded);

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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
            grounded = false;
        }

        // ATTACK
        if (k.jKey.wasPressedThisFrame)
        {
            anim.SetTrigger("Attack");
            audioSource.PlayOneShot(swordSlash);
        }
    }

    void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;
        
        int randomIndex = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[randomIndex]);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        grounded = true;
    }
}