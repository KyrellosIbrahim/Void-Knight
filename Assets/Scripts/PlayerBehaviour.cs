using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    public Animator anim;
    public Rigidbody2D rb;
    public AudioSource audioSource;
    public AudioClip swordSlash;
    
    // Footstep audio
    public AudioClip[] footstepSounds; // Array of 3 footstep sounds
    private float footstepTimer = 0f;
    private float footstepInterval = 0.6f; // Time between footsteps (adjust based on animation speed)

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

        anim.SetFloat("Speed", Mathf.Abs(move));
        anim.SetBool("Grounded", grounded);

        // FOOTSTEPS
        if (Mathf.Abs(move) > 0.1f && grounded)
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
            footstepTimer = 0f; // Reset timer when not moving
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