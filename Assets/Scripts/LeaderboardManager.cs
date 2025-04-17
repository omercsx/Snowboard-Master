using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;

    public LeaderboardEntry(string name, int score)
    {
        this.playerName = name;
        this.score = score;
    }
}

public static class LeaderboardManager
{
    private static string leaderboardKey = "LeaderboardData";
    private static int maxEntries = 5;

    private static string[] defaultNames = { "Frosty", "Avalanche", "PowderKing", "IceBlade", "SnowRider", "ChillyBro", "Blizzard" };

    public static List<LeaderboardEntry> LoadLeaderboard()
    {
        if (!PlayerPrefs.HasKey(leaderboardKey))
        {
            CreateInitialLeaderboard();
        }

        string json = PlayerPrefs.GetString(leaderboardKey);
        return JsonUtility.FromJson<LeaderboardDataWrapper>(json).entries;
    }

    public static void AddNewScore(string playerName, int newScore)
    {
        List<LeaderboardEntry> leaderboard = LoadLeaderboard();

        // Remove existing YOU entries
        leaderboard.RemoveAll(entry => entry.playerName == playerName);

        // Add the new player score
        leaderboard.Add(new LeaderboardEntry(playerName, newScore));

        // Sort by score descending
        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

        // Trim if too many
        if (leaderboard.Count > maxEntries)
            leaderboard = leaderboard.GetRange(0, maxEntries);

        // Fill with random entries if too few
        while (leaderboard.Count < maxEntries)
        {
            string randomName = GetRandomNameExcluding(leaderboard);
            int randomScore = Random.Range(200, 1000);
            leaderboard.Add(new LeaderboardEntry(randomName, randomScore));
        }

        SaveLeaderboard(leaderboard);
    }


    private static string GetRandomNameExcluding(List<LeaderboardEntry> existingEntries)
    {
        List<string> availableNames = new List<string>(defaultNames);

        foreach (var entry in existingEntries)
        {
            availableNames.Remove(entry.playerName);
        }

        if (availableNames.Count == 0)
            return "Guest" + Random.Range(1, 1000);

        return availableNames[Random.Range(0, availableNames.Count)];
    }



    private static void SaveLeaderboard(List<LeaderboardEntry> entries)
    {
        LeaderboardDataWrapper wrapper = new LeaderboardDataWrapper { entries = entries };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(leaderboardKey, json);
        PlayerPrefs.Save();
    }

    private static void CreateInitialLeaderboard()
    {
        List<LeaderboardEntry> dummyData = new List<LeaderboardEntry>();

        for (int i = 0; i < maxEntries - 1; i++) // 4 entries
        {
            string name = defaultNames[Random.Range(0, defaultNames.Length)];
            int score = Random.Range(200, 1000);
            dummyData.Add(new LeaderboardEntry(name, score));
        }

        SaveLeaderboard(dummyData);
    }

    [System.Serializable]
    private class LeaderboardDataWrapper
    {
        public List<LeaderboardEntry> entries;
    }
}
