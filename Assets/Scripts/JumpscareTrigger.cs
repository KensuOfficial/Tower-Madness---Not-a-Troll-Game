using UnityEngine;
using UnityEngine.Video;

public class JumpscareTrigger : MonoBehaviour
{
    public enum SpawnCondition { RunOnce, ChanceBased }

    [Header("Activation Setup")]
    [SerializeField] private SpawnCondition activationType = SpawnCondition.RunOnce;
    [Range(0f, 100f)][SerializeField] private float triggerChance = 100f;

    [Header("Custom Troll Profiles")]
    [SerializeField] private JumpscareManager.VisualType style = JumpscareManager.VisualType.FullscreenImage;
    [SerializeField] private float displayDuration = 1.5f;

    [Header("Media Slots")]
    [SerializeField] private AudioClip soundClip;
    [SerializeField] private VideoClip videoClip;

    [Header("Troll Behavior Rules")]
    [Tooltip("CHECK THIS to freeze physics, traps, and movement during the scare screen!")]
    [SerializeField] private bool pauseGameWorld = true;
    [SerializeField] private bool muteBackgroundMusic = true;

    [Header("Customized Area Mode Only")]
    [SerializeField] private GameObject customUIPanelElement;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (activationType == SpawnCondition.RunOnce && hasTriggered) return;
            EvaluateTrigger();
        }
    }

    private void EvaluateTrigger()
    {
        if (activationType == SpawnCondition.RunOnce) hasTriggered = true;
        else if (activationType == SpawnCondition.ChanceBased)
        {
            if (Random.Range(0f, 100f) > triggerChance) return;
        }

        // Fire parameters over to real-time processor loop engines
        JumpscareManager.Instance.TriggerScare(style, customUIPanelElement, soundClip, videoClip, displayDuration, pauseGameWorld, muteBackgroundMusic);

        if (activationType == SpawnCondition.RunOnce)
        {
            Destroy(gameObject);
        }
    }
}
