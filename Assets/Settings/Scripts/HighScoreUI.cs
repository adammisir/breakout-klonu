using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class HighScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreListText;

    void Start()
    {
        List<int> scores = HighScoreManager.instance.GetScores();

        scoreListText.text = "";

        int max = 10;

        for (int i = 0; i < max; i++)
        {
            if (i < scores.Count)
            {
                scoreListText.text +=
                    $"{i + 1}. {scores[i].ToString("000000")}\n";
            }
            else
            {
                scoreListText.text +=
                    $"{i + 1}. 000000\n";
            }
        }
    }
}
