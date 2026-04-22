using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text maxScoreText;
    [SerializeField] private TMP_Text scoreProgressText;

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

        if (maxScoreText != null)
            maxScoreText.text = maxScore.ToString();

        if (scoreProgressText != null)
            scoreProgressText.text = $"{currentScore} / {maxScore}";
    }

    private void UpdateBattery(float normalizedScore)
    {
        if (batteryFillImage != null)
            batteryFillImage.fillAmount = normalizedScore;
    }

    private void UpdateEmon(int score)
    {
        if (emonImage == null)
            return;

        if (score >= 60)
        {
            if (finalSprite != null)
                emonImage.sprite = finalSprite;
        }
        else if (score >= 45)
        {
            if (adultSprite != null)
                emonImage.sprite = adultSprite;
        }
        else if (score >= 25)
        {
            if (growthSprite != null)
                emonImage.sprite = growthSprite;
        }
        else if (score >= 10)
        {
            if (babySprite != null)
                emonImage.sprite = babySprite;
        }
        else
        {
            if (eggSprite != null)
                emonImage.sprite = eggSprite;
        }
    }
}
