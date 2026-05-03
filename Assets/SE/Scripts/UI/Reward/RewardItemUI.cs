using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text rewardNameText;

    [Header("Button")]
    [SerializeField] private Button exchangeButton;

    private RewardData data;
    private RewardListUIController controller;

    public void Setup(RewardData data, RewardListUIController controller)
    {
        this.data = data;
        this.controller = controller;

        if (costText != null)
            costText.text = $"{data.cost}Wh";

        if (rewardNameText != null)
            rewardNameText.text = data.rewardName;


        if (exchangeButton != null)
        {
            exchangeButton.onClick.RemoveAllListeners();
            exchangeButton.onClick.AddListener(OnClickExchange);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (exchangeButton == null || ScoreManager.Instance == null || data == null)
            return;

        exchangeButton.interactable = ScoreManager.Instance.CurrentScore >= data.cost;
    }

    private void OnClickExchange()
    {
        if (ScoreManager.Instance == null || data == null || controller == null)
            return;

        bool success = ScoreManager.Instance.TrySpendScore(data.cost);

        if (success)
        {
            controller.ShowPopup(data.popupMessage);
            controller.RefreshAllItems();
        }
    }
}