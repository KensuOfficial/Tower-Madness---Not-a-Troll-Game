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
    private bool isVisualScareActive = false;
    private MonoBehaviour playerMovementScript;

    // Track active background audio sequences so we can kill them instantly on overlap
    private Coroutine activeAudioCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (fullscreenUIPanel != null) fullscreenUIPanel.SetActive(false);
        if (customizedUIPanel != null) customizedUIPanel.SetActive(false);

        if (videoUIPanel != null)
        {
            videoPlayerProcessor = videoUIPanel.GetComponent<VideoPlayer>();
            if (videoPlayerProcessor != null && videoPlayerProcessor.targetTexture != null)
            {
                videoPlayerProcessor.targetTexture.Release();
            }
            videoUIPanel.SetActive(false);
        }
    }

    public void TriggerScare(VisualType type, GameObject customUIPanelOverride, AudioClip soundToPlay, VideoClip videoToPlay, float duration, bool pauseGame, bool muteBGM)
    {
        // 1. IF AUDIO ONLY: Track and execute separately
        if (type == VisualType.AudioOnly)
        {
            // If another ambient audio was already running, stop it first before starting a new one
            if (activeAudioCoroutine != null) StopCoroutine(activeAudioCoroutine);
            activeAudioCoroutine = StartCoroutine(ExecuteAudioOnlyQueue(soundToPlay, duration, muteBGM));
            return;
        }

        // 2. IF VISUAL OVERRIDE: Instantly kill any running ambient track so they never overlap!
        if (activeAudioCoroutine != null)
        {
            StopCoroutine(activeAudioCoroutine);
            activeAudioCoroutine = null;
        }
        if (jumpscareAudioSource != null && jumpscareAudioSource.isPlaying)
        {
            jumpscareAudioSource.Stop(); // Cleanly cuts the running audio tail right here
        }

        // Prevent dual visual clip overlaps
        if (isVisualScareActive) return;
        StartCoroutine(ExecuteVisualScareSequence(type, customUIPanelOverride, soundToPlay, videoToPlay, duration, pauseGame, muteBGM));
    }

    private IEnumerator ExecuteAudioOnlyQueue(AudioClip soundToPlay, float duration, bool muteBGM)
    {
        if (muteBGM && backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause();
        }

        if (jumpscareAudioSource != null && soundToPlay != null)
        {
            // Assign clip directly to source instead of using PlayOneShot so .Stop() works reliably
            jumpscareAudioSource.clip = soundToPlay;
            jumpscareAudioSource.Play();
        }

        yield return new WaitForSeconds(duration);

        if (jumpscareAudioSource != null)
        {
            jumpscareAudioSource.Stop();
        }

        if (muteBGM && backgroundMusicSource != null)
        {
            backgroundMusicSource.UnPause();
        }

        activeAudioCoroutine = null;
    }

    private IEnumerator ExecuteVisualScareSequence(VisualType type, GameObject customUIPanelOverride, AudioClip soundToPlay, VideoClip videoToPlay, float duration, bool pauseGame, bool muteBGM)
    {
        isVisualScareActive = true;

        GameObject targetUIPanel = null;
        if (type == VisualType.FullscreenImage) targetUIPanel = fullscreenUIPanel;
        else if (type == VisualType.CustomizedAreaImage) targetUIPanel = (customUIPanelOverride != null) ? customUIPanelOverride : customizedUIPanel;
        else if (type == VisualType.VideoClip) targetUIPanel = videoUIPanel;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (pauseGame && player != null)
        {
            playerMovementScript = player.GetComponent("PlayerMovement") as MonoBehaviour;
            if (playerMovementScript != null) playerMovementScript.enabled = false;
        }

        if (muteBGM && backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause();
        }

        // If playing a video clip, calculate duration dynamically if set to 0
        float finalDuration = duration;
        if (type == VisualType.VideoClip && videoPlayerProcessor != null && videoToPlay != null)
        {
            videoPlayerProcessor.clip = videoToPlay;
            targetUIPanel.SetActive(true);
            videoPlayerProcessor.Play();

            // SCRIPT MAGIC: If duration in trigger is 0, match video clip perfectly
            if (finalDuration <= 0f) finalDuration = (float)videoToPlay.length;
        }
        else if (targetUIPanel != null)
        {
            targetUIPanel.SetActive(true);
        }

        if (jumpscareAudioSource != null && soundToPlay != null)
        {
            jumpscareAudioSource.clip = soundToPlay;
            jumpscareAudioSource.Play();
        }

        if (pauseGame) Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(finalDuration);

        if (pauseGame) Time.timeScale = 1f;

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

        if (pauseGame && playerMovementScript != null) playerMovementScript.enabled = true;

        isVisualScareActive = false;
    }
}
