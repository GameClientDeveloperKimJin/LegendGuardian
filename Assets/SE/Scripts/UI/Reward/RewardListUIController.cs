using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardListUIController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private RewardCategoryUI categoryPrefab;
    [SerializeField] private RewardItemUI itemPrefab;

    [Header("Parent")]
    [SerializeField] private Transform contentParent;

    [Header("Popup")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private Button popupCloseButton;

    private readonly List<RewardItemUI> spawnedItems = new List<RewardItemUI>();

    private void Awake()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(HidePopup);
    }

    private void Start()
    {
        // 테스트용
        //ScoreManager.Instance.SetScore(50);

        LoadDummyRewards();
    }

    private void LoadDummyRewards()
    {
        List<RewardData> rewards = new List<RewardData>
        {
            new RewardData
            {
                categoryName = "간식 교환",
                cost = 30,
                rewardName = "사탕 2개",
                description = "가장 저렴한 옵션",
                popupMessage = "전기밥솥 30분 보온 분량!"
            },
            new RewardData
            {
                categoryName = "간식 교환",
                cost = 50,
                rewardName = "음료 1개",
                description = "인기 음료 준비",
                popupMessage = "스마트폰 1회 충전 분량!"
            },
            new RewardData
            {
                categoryName = "간식 교환",
                cost = 80,
                rewardName = "간식 세트",
                description = "고급 보상",
                popupMessage = "선풍기 2시간 분량 간식!"
            },

            new RewardData
            {
                categoryName = "칭호 발급소",
                cost = 20,
                rewardName = "에너지 절약 스티커",
                description = "절약 실천원",
                popupMessage = "이 에너지는 LED 10시간 분량!"
            },
            new RewardData
            {
                categoryName = "칭호 발급소",
                cost = 40,
                rewardName = "에코 탐험대 배지",
                description = "에코 탐험대원",
                popupMessage = "이 에너지는 스마트폰 80% 충전!"
            },
            new RewardData
            {
                categoryName = "칭호 발급소",
                cost = 60,
                rewardName = "특별 인증서",
                description = "에코 히어로",
                popupMessage = "이 에너지는 선풍기 1시간 분량!"
            },

            new RewardData
            {
                categoryName = "랭킹 보너스",
                cost = 0,
                rewardName = "1~3위 특별 상품",
                description = "트로피 애니메이션 + 축하 메시지",
                popupMessage = "상위 랭킹 보상 안내입니다!"
            },
            new RewardData
            {
                categoryName = "랭킹 보너스",
                cost = 0,
                rewardName = "4~10위 소정 상품",
                description = "순위 + 격려 메시지",
                popupMessage = "랭킹 보너스 대상입니다!"
            }
        };

        BuildRewardList(rewards);
    }

    private void BuildRewardList(List<RewardData> rewards)
    {
        ClearList();

        Dictionary<string, RewardCategoryUI> categoryMap = new Dictionary<string, RewardCategoryUI>();

        foreach (RewardData reward in rewards)
        {
            if (!categoryMap.TryGetValue(reward.categoryName, out RewardCategoryUI categoryUI))
            {
                categoryUI = Instantiate(categoryPrefab, contentParent);
                categoryUI.Setup(reward.categoryName);
                categoryMap.Add(reward.categoryName, categoryUI);
            }

            RewardItemUI itemUI = Instantiate(itemPrefab, categoryUI.ItemContainer);
            itemUI.Setup(reward, this);
            spawnedItems.Add(itemUI);
        }
    }

    private void ClearList()
    {
        spawnedItems.Clear();

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }

    public void RefreshAllItems()
    {
        foreach (RewardItemUI item in spawnedItems)
        {
            if (item != null)
                item.Refresh();
        }
    }

    public void ShowPopup(string message)
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        if (popupText != null)
            popupText.text = message;
    }

    private void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}