using UnityEngine;

public class LevelDoor : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // EMERGENCY TEST: If ANY object overlaps this door, jump straight to the change
        GameManager.Instance.ProgressToNextFloor();
    }
}
