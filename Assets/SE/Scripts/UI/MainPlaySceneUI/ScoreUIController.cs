using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIController : MonoBehaviour
{
    [System.Serializable]
    public class ElementEmonData
    {
        [Header("Element Info")]
        public string elementName;
        public Sprite eggSprite;
        public Sprite hatchSprite;
        public Sprite growthSprite;
        public Sprite adultSprite;
        public Sprite finalSprite;
    }

    [Header("Text")]
    [SerializeField] private TMP_Text scoreProgressText;
    [SerializeField] private TMP_Text emonStateText;
    [SerializeField] private TMP_Text elementText;

    [Header("Battery UI")]
    [SerializeField] private Image batteryFillImage;
    [SerializeField] private float fillSpeed = 5f;

    [Header("Emon UI")]
    [SerializeField] private Image emonImage;

    [Header("Element Emon Data")]
    [SerializeField] private ElementEmonData[] elementDataList;

    private Coroutine batteryCoroutine;
    private ElementEmonData currentElementData;

    private async void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetUI(this);
        }

        await SetElementByTeam();
    }

    public void UpdateUI(int currentScore, int maxScore, float normalizedScore)
    {
        UpdateTexts(currentScore, maxScore);
        UpdateBattery(normalizedScore);
        UpdateEmon(currentScore, maxScore);
    }

    private void UpdateTexts(int currentScore, int maxScore)
    {
        if (scoreProgressText == null)
            return;

        if (maxScore <= 0)
            scoreProgressText.text = $"{currentScore} / -";
        else
            scoreProgressText.text = $"{currentScore} / {maxScore}";
    }

    private async System.Threading.Tasks.Task SetElementByTeam()
    {
        if (AuthManager.Instance == null)
            return;

        if (elementDataList == null || elementDataList.Length == 0)
            return;

        string teamName = await AuthManager.Instance.GetUserTeamName();

        int index = Mathf.Abs(teamName.GetHashCode()) % elementDataList.Length;
        currentElementData = elementDataList[index];

        if (elementText != null)
            elementText.text = currentElementData.elementName;

        if (ScoreManager.Instance != null)
            UpdateEmon(ScoreManager.Instance.CurrentScore, ScoreManager.Instance.MaxScore);
    }

    private void UpdateBattery(float normalizedScore)
    {
        if (batteryFillImage == null)
            return;

        if (batteryCoroutine != null)
            StopCoroutine(batteryCoroutine);

        batteryCoroutine = StartCoroutine(SmoothBatteryFill(normalizedScore));
    }

    private void UpdateEmon(int score, int maxScore)
    {
        if (currentElementData == null)
            return;

        if (maxScore <= 0)
        {
            SetEmon(currentElementData.eggSprite, "알");
            return;
        }

        float step = maxScore / 4f;

        if (score >= maxScore)
            SetEmon(currentElementData.finalSprite, "최종");
        else if (score >= step * 3f)
            SetEmon(currentElementData.adultSprite, "성체");
        else if (score >= step * 2f)
            SetEmon(currentElementData.growthSprite, "성장기");
        else if (score >= step)
            SetEmon(currentElementData.hatchSprite, "부화");
        else
            SetEmon(currentElementData.eggSprite, "알");
    }

    private void SetEmon(Sprite sprite, string state)
    {
        if (emonImage != null && sprite != null)
            emonImage.sprite = sprite;

        if (emonStateText != null)
            emonStateText.text = state;
    }

    private IEnumerator SmoothBatteryFill(float targetAmount)
    {
        float startAmount = batteryFillImage.fillAmount;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * fillSpeed;
            batteryFillImage.fillAmount = Mathf.Lerp(startAmount, targetAmount, time);
            yield return null;
        }

        batteryFillImage.fillAmount = targetAmount;
    }
}