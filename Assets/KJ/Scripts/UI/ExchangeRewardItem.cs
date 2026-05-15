using TMPro;
using UnityEngine;

public class ExchangeRewardItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI studentNameTMP; //교환 성공한 학생 닉네임
    [SerializeField] private TextMeshProUGUI studentIDTMP; //교환 성공한 학생 ID
    [SerializeField] private TextMeshProUGUI rewardNameTMP; //교환 보상 이름


    public void Init(RewardResultData rewardResultData)
    {
        studentIDTMP.text = $"ID: {rewardResultData.RewardStudentID}";
        studentNameTMP.text = $"이름(닉네임) : {rewardResultData.RewardStudentName}";
        rewardNameTMP.text = $"보상 물품: {rewardResultData.RewardName}";

    }
}
