using System.Collections; // Required for Coroutines
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

    [Header("UI Overlay Reference")]
    public GameObject pauseMenuPanel;

    private bool isPaused = false;
    private MonoBehaviour playerMovementScript;
    private bool isExiting = false;

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
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartLevel();
        }

        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isExiting) return; // Block pausing if we are already transitioning out

        isPaused = !isPaused;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && playerMovementScript == null)
        {
            playerMovementScript = player.GetComponent("PlayerMovement") as MonoBehaviour;
        }

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            Debug.Log("Game Paused!");
        }
        else
        {
            Time.timeScale = 1f;
            if (playerMovementScript != null) playerMovementScript.enabled = true;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
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

    // === UPDATED: EXIT TO HOME WITH SMOOTH FADE ===
    public void GoToHomeMenu()
    {
        if (isExiting) return;
        StartCoroutine(ExecuteHomeFadeSequence());
    }

    private IEnumerator ExecuteHomeFadeSequence()
    {
        isExiting = true;
        isPaused = false;

        // 1. Hide the pause menu graphic pop-up card so only the black fade shows
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // 2. Fetch the current scene's active FadeOverlay via the GameManager
        CanvasGroup fadeOverlay = null;
        if (GameManager.Instance != null)
        {
            fadeOverlay = GameManager.Instance.fadeCanvasGroup;
        }

        // 3. Run the smooth fade transition to solid black
        if (fadeOverlay != null)
        {
            float timer = 0f;
            float duration = GameManager.Instance != null ? GameManager.Instance.fadeDuration : 0.5f;

            while (timer < duration)
            {
                // Must use unscaledDeltaTime because Time.timeScale is frozen at 0f!
                timer += Time.unscaledDeltaTime;
                fadeOverlay.alpha = timer / duration;
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(0.1f); // Brief processing pause

        // 4. Critical: Unfreeze the global game clock engine completely BEFORE scene swapping
        Time.timeScale = 1f;
        isExiting = false;

        // 5. Load the title screen asset at Build Index 0
        Debug.Log("Exiting level. Loading MainMenuScene (Build Index 0)...");
        SceneManager.LoadScene(0);
    }
}
