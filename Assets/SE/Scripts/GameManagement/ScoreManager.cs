using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    private int currentScore = 0;
    private int maxScore = 0;

    [Header("References")]
    [SerializeField] private ScoreUIController scoreUIController;

    private HeaderBarScoreText headerBarScoreText;

    public int CurrentScore => currentScore;
    public int MaxScore => maxScore;
    public bool HasMaxScore => maxScore > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateAllUI();
    }

    public void SetUI(ScoreUIController ui)
    {
        scoreUIController = ui;
        UpdateAllUI();
    }

    public void SetScoreTextUI(HeaderBarScoreText ui)
    {
        headerBarScoreText = ui;
        UpdateAllUI();
    }

    // 점수 추가 + Firebase 저장
    public async void AddScore(int amount)
    {
        currentScore = Mathf.Max(0, currentScore + amount);

        UpdateAllUI();

        if (FirebaseManager.Instance != null &&
            AuthManager.Instance != null)
        {
            await FirebaseManager.Instance.UpdateUserScore(
                AuthManager.Instance.LoginUserID,
                currentScore
            );
        }
    }

    // Firebase에서 불러온 점수 세팅용
    public void SetScore(int value)
    {
        currentScore = Mathf.Max(0, value);
        UpdateAllUI();
    }

    public void SetMaxScore(int value)
    {
        maxScore = Mathf.Max(0, value);
        UpdateAllUI();
    }

    // 점수 사용 + Firebase 저장
    public bool TrySpendScore(int amount)
    {
        if (currentScore < amount)
            return false;

        currentScore -= amount;

        UpdateAllUI();

        if (FirebaseManager.Instance != null &&
            AuthManager.Instance != null)
        {
            _ = FirebaseManager.Instance.UpdateUserScore(
                AuthManager.Instance.LoginUserID,
                currentScore
            );
        }

        return true;
    }

    public float GetNormalizedScore()
    {
        if (maxScore <= 0)
            return 0f;

        return Mathf.Clamp01((float)currentScore / maxScore);
    }

    private void UpdateAllUI()
    {
        if (scoreUIController != null)
        {
            scoreUIController.UpdateUI(
                currentScore,
                maxScore,
                GetNormalizedScore()
            );
        }

        if (headerBarScoreText != null)
        {
            headerBarScoreText.UpdateScoreText(currentScore);
        }
    }
}