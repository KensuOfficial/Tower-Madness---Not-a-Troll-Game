using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class AutoCameraFollowWithLookahead : MonoBehaviour
{
    [Header("Target & Offset")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Vertical Framing")]
    [Tooltip("Shifts the camera up or down relative to the player. Positive values push the player down on the screen so you can see higher up.")]
    [SerializeField] private float verticalOffset = 2f;

    [Header("Smoothness")]
    [Range(0.01f, 1f)][SerializeField] private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    [Header("Lookahead Settings")]
    [SerializeField] private float lookAheadDistance = 4f;
    [SerializeField] private float lookAheadSpeed = 3f;

    [Header("Auto Bounds Settings")]
    [SerializeField] private Tilemap boundsTilemap;

    private Camera cam;
    private SpriteRenderer playerSpriteRenderer;
    private float minX, maxX, minY, maxY;
    private float currentLookAheadX;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (target != null)
        {
            playerSpriteRenderer = target.GetComponent<SpriteRenderer>() ?? target.GetComponentInChildren<SpriteRenderer>();
        }

        CalculateMapBounds();
    }

    void CalculateMapBounds()
    {
        if (boundsTilemap == null) return;
        boundsTilemap.CompressBounds();
        Bounds cellBounds = boundsTilemap.localBounds;
        minX = cellBounds.min.x;
        maxX = cellBounds.max.x;
        minY = cellBounds.min.y;
        maxY = cellBounds.max.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Evaluate facing direction safely
        float facingDirection = 1f;
        if (playerSpriteRenderer != null)
        {
            facingDirection = playerSpriteRenderer.flipX ? -1f : 1f;
        }
        else
        {
            facingDirection = Mathf.Sign(target.localScale.x);
        }

        // 2. Smooth lookahead calculation
        float targetLookAheadX = facingDirection * lookAheadDistance;
        currentLookAheadX = Mathf.MoveTowards(currentLookAheadX, targetLookAheadX, Time.deltaTime * lookAheadSpeed);

        // 3. Target position with horizontal lookahead, vertical offset, and base offset
        float targetY = target.position.y + verticalOffset;
        Vector3 targetPosition = new Vector3(target.position.x + currentLookAheadX, targetY, 0f) + offset;

        // 4. Clamp to Tilemap Bounds (Dynamic aspect ratio calculation prevents edge slipping)
        if (boundsTilemap != null && cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float clampedX = Mathf.Clamp(targetPosition.x, minX + halfWidth, maxX - halfWidth);
            float clampedY = Mathf.Clamp(targetPosition.y, minY + halfHeight, maxY - halfHeight);

            targetPosition = new Vector3(clampedX, clampedY, targetPosition.z);
        }

        // 5. Smooth Follow
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}