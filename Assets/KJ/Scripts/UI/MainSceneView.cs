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

/// <summary>
/// 미션 UI 연출 및 미션 데이터 관리 (미션 
/// </summary>
public class MainSceneView : MonoBehaviour
{
    [Header("미션 UI 관련")]
    [SerializeField]
    Transform routeButtonContent;

    [SerializeField]
    private GameObject missionCanvas;

    [SerializeField]
    private TextMeshProUGUI titleMapTMP;

    [SerializeField]
    Image mapImage;

    [SerializeField]
    Sprite outdoorMap, floorMap_1, floorMap_2, floorMap_3;

    [SerializeField]
    private TextMeshProUGUI missionTitleTMP;

    [SerializeField]
    private TextMeshProUGUI missionDetailTMP;

    [SerializeField]
    private TextMeshProUGUI rewardTMP;

    [Header("버튼")]
    [SerializeField]
    Button routeButtonPrefab;
    [SerializeField]
    Button teacherSendBtn;

    [Header("연출 관련")]
    [SerializeField]
    GameObject waitCanvas;

    [SerializeField]
    TextMeshProUGUI LodingTMP;

    [SerializeField]
    Canvas quizeCanvas;

    private Dictionary<string, MissionData> missionDic = new(); //미션 ID : 미션 데이터

    Dictionary<string, List<string>> routeMissionDic = new(); //루트 ID : 미션 ID리스트

    private string currentRouteID;

    private string currentMissionID;

    Button[] routeButtons;

    public MissionMapClearType MissionClearType { get; private set; } = MissionMapClearType.None;

    public static Action<QuizArea> OnQuizStarted;

    [SerializeField]
    int loadingMaxCount = 3;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => AuthManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance != null);

        Init();
    }

    /// <summary>
    /// 초기화
    /// </summary>
    private async void Init()
    {
        string teamID = await AuthManager.Instance.GetUserTeamName();

        var (routesID, routesName) = await FirebaseManager.Instance.GetTeamIDToRoutes(teamID);

        routeButtons = new Button[routesID.Length];

        for (int i = 0; i < routesID.Length; i++)
        {
            routeButtons[i] = Instantiate(routeButtonPrefab, routeButtonContent);
            routeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = routesName[i];

            routeButtons[i].GetComponent<RouteButtonItem>().Init(i, routesID[i], routesName[i]); //인덱스번호, 루트ID, 루트 이름 

            await OnMissionDataSet(teamID, routesID[i]);

            int captureIndex = i; //클로저 문제 

            routeButtons[i].onClick.AddListener(() => OnMissionButtonClicked(routesID[captureIndex]));
        }

        teacherSendBtn.onClick.AddListener(OnTeacherSendBtn);

    }



    private async void OnRequestStatusChanged(string status)
    {
        if (status == "pending") return;

        //string studentPrefix = AuthManager.Instance.LoginUserID.Split('@')[0];

        FirebaseManager.Instance.StopListenMyRequest();

        // await FirebaseManager.Instance.DeleteMissionRequest(studentPrefix);

        if (status == "approved")
        {
            if (missionDic.TryGetValue(currentMissionID, out MissionData mission))
            {
                mission.IsMissionClear = true;

                //보상 지급
                long missionRewardValue = long.Parse(mission.MissionReward);

                await FirebaseManager.Instance.UpdateUserScore(AuthManager.Instance?.LoginUserID, missionRewardValue);
                await FirebaseManager.Instance.IncrementCompletedMissionCount(AuthManager.Instance?.LoginUserID);

            }

            AllMissionClear();
        }

        teacherSendBtn.interactable = true;
    }

    #region 미션 데이터 설정
    /// <summary>
    /// 미션 데이터 1회 설정
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
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }


    /// <summary>
    /// 미션 딕셔너리 데이터 설정
    /// </summary>
    /// <param name="routeID"></param>
    private async Task OnMissionData(string routeID, List<string> missionList)
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
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }
    #endregion

    #region UI 연출
    /// <summary>
    /// 미션 ui 활성화
    /// </summary>
    /// <param name="routeID"></param>
    /// <param name="missionList"></param>
    private void OnMissionView(string routeID, List<string> missionList)
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

            currentMissionID = missionID;

            missionCanvas.gameObject.SetActive(true);

            if (routeID == MissionMapType.outdoor.ToString())
            {
                titleMapTMP.text = $"현재 위치 야외";
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
    IEnumerator LoadingTMP(string missionID)
    {
        if (!missionDic.TryGetValue(missionID, out var data))
        {
            yield break;
        }

        missionCanvas.gameObject.SetActive(false);
        waitCanvas.gameObject.SetActive(true);

        LodingTMP.text = "";

        int currentLoadingCount = 0;



        while (!data.IsMissionClear)
        {
            currentLoadingCount++;

            if (currentLoadingCount > loadingMaxCount)
            {
                currentLoadingCount = 0;
                LodingTMP.text = "";
            }
            else
            {
                string tmp = new string('.', currentLoadingCount);

                LodingTMP.text = tmp;
            }

            yield return new WaitForSeconds(0.5f);
        }

        teacherSendBtn.interactable = true;
        waitCanvas.gameObject.SetActive(false);
    }
    #endregion

    /// <summary>
    /// 미션 클리어 여부 및 퀴즈 시작
    /// </summary>
    private void AllMissionClear()
    {
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

        if (Enum.IsDefined(typeof(MissionMapClearType), nextType)) //범위 체크
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

        quizeCanvas.gameObject.SetActive(true);

        switch (currentRouteID)
        {
            case nameof(MissionMapType.outdoor):
                OnQuizStarted?.Invoke(QuizArea.Outside);
                break;
            case nameof(MissionMapType.floor1):
                OnQuizStarted?.Invoke(QuizArea.Floor1);
                break;
            case nameof(MissionMapType.floor2):
                OnQuizStarted?.Invoke(QuizArea.Floor2);
                break;
            case nameof(MissionMapType.floor3):
                OnQuizStarted?.Invoke(QuizArea.Floor3);
                break;


        }
    }

    #region 각 루트 버튼 연출
    /// <summary>
    /// 2번째 미션 완료 시,
    /// </summary>
    public void OnSecondRouteButtonActive()
    {
        for (int i = 0; i < routeButtons.Length; i++)
        {
            if (i == 0)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().CheckButtonActive();
            }
            if (i == 1)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
            }
        }
    }


    /// <summary>
    /// 3번째 미션 완료 시,
    /// </summary>
    public void OnThirdRouteButtonActive()
    {
        for (int i = 0; i < routeButtons.Length; i++)
        {
            if (i == 0 || i == 1)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().CheckButtonActive();
            }
            if (i == 2)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
            }
        }
    }


    /// <summary>
    /// 4번째 미션 완료 시,
    /// </summary>
    public void OnFourRouteButtonActive()
    {
        for (int i = 0; i < routeButtons.Length; i++)
        {
            if (i == 0 || i == 1 || i == 2) //1,2,3 
            {
                routeButtons[i].GetComponent<RouteButtonItem>().CheckButtonActive();
            }
            if (i == 3)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
            }
        }

    }
    #endregion

    #region 버튼 콜백
    private void OnGUI()
    {
        if (GUILayout.Button("1번째 미션 완료"))
        {
            if (routeMissionDic.TryGetValue(currentRouteID, out var missions) && missionDic.ContainsKey(missions[0]))
            {
                missionDic[missions[0]].IsMissionClear = true;
                AllMissionClear();

            }

        }
        if (GUILayout.Button("2번째 미션 완료"))
        {
            if (routeMissionDic.TryGetValue(currentRouteID, out var missions) && missionDic.ContainsKey(missions[1]))
            {
                missionDic[missions[1]].IsMissionClear = true;
                AllMissionClear();
            }
        }
        if (GUILayout.Button("3번째 미션 완료"))
        {
            if (routeMissionDic.TryGetValue(currentRouteID, out var missions) && missionDic.ContainsKey(missions[2]))
            {
                missionDic[missions[2]].IsMissionClear = true;
                AllMissionClear();
            }
        }
    } 
    private void OnMissionButtonClicked(string routeID)
    {
        currentRouteID = routeID;

        if (!routeMissionDic.TryGetValue(routeID, out var missions))
        {
            return;
        }

        OnMissionView(routeID, missions);
    }

    private async void OnTeacherSendBtn()
    {
        if (string.IsNullOrEmpty(currentMissionID)) return;

        teacherSendBtn.interactable = false;

        string studentPrefix = AuthManager.Instance.LoginUserID.Split('@')[0]; //wls6189 
        string teamId = await AuthManager.Instance.GetUserTeamName();

        await FirebaseManager.Instance.ClearMyPendingRequest(studentPrefix);

        await FirebaseManager.Instance.SendMissionApprovalRequest(
            studentPrefix,
            AuthManager.Instance.LoginUserID,
            AuthManager.Instance.LoginUserName,
            teamId,
            currentMissionID,
            missionDic[currentMissionID].MissionName,
            missionDic[currentMissionID].MissionReward,
            currentRouteID
        );

        FirebaseManager.Instance.ListenMyRequest(studentPrefix, OnRequestStatusChanged);

        StartCoroutine(LoadingTMP(currentMissionID));
    }

    #endregion

    private async void OnDestroy()
    {
        teacherSendBtn.onClick.RemoveAllListeners();

        for(int i = 0; i< routeButtons.Length; i++)
        {
            routeButtons[i].onClick.RemoveAllListeners();
        }

        string studentPrefix = AuthManager.Instance.LoginUserID.Split('@')[0];

        await FirebaseManager.Instance.DeleteMissionRequest(studentPrefix);

        FirebaseManager.Instance?.StopListenMyRequest();
    }
}
