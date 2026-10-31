using UnityEngine;

public class LevelDoor : MonoBehaviour
{
    [Header("Door Configuration")]
    [Tooltip("Check this TRUE only on the very last door of the entire multi-floor level!")]
    [SerializeField] private bool isFinalLevelExit = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (isFinalLevelExit)
            {
                if (LevelEndManager.Instance != null)
                {
                    LevelEndManager.Instance.ShowLevelCompleteScreen();
                }
            }
            else
            {
                GameManager.Instance.ProgressToNextFloor();
            }
        }
    }
}
