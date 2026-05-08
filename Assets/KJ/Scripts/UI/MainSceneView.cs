using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MissionMapType
{
    outdoor,
    floor1,
    floor2,
    floor3,
}

public class MainSceneView : MonoBehaviour 
{
    [SerializeField]
    TextMeshProUGUI nickNameTMP;

    [SerializeField]
    TextMeshProUGUI scoreTMP;

    [SerializeField]
    Button routeButtonPrefab; //생성할 버튼 프리펩

    [SerializeField]
    Transform routeButtonContent; //생성할 버튼의 위치

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => AuthManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
       
        Init();
    }

    Button[] routeButtons;

    /// <summary>
    /// 메인 씬 전환 시, 초기화 할 내용 ( 1회성)
    /// </summary>
    private async void Init()
    {
        // 팀에 맞는 루트 버튼 동적 생성
        string teamID = await AuthManager.Instance.GetUserTeamName();

        var (routesID,routesName) = await FirebaseManager.Instance.GetTeamIDToRoutes(teamID);

        routeButtons = new Button[routesID.Length];

        for (int i = 0; i < routesID.Length; i++)
        {
            routeButtons[i] = Instantiate(routeButtonPrefab, routeButtonContent);
            routeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = routesName[i];

            routeButtons[i].GetComponent<RouteButtonItem>().Init(i,routesID[i], routesName[i]); //버튼 인덱스 번호, 루트 ID , 루트 네임

            int captureIndex = i; //클로저 문제
            routeButtons[i].onClick.AddListener(() => OnMissionButtonClicked(teamID, routesID[captureIndex]));
        }


    }

    /// <summary>
    /// 루트 버튼 클릭 시, 콜백
    /// </summary>
    /// <param name="teamID"></param>
    /// <param name="routeID"></param>
    private async void OnMissionButtonClicked(string teamID, string routeID)
    {
        var dic = await FirebaseManager.Instance.GetRouteIDToMission(teamID);

        Debug.Log(dic != null);

        foreach(var a in dic)
        {
            if(a.Key == routeID)
            {
                OnMissionUIView(a.Key, a.Value); //a.Key = 루트 ID , a.Value = 미션 ID
            }
        }

    }

    [SerializeField]
    private GameObject missionImage; //미션 UI

    [SerializeField]
    private TextMeshProUGUI titleMapTMP; //맵 설명

    [SerializeField]
    Image mapImage; //맵 이미지

    [SerializeField]
    Sprite outdoorMap, floorMap_1, floorMap_2, floorMap_3;

    [SerializeField]
    private TextMeshProUGUI missionTitleTMP; //미션 제목

    [SerializeField]
    private TextMeshProUGUI missionDetailTMP; //미션 내용

    [SerializeField]
    private TextMeshProUGUI rewardTMP; //미션 보상

    /// <summary>
    /// 미션 UI 활성화
    /// </summary>
    /// <param name="routeID"></param>
    private async void OnMissionUIView(string routeID,string missionID)
    {
        missionImage.gameObject.SetActive(true);

        if(routeID == MissionMapType.outdoor.ToString())
        {
            titleMapTMP.text = $"현재 위치 야외 발전소";
            mapImage.sprite = outdoorMap;
        }

        Dictionary<string,object> missionDic =  await FirebaseManager.Instance.GetMissionAllData(missionID);

        missionTitleTMP.text = $"{missionDic["name"].ToString()}"; //미션 이름

        if (missionDic.TryGetValue("detail", out var detail))
            missionDetailTMP.text = detail.ToString();

        rewardTMP.text = $"보상: {missionDic["reward"].ToString()}wh"; //미션 보상 

    }



    /// <summary>
    /// 두번째 루트 버튼 활성화 
    /// </summary>
    public void OnSecondRouteButtonActive()
    {
        for (int i = 0; i < routeButtons.Length; i++)
        {
            if (i == 0) //첫번째 버튼
            {
                routeButtons[i].interactable = false; //두번째 버튼 제외한 나머지 버튼 클릭 불가 
            }
            if(i == 1) //2번째 버튼
            {
                routeButtons[i].interactable = true;

                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
            }
            if( i == 2 || i == 3) //3번째,4번째 버튼
            {
                routeButtons[i].interactable = false;

                routeButtons[i].GetComponent<RouteButtonItem>().ColorGrayButton();
            }
        }
        //첫번째 루트 버튼 -> 체크 표시 ,버튼 클릭 불가 
        //두번째 루트 버튼 색상 변경 -> 기존 회색에서 흰색으로 변경 , 버튼 클릭 가능 
        //나머지 버튼 -> 기존 회색 유지 , 버튼 클릭 불가
    }

    
    /// <summary>
    /// 세번째 루트 버튼 활성화 
    /// </summary>
    public void OnThirdRouteButtonActive()
    {
        for (int i = 0; i < routeButtons.Length; i++)
        {

        }
        //첫번째 루트 버튼 -> 체크 표시 , 버튼 클릭 불가 
        //두번째 루트 버튼 -> 체크 표시 , 버튼 클릭 불가

        //세번째 루트 버튼 색상 변경 -> 기존 회색에서 흰색으로 변경 , 버튼 클릭 가능 

        //나머지 버튼 -> 기존 회색 유지 , 버튼 클릭 불가
    }


    /// <summary>
    /// 네번쨰 루트 버튼 활성화 
    /// </summary>
    public void OnFourRouteButtonActive()
    {
        for (int i = 0; i < routeButtons.Length; i++)
        {

        }
        //첫번째 루트 버튼 -> 체크 표시 , 버튼 클릭 불가 
        //두번째 루트 버튼 -> 체크 표시 , 버튼 클릭 불가
        //세번째 루트 버튼 -> 체크 표시 , 버튼 클릭 불가

        //4번째 루트 버튼 색상 변경 -> 기존 회색에서 흰색으로 변경 , 버튼 클릭 가능 
    }

}
