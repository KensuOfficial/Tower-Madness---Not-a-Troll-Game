using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public enum SpikeType { Static, CustomizableMove, Falling, Continuous, PlayerChaser, FakeMoveToSafeBlock }
    public enum TriggerCondition { InstantOnRadius, JustOnce, OnPlayerExit }

    [Header("Trap Configuration")]
    public SpikeType trapType = SpikeType.Static;
    public TriggerCondition triggerCondition = TriggerCondition.InstantOnRadius;

    [Header("Move / Fall Settings")]
    public Vector2 moveDirection = Vector2.up;
    public float movementSpeed = 5f;
    public float activationDistance = 4f;
    public float maxMoveDistance = 5f;
    private float distanceTraveled = 0f;

    [Header("Continuous Spikes Settings")]
    [Tooltip("How many total spikes to pop up sequentially in a line")]
    public int popCount = 5;
    [Tooltip("Distance spacing between each sequential popping spike")]
    public float popSpacing = 0.5f;
    [Tooltip("Time delay in seconds between each successive spike pop")]
    public float popDelay = 0.15f;
    private bool isPoppingSequenceActive = false;

    [Header("Player Chaser Settings")]
    public float chaseSpeed = 3f;

    [Header("Fake Move / Troll Settings")]
    public Transform safeBlockTarget;
    public float trollMoveSpeed = 15f;

    private Transform player;
    private bool isTriggered = false;
    private bool playerWasInRadius = false;
    private Vector3 initialPosition;
    private SpriteRenderer mainSpriteRenderer;
    private Collider2D mainCollider;

    private void Start()
    {
        initialPosition = transform.position;
        distanceTraveled = 0f;

        mainSpriteRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerCurrentlyInRadius = distanceToPlayer <= activationDistance;

        // --- UNIVERSAL TRIGGER CONDITION ENGINE ---
        bool shouldExecuteTrapBehavior = false;

        switch (triggerCondition)
        {
            case TriggerCondition.InstantOnRadius:
                if (playerCurrentlyInRadius)
                {
                    shouldExecuteTrapBehavior = true;
                    isTriggered = true;
                }
                break;

            case TriggerCondition.JustOnce:
                if (!isTriggered && playerCurrentlyInRadius)
                {
                    isTriggered = true;
                }
                if (isTriggered)
                {
                    shouldExecuteTrapBehavior = true;
                }
                break;

            case TriggerCondition.OnPlayerExit:
                if (playerCurrentlyInRadius)
                {
                    playerWasInRadius = true;
                }
                if (playerWasInRadius && !playerCurrentlyInRadius)
                {
                    isTriggered = true;
                    shouldExecuteTrapBehavior = true;
                }
                break;
        }

        if (shouldExecuteTrapBehavior)
        {
            ExecuteTrapMovement(distanceToPlayer);
        }
    }

    private void ExecuteTrapMovement(float distanceToPlayer)
    {
        switch (trapType)
        {
            case SpikeType.CustomizableMove:
            case SpikeType.Falling:
                if (distanceTraveled < maxMoveDistance)
                {
                    Vector3 translation = moveDirection.normalized * movementSpeed * Time.deltaTime;
                    transform.Translate(translation);
                    distanceTraveled = Vector3.Distance(initialPosition, transform.position);
                }
                break;

            case SpikeType.Continuous:
                if (!isPoppingSequenceActive)
                {
                    StartCoroutine(SpawnContinuousPoppingSequence());
                }
                break;

            case SpikeType.PlayerChaser:
                float directionX = Mathf.Sign(player.position.x - transform.position.x);
                transform.Translate(new Vector3(directionX * chaseSpeed * Time.deltaTime, 0, 0));
                break;

            case SpikeType.FakeMoveToSafeBlock:
                if (safeBlockTarget != null)
                {
                    transform.position = Vector3.MoveTowards(transform.position, safeBlockTarget.position, trollMoveSpeed * Time.deltaTime);
                }
                break;
        }
    }

    private IEnumerator SpawnContinuousPoppingSequence()
    {
        isPoppingSequenceActive = true;
        Vector3 spawnStepVector = (Vector3)moveDirection.normalized * popSpacing;

        for (int i = 1; i <= popCount; i++)
        {
            yield return new WaitForSeconds(popDelay);

            Vector3 spawnPosition = transform.position + (spawnStepVector * i);
            GameObject duplicateSpike = Instantiate(gameObject, spawnPosition, transform.rotation);

            // Give clones a special temporary name to track and destroy them cleanly
            duplicateSpike.name = "ContinuousSpikeClone_Troll";

            // Remove the main master script to avoid infinite duplication glitches
            SpikeTrap cloneScript = duplicateSpike.GetComponent<SpikeTrap>();
            if (cloneScript != null) Destroy(cloneScript);

            // FIX: Add a tiny helper proxy component to the clone so it can kill the player on touch!
            duplicateSpike.AddComponent<SpikeCloneKiller>();

            // Match master visual colors perfectly
            SpriteRenderer cloneSprite = duplicateSpike.GetComponent<SpriteRenderer>();
            if (cloneSprite != null && mainSpriteRenderer != null)
            {
                cloneSprite.sprite = mainSpriteRenderer.sprite;
                cloneSprite.color = mainSpriteRenderer.color;
                cloneSprite.sortingOrder = mainSpriteRenderer.sortingOrder;
            }
        }
    }

    public void ResetTrap()
    {
        StopAllCoroutines();
        isTriggered = false;
        playerWasInRadius = false;
        isPoppingSequenceActive = false;
        distanceTraveled = 0f;
        transform.position = initialPosition;

        // FIX: Find and delete all active cloned popping spikes in the scene
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "ContinuousSpikeClone_Troll")
            {
                Destroy(obj);
            }
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.RespawnPlayer();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, activationDistance);

        if (trapType == SpikeType.CustomizableMove || trapType == SpikeType.Falling)
        {
            Gizmos.color = Color.green;
            Vector3 endPosition = transform.position + (Vector3)(moveDirection.normalized * maxMoveDistance);
            Gizmos.DrawLine(transform.position, endPosition);
        }
        else if (trapType == SpikeType.Continuous)
        {
            Gizmos.color = Color.cyan;
            Vector3 finalPopPosition = transform.position + (Vector3)(moveDirection.normalized * (popCount * popSpacing));
            Gizmos.DrawLine(transform.position, finalPopPosition);
        }
    }
}

// === FIX COMPONENT FOR CLONES ===
// This tiny script automatically sits on duplicated spikes to ensure they keep tracking player death boxes!
public class SpikeCloneKiller : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.RespawnPlayer();
        }
    }
}
