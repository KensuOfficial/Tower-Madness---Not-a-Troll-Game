using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance;

    public enum VisualType { FullscreenImage, CustomizedAreaImage, VideoClip, AudioOnly }

    [Header("UI Panel Containers")]
    [SerializeField] private GameObject fullscreenUIPanel;
    [SerializeField] private GameObject customizedUIPanel;
    [SerializeField] private GameObject videoUIPanel;

    [Header("Audio Components")]
    [SerializeField] private AudioSource jumpscareAudioSource;
    [SerializeField] private AudioSource backgroundMusicSource;

    private VideoPlayer videoPlayerProcessor;
    private bool isScaring = false;
    private MonoBehaviour playerMovementScript;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Clear out visibility loops by default
        if (fullscreenUIPanel != null) fullscreenUIPanel.SetActive(false);
        if (customizedUIPanel != null) customizedUIPanel.SetActive(false);

        if (videoUIPanel != null)
        {
            videoPlayerProcessor = videoUIPanel.GetComponent<VideoPlayer>();

            // Clear memory buffer artifacts immediately on boot
            if (videoPlayerProcessor != null && videoPlayerProcessor.targetTexture != null)
            {
                videoPlayerProcessor.targetTexture.Release();
            }

            videoUIPanel.SetActive(false);
        }
    }

    public void TriggerScare(VisualType type, GameObject customUIPanelOverride, AudioClip soundToPlay, VideoClip videoToPlay, float duration, bool pauseGame, bool muteBGM)
    {
        if (isScaring) return;
        StartCoroutine(ExecuteScareSequence(type, customUIPanelOverride, soundToPlay, videoToPlay, duration, pauseGame, muteBGM));
    }

    private IEnumerator ExecuteScareSequence(VisualType type, GameObject customUIPanelOverride, AudioClip soundToPlay, VideoClip videoToPlay, float duration, bool pauseGame, bool muteBGM)
    {
        isScaring = true;

        // 1. SELECT TARGET OVERLAY WINDOW Medium
        GameObject targetUIPanel = null;
        if (type == VisualType.FullscreenImage) targetUIPanel = fullscreenUIPanel;
        else if (type == VisualType.CustomizedAreaImage) targetUIPanel = (customUIPanelOverride != null) ? customUIPanelOverride : customizedUIPanel;
        else if (type == VisualType.VideoClip) targetUIPanel = videoUIPanel;

        // 2. DISABLE PLAYER CONTROLS SEPARATELY (Prevents buffered input glitches)
        // FIX: Only disable the player controls if pauseGame is actually checked true!
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (pauseGame && player != null)
        {
            playerMovementScript = player.GetComponent("PlayerMovement") as MonoBehaviour;
            if (playerMovementScript != null) playerMovementScript.enabled = false;
        }


        // 3. MUTE BACKGROUND MUSIC (Optional)
        if (muteBGM && backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause();
        }

        // 4. ACTIVATE VISUALS & BLAST AUDIO CLIP
        if (type == VisualType.VideoClip && videoPlayerProcessor != null && videoToPlay != null)
        {
            videoPlayerProcessor.clip = videoToPlay;
            targetUIPanel.SetActive(true);
            videoPlayerProcessor.Play();
        }
        else if (targetUIPanel != null)
        {
            targetUIPanel.SetActive(true);
        }

        if (jumpscareAudioSource != null && soundToPlay != null)
        {
            jumpscareAudioSource.PlayOneShot(soundToPlay);
        }

        // 5. PAUSE THE GLOBAL PHYSICS GAME CLOCK (Optional)
        if (pauseGame)
        {
            Time.timeScale = 0f; // Everything drops to 0 FPS physics calculation
        }

        // 6. DELAY COUNTDOWN (Must use WaitForSecondsRealtime since standard Time clock is frozen!)
        yield return new WaitForSecondsRealtime(duration);

        // 7. RESTORE GLOBAL TIME CLOCK VELOCITIES
        if (pauseGame)
        {
            Time.timeScale = 1f; // Game clock smoothly kicks back into full speed motion
        }

        // 8. TEARDOWN SEQUENCE AND SYSTEM STATE RECOVERY
        if (type == VisualType.VideoClip && videoPlayerProcessor != null)
        {
            videoPlayerProcessor.Stop();
        }

        if (targetUIPanel != null) targetUIPanel.SetActive(false);
        if (customUIPanelOverride != null) customUIPanelOverride.SetActive(false);

        if (muteBGM && backgroundMusicSource != null)
        {
            backgroundMusicSource.UnPause();
        }

        // Re-enable input commands seamlessly only if they were disabled
        if (pauseGame && playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
    }
}
