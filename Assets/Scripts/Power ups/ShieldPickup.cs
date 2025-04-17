using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f; // Speed at which the power-up moves
    [SerializeField] private float rotateSpeed = 50f; // Speed of rotation
    [SerializeField] private AudioClip pickupSound;

    private void Start()
    {
        // If you want the power-up to move or rotate like a floating object:
        // you can add code here to make it move up and down or rotate.
    }

    private void Update()
    {
        // Optional: You can add a little movement or rotation to the power-up
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SnowboarderController player = other.GetComponent<SnowboarderController>();
        if (player != null)
        {
            player.ActivateShield();

            // Play the pickup sound (if assigned)
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject); // Remove pickup
        }
    }
}
