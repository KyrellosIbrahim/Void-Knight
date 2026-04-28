using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType { Coin, Heart }

    public PickupType type = PickupType.Coin;
    public AudioClip pickupSound;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerBehaviour>();
        if (player == null) return;

        if (type == PickupType.Coin) {
            player.AddCoin();
        }
        else {
            player.Heal(1);
        }

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Destroy(gameObject);
    }
}
