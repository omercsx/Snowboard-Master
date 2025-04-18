using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{

    [SerializeField] float delayTime = 2f;
    [SerializeField] ParticleSystem finishEffect;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player")
        {
            Debug.Log("You Finished!");
            finishEffect.Play();
            GetComponent<AudioSource>().Play();
            Invoke("ReloadScene", delayTime); // Invoke has to use a method that you are delaying
        }
    }

    void ReloadScene() //Created reload scene method in order to use invoke
    {

        var player = FindObjectOfType<SnowboarderController>();
        if (player != null)
            player.DisableControls();

        GameModeManager modeManager = FindObjectOfType<GameModeManager>();

        modeManager.EndGame(modeManager.scoreManager.score);
        //SceneManager.LoadScene(0);
    }


}
