using UnityEngine;

public class BossBehaviour : MonoBehaviour
{
    public Animator anim;
    public Rigidbody2D rb;
    public AudioSource audioSource;
    public AudioClip roarSound;
    public SpriteRenderer spriteRenderer;

    // Footstep audio
    public AudioClip[] footstepSounds;
    private float footstepTimer = 0f;
    private float footstepInterval = 0.4f;

    [Header("Movement")]
    public float speed = 4f;

    // How long the boss will idle before deciding to move (min/max seconds)
    public float idleTimeMin = 1f;
    public float idleTimeMax = 3f;

    // How long the boss will walk before stopping (min/max seconds)
    public float walkTimeMin = 1f;
    public float walkTimeMax = 2.5f;

    // --- AI State ---
    private enum BossState { Idle, Walking, Chasing, Attacking }
    private BossState currentState = BossState.Idle;

    private float stateTimer = 0f;       // counts down to next state switch
    private float move = 0f;             // -1, 0, or 1  (used by movement + animator)

    void Start()
    {
        EnterIdle();
    }

    void Update()
    {
        // Tick the state machine
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case BossState.Idle:
                move = 0f;
                if (stateTimer <= 0f)
                    EnterWalking();
                break;

            case BossState.Walking:
                // move direction is set when we enter the state; just wait for the timer
                if (stateTimer <= 0f)
                    EnterIdle();
                break;
            case BossState.Chasing:
            // Implement chasing thing here
            break;
            case BossState.Attacking:
            // Implement attacking thing here
            break;
        }

        // --- Sprite flip ---
        if (move < 0)
            spriteRenderer.flipX = false;
        else if (move > 0)
            spriteRenderer.flipX = true;

        // --- Animator ---
        bool isMoving = Mathf.Abs(move) > 0.1f;
        anim.SetBool("isMoving", isMoving);

        // --- Footsteps ---
        if (isMoving)
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
    }

    void FixedUpdate()
    {
        // Apply horizontal movement while preserving vertical velocity (gravity/jumping)
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
    }

    // ---------------------------------------------------------------
    //  State transitions
    // ---------------------------------------------------------------

    void EnterIdle()
    {
        currentState = BossState.Idle;
        move = 0f;
        stateTimer = Random.Range(idleTimeMin, idleTimeMax);
    }

    void EnterWalking()
    {
        currentState = BossState.Walking;

        // Randomly pick left (-1) or right (1)
        move = (Random.value < 0.5f) ? -1f : 1f;
        stateTimer = Random.Range(walkTimeMin, walkTimeMax);
    }
    
    void EnterChasing()
    {
        currentState = BossState.Chasing;
        // Set move direction towards player (not implemented here)
        // stateTimer could be used to limit how long the boss chases before re-evaluating
    }

    void EnterAttacking()
    {
        currentState = BossState.Attacking;
        // Implement attacking logic here
    }

    // ---------------------------------------------------------------
    //  Helpers
    // ---------------------------------------------------------------

    void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;

        int randomIndex = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[randomIndex]);
    }
}