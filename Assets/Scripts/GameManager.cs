using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Settings")]
    public Transform playerTransform;

    [Header("Floor Management")]
    public Transform[] floorSpawnPoints;
    private int currentFloorIndex = 0;
    private Vector3 activeCheckpointPosition;

    [Header("UI Reference")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

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

    private void Start()
    {
        if (playerTransform != null && floorSpawnPoints.Length > 0)
        {
            SetFloor(0);
        }
    }

    public void SetFloor(int floorIndex)
    {
        currentFloorIndex = Mathf.Clamp(floorIndex, 0, floorSpawnPoints.Length - 1);
        activeCheckpointPosition = floorSpawnPoints[currentFloorIndex].position;
    }

    public void UpdateCheckpoint(Vector3 newCheckpointPosition)
    {
        activeCheckpointPosition = newCheckpointPosition;
    }

    public void RespawnPlayer()
    {
        if (playerTransform != null)
        {
            playerTransform.position = activeCheckpointPosition;
            Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    public void ProgressToNextFloor()
    {
        Debug.Log("ProgressToNextFloor was called successfully!");
        StartCoroutine(FadeAndAdvanceFloor());
    }

    private IEnumerator FadeAndAdvanceFloor()
    {
        // 1. Fade out to black safely
        if (fadeCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        // 2. FORCE TELEPORT (This runs even if the Canvas layout above has an issue!)
        currentFloorIndex++;
        Debug.Log("Moving Player to Floor Index: " + currentFloorIndex);

        if (currentFloorIndex < floorSpawnPoints.Length)
        {
            SetFloor(currentFloorIndex);
            RespawnPlayer();
        }
        else
        {
            Debug.Log("Level Complete! Reached the final floor.");
        }

        yield return new WaitForSeconds(0.2f);

        // 3. Fade back in safely
        if (fadeCanvasGroup != null)
        {
            float timer = fadeDuration;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                fadeCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }
    }
}
