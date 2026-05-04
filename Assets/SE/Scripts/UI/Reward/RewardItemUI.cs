using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text rewardNameText;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text costText;

    [Header("Button")]
    [SerializeField] private Button exchangeButton;

    [SerializeField] private int debugScore = 50; //테스트용입니당
    private RewardData data;
    private RewardListUIController controller;

    public void Setup(RewardData data, RewardListUIController controller)
    {
        this.data = data;
        this.controller = controller;

        if (rewardNameText != null)
            rewardNameText.text = data.rewardName;

        if (labelText != null)
            labelText.text = "필요 에너지 :";

        if (costText != null)
            costText.text = $"{data.cost}Wh";

        if (exchangeButton != null)
        {
            exchangeButton.onClick.RemoveAllListeners();
            exchangeButton.onClick.AddListener(OnClickExchange);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (exchangeButton == null || data == null)
            return;

        int currentScore = ScoreManager.Instance != null
            ? ScoreManager.Instance.CurrentScore
            : debugScore;

        exchangeButton.interactable = currentScore >= data.cost;
    }

    private void OnClickExchange()
    {
        if (data == null || controller == null)
            return;

        bool success = true;

        if (ScoreManager.Instance != null) //교환 버튼 누를 시 점수 차감
        {
            success = ScoreManager.Instance.TrySpendScore(data.cost);
        }

        if (success)
        {
            controller.ShowPopup(data.popupMessage);
            controller.RefreshAllItems();
        }
    }
}