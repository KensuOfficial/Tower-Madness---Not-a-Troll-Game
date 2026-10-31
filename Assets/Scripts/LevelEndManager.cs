using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndManager : MonoBehaviour
{
    public static LevelEndManager Instance;

    [Header("UI Canvas Panel References")]
    public CanvasGroup globalFadeCanvasGroup;
    private GameObject levelCompleteOptionsMenu; // Set dynamically by LevelSceneConnector

    private bool isEndingLevel = false;

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

    // Called automatically by LevelSceneConnector on scene boot
    public void RegisterSceneMenu(GameObject sceneMenu)
    {
        isEndingLevel = false; // Reset level progression state barrier checks completely
        levelCompleteOptionsMenu = sceneMenu;

        if (levelCompleteOptionsMenu != null)
        {
            levelCompleteOptionsMenu.SetActive(false); // Force hide it by default
        }
    }

    public void ShowLevelCompleteScreen()
    {
        if (isEndingLevel) return;
        StartCoroutine(ExecuteLevelCompleteSequence());
    }

    private IEnumerator ExecuteLevelCompleteSequence()
    {
        isEndingLevel = true;

        // 1. PAUSE GLOBAL TIME
        Time.timeScale = 0f;

        // 2. FORCE STOP PLAYER AUDIO
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            AudioSource[] playerAudioChannels = player.GetComponents<AudioSource>();
            foreach (AudioSource source in playerAudioChannels)
            {
                if (source != null) source.Stop();
            }
        }

        // 3. REUSE THE DYNAMIC FADE OVERLAY SYSTEM
        if (GameManager.Instance != null && GameManager.Instance.fadeCanvasGroup != null)
        {
            globalFadeCanvasGroup = GameManager.Instance.fadeCanvasGroup;
        }

        // 4. FADE CAMERA TO SOLID BLACK
        if (globalFadeCanvasGroup != null)
        {
            float timer = 0f;
            float fadeDuration = GameManager.Instance != null ? GameManager.Instance.fadeDuration : 0.5f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                globalFadeCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            globalFadeCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        // 5. ACTIVATE THE SELECTION OVERLAY MENU
        if (levelCompleteOptionsMenu != null)
        {
            levelCompleteOptionsMenu.SetActive(true);
        }

        // 6. FADE BLACK BACK OUT
        if (globalFadeCanvasGroup != null)
        {
            float timer = GameManager.Instance != null ? GameManager.Instance.fadeDuration : 0.5f;
            float maxTime = timer;
            while (timer > 0f)
            {
                timer -= Time.unscaledDeltaTime;
                globalFadeCanvasGroup.alpha = timer / maxTime;
                yield return null;
            }
            globalFadeCanvasGroup.alpha = 0f;
        }
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextSceneBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            // === SAVE PROGRESS UNLOCK ENGINE ===
            // Compares and updates the player data tracker so the main menu knows what buttons to wake up!
            int currentSaveIndex = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);
            if (nextSceneBuildIndex > currentSaveIndex)
            {
                PlayerPrefs.SetInt("HighestUnlockedLevel", nextSceneBuildIndex);
                PlayerPrefs.Save(); // Locks the value firmly onto your system hard drive disk memory profiles
            }
            // ===================================

            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }

    public void GoToHomeMenu()
    {
        StartCoroutine(ExecuteWinMenuHomeFade());
    }

    private IEnumerator ExecuteWinMenuHomeFade()
    {
        // 1. Fetch active canvas tracking references
        if (GameManager.Instance != null && GameManager.Instance.fadeCanvasGroup != null)
        {
            globalFadeCanvasGroup = GameManager.Instance.fadeCanvasGroup;
        }

        // 2. Process fade animation layers up to solid darkness
        if (globalFadeCanvasGroup != null)
        {
            float timer = 0f;
            float duration = GameManager.Instance != null ? GameManager.Instance.fadeDuration : 0.5f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                globalFadeCanvasGroup.alpha = timer / duration;
                yield return null;
            }
            globalFadeCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(0.1f);

        // 3. Clear clocks and route back home
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

}
