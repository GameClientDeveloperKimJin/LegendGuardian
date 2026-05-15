using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeacherExchangeView : MonoBehaviour
{
    [SerializeField]
    Transform exchangeContent;

    [SerializeField]
    GameObject exchangeItemPrefab;

    [SerializeField]
    TextMeshProUGUI exchangeTMP;
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance.IsConnect);

        FirebaseManager.Instance.ListenRewardReslt(OnExchangeResult);
    }

    private void OnExchangeResult(List<RewardResultData> rewardResultList)
    {
        foreach (Transform child in exchangeContent)
            Destroy(child.gameObject);

        if (rewardResultList.Count > 0)
        {
            foreach (RewardResultData data in rewardResultList)
            {
                GameObject item = Instantiate(exchangeItemPrefab, exchangeContent);
                item.GetComponent<ExchangeRewardItem>().Init(data);
            }

            StartCoroutine(RefereshChildCountCor());
        }
    }

    IEnumerator RefereshChildCountCor()
    {
        yield return null;

        RefreshCountTexts();
    }
    /// <summary>
    /// 건 수 업데이트
    /// </summary>
    private void RefreshCountTexts()
    {
        exchangeTMP.text = $"교환({exchangeContent.childCount})";
    }

    private void OnDestroy()
    {
        FirebaseManager.Instance.StopListenRewardResult();
    }
}
