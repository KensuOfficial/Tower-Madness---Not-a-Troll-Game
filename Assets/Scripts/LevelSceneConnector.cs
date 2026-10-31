using UnityEngine;
using UnityEngine.UI; // Required to access the Button component functions

public class LevelSceneConnector : MonoBehaviour
{
    [Header("UI Scene Canvas Panel References")]
    [Tooltip("Drag the 'FadeOverlay' Canvas Group component from this scene's UI here.")]
    public CanvasGroup sceneFadeOverlay;

    [Tooltip("Drag the 'LevelCompleteMenu' panel object from this scene's UI here.")]
    public GameObject sceneLevelCompleteMenu;

    private void Start()
    {
        // 1. Force the GameManager to connect to this scene's black fade screen
        if (GameManager.Instance != null && sceneFadeOverlay != null)
        {
            GameManager.Instance.fadeCanvasGroup = sceneFadeOverlay;
            sceneFadeOverlay.gameObject.SetActive(true);
            sceneFadeOverlay.alpha = 0f;
        }

        // 2. Force the LevelEndManager to register this scene's win overlay menu
        if (LevelEndManager.Instance != null && sceneLevelCompleteMenu != null)
        {
            LevelEndManager.Instance.RegisterSceneMenu(sceneLevelCompleteMenu);
        }

        // 3. AUTOMATICALLY REWIRE THE SELECTION BUTTON LISTENERS (The Fix for Level 3 & Level 4!)
        if (sceneLevelCompleteMenu != null)
        {
            // Search inside the menu object for the buttons by tracking their text components or layout positions
            Button[] menuButtons = sceneLevelCompleteMenu.GetComponentsInChildren<Button>(true);

            foreach (Button btn in menuButtons)
            {
                // Clear any broken, dead references copied over from old scenes
                btn.onClick.RemoveAllListeners();

                // If the button has a name containing "Next", hook it up to the current active LevelEndManager
                if (btn.name.ToLower().Contains("next") || btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToLower().Contains("next") == true || btn.GetComponentInChildren<Text>()?.text.ToLower().Contains("next") == true)
                {
                    btn.onClick.AddListener(() =>
                    {
                        if (LevelEndManager.Instance != null) LevelEndManager.Instance.LoadNextLevel();
                    });
                    Debug.Log($"Automatically wired up {btn.name} to LoadNextLevel()!");
                }
                // If the button has a name containing "Home", hook it up to GoToHomeMenu
                else if (btn.name.ToLower().Contains("home") || btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToLower().Contains("home") == true || btn.GetComponentInChildren<Text>()?.text.ToLower().Contains("home") == true)
                {
                    btn.onClick.AddListener(() =>
                    {
                        if (LevelEndManager.Instance != null) LevelEndManager.Instance.GoToHomeMenu();
                    });
                    Debug.Log($"Automatically wired up {btn.name} to GoToHomeMenu()!");
                }
            }
        }
    }
}