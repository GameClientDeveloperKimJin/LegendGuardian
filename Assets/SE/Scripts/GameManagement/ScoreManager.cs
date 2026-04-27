using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int maxScore = 60;

    [Header("References")]
    [SerializeField] private ScoreUIController scoreUIController;

    public int CurrentScore => currentScore;
    public int MaxScore => maxScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateAllUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        if (currentScore < 0)
            currentScore = 0;

        UpdateAllUI();
    }

    public void SetScore(int value)
    {
        currentScore = Mathf.Max(0, value);
        UpdateAllUI();
    }

    public void SetMaxScore(int value)
    {
        maxScore = Mathf.Max(1, value);
        UpdateAllUI();
    }

    public float GetNormalizedScore()
    {
        return Mathf.Clamp01((float)currentScore / maxScore);
    }

    private void UpdateAllUI()
    {
        if (scoreUIController != null)
        {
            scoreUIController.UpdateUI(currentScore, maxScore, GetNormalizedScore());
        }
    }
}
