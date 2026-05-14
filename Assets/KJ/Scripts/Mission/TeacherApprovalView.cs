using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 선생님이 학생의 미션 승인 요청 관리 및 관련 데이터 관리
/// </summary>
public class TeacherApprovalView : MonoBehaviour
{
    [SerializeField]
    private GameObject requestItemPrefab;
    [SerializeField]
    private Transform requestItemAllContent, requestItemOutdoorContent, requestItem1FContent, requestItem2FContent, requestItem3FContent;
    [SerializeField]
    private TextMeshProUGUI emptyLabel;

    public int AllRequestCount => requestItemAllContent.childCount; //승인 대기 건수

    private List<GameObject> spawnedAllIRequesttems = new(); //전체에 해당하는 요청 아이템 리스트
    private List<GameObject> spawnedTypeRequestItems = new(); //루트 타입에 해당하는 요청 아이템 리스트

    [SerializeField]
    private TextMeshProUGUI requestItemAllTMP, requestItemOutDoorTMP,requestItem1FTMP, requestItem2FTMP , requestItem3FTMP;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance.IsConnect);

        FirebaseManager.Instance.ListenPendingRequests(OnRequestUpdated);
    }

    private void OnRequestUpdated(List<MissionRequestData> requestDataList)
    {
        Debug.Log($"호출됨 Count: {requestDataList.Count}");

        //emptyLabel.gameObject.SetActive(requestItemContent.childCount == 0);


        if (requestDataList.Count >  0)
        {
            ClearRequestItem();

            RefreshCountTexts();
            Debug.Log("승인 요청 업데이트");

            foreach (var requestData in requestDataList)
            {
                GameObject item = Instantiate(requestItemPrefab, requestItemAllContent);
                item.GetComponent<ApproveRequestItem>().Init(this,requestData);
                spawnedAllIRequesttems.Add(item);

                MissionType(requestData.StudentPrefix, requestData);

                Debug.Log($"미션 보상: {requestData.MissionReward}");
            }
        }

        //emptyLabel.gameObject.SetActive(requestItemContent.childCount == 0);
    }

    /// <summary>
    /// 미션 타입에 해당하는 루트 분리
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="requestData"></param>
    private async void MissionType(string userID,MissionRequestData requestData) //@ 앞 부분에 해당하는 요청한 학생의 ID
    {
        string teamID = await FirebaseManager.Instance.GetUserIDToTeamID(userID);

        Dictionary<string,List<string>> routeDic =  await FirebaseManager.Instance.GetRouteIDToMission(teamID);

        foreach(var routes in routeDic)
        {
            foreach (string routeMissionID in routes.Value) //
            {
                if(routeMissionID == requestData.MissionID) //루트 안에 있는 미션 ID와 요청한 미션 ID 일치 하는지 확인
                {
                    string routeName = routes.Key; //루트 이름

                    RouteTypeRequestItem(routeName, requestData);
                }
            }
        }
    }

    /// <summary>
    /// 루트 타입에 따른 요청 아이템 생성
    /// </summary>
    private void RouteTypeRequestItem(string routeName,MissionRequestData requestData)
    {
        GameObject item = null;

        switch (routeName)
        {
            case "outdoor":
                item = Instantiate(requestItemPrefab, requestItemOutdoorContent);
                break;
            case "floor1":
                item = Instantiate(requestItemPrefab, requestItem1FContent);
                break;
            case "floor2":
                item = Instantiate(requestItemPrefab, requestItem2FContent);
                break;
            case "floor3":
                item = Instantiate(requestItemPrefab, requestItem3FContent);
                break;

        }

        item.GetComponent<ApproveRequestItem>().Init(this,requestData);
        spawnedTypeRequestItems.Add(item);

        RefreshCountTexts();
    }

    public Dictionary<string, int> rewardedUserDic { get; private set; } = new ();

    /// <summary>
    /// 유저에게 지급한 보상 관리
    /// </summary>
    public void CompensationPaid(string userID,int reward)
    {
        if(rewardedUserDic.ContainsKey(userID))
        {
            rewardedUserDic[userID] += reward;
        }
        else
        {
            rewardedUserDic[userID] = reward;
        }
    }
    /// <summary>
    /// 승인 요청 업데이트 시, 기존에 생성된 아이템 삭제
    /// </summary>
    public void ClearRequestItem()
    {
        foreach (var item in spawnedAllIRequesttems)
        {
            Destroy(item);
        }
        foreach (var item in spawnedTypeRequestItems)
        {
            Destroy(item);
        }

        spawnedAllIRequesttems.Clear();
        spawnedTypeRequestItems.Clear();

        StartCoroutine(RefereshChildCountCor());
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
        requestItemAllTMP.text = $"전체({requestItemAllContent.childCount})";
        requestItemOutDoorTMP.text = $"야외({requestItemOutdoorContent.childCount})";
        requestItem1FTMP.text = $"1층({requestItem1FContent.childCount})";
        requestItem2FTMP.text = $"2층({requestItem2FContent.childCount})";
        requestItem3FTMP.text = $"3층({requestItem3FContent.childCount})";
    }
    private void OnDestroy()
    {
        FirebaseManager.Instance.StopListenPendingRequests();
    }
}
