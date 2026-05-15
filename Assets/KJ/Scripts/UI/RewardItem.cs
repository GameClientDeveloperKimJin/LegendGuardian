using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI titleTMP;
    [SerializeField]
    TextMeshProUGUI detatilTMP;

    [SerializeField]
    Button exchangeBtn;

    public event Action OnExchangeded; //교환 성공 이벤트
    public event Action<int> OnExchangeRejected; //교환 실패 이벤트
    public void Init(RewardSO rewardSO)
    {
        titleTMP.text = rewardSO.RewardName;
        detatilTMP.text = $"필요 에너지: {rewardSO.RequiredScore}/wh";

        exchangeBtn.onClick.AddListener(() =>
        {
            OnExchangeBtn(rewardSO);
        });

    }

    private async void OnExchangeBtn(RewardSO rewardSO)
    {
        if(ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.CurrentScore;

            if(currentScore >= rewardSO.RequiredScore) //교환 조건
            {
                ScoreManager.Instance.AddScore(-rewardSO.RequiredScore); //점수 차감

                OnExchangeded?.Invoke();

                string studentPrefix = AuthManager.Instance.LoginUserID.Split('@')[0];

                await FirebaseManager.Instance?.RewardResult(
                    studentPrefix,
                    AuthManager.Instance?.LoginUserName,
                    rewardSO.RewardName);
            }
            else
            {
                OnExchangeRejected?.Invoke(rewardSO.RequiredScore - currentScore);
            }

        }
    }


    private void OnDestroy()
    {
        exchangeBtn.onClick.RemoveAllListeners();
    }
}
