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

    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Attack")]
    // Assign a child Transform positioned in front of the player to act as the hitbox origin
    public Transform attackPoint;
    public float attackRange = 0.6f;
    public int attackDamage = 1;
    public LayerMask enemyLayers;
    // Cooldown so the player can't spam attacks
    public float attackCooldown = 0.5f;
    private float attackCooldownTimer = 0f;

    [Header("Hurt")]
    public float invincibilityDuration = 0.8f;   // brief i-frames after being hit
    private float invincibilityTimer = 0f;

    private bool grounded = false;
    private float move = 0f;
    private float lastMoveDirection = 0f;
    private bool jumpQueued = false;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        Keyboard k = Keyboard.current;

        // Tick timers
        attackCooldownTimer  -= Time.deltaTime;
        invincibilityTimer   -= Time.deltaTime;

        // MOVEMENT INPUT
        move = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed)  move = -1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) move =  1f;

        // TURNAROUND
        if (move != 0f && lastMoveDirection != 0f && move != lastMoveDirection)
            anim.SetTrigger("TurnAround");

        if (move != 0f)
            lastMoveDirection = move;

        // FLIP SPRITE
        if (move < 0)
        {
            spriteRenderer.flipX = true;
            if (attackPoint != null)
            {
                Vector3 p = attackPoint.localPosition;
                p.x = -Mathf.Abs(p.x);
                attackPoint.localPosition = p;
            }
        }
        else if (move > 0)
        {
            spriteRenderer.flipX = false;
            if (attackPoint != null)
            {
                Vector3 p = attackPoint.localPosition;
                p.x = Mathf.Abs(p.x);
                attackPoint.localPosition = p;
            }
        }

        // ANIMATOR
        bool isRunning = Mathf.Abs(move) > 0.1f;
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
        if (k.jKey.wasPressedThisFrame && attackCooldownTimer <= 0f)
        {
            anim.SetTrigger("Attack");
            audioSource.PlayOneShot(swordSlash);
            attackCooldownTimer = attackCooldown;
            // Damage is applied mid-animation via PerformAttack() called from an Animation Event,
            // OR immediately here if you don't have animation events set up yet.
            PerformAttack();
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        if (jumpQueued)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpQueued = false;
        }
    }

    // ---------------------------------------------------------------
    //  Attack
    // ---------------------------------------------------------------

    /// <summary>
    /// Called directly on key press, OR can be called from an Animation Event
    /// at the frame the weapon connects for perfect timing.
    /// </summary>
    public void PerformAttack()
    {
       float facing = spriteRenderer.flipX ? -1f : 1f;
        Vector2 origin = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + Vector2.right * facing * attackRange * 0.75f;

        // Use layerMask if assigned, otherwise hit all layers and filter by component
        Collider2D[] hits = enemyLayers.value != 0
            ? Physics2D.OverlapCircleAll(origin, attackRange, enemyLayers)
            : Physics2D.OverlapCircleAll(origin, attackRange);
        foreach (Collider2D hit in hits)
        {
            BossBehaviour boss = hit.GetComponent<BossBehaviour>();
            if (boss != null)
                boss.TakeDamage(attackDamage);
        }
    }

    // ---------------------------------------------------------------
    //  Health / Damage
    // ---------------------------------------------------------------

    public void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0f || isDead) return;

        currentHealth -= damage;
        invincibilityTimer = invincibilityDuration;

        Debug.Log($"Player took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        // TODO: trigger game-over flow here
        Debug.Log("Player died.");
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

    // Draw the attack hitbox in the Scene view for easy positioning
    void OnDrawGizmosSelected()
    {
        float facing = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        Vector2 origin = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + Vector2.right * facing * attackRange * 0.75f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, attackRange);
    }
}