using UnityEngine;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer chestRenderer;
    public Sprite closedSprite;
    public Sprite openSprite;
    public GameObject pressEPrompt;

    [Header("Loot")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;
    public int coinDrops = 5;
    [Range(0f, 1f)] public float heartDropChance = 0.5f;
    public float dropScatter = 0.6f;
    public float dropUpwardForce = 4f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;

    [Header("Victory (boss chest only)")]
    public bool triggersVictory = false;
    public float victoryDelay = 2f;

    private bool playerInRange = false;
    private bool opened = false;

    void Start()
    {
        if (chestRenderer != null && closedSprite != null)
            chestRenderer.sprite = closedSprite;
        if (pressEPrompt != null)
            pressEPrompt.SetActive(false);
    }

    void Update()
    {
        if (opened || !playerInRange) return;

        Keyboard k = Keyboard.current;
        if (k != null && k.eKey.wasPressedThisFrame)
            Open();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (opened) return;
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (pressEPrompt != null) pressEPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (pressEPrompt != null) pressEPrompt.SetActive(false);
    }

    void Open()
    {
        opened = true;
        if (chestRenderer != null && openSprite != null)
            chestRenderer.sprite = openSprite;
        if (pressEPrompt != null)
            pressEPrompt.SetActive(false);
        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        SpawnLoot();

        if (triggersVictory)
            Invoke(nameof(ShowVictoryDelayed), victoryDelay);
    }

    void ShowVictoryDelayed()
    {
        var menu = FindObjectOfType<GameMenuManager>();
        if (menu != null) menu.ShowVictory();
    }

    void SpawnLoot()
    {
        for (int i = 0; i < coinDrops; i++)
        {
            if (coinPrefab == null) break;
            Vector2 offset = new Vector2(Random.Range(-dropScatter, dropScatter), Random.Range(0.2f, dropScatter));
            var coin = Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            var rb = coin.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), dropUpwardForce);
        }

        if (heartPrefab != null && Random.value < heartDropChance)
        {
            Vector2 offset = new Vector2(Random.Range(-dropScatter, dropScatter), 0.3f);
            var heart = Instantiate(heartPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            var rb = heart.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = new Vector2(Random.Range(-1.5f, 1.5f), dropUpwardForce);
        }
    }
}
