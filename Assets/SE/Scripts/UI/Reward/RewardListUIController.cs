using System.Collections;
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
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private Button popupCloseButton;

    [Header("Loading")]
    [SerializeField] private float loadingCharDelay = 0.25f;

    private readonly List<RewardItemUI> spawnedItems = new List<RewardItemUI>();
    private Coroutine loadingCoroutine;

    private void Awake()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (popupCloseButton != null)
        {
            popupCloseButton.onClick.RemoveAllListeners();
            popupCloseButton.onClick.AddListener(HidePopup);
        }
    }

    private void Start()
    {
        LoadDummyRewards();//더미용 데이터
    }

    private void LoadDummyRewards() //더미용 데이터
    {
        List<RewardData> rewards = new List<RewardData>
        {
            new RewardData { categoryName = "간식 교환", cost = 30, rewardName = "사탕 2개", popupMessage = "전기밥솥 30분 보온 분량!" },
            new RewardData { categoryName = "간식 교환", cost = 50, rewardName = "음료 1개", popupMessage = "스마트폰 1회 충전 분량!" },
            new RewardData { categoryName = "간식 교환", cost = 80, rewardName = "간식 세트", popupMessage = "선풍기 2시간 분량 간식!" },

            new RewardData { categoryName = "칭호 발급소", cost = 20, rewardName = "에너지 절약 스티커", popupMessage = "이 에너지는 LED 10시간 분량!" },
            new RewardData { categoryName = "칭호 발급소", cost = 40, rewardName = "에코 탐험대 배지", popupMessage = "이 에너지는 스마트폰 80% 충전!" },
            new RewardData { categoryName = "칭호 발급소", cost = 60, rewardName = "특별 인증서", popupMessage = "이 에너지는 선풍기 1시간 분량!" },

            new RewardData { categoryName = "랭킹 보너스", cost = 0, rewardName = "1~3위 특별 상품", popupMessage = "상위 랭킹 보상 안내" },
            new RewardData { categoryName = "랭킹 보너스", cost = 0, rewardName = "4~10위 소정 상품", popupMessage = "랭킹 보너스 안내" }
        };

        BuildRewardList(rewards);
    }

    private void BuildRewardList(List<RewardData> rewards)
    {
        ClearList();

        string currentCategory = "";

        foreach (RewardData reward in rewards)
        {
            if (currentCategory != reward.categoryName)
            {
                currentCategory = reward.categoryName;

                RewardCategoryUI categoryUI = Instantiate(categoryPrefab, contentParent);
                categoryUI.Setup(currentCategory);
            }

            RewardItemUI itemUI = Instantiate(itemPrefab, contentParent);
            itemUI.Setup(reward, this);
            spawnedItems.Add(itemUI);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
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

    public void ShowExchangePopup(string message)
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        if (popupText != null)
            popupText.text = message;

        if (popupCloseButton != null)
            popupCloseButton.gameObject.SetActive(false);

        if (loadingText != null)
            loadingText.text = "";

        if (loadingCoroutine != null)
            StopCoroutine(loadingCoroutine);

        loadingCoroutine = StartCoroutine(LoadingTextRoutine());

        StartCoroutine(WaitExchangeCompleteRoutine());
    }

    private IEnumerator LoadingTextRoutine()
    {
        string loadingMessage = "로딩중...";

        while (true)
        {
            if (loadingText != null)
                loadingText.text = "";

            for (int i = 0; i < loadingMessage.Length; i++)
            {
                if (loadingText != null)
                    loadingText.text += loadingMessage[i];

                yield return new WaitForSeconds(loadingCharDelay);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator WaitExchangeCompleteRoutine()
    {
        // 나중에 여기에서 실제 로딩 완료 여부 확인 넣으면 됨.
        // yield return new WaitUntil(() => FirebaseManager.Instance.IsExchangeComplete);
        // yield return new WaitUntil(() => isExchangeComplete == true);

        yield return new WaitForSeconds(3f); // 임시 테스트용

        CompleteLoading();
    }

    public void CompleteLoading()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        if (loadingText != null)
            loadingText.text = "";

        if (popupCloseButton != null)
            popupCloseButton.gameObject.SetActive(true);

        RefreshAllItems();
    }

    private void HidePopup()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}