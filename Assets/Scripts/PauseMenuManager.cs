using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

    [Header("UI Overlay Reference")]
    [Tooltip("The 'PauseMenuMenu' panel container object holding the Resume/Home options")]
    public GameObject pauseMenuPanel;

    private bool isPaused = false;
    private MonoBehaviour playerMovementScript;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // KEYBOARD SHORTCUT: Instant restart via R key
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartLevel();
        }

        // OPTIONAL KEYBOARD SHORTCUT: Escape/P toggles pause overlay
        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && playerMovementScript == null)
        {
            playerMovementScript = player.GetComponent("PlayerMovement") as MonoBehaviour;
        }

        if (isPaused)
        {
            Time.timeScale = 0f; // Freeze everything
            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true); // Pop open overlay
            Debug.Log("Game Paused!");
        }
        else
        {
            Time.timeScale = 1f; // Unfreeze
            if (playerMovementScript != null) playerMovementScript.enabled = true;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false); // Hide overlay
            Debug.Log("Game Resumed!");
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToHomeMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // Troll fallback: reload current scene until home screen exists
        Debug.Log("Go Home from Pause clicked! Reloading current stage...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
