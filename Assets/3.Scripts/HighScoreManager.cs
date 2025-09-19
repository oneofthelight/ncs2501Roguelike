using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class HighScoreManager : MonoBehaviour
{
    public UIDocument UIDoc;
    private Label HighScoreLabel;
    private const string HIGH_SCORE_KEY = "highScores";
    private const int MAX_SCORES = 6;

    private Label m_HighScoreLabel;
    private List<int> m_HighScores = new List<int>();

    [System.Serializable]
    private class HighScoreData
    {
        public List<int> scores;
    }

    // This method should be called from GameManager's Start()
    public void LoadHighScores()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        m_HighScoreLabel = root.Q<Label>("HighScoreLabel");

        if (PlayerPrefs.HasKey(HIGH_SCORE_KEY))
        {
            string jsonString = PlayerPrefs.GetString(HIGH_SCORE_KEY);
            HighScoreData data = JsonUtility.FromJson<HighScoreData>(jsonString);
            m_HighScores = data.scores ?? new List<int>();
        }
        UpdateHighScoresUI();
    }

    // This method is called by GameManager when the game is over
    public void AddScore(int level)
    {
        m_HighScores.Add(level);
        m_HighScores = m_HighScores.OrderByDescending(s => s).ToList(); // Sort descending

        if (m_HighScores.Count > MAX_SCORES)
        {
            m_HighScores.RemoveRange(MAX_SCORES, m_HighScores.Count - MAX_SCORES); // Keep only the top 6
        }

        SaveHighScores();
        UpdateHighScoresUI();
    }

    private void SaveHighScores()
    {
        HighScoreData data = new HighScoreData { scores = m_HighScores };
        string jsonString = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(HIGH_SCORE_KEY, jsonString);
        PlayerPrefs.Save();
    }

    private void UpdateHighScoresUI()
    {
        if (m_HighScoreLabel != null)
        {
            string scoreText = "High Scores:\n";
            for (int i = 0; i < m_HighScores.Count; i++)
            {
                scoreText += $"{i + 1}. Level {m_HighScores[i]}\n";
            }
            m_HighScoreLabel.text = scoreText;
        }
    }
}
