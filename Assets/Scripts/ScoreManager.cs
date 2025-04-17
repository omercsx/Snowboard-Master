using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreText;  // UI Text to display the score
    public int score { get; private set; }

    private float maxXReached; // Track the farthest point reached on X axis
    private int trickScore;

    private SnowboarderController playerController;

    private void Awake()
    {
        playerController = FindObjectOfType<SnowboarderController>();
        if (playerController != null)
        {
            maxXReached = playerController.transform.position.x;
        }

        score = 0;
    }

    private void Update()
    {
        if (playerController == null) return;

        // Distance score: Only increase score if new X position exceeds maxXReached
        float currentX = playerController.transform.position.x;

        if (currentX > maxXReached)
        {
            float distance = currentX - maxXReached;
            score += Mathf.FloorToInt(distance * 10); // Score based on distance moved right
            maxXReached = currentX;
        }

        // Trick score: Detect and calculate tricks
        if (!playerController.isGrounded)
        {
            if (Mathf.Abs(playerController.rb.rotation - playerController.rotationStart) > 360f)
            {
                trickScore++;
                playerController.rotationStart = playerController.rb.rotation;
                score += 100; // Add trick points
            }
        }

        UpdateScoreUI();
    }

    // Update score display
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    // Reset score (for restarting, new game, etc.)
    public void ResetScore()
    {
        score = 0;
        trickScore = 0;

        if (playerController != null)
        {
            maxXReached = playerController.transform.position.x;
        }

        UpdateScoreUI();
    }
}
