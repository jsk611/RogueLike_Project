using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerScoreData
{
    public string playerName;
    public int score;
    public int rank;
}

[System.Serializable]
public class DailyScoreboard
{
    public string date; // YYYY-MM-DD Çü½Ä
    public List<PlayerScoreData> players = new List<PlayerScoreData>();
}
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public ScoreboardUI ui;
    private DailyScoreboard current;

    private void Awake() => Instance = this;
    public void SubmitScore(string name, int score)
    {
        LoadTodayBoard();
        current.players.Add(new PlayerScoreData { playerName = name, score = score });
        current.players = current.players.OrderByDescending(p => p.score).ToList();

        for (int i = 0; i < current.players.Count; i++)
            current.players[i].rank = i + 1;

        Save();
        ui.UpdateUI(current);
    }

    void LoadTodayBoard()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        string path = Application.persistentDataPath + "/scoreboard_" + today + ".json";
        if (File.Exists(path))
            current = JsonUtility.FromJson<DailyScoreboard>(File.ReadAllText(path));
        else
            current = new DailyScoreboard { date = today };
    }

    void Save()
    {
        string json = JsonUtility.ToJson(current, true);
        File.WriteAllText(Application.persistentDataPath + "/scoreboard_" + current.date + ".json", json);
    }
}
