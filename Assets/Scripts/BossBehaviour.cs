using UnityEngine;

public class BossBehaviour : MonoBehaviour
{
    public Animator anim;
    public Rigidbody2D rb;
    public AudioSource audioSource;
    public AudioClip roarSound1;
    public AudioClip roarSound2;
    public AudioClip deathSound;
    public SpriteRenderer spriteRenderer;

    // Footstep audio
    public AudioClip[] footstepSounds;
    private float footstepTimer = 0f;
    private float footstepInterval = 0.4f;

    [Header("Sprite")]
    public bool flipXInverted = false;

    [Header("Movement")]
    public float speed = 4f;
    public float chaseSpeed = 5.5f;

    [Header("Idle / Walk AI")]
    public float idleTimeMin = 1f;
    public float idleTimeMax = 3f;
    public float walkTimeMin = 1f;
    public float walkTimeMax = 2.5f;

    [Header("Detection")]
    public float detectionRange  = 15f;   // how far the boss can see the player
    public float attackRange     = 3f; // how close before attacking
    public float loseAggroRange  = 20f;  // distance before boss gives up chasing

    [Header("Attack")]
    // Assign a child Transform positioned in front of the boss as the hitbox origin
    public Transform attackPoint;
    public float attackHitRange  = 3.5f;
    public int   attackDamage    = 1;
    public float attackCooldown  = 1.5f;
    private float attackCooldownTimer = 0f;
    private float attackDuration = 0.8f;
    private bool attackFired = false;

    [Header("Health")]
    public int maxHealth = 8;
    private int currentHealth;

    [Header("Drops")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;
    public int minCoinDrops = 2;
    public int maxCoinDrops = 5;
    [Range(0f, 1f)] public float heartDropChance = 0.3f;
    public float dropScatter = 1.5f;

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

                if (!attackFired && stateTimer <= attackDuration * 0.5f)
                {
                    PerformAttack();
                    attackFired = true;
                }

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
        if (move < 0)      spriteRenderer.flipX = flipXInverted;
        else if (move > 0) spriteRenderer.flipX = !flipXInverted;

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
        if (roarSound1 != null && roarSound2 != null && audioSource != null) {
            // play one of two roar sounds
            AudioClip roarToPlay = (Random.value < 0.5f) ? roarSound1 : roarSound2;
            audioSource.PlayOneShot(roarToPlay);
        }
    }

    void EnterAttacking()
    {
        currentState = BossState.Attacking;
        attackCooldownTimer = attackCooldown;


        stateTimer = attackDuration;
        attackFired = false;

        anim.SetTrigger("Attack");
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
            Debug.Log($"Boss attacked player. Player HP: {playerBehaviour.CurrentHealth}/{playerBehaviour.maxHealth}");
    }

    // ---------------------------------------------------------------
    //  Health / Damage
    // ---------------------------------------------------------------

    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Dead) return;

        currentHealth -= damage;
        Debug.Log($"Boss took {damage} damage. HP: {currentHealth}/{maxHealth}");

        anim.ResetTrigger("Hurt");
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
        audioSource.PlayOneShot(deathSound);
        // Disable colliders so the player can walk through the corpse
        foreach (var col in GetComponents<Collider2D>()) {
            col.enabled = false;
        }
        // Disable gravity so boss doesn't fall through floor when death animation plays
        rb.gravityScale = 0f;
            
        Debug.Log("Boss died.");
        SpawnDrops();
    }

    void SpawnDrops()
    {
        if (coinPrefab != null)
        {
            int count = Random.Range(minCoinDrops, maxCoinDrops + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2(Random.Range(-dropScatter, dropScatter), Random.Range(0f, dropScatter));
                Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }

        if (heartPrefab != null && Random.value < heartDropChance)
        {
            Vector2 offset = new Vector2(Random.Range(-dropScatter, dropScatter), Random.Range(0f, dropScatter));
            Instantiate(heartPrefab, (Vector2)transform.position + offset, Quaternion.identity);
        }
    }


    void PlayRandomFootstep()
    {
        if (footstepSounds.Length == 0) return;
        // periodically play footsteps. Only if boss is moving.
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