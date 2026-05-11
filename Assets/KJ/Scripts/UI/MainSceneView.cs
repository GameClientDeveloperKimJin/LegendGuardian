using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
public enum MissionMapClearType
{
    None,Clear1,Clear2
}

public class MissionData
{
    public string MissionName;
    public string MissionID;
    public string MissionDetail;
    public string MissionReward;
    public bool IsMissionClear;
}
public class MainSceneView : MonoBehaviour 
{
    [SerializeField]
    Button routeButtonPrefab; //생성할 버튼 프리펩

    [SerializeField]
    Transform routeButtonContent; //생성할 버튼의 위치


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

    Button[] routeButtons;

    private Dictionary<string, MissionData> missionDic = new(); //미션 ID : 미션 데이터 = 키 : 값

    Dictionary<string, List<string>> routeMissionDic = new(); //루트 ID : 미션 리스트 = 키 : 값

    private string currentRouteID;

    public MissionMapClearType MissionClearType { get; private set; } = MissionMapClearType.None; //초기값: 미 클리어 상태

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => AuthManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
       
        Init();
    }

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

            await OnMissionDataSet(teamID, routesID[i]);

            int captureIndex = i; //클로저 문제

            routeButtons[i].onClick.AddListener(() => OnMissionButtonClicked(routesID[captureIndex]) );
        }


    }

    /// <summary>
    /// 현재 루트를 필터링하여 미션 데이터 세팅
    /// </summary>
    /// <param name="teamID"></param>
    /// <param name="routeID"></param>
    private async Task OnMissionDataSet(string teamID, string routeID)
    {
        try
        {
            Dictionary<string, List<string>> dic = await FirebaseManager.Instance.GetRouteIDToMission(teamID);

            foreach (var a in dic)
            {
                if (routeID == a.Key)
                {
                    foreach (string missionID in a.Value)
                    {
                        await OnMissionData(a.Key, a.Value);
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }

    }


    /// <summary>
    /// 로컬 데이터로 미션 데이터들을 저장
    /// </summary>
    /// <param name="routeID"></param>
    private async Task OnMissionData(string routeID,List<string> missionList)
    {
        try
        {
            routeMissionDic[routeID] = new List<string>(missionList);

            foreach (string missionID in missionList)
            {
                if (string.IsNullOrEmpty(missionID)) continue;

                if (!missionDic.ContainsKey(missionID))
                {
                    Dictionary<string, object> missionAllDic = await FirebaseManager.Instance.GetMissionAllData(missionID);

                    missionDic[missionID] = new MissionData()
                    {
                        MissionName = missionAllDic.TryGetValue("name", out var name) ? name.ToString() : "",
                        MissionDetail = missionAllDic.TryGetValue("detail", out var detail) ? detail.ToString() : "",
                        MissionReward = missionAllDic.TryGetValue("reward", out var reward) ? reward.ToString() : "",
                        MissionID = missionID,

                        IsMissionClear = false,
                    };
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
      
    }

    private void OnMissionButtonClicked(string routeID)
    {
        currentRouteID = routeID;

        if(!routeMissionDic.TryGetValue(routeID,out var missions))
        {
            return;
        }

        OnMissionView(routeID, missions);
    }



    /// <summary>
    /// 미션 UI 활성화 -> 데이터 적용
    /// </summary>
    /// <param name="routeID"></param>
    /// <param name="missionList"></param>
    private void OnMissionView(string routeID,List<string> missionList)
    {
        foreach (string missionID in missionList)
        {
            if (string.IsNullOrEmpty(missionID))
            {
                continue;
            }
            if (missionDic[missionID].IsMissionClear)
            {
                continue;
            }

            missionImage.gameObject.SetActive(true);

            if (routeID == MissionMapType.outdoor.ToString())
            {
                titleMapTMP.text = $"현재 위치 야외 발전소";
                mapImage.sprite = outdoorMap;
            }
            if (routeID == MissionMapType.floor1.ToString())
            {
                titleMapTMP.text = $"현재 위치 1층";
                mapImage.sprite = floorMap_1;
            }

            missionTitleTMP.text = missionDic[missionID].MissionName;
            missionDetailTMP.text = missionDic[missionID].MissionDetail;
            rewardTMP.text = $"보상 : {missionDic[missionID].MissionReward} wh";
            return;

        }
    }

    /// <summary>
    /// 루트에 속한 미션들을 모두 클리어 했을 때 로직
    /// </summary>
    private void AllMissionClear()
    {
        //루트에 속한 미션 올 클리어 ( 위 return 실행 안됐을 때 )
        if (!routeMissionDic.TryGetValue(currentRouteID, out var missions)) return;

        bool allClear = true;

        foreach (string missionID in missions)
        {
            if (!missionDic[missionID].IsMissionClear)
            {
                allClear = false; 
                break; 
            }
        }

        if (!allClear)
        {
            return;
        }

        MissionMapClearType currentType = MissionClearType;
        MissionMapClearType nextType = (MissionMapClearType)currentType + 1;

        if (Enum.IsDefined(typeof(MissionMapClearType), nextType)) //MissionMapClearType 안에 nextType 값이 존재하면 true
        {
            currentType = (MissionMapClearType)nextType;
            MissionClearType = currentType;

            switch (MissionClearType)
            {
                case MissionMapClearType.Clear1:
                    OnSecondRouteButtonActive();
                    break;
                case MissionMapClearType.Clear2:
                    OnThirdRouteButtonActive();
                    break;
            }
        }

        switch (currentRouteID)
        {
            case nameof(MissionMapType.outdoor):
                break;
            case nameof(MissionMapType.floor1):
                break;
            case nameof(MissionMapType.floor2):
                break;
            case nameof(MissionMapType.floor3):
                break;
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("첫번째 미션 강제 완료 시키기"))
        {
            if (routeMissionDic.TryGetValue(currentRouteID,out var missions) && missionDic.ContainsKey(missions[0]))
            {
                missionDic[missions[0]].IsMissionClear = true;
                AllMissionClear();

                
            }

        }
        if (GUILayout.Button("두번째 미션 강제 완료 시키기"))
        {
            if (routeMissionDic.TryGetValue(currentRouteID, out var missions) && missionDic.ContainsKey(missions[1]))
            {
                missionDic[missions[1]].IsMissionClear = true;
                AllMissionClear();
            }
        }
        if (GUILayout.Button("세번째 미션 강제 완료 시키기"))
        {
            if (routeMissionDic.TryGetValue(currentRouteID, out var missions) && missionDic.ContainsKey(missions[2]))
            {
                missionDic[missions[2]].IsMissionClear = true;
                AllMissionClear();
            }
        }
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
                routeButtons[i].GetComponent<RouteButtonItem>().CheckButtonActive();
            }
            if(i == 1) //2번째 버튼
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
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
            if (i == 0 || i == 1) //1,2번째 버튼
            {
                routeButtons[i].GetComponent<RouteButtonItem>().CheckButtonActive();
            }
            if (i == 2) //3번째 버튼
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
            }
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
            if (i == 0 || i == 1 || i == 2) //1,2,3 번째 버튼
            {
                routeButtons[i].GetComponent<RouteButtonItem>().CheckButtonActive();
            }
            if (i == 3) //4번째 버튼
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
            }
        }
        //첫번째 루트 버튼 -> 체크 표시 , 버튼 클릭 불가 
        //두번째 루트 버튼 -> 체크 표시 , 버튼 클릭 불가
        //세번째 루트 버튼 -> 체크 표시 , 버튼 클릭 불가

        //4번째 루트 버튼 색상 변경 -> 기존 회색에서 흰색으로 변경 , 버튼 클릭 가능 
    }

}
