using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardResultData
{
    public string RewardStudentID; //보상 받은 학생 id
    public string RewardStudentName; //보상 받은 학생 이름
    public string RewardName; //보상 이름

    public long Count; //보상 개수
}
public class RewardView : MonoBehaviour
{
    [SerializeField]
    Transform objectContent, foodContent, badgeContent;

    [SerializeField]
    GameObject rewardItemPrefab;

    [SerializeField]
    RewardSO[] rewardArray;

    [SerializeField]
    GameObject infoImage;

    [SerializeField]
    Button infoCheckBtn;

    [SerializeField]
    TextMeshProUGUI infoTMP;

    private void Start()
    {
        UpdateReward();
    }

    private void OnDisable()
    {
        infoImage.gameObject.SetActive(false);
    }
    private void UpdateReward()
    {
        foreach(var rewardSO in rewardArray)
        {
            CreateRewardItem(rewardSO.Type,rewardSO);
        }
    }

    private void CreateRewardItem(RewardSO.RewardType rewardType , RewardSO rewardSO)
    {
        GameObject rewardObj = null;

        switch(rewardType)
        {
            case RewardSO.RewardType.Object:
                rewardObj = Instantiate(rewardItemPrefab, objectContent);
                break;
            case RewardSO.RewardType.Food:
                rewardObj = Instantiate(rewardItemPrefab, foodContent);
                break;
            case RewardSO.RewardType.Badge:
                rewardObj = Instantiate(rewardItemPrefab, badgeContent);
                break;
        }


        if (rewardObj!.TryGetComponent(out RewardItem rewardItem))
        {
            rewardItem.Init(rewardSO);

            //교환 성공
            rewardItem.OnExchangeded += () =>
            {             
                infoImage.gameObject.SetActive(true);
                infoTMP.text = $"교환 성공!";

                infoCheckBtn.onClick.AddListener(() =>
                {
                    infoImage.gameObject.SetActive(false);
                });
            };

            //교환 실패
            rewardItem.OnExchangeRejected += (score) =>
            {
                infoImage.gameObject.SetActive(true);
                infoTMP.text = $" 교환 실패! {score}/wh 이상 필요합니다.";

                infoCheckBtn.onClick.AddListener(() =>
                {
                    infoImage.gameObject.SetActive(false);
                });
            };
        }

    }
}
