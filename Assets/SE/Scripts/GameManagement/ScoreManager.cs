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

    public void AddScore(int amount)
    {
        currentScore = Mathf.Clamp(currentScore + amount, 0, maxScore);
        UpdateAllUI();
    }

    public void SetScore(int value)
    {
        currentScore = Mathf.Clamp(value, 0, maxScore);
        UpdateAllUI();
    }

    public void SetMaxScore(int value)
    {
        maxScore = Mathf.Max(1, value);
        currentScore = Mathf.Clamp(currentScore, 0, maxScore);
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
