using TMPro;
using UnityEngine;

public class HeaderBarScoreText : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScoreTextUI(this);
        }
    }

    public void UpdateScoreText(int currentScore)
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }
}