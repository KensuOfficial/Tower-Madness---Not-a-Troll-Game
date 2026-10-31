using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        // SINGLETON CLEANUP
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to Unity's scene loading system event matrix
            SceneManager.sceneLoaded += OnSceneLoadedRefresh;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        // Unsubscribe when destroyed to avoid memory leak tracking errors
        SceneManager.sceneLoaded -= OnSceneLoadedRefresh;
    }
    // THIS METHOD AUTOMATICALLY RUNS THE FRAME A NEW SCENE LOADS
    private void OnSceneLoadedRefresh(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("New Scene Loaded: " + scene.name + ". Refreshing Manager Connections!");

        // 1. Automatically find the new Level's Player frog via Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerMovementScript = playerTransform.GetComponent("PlayerMovement") as MonoBehaviour;
            playerAnimator = playerTransform.GetComponent<Animator>();
            playerSpriteRenderer = playerTransform.GetComponent<SpriteRenderer>();
        }

        // 2. Automatically locate the new Level's Canvas Group Fade Overlay
        // FIX: Find the overlay even if your level designer turned it off by default!
        fadeCanvasGroup = null; // Clear out old Level 1 reference residue

        CanvasGroup[] allCanvasGroupsInScene = Resources.FindObjectsOfTypeAll<CanvasGroup>();
        foreach (CanvasGroup group in allCanvasGroupsInScene)
        {
            // Specifically verify name matches your layout file asset structure
            if (group.gameObject.name == "FadeOverlay")
            {
                fadeCanvasGroup = group;

                // Safety Enforcement: Make sure the image itself is active, 
                // but its Alpha transparency opacity is set to invisible (0) on boot!
                fadeCanvasGroup.gameObject.SetActive(true);
                fadeCanvasGroup.alpha = 0f;
                break;
            }
        }

        if (fadeCanvasGroup == null)
        {
            Debug.LogError("GameManager failed to locate an object named 'FadeOverlay' in the new scene!");
        }
        
        // 3. Reset floor indices so Level 2 doesn't look for Floor 4!
        currentFloorIndex = 0;

        // 4. Locate your new floor spawn markers dynamically inside the new scene layout
        // To make this robust for your level designer, we look for objects named Floor1_Start, etc.
        GameObject f1 = GameObject.Find("Floor1_Start");
        GameObject f2 = GameObject.Find("Floor2_Start");
        GameObject f3 = GameObject.Find("Floor3_Start");

        int count = 0;
        if (f1 != null) count++;
        if (f2 != null) count++;
        if (f3 != null) count++;

        if (count > 0)
        {
            floorSpawnPoints = new Transform[count];
            int idx = 0;
            if (f1 != null) floorSpawnPoints[idx++] = f1.transform;
            if (f2 != null) floorSpawnPoints[idx++] = f2.transform;
            if (f3 != null) floorSpawnPoints[idx++] = f3.transform;

            SetFloor(0);
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
