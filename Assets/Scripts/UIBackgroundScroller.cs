using UnityEngine;
using UnityEngine.UI; // Required for Raw Image components

public class UIBackgroundScroller : MonoBehaviour
{
    [Header("Scroll Speed Configuration")]
    [Tooltip("Adjust how fast this specific layer crawls horizontally. Farther layers should be slower!")]
    [SerializeField] private float scrollSpeedX = 0.05f;
    [SerializeField] private float scrollSpeedY = 0f; // Leave at 0 unless you want it moving vertically too

    private RawImage targetRawImage;
    private Rect currentTextureUV;

    private void Start()
    {
        // Capture the Raw Image component attached to this canvas layer object
        targetRawImage = GetComponent<RawImage>();

        if (targetRawImage == null)
        {
            Debug.LogError($"UIBackgroundScroller on {gameObject.name} requires a RawImage component, not a standard Image!");
        }
    }

    private void Update()
    {
        if (targetRawImage == null) return;

        // Pull the current UV texture wrapping coordinate layout box boundaries
        currentTextureUV = targetRawImage.uvRect;

        // Shift the UV coordinate position linearly over time using real-time delta speed clocks
        // Using Time.unscaledDeltaTime ensures it keeps scrolling beautifully even if the game is paused!
        currentTextureUV.x += scrollSpeedX * Time.unscaledDeltaTime;
        currentTextureUV.y += scrollSpeedY * Time.unscaledDeltaTime;

        // Feed the updated wrapping coordinates back to the canvas UI texture drawer
        targetRawImage.uvRect = currentTextureUV;
    }
}
