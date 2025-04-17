using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private const string PlayerNameKey = "PlayerName";
    private const string HighScoreKey = "HighScore";

    public static void SetPlayerName(string name)
    {
        PlayerPrefs.SetString(PlayerNameKey, name);
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(PlayerNameKey, "Player");
    }

    public static void SetHighScore(int score)
    {
        int currentHigh = GetHighScore();
        if (score > currentHigh)
        {
            PlayerPrefs.SetInt(HighScoreKey, score);
        }
    }

    public static int GetHighScore()
    {
        return PlayerPrefs.GetInt(HighScoreKey, 0);
    }
}
