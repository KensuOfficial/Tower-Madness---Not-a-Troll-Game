using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndManager : MonoBehaviour
{
    public static LevelEndManager Instance;

    [Header("UI Canvas Panel References")]
    [SerializeField] private CanvasGroup globalFadeCanvasGroup;
    [SerializeField] private GameObject levelCompleteOptionsMenu;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.8f;
    [Tooltip("Type the exact scene name here (Case-Sensitive, No extension)")]
    [SerializeField] private string nextLevelSceneName = "Level_02";

    private bool isEndingLevel = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (levelCompleteOptionsMenu != null)
        {
            levelCompleteOptionsMenu.SetActive(false);
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

        // 1. HARD TIME SCALE FREEZE
        // This stops your player script loops, rigidbodies, and animators dead in their tracks!
        Time.timeScale = 0f;

        // 2. FORCE STOP PLAYER AUDIO CHANNELS IMMEDIATELY
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            AudioSource[] playerAudioChannels = player.GetComponents<AudioSource>();
            foreach (AudioSource source in playerAudioChannels)
            {
                if (source != null) source.Stop(); // Kills any running footsteps or jump clips instantly
            }
        }

        // 3. FADE CAMERA TO SOLID BLACK (Must use Realtime because timeScale is 0!)
        if (globalFadeCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime; // Ignores the game freeze to process the fade animations
                globalFadeCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            globalFadeCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        // 4. ACTIVATE OPTIONS WINDOW OVERLAY
        if (levelCompleteOptionsMenu != null)
        {
            levelCompleteOptionsMenu.SetActive(true);
        }

        // 5. FADE BLACK BACK OUT
        if (globalFadeCanvasGroup != null)
        {
            float timer = fadeDuration;
            while (timer > 0f)
            {
                timer -= Time.unscaledDeltaTime;
                globalFadeCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            globalFadeCanvasGroup.alpha = 0f;
        }
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        // This automatically pulls the index number of whatever scene is currently open, 
        // adds 1 to it, and loads whatever file is resting next in your Build Settings window list!
        int nextSceneBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Safe check to ensure we aren't trying to load past the end of your build list profiles
        if (nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
        else
        {
            Debug.LogError("There is no scene following this one inside your Build Settings window list!");
        }
    }


    public void GoToHomeMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
