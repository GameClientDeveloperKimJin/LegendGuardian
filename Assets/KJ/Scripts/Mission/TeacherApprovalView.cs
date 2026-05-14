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
    private TextMeshProUGUI emptyLabel;

    private List<GameObject> spawnedItems = new();
    private List<MissionRequestData> lastRequests = new();

    IEnumerator Start()
    {
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance.IsConnect);

        FirebaseManager.Instance.ListenPendingRequests(OnRequestUpdated);
    }

    private void OnRequestUpdated(List<MissionRequestData> requestDataList)
    {
        Debug.Log($"호출됨 Count: {requestDataList.Count}");

        //if (IsSameRequests(requestDataList)) return;
        //lastRequests = new List<MissionRequestData>(requestDataList);

        emptyLabel.gameObject.SetActive(requestItemContent.childCount == 0);


        if (requestDataList.Count >  0)
        {
            foreach (var item in spawnedItems)
                Destroy(item);

            Debug.Log("승인 요청 업데이트");
            spawnedItems.Clear();

            foreach (var requestData in requestDataList)
            {
                GameObject item = Instantiate(requestItemPrefab, requestItemContent);
                item.GetComponent<ApproveRequestItem>().Init(requestData);
                spawnedItems.Add(item);
            }
        }
      
    }

    private bool IsSameRequests(List<MissionRequestData> newList)
    {
        if (newList.Count != lastRequests.Count) return false;

        for (int i = 0; i < newList.Count; i++)
        {
            if (newList[i].StudentPrefix != lastRequests[i].StudentPrefix) return false;
            if (newList[i].Status != lastRequests[i].Status) return false;
        }
        return true;
    }

    private void OnDestroy()
    {
        FirebaseManager.Instance.StopListenPendingRequests();
    }
}
