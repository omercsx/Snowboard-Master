using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    public enum GameMode
    {
        Endless,
        TimeTrial
    }

    public GameMode currentGameMode;
    public float timeLimit = 300f;  // Time limit for Time Trial (in seconds)
    private float remainingTime;

    private bool isGameOver;

    public GameObject timeTrialUI;
    public Text timerText;
    public GameObject endlessUI;
    public GameObject finishLine;

    public ScoreManager scoreManager;
    private void Start()
    {
        if(PlayerPrefs.GetInt("GameMode") == 1)
        {
            currentGameMode = GameMode.Endless;
            finishLine.SetActive(false);
        }
        else
        {
            currentGameMode = GameMode.TimeTrial;
            finishLine.SetActive(true);
        }

        // Initialize the game mode (could be set via menu or loading screen)
        SetGameMode(currentGameMode);

        if (currentGameMode == GameMode.TimeTrial)
        {
            remainingTime = timeLimit;
        }

        scoreManager = FindObjectOfType<ScoreManager>();
    }

    private void Update()
    {
        if (isGameOver)
            return;

        if (currentGameMode == GameMode.TimeTrial)
        {
            HandleTimeTrial();
        }
    }

    // Handle Time Trial Mode (Countdown Timer)
    private void HandleTimeTrial()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0 && !isGameOver)
        {
            remainingTime = 0f;
            var player = FindObjectOfType<SnowboarderController>();
            if (player == null) return;

            player.DisableControls();
            EndGame(scoreManager.score);
        }

        // Update Time UI (e.g., a countdown)
        UpdateTimeTrialUI();
    }

    // Update Time Trial UI
    private void UpdateTimeTrialUI()
    {
        if (timeTrialUI != null)
        {
            // Show remaining time (this could be hooked up to a UI element)
            if(timerText != null)
                timerText.text = Mathf.Max(remainingTime, 0).ToString("F1");
        }
    }

    // Set Game Mode
    public void SetGameMode(GameMode mode)
    {
        currentGameMode = mode;
        isGameOver = false;

        // Disable unnecessary UI for the selected game mode
        if (mode == GameMode.TimeTrial)
        {
            if (timeTrialUI != null) timeTrialUI.SetActive(true);
            if (endlessUI != null) endlessUI.SetActive(false);
        }
        else
        {
            if (endlessUI != null) endlessUI.SetActive(true);
            if (timeTrialUI != null) timeTrialUI.SetActive(false);
        }

        // Reset game elements (terrain, score, etc.)
        ResetGame();
    }

    // Reset game (called on game mode switch or restart)
    private void ResetGame()
    {
        // Reset the player, terrain, score, etc.
        // e.g., FindObjectOfType<PlayerController>().ResetPlayer();
        FindObjectOfType<ScoreManager>().ResetScore();
        isGameOver = false;
    }


    public GameObject gameplayUI;
    public GameObject finishPanel;

    // End Game (called when time is up in Time Trial or player crashes)
    public void EndGame(int finalScore)
    {

        isGameOver = true;
        string playerName = PlayerDataManager.GetPlayerName();

        PlayerDataManager.SetHighScore(finalScore);
        LeaderboardManager.AddNewScore(playerName, finalScore);

        gameplayUI.SetActive(false);
        finishPanel.SetActive(true);

        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
