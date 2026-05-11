using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager instance;

    const int MAX_SCORES = 10;
    const string PREF_KEY = "HighScores";

    List<int> scores = new List<int>();

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadScores();
    }

    // 🎯 Yeni skor ekle
    public void AddScore(int newScore)
    {
        scores.Add(newScore);

        scores = scores
            .OrderByDescending(s => s)
            .Take(MAX_SCORES)
            .ToList();

        SaveScores();
    }

    public List<int> GetScores()
    {
        return new List<int>(scores);
    }

    void SaveScores()
    {
        string data = string.Join(",", scores);
        PlayerPrefs.SetString(PREF_KEY, data);
        PlayerPrefs.Save();
    }

    void LoadScores()
    {
        scores.Clear();

        if (!PlayerPrefs.HasKey(PREF_KEY))
            return;

        string data = PlayerPrefs.GetString(PREF_KEY);
        string[] split = data.Split(',');

        foreach (string s in split)
        {
            if (int.TryParse(s, out int value))
                scores.Add(value);
        }
    }
}
