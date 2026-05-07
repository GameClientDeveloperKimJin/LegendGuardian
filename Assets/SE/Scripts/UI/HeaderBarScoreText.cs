using System.Collections;
using TMPro;
using UnityEngine;

public class HeaderBarScoreText : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text nickNameText;
    [SerializeField] private TMP_Text scoreText;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => AuthManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance != null);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScoreTextUI(this);
        }

        InitHeaderBar();

        FirebaseManager.Instance.OnScoreUpdated += OnScoreUpdated;
    }

    private async void InitHeaderBar()
    {
        if (nickNameText != null)
        {
            nickNameText.text = AuthManager.Instance.LoginUserName;
        }

        string score = await FirebaseManager.Instance.GetUserIDToScore(AuthManager.Instance.LoginUserID);

        int currentScore = 0;
        int.TryParse(score, out currentScore);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScore(currentScore);
        }
        else
        {
            UpdateScoreText(currentScore);
        }
    }

    private void OnScoreUpdated(long score)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScore((int)score);
        }
        else
        {
            UpdateScoreText((int)score);
        }
    }

    public void UpdateScoreText(int currentScore)
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnScoreUpdated -= OnScoreUpdated;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScoreTextUI(null);
        }
    }
}