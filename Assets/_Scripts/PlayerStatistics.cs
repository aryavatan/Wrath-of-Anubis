using UnityEngine;

public class PlayerStatistics
{
    public int highestRound = 0;
    public int highestKillsPerGame = 0;
    public int totalKills = 0;

    public static PlayerStatistics Load()
    {
        // Get the player statistics object
        string jsonStr = PlayerPrefs.GetString("PlayerStatistics", null);
        PlayerStatistics stats;

        if (jsonStr == "")
            stats = new PlayerStatistics();
        else
            stats = JsonUtility.FromJson<PlayerStatistics>(jsonStr);

        return stats;
    }

    public static void Save(PlayerStatistics stats)
    {
        string newJsonStr = JsonUtility.ToJson(stats);
        PlayerPrefs.SetString("PlayerStatistics", newJsonStr);
        PlayerPrefs.Save();
    }

    public static void ResetStats()
    {
        Save(new PlayerStatistics());
    }
}