using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Settings")]
    public Transform playerTransform;
    private MonoBehaviour playerMovementScript;
    private Animator playerAnimator;
    private SpriteRenderer playerSpriteRenderer;

    [Header("Floor Management")]
    public Transform[] floorSpawnPoints;
    private int currentFloorIndex = 0;
    private Vector3 activeCheckpointPosition;

    [Header("UI Reference")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

    [Header("Death Settings")]
    [Tooltip("How many seconds to wait AFTER hitting a trap BEFORE the player teleports back")]
    public float respawnDelay = 1.0f;
    private bool isDying = false;

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
        if (playerTransform != null)
        {
            playerMovementScript = playerTransform.GetComponent("PlayerMovement") as MonoBehaviour;
            playerAnimator = playerTransform.GetComponent<Animator>();
            playerSpriteRenderer = playerTransform.GetComponent<SpriteRenderer>();

            if (floorSpawnPoints.Length > 0)
            {
                SetFloor(0);
            }
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
        if (isDying) return;
        StartCoroutine(ExecuteDeathSequence());
    }

    private IEnumerator ExecuteDeathSequence()
    {
        isDying = true;

        // 1. FREEZE PLAYER PHYSICS & CONTROLS
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 2. PAUSE ANIMATIONS
        if (playerAnimator != null) playerAnimator.speed = 0f;
        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.red;

        // 3. THE COOLDOWN DELAY
        yield return new WaitForSeconds(respawnDelay);

        // 4. FADE TO BLACK
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

        // === RESET ALL ACTIVE TRAPS HERE WHILE SCREEN IS BLACK ===
        SpikeTrap[] allTraps = FindObjectsByType<SpikeTrap>();

        foreach (SpikeTrap trap in allTraps)
        {
            if (trap != null)
            {
                trap.ResetTrap();
            }
        }
        // =========================================================

        // 5. TELEPORT AND RESET VISUAL ORIENTATION
        playerTransform.position = activeCheckpointPosition;
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = Color.white;
            playerSpriteRenderer.flipX = false;
        }

        // 6. SAFE ANIMATION RESET
        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
            foreach (var parameter in playerAnimator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Float)
                    playerAnimator.SetFloat(parameter.name, 0f);
                else if (parameter.type == AnimatorControllerParameterType.Bool)
                    playerAnimator.SetBool(parameter.name, false);
            }
        }

        yield return new WaitForSeconds(0.2f);

        // 7. FADE BACK IN
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

        // 8. UNFREEZE PLAYER
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
        if (playerMovementScript != null) playerMovementScript.enabled = true;

        isDying = false;
    }


    public void ProgressToNextFloor()
    {
        StartCoroutine(FadeAndAdvanceFloor());
    }

    private IEnumerator FadeAndAdvanceFloor()
    {
        // Face the exit door direction (Right) before transitioning
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.flipX = false;
        }

        if (playerAnimator != null)
        {
            // Safely clear parameters for smooth transition entry
            foreach (var parameter in playerAnimator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Float)
                    playerAnimator.SetFloat(parameter.name, 0f);
                else if (parameter.type == AnimatorControllerParameterType.Bool)
                    playerAnimator.SetBool(parameter.name, false);
            }
        }

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

        currentFloorIndex++;
        if (currentFloorIndex < floorSpawnPoints.Length)
        {
            SetFloor(currentFloorIndex);
            playerTransform.position = activeCheckpointPosition;

            Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(0.2f);

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
