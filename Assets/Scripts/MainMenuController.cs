using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainPanelContainer;
    [SerializeField] private GameObject levelSelectPanelContainer;
    [SerializeField] private GameObject optionsPanelContainer;
    [SerializeField] private GameObject creditsPanelContainer;

    [Header("Level Pages Setup")]
    [Tooltip("Create separate empty child GameObjects under LevelSelectPanel for each page (e.g., Page1, Page2) and drop them here.")]
    [SerializeField] private GameObject[] levelPages;

    [Header("Navigation Arrows")]
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;

    [Header("All Level Buttons (For Unlocking)")]
    [Tooltip("Drag ALL your Level Selection Buttons here in strict sequential build index order.")]
    [SerializeField] private Button[] allLevelButtons;

    private int currentPageIndex = 0;

    private void Start()
    {
        ShowMainPanel();
        Time.timeScale = 1f;
        ConfigureLevelSelectButtons();
    }

    private void ConfigureLevelSelectButtons()
    {
        // 1. Progressive Level Unlocks
        int highestUnlockedLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);

        for (int i = 0; i < allLevelButtons.Length; i++)
        {
            if (allLevelButtons[i] == null) continue;

            int levelBuildIndex = i + 1; // Level 1 = Index 1, Level 2 = Index 2, etc.

            if (levelBuildIndex <= highestUnlockedLevel)
            {
                allLevelButtons[i].interactable = true;
                allLevelButtons[i].onClick.RemoveAllListeners();
                allLevelButtons[i].onClick.AddListener(() => LoadTargetLevel(levelBuildIndex));
            }
            else
            {
                allLevelButtons[i].interactable = false;
            }
        }

        // 2. Refresh the visible page layout frame
        UpdatePageVisibility();
    }

    private void UpdatePageVisibility()
    {
        if (levelPages == null || levelPages.Length == 0) return;

        // Ensure index bounds stay safe
        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, levelPages.Length - 1);

        // Toggle page object active states
        for (int i = 0; i < levelPages.Length; i++)
        {
            if (levelPages[i] != null)
            {
                levelPages[i].SetActive(i == currentPageIndex);
            }
        }

        // 3. Intelligently hide/show navigation arrows based on current page positions
        if (previousPageButton != null)
        {
            previousPageButton.gameObject.SetActive(currentPageIndex > 0);
        }

        if (nextPageButton != null)
        {
            nextPageButton.gameObject.SetActive(currentPageIndex < levelPages.Length - 1);
        }
    }

    // === PUBLIC PAGINATION ARROW METHODS ===

    public void ClickNextPage()
    {
        if (currentPageIndex < levelPages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageVisibility();
        }
    }

    public void ClickPreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageVisibility();
        }
    }

    public void LoadTargetLevel(int buildIndex)
    {
        if (buildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(buildIndex);
        }
        else
        {
            Debug.LogError($"Scene index {buildIndex} out of bounds!");
        }
    }

    // === UI NAVIGATION ROUTER METHODS ===

    public void ShowMainPanel()
    {
        if (mainPanelContainer != null) mainPanelContainer.SetActive(true);
        if (levelSelectPanelContainer != null) levelSelectPanelContainer.SetActive(false);
        if (optionsPanelContainer != null) optionsPanelContainer.SetActive(false);
        if (creditsPanelContainer != null) creditsPanelContainer.SetActive(false);
    }

    public void ShowLevelSelectPanel()
    {
        if (mainPanelContainer != null) mainPanelContainer.SetActive(false);
        if (levelSelectPanelContainer != null) levelSelectPanelContainer.SetActive(true);

        currentPageIndex = 0; // Default back to Page 1 upon entering the menu
        ConfigureLevelSelectButtons();
    }

    public void ShowOptionsPanel()
    {
        if (mainPanelContainer != null) mainPanelContainer.SetActive(false);
        if (optionsPanelContainer != null) optionsPanelContainer.SetActive(true);
    }

    public void ShowCreditsPanel()
    {
        if (mainPanelContainer != null) mainPanelContainer.SetActive(false);
        if (creditsPanelContainer != null) creditsPanelContainer.SetActive(true);
    }

    public void ResetAllProgressSaveData()
    {
        PlayerPrefs.DeleteKey("HighestUnlockedLevel");
        currentPageIndex = 0;
        ConfigureLevelSelectButtons();
        Debug.Log("Progress purged!");
    }
}
