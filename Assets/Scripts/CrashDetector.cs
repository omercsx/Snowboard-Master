using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] float delayTime = 1f;
    [SerializeField] ParticleSystem crashEffect;
    [SerializeField] AudioClip crashSFX;

    private bool hasCrashed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ground" && !hasCrashed)
        {
            var player = FindObjectOfType<SnowboarderController>();
            if (player == null) return;

            if (player.isShielded)
            {
                player.isShielded = false;
                if(player.shieldVisual != null)
                    player.shieldVisual.SetActive(false);

                player.CorrectRotation();
                return;
            }

            if (player.HasExtraLife())
            {
                player.UseExtraLife();
                player.CorrectRotation();

                // Optional: bounce a bit to get unstuck
                player.transform.position += Vector3.up * 1.5f;
                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

                Debug.Log("Used Extra Life!");
                return;
            }

            // No shield or extra life, actual crash
            hasCrashed = true;
            player.DisableControls();
            player.enabled = false;
            Debug.Log("Ouch!");
            crashEffect.Play();
            GetComponent<AudioSource>().PlayOneShot(crashSFX);
            Invoke("ReloadScene", delayTime);
        }

        if(other.CompareTag("Fall"))
        {
            var player = FindObjectOfType<SnowboarderController>();
            if (player == null) return;

            player.DisableControls();
            player.enabled = false;
            Debug.Log("Ouch!");
            crashEffect.Play();
            GetComponent<AudioSource>().PlayOneShot(crashSFX);
            ReloadScene();
        }

    }


    void ReloadScene()
    {
        GameModeManager modeManager = FindObjectOfType<GameModeManager>();
        if(modeManager == null) return;

        modeManager.EndGame(modeManager.scoreManager.score);
        //SceneManager.LoadScene(0);
    }
}
