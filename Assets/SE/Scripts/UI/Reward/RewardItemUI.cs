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

    [SerializeField] private int debugScore = 50;

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

        controller.ShowExchangePopup(data.popupMessage);
    }
}