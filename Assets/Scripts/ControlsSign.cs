using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsSign : MonoBehaviour
{
    [Header("UI")]
    public GameObject pressEPrompt;
    public GameObject controlsPanel;

    [Header("Behaviour")]
    public bool pauseGameWhileOpen = true;

    private bool playerInRange = false;
    private bool panelOpen = false;

    void Start()
    {
        if (pressEPrompt   != null) pressEPrompt.SetActive(false);
        if (controlsPanel  != null) controlsPanel.SetActive(false);
    }

    void Update()
    {
        Keyboard k = Keyboard.current;
        if (k == null) return;

        // Allow toggling while panel is open even if player has walked out of range
        if (panelOpen && k.eKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (playerInRange && !panelOpen && k.eKey.wasPressedThisFrame)
            Open();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (!panelOpen && pressEPrompt != null)
            pressEPrompt.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (pressEPrompt != null) pressEPrompt.SetActive(false);
    }

    void Open()
    {
        panelOpen = true;
        if (controlsPanel != null) controlsPanel.SetActive(true);
        if (pressEPrompt  != null) pressEPrompt.SetActive(false);
        if (pauseGameWhileOpen) Time.timeScale = 0f;
    }

    void Close()
    {
        panelOpen = false;
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (playerInRange && pressEPrompt != null)
            pressEPrompt.SetActive(true);
        if (pauseGameWhileOpen) Time.timeScale = 1f;
    }
}
