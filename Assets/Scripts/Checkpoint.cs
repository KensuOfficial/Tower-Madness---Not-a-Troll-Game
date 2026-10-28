using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public enum CheckpointType { Standard, Fake, MovingTroll }

    [Header("Checkpoint Type")]
    public CheckpointType type = CheckpointType.Standard;

    [Header("Visual Feedback")]
    public Sprite activatedSprite;
    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    [Header("Moving Troll Settings")]
    public Vector3 moveOffset = new Vector3(-5f, 0f, 0f); // Moves backward by default
    public float moveSpeed = 10f;
    private bool shouldMove = false;
    private Vector3 targetPosition;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position + moveOffset;
    }

    private void Update()
    {
        // If it's a moving troll checkpoint, run away when triggered
        if (type == CheckpointType.MovingTroll && shouldMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Make sure only the player triggers it
        if (collision.CompareTag("Player") && !isActivated)
        {
            HandleCheckpointBehavior();
        }
    }

    private void HandleCheckpointBehavior()
    {
        switch (type)
        {
            case CheckpointType.Standard:
                ActivateCheckpoint();
                break;

            case CheckpointType.Fake:
                // TROLL: Looks like it activates, but doesn't actually update the respawn point!
                TriggerVisuals();
                Debug.Log("Haha! Fake checkpoint triggered.");
                break;

            case CheckpointType.MovingTroll:
                // TROLL: The checkpoint flees from the player before they can touch its center
                shouldMove = true;
                Debug.Log("The checkpoint is running away!");
                break;
        }
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;
        TriggerVisuals();

        // Send this checkpoint's position to the GameManager
        GameManager.Instance.UpdateCheckpoint(transform.position);
    }

    private void TriggerVisuals()
    {
        if (spriteRenderer != null && activatedSprite != null)
        {
            spriteRenderer.sprite = activatedSprite;
        }
    }
}
