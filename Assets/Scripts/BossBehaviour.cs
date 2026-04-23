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
    public float chaseSpeed = 5.5f;

    [Header("Idle / Walk AI")]
    public float idleTimeMin = 1f;
    public float idleTimeMax = 3f;
    public float walkTimeMin = 1f;
    public float walkTimeMax = 2.5f;

    [Header("Detection")]
    public float detectionRange  = 8f;   // how far the boss can see the player
    public float attackRange     = 1.2f; // how close before attacking
    public float loseAggroRange  = 10f;  // distance before boss gives up chasing

    [Header("Attack")]
    // Assign a child Transform positioned in front of the boss as the hitbox origin
    public Transform attackPoint;
    public float attackHitRange  = 1.0f;
    public int   attackDamage    = 1;
    public float attackCooldown  = 1.5f;
    private float attackCooldownTimer = 0f;

    [Header("Health")]
    public int maxHealth = 8;
    private int currentHealth;

    // Reference to the player
    private Transform player;
    private PlayerBehaviour playerBehaviour;

    //  State machine
    private enum BossState { Idle, Walking, Chasing, Attacking, Dead }
    private BossState currentState = BossState.Idle;

    private float stateTimer = 0f;
    private float move = 0f;
    

    void Start()
    {
        currentHealth = maxHealth;

        // Auto-find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player          = playerObj.transform;
            playerBehaviour = playerObj.GetComponent<PlayerBehaviour>();
        }

        EnterIdle();
    }

    void Update()
    {
        if (currentState == BossState.Dead) return;

        stateTimer            -= Time.deltaTime;
        attackCooldownTimer   -= Time.deltaTime;

        float distToPlayer = player != null
            ? Vector2.Distance(transform.position, player.position)
            : float.MaxValue;

        switch (currentState)
        {
            case BossState.Idle:
                move = 0f;

                // Spot the player → start chasing
                if (distToPlayer <= detectionRange)
                {
                    EnterChasing();
                    break;
                }

                if (stateTimer <= 0f)
                    EnterWalking();
                break;

            case BossState.Walking:
                // Spot the player → start chasing
                if (distToPlayer <= detectionRange)
                {
                    EnterChasing();
                    break;
                }

                if (stateTimer <= 0f)
                    EnterIdle();
                break;

            case BossState.Chasing:
                // Lose aggro if player runs too far
                if (distToPlayer > loseAggroRange)
                {
                    EnterIdle();
                    break;
                }

                // Close enough to attack
                if (distToPlayer <= attackRange && attackCooldownTimer <= 0f)
                {
                    EnterAttacking();
                    break;
                }

                // Move towards player
                move = (player.position.x > transform.position.x) ? 1f : -1f;
                break;

            case BossState.Attacking:
                move = 0f;

                // The attack animation should call PerformAttack() via an Animation Event.
                // stateTimer holds the attack animation duration — when it expires, go back to chasing.
                if (stateTimer <= 0f)
                {
                    if (distToPlayer <= loseAggroRange)
                        EnterChasing();
                    else
                        EnterIdle();
                }
                break;
        }

        // --- Sprite flip ---
        if (move < 0)      spriteRenderer.flipX = false;
        else if (move > 0) spriteRenderer.flipX = true;

        // --- Animator ---
        bool isMoving = Mathf.Abs(move) > 0.1f;
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isChasing", currentState == BossState.Chasing);

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
        if (currentState == BossState.Dead) return;

        float currentSpeed = (currentState == BossState.Chasing) ? chaseSpeed : speed;
        rb.linearVelocity = new Vector2(move * currentSpeed, rb.linearVelocity.y);
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
        move = (Random.value < 0.5f) ? -1f : 1f;
        stateTimer = Random.Range(walkTimeMin, walkTimeMax);
    }

    void EnterChasing()
    {
        currentState = BossState.Chasing;
        // Play roar the first time aggro is gained (optional)
        if (roarSound != null && audioSource != null)
            audioSource.PlayOneShot(roarSound);
    }

    void EnterAttacking()
    {
        currentState = BossState.Attacking;
        attackCooldownTimer = attackCooldown;

        // Estimate how long the attack animation lasts so we know when to return to chasing.
        // Adjust this to match your actual animation clip length.
        stateTimer = 0.8f;

        anim.SetTrigger("Attack");

        PerformAttack();
    }

    // ---------------------------------------------------------------
    //  Attack hitbox — call this from an Animation Event for best timing
    // ---------------------------------------------------------------

    public void PerformAttack()
    {
        if (playerBehaviour == null) return;

        Vector2 origin = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        float dist = Vector2.Distance(origin, player.position);
        if (dist <= attackHitRange)
            playerBehaviour.TakeDamage(attackDamage);
    }

    // ---------------------------------------------------------------
    //  Health / Damage
    // ---------------------------------------------------------------

    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Dead) return;

        currentHealth -= damage;
        Debug.Log($"Boss took {damage} damage. HP: {currentHealth}/{maxHealth}");

        anim.SetTrigger("Hurt");
        // Getting hit snaps the boss into chasing state
        if (currentState != BossState.Attacking)
            EnterChasing();

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        currentState = BossState.Dead;
        move = 0f;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Die");
        // Disable colliders so the player can walk through the corpse
        // foreach (var col in GetComponents<Collider2D>())
        //     col.enabled = false;
        Debug.Log("Boss died.");
        // TODO: trigger victory / loot spawn here
    }

    // ---------------------------------------------------------------
    //  Helpers
    // ---------------------------------------------------------------

    void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;
        int idx = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[idx]);
    }

    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Attack hitbox
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRange);
        }
    }
}