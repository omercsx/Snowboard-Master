using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [System.Serializable]
    public class LeaderboardEntryUI
    {
        public Text nameText;
        public Text scoreText;
    }

    [Header("UI References")]
    public List<LeaderboardEntryUI> uiEntries = new List<LeaderboardEntryUI>();

    private string[] randomNames = { "Blizzard", "Frosty", "SnowRider", "Chilly", "IceBreak", "PowderKing", "Avalanche" };

    private void OnEnable()
    {
        UpdateLeaderboardUI();
    }

    public void UpdateLeaderboardUI()
    {
        List<LeaderboardEntry> entries = LeaderboardManager.LoadLeaderboard();
        string playerName = PlayerDataManager.GetPlayerName();

        for (int i = 0; i < uiEntries.Count; i++)
        {
            if (i < entries.Count)
            {
                string nameToShow = entries[i].playerName == playerName ? "YOU" : GetRandomName(i);
                uiEntries[i].nameText.text = nameToShow;
                uiEntries[i].scoreText.text = entries[i].score.ToString();
            }
            else
            {
                uiEntries[i].nameText.text = "-";
                uiEntries[i].scoreText.text = "-";
            }
        }
    }

    private string GetRandomName(int index)
    {
        return randomNames[index % randomNames.Length];
    }
}
