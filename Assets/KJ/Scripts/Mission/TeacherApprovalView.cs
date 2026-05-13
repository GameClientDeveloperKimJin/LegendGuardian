using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeacherApprovalView : MonoBehaviour
{
    [SerializeField]
    private GameObject requestItemPrefab;
    [SerializeField]
    private Transform requestItemContent;
    [SerializeField]
    private TextMeshProUGUI emptyLabel; // 승인 상태 텍스트 

    private List<GameObject> spawnedItems = new();

    IEnumerator Start()
    {
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance.IsConnect);

        FirebaseManager.Instance.ListenPendingRequests(OnRequestUpdated);
    }

    private List<MissionRequestData> lastRequests = new();

    private void OnRequestUpdated(List<MissionRequestData> requestDataList)
    {
        if (IsSameRequests(requestDataList)) return;

        lastRequests = requestDataList;

        foreach (var item in spawnedItems)
            Destroy(item);

        Debug.Log("승인 요청 업데이트");
        spawnedItems.Clear();

        emptyLabel.gameObject.SetActive(requestDataList.Count == 0);


        foreach (var requestData in requestDataList)
        {
            GameObject item = Instantiate(requestItemPrefab, requestItemContent);
            item.GetComponent<ApproveRequestItem>().Init(requestData);
            spawnedItems.Add(item);
        }
    }

    private bool IsSameRequests(List<MissionRequestData> newList)
    {
        if (newList.Count != lastRequests.Count) return false;

        for (int i = 0; i < newList.Count; i++)
        {
            if (newList[i].StudentPrefix != lastRequests[i].StudentPrefix) return false;
        }
        return true;
    }

    private void OnDestroy()
    {
        FirebaseManager.Instance.StopListenPendingRequests();
    }
}
