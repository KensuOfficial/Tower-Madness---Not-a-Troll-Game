using UnityEngine;
using UnityEngine.UI;

public class LevelSceneConnector : MonoBehaviour
{
    [Header("UI Scene Canvas Panel References")]
    public CanvasGroup sceneFadeOverlay;
    public GameObject sceneLevelCompleteMenu;

    [Header("Separated Gameplay HUDs")]
    public GameObject scenePauseHUD;
    public GameObject sceneRestartHUD;

    [Header("Pause Overlay Menu Options")]
    [Tooltip("Drag the 'PauseMenuMenu' container object from your canvas here!")]
    public GameObject scenePauseMenuPanel;

    private void Start()
    {
        // 1. Link fade overlay
        if (GameManager.Instance != null && sceneFadeOverlay != null)
        {
            GameManager.Instance.fadeCanvasGroup = sceneFadeOverlay;
            sceneFadeOverlay.gameObject.SetActive(true);
            sceneFadeOverlay.alpha = 0f;
        }

        // 2. Link level win menu
        if (LevelEndManager.Instance != null && sceneLevelCompleteMenu != null)
        {
            LevelEndManager.Instance.RegisterSceneMenu(sceneLevelCompleteMenu);
        }

        // 3. Link and Register the Pause Overlay Panel Container
        if (PauseMenuManager.Instance != null && scenePauseMenuPanel != null)
        {
            PauseMenuManager.Instance.pauseMenuPanel = scenePauseMenuPanel;
            scenePauseMenuPanel.SetActive(false); // Ensure it starts completely hidden!
        }

        // 4. AUTO-WIRE LEVEL COMPLETE WIN BUTTONS
        if (sceneLevelCompleteMenu != null)
        {
            Button[] menuButtons = sceneLevelCompleteMenu.GetComponentsInChildren<Button>(true);
            foreach (Button btn in menuButtons)
            {
                btn.onClick.RemoveAllListeners();
                if (btn.name.ToLower().Contains("next") || btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToLower().Contains("next") == true || btn.GetComponentInChildren<Text>()?.text.ToLower().Contains("next") == true)
                {
                    btn.onClick.AddListener(() => { if (LevelEndManager.Instance != null) LevelEndManager.Instance.LoadNextLevel(); });
                }
                else if (btn.name.ToLower().Contains("home") || btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToLower().Contains("home") == true || btn.GetComponentInChildren<Text>()?.text.ToLower().Contains("home") == true)
                {
                    btn.onClick.AddListener(() => { if (LevelEndManager.Instance != null) LevelEndManager.Instance.GoToHomeMenu(); });
                }
            }
        }

        // 5. AUTO-WIRE TOP HUD PAUSE BUTTON
        WireUpHUDButton(scenePauseHUD, "pause", () =>
        {
            if (PauseMenuManager.Instance != null) PauseMenuManager.Instance.TogglePause();
        });

        // 6. AUTO-WIRE TOP HUD RESTART BUTTON
        WireUpHUDButton(sceneRestartHUD, "restart", () =>
        {
            if (PauseMenuManager.Instance != null) PauseMenuManager.Instance.RestartLevel();
        });

        // 7. AUTO-WIRE INNER PAUSE OVERLAY BUTTONS (Resume / Home)
        if (scenePauseMenuPanel != null)
        {
            Button[] pausePanelButtons = scenePauseMenuPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in pausePanelButtons)
            {
                btn.onClick.RemoveAllListeners();

                // Connect Resume/Return buttons
                if (btn.name.ToLower().Contains("resume") || btn.name.ToLower().Contains("return") || btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToLower().Contains("resume") == true)
                {
                    btn.onClick.AddListener(() => { if (PauseMenuManager.Instance != null) PauseMenuManager.Instance.TogglePause(); });
                    Debug.Log($"Wired {btn.name} to TogglePause() Resume.");
                }
                // Connect Home/Menu buttons
                else if (btn.name.ToLower().Contains("home") || btn.name.ToLower().Contains("menu") || btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToLower().Contains("home") == true)
                {
                    btn.onClick.AddListener(() => { if (PauseMenuManager.Instance != null) PauseMenuManager.Instance.GoToHomeMenu(); });
                    Debug.Log($"Wired {btn.name} to GoToHomeMenu().");
                }
            }
        }
    }

    private void WireUpHUDButton(GameObject hudTarget, string keyword, UnityEngine.Events.UnityAction actionToBind)
    {
        if (hudTarget == null) return;
        Button[] buttons = hudTarget.GetComponentsInChildren<Button>(true);
        if (buttons.Length == 0 && hudTarget.GetComponent<Button>() != null) buttons = new Button[] { hudTarget.GetComponent<Button>() };

        foreach (Button btn in buttons)
        {
            btn.onClick.RemoveAllListeners();
            if (btn.name.ToLower().Contains(keyword) || btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToLower().Contains(keyword) == true || buttons.Length == 1)
            {
                btn.onClick.AddListener(actionToBind);
            }
        }
    }
}
