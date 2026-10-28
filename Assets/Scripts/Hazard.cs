using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player touches the hazard, instantly respawn them
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.RespawnPlayer();
        }
    }
}
