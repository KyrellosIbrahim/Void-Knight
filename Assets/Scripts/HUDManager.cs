using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Hearts")]
    public RectTransform heartsPanel;
    public Sprite heartSprite;
    public float heartSize = 60f;
    public float heartSpacing = 8f;

    private PlayerBehaviour player;
    private Image[] heartImages;
    private Text coinText;

    private int lastHealth = -1;
    private int lastCoins  = -1;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerBehaviour>();

        if (player == null)
        {
            Debug.LogWarning("HUDManager: Player not found — HUD disabled.");
            return;
        }

        BuildHearts();
        BuildCoinLabel();
        ForceRefresh();
    }

    void Update()
    {
        if (player == null) return;

        int hp    = player.CurrentHealth;
        int coins = player.coinCount;

        if (hp != lastHealth)
        {
            RefreshHearts(hp);
            lastHealth = hp;
        }

        if (coins != lastCoins)
        {
            RefreshCoins(coins);
            lastCoins = coins;
        }
    }

    void BuildHearts()
    {
        if (heartsPanel == null)
        {
            var go = GameObject.Find("HeartsPanel");
            if (go != null) heartsPanel = go.GetComponent<RectTransform>();
        }

        if (heartsPanel == null) { Debug.LogWarning("HUDManager: HeartsPanel not found."); return; }

        // Clear any pre-existing children
        foreach (Transform child in heartsPanel)
            Destroy(child.gameObject);

        // Horizontal layout so hearts space themselves automatically
        var layout = heartsPanel.GetComponent<HorizontalLayoutGroup>()
                  ?? heartsPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing               = heartSpacing;
        layout.childAlignment        = TextAnchor.MiddleLeft;
        layout.childControlWidth     = false;
        layout.childControlHeight    = false;
        layout.childForceExpandWidth  = false;
        layout.childForceExpandHeight = false;
        layout.padding               = new RectOffset(10, 10, 8, 8);

        // Resize panel to snugly fit all hearts
        heartsPanel.sizeDelta = new Vector2(
            player.maxHealth * (heartSize + heartSpacing) - heartSpacing + 20f,
            heartSize + 16f);

        heartImages = new Image[player.maxHealth];
        for (int i = 0; i < player.maxHealth; i++)
        {
            var go = new GameObject("Heart_" + i);
            go.transform.SetParent(heartsPanel, false);
            go.AddComponent<CanvasRenderer>();

            var rt       = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(heartSize, heartSize);

            var img          = go.AddComponent<Image>();
            img.sprite       = heartSprite;
            img.preserveAspect = true;

            heartImages[i] = img;
        }
    }

    void BuildCoinLabel()
    {
        var coinsPanelGo = GameObject.Find("CoinsPanel");
        if (coinsPanelGo == null) { Debug.LogWarning("HUDManager: CoinsPanel not found."); return; }

        // Horizontal layout so icon + text sit side-by-side
        var layout = coinsPanelGo.GetComponent<HorizontalLayoutGroup>()
                  ?? coinsPanelGo.AddComponent<HorizontalLayoutGroup>();
        layout.spacing               = 8f;
        layout.childAlignment        = TextAnchor.MiddleLeft;
        layout.childControlWidth     = false;
        layout.childControlHeight    = false;
        layout.childForceExpandWidth  = false;
        layout.childForceExpandHeight = false;
        layout.padding               = new RectOffset(10, 10, 8, 8);

        // Resize existing Coin icon to match heart size
        var coinImg = coinsPanelGo.GetComponentInChildren<Image>();
        if (coinImg != null)
            coinImg.GetComponent<RectTransform>().sizeDelta = new Vector2(heartSize, heartSize);

        // Size the panel: icon + spacing + text
        coinsPanelGo.GetComponent<RectTransform>().sizeDelta =
            new Vector2(heartSize + 8f + 90f + 20f, heartSize + 16f);

        // Reuse existing Text child, or create one
        coinText = coinsPanelGo.GetComponentInChildren<Text>();
        if (coinText == null)
        {
            var textGo = new GameObject("CoinCount");
            textGo.transform.SetParent(coinsPanelGo.transform, false);

            var rt       = textGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90f, heartSize);

            coinText           = textGo.AddComponent<Text>();
            coinText.font      = GetBuiltinFont();
            coinText.fontSize  = 38;
            coinText.fontStyle = FontStyle.Bold;
            coinText.color     = Color.white;
            coinText.alignment = TextAnchor.MiddleLeft;

            // Thin drop-shadow via Outline component
            var outline       = textGo.AddComponent<Outline>();
            outline.effectColor    = new Color(0f, 0f, 0f, 0.6f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }
    void ForceRefresh()
    {
        RefreshHearts(player.CurrentHealth);
        RefreshCoins(player.coinCount);
        lastHealth = player.CurrentHealth;
        lastCoins  = player.coinCount;
    }

    void RefreshHearts(int hp)
    {
        if (heartImages == null) return;
        for (int i = 0; i < heartImages.Length; i++)
            heartImages[i].color = i < hp
                ? Color.white
                : new Color(0.15f, 0.15f, 0.15f, 0.35f);
    }

    void RefreshCoins(int coins)
    {
        if (coinText != null)
            coinText.text = "x" + coins;
    }

    static Font GetBuiltinFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
