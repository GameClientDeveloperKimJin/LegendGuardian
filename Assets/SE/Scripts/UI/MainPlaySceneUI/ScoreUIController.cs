using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text scoreProgressText;
    [SerializeField] private TMP_Text emonStateText;

    [Header("Battery UI")]
    [SerializeField] private Image batteryFillImage;

    [Header("Emon UI")]
    [SerializeField] private Image emonImage;
    [SerializeField] private Sprite eggSprite;          // 0~9
    [SerializeField] private Sprite babySprite;         // 10~24
    [SerializeField] private Sprite growthSprite;       // 25~44
    [SerializeField] private Sprite adultSprite;        // 45~59
    [SerializeField] private Sprite finalSprite;        // 60+

    public void UpdateUI(int currentScore, int maxScore, float normalizedScore)
    {
        UpdateTexts(currentScore, maxScore);
        UpdateBattery(normalizedScore);
        UpdateEmon(currentScore);
    }

    private void UpdateTexts(int currentScore, int maxScore)
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();

        if (scoreProgressText != null)
            scoreProgressText.text = $"{currentScore} / {maxScore}";
    }

    private void UpdateBattery(float normalizedScore)
    {
        if (batteryFillImage != null)
            batteryFillImage.fillAmount = normalizedScore;
    }
    private void SetEmon(Sprite sprite, string state)
    {
        if (emonImage != null && sprite != null)
            emonImage.sprite = sprite;

        if (emonStateText != null)
            emonStateText.text = state;
    }

    private void UpdateEmon(int score)
    {
        if (score >= 60)
            SetEmon(finalSprite, "최종");
        else if (score >= 45)
            SetEmon(adultSprite, "성체");
        else if (score >= 25)
            SetEmon(growthSprite, "성장기");
        else if (score >= 10)
            SetEmon(babySprite, "아기");
        else
            SetEmon(eggSprite, "알");
    }
}
