using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    None,Clear1,Clear2,Clear3,Clear4
}

public class MissionData
{
    public string MissionName;
    public string MissionID;
    public string MissionDetail;
    public long MissionReward;
    public bool IsMissionClear;
}

/// <summary>
/// 미션 UI 연출 및 미션 데이터 관리 (미션
/// </summary>
public class MainSceneView : MonoBehaviour
{
    #region 직렬화 필드 - 미션 UI
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
    #endregion

    #region 직렬화 필드 - 버튼
    [Header("버튼")]
    [SerializeField]
    Button routeButtonPrefab;
    [SerializeField]
    Button teacherSendBtn;
    [SerializeField]
    private Button backButton;
    #endregion

    #region 직렬화 필드 - 연출
    [Header("연출 관련")]
    [SerializeField]
    GameObject waitCanvas;

    [SerializeField]
    TextMeshProUGUI LodingTMP;

    [SerializeField]
    Canvas quizeCanvas;

    [SerializeField]
    GameObject WaitForNextAreaImage;
    [SerializeField]
    TextMeshProUGUI lodingNextTMP;

    [SerializeField]
    int loadingMaxCount = 3;
    #endregion

    #region 이벤트
    public static Action<QuizArea> OnQuizStarted;
    public static Action<bool> OnButtonEvent; //버튼 입력 기능 활성화/비활성화 이벤트
    #endregion

    #region 내부 상태
    private Dictionary<string, MissionData> missionDic = new(); //미션 ID : 미션 데이터

    Dictionary<string, List<string>> routeMissionDic = new(); //루트 ID : 미션 ID리스트

    private string currentRouteID;

    private string currentMissionID;

    // [Analytics] 미션 이벤트 파라미터용 팀 ID 캐싱
    private string _currentTeamID;

    Button[] routeButtons;

    public MissionMapClearType MissionClearType { get; private set; } = MissionMapClearType.None;
    #endregion

    #region 라이프사이클
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => AuthManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance != null);

        AuthManager.OnForceQuit += HandleForceQuit;

        Init();
    }

    /// <summary>
    /// 초기화
    /// </summary>
    private async void Init()
    {
        string teamID = await AuthManager.Instance.GetUserTeamName();

        // [Analytics] 팀 ID 캐싱
        _currentTeamID = teamID;

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

        backButton.onClick.AddListener(OnBackButtonClicked);
        teacherSendBtn.onClick.AddListener(OnTeacherSendBtn);
    }

    private async void OnDestroy()
    {
        teacherSendBtn.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();

        for (int i = 0; i< routeButtons.Length; i++)
        {
            routeButtons[i].onClick.RemoveAllListeners();
        }

        string studentPrefix = AuthManager.Instance.LoginUserID.Split('@')[0];

        await FirebaseManager.Instance.DeleteMissionRequest(studentPrefix);

        FirebaseManager.Instance?.StopListenMyRequest();
        AuthManager.OnForceQuit -= HandleForceQuit;
    }
    #endregion

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
                        MissionReward = missionAllDic.TryGetValue("reward", out var reward) ? ParseReward(reward) : 0L,


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

    private long ParseReward(object reward)
    {
        string raw = reward?.ToString()?.Trim().ToLower().Replace("wh", "") ?? "0";
        return long.TryParse(raw, out long result) ? result : 0L;
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
            if (!missionDic.TryGetValue(missionID, out var mission)) continue;
            if (mission.IsMissionClear) continue;

            currentMissionID = missionID;

            // [Analytics] 미션 진입 이벤트 (Funnel 4단계)
            AnalyticsManager.Instance?.LogMissionStarted(
                missionID,
                missionDic[missionID].MissionName,
                _currentTeamID,
                missionDic[missionID].MissionReward);

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

        OnButtonEvent.Invoke(false);

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

        OnButtonEvent.Invoke(true);

        teacherSendBtn.interactable = true;
        waitCanvas.gameObject.SetActive(false);
    }
    #endregion

    #region 미션 클리어
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
                case MissionMapClearType.Clear3:
                    OnFourRouteButtonActive();
                    break;
                case MissionMapClearType.Clear4:
                    OnFiveRouteButtonActive();

                    FirebaseManager.Instance.StopListenRewardResult();
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
    #endregion

    #region 각 루트 버튼 연출
    /// <summary>
    /// 2번째 미션 완료 시,
    /// </summary>
    public void OnSecondRouteButtonActive()
    {
        ButtonInfo();

        for (int i = 0; i < routeButtons.Length; i++)
        {
            if (i == 0)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorGrayButton();
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
        ButtonInfo();

        for (int i = 0; i < routeButtons.Length; i++)
        {
            if (i == 0 || i == 1)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorGrayButton();
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
        ButtonInfo();

        for (int i = 0; i < routeButtons.Length; i++)
        {
            if (i == 0 || i == 1 || i == 2) //1,2,3
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorGrayButton();
                routeButtons[i].GetComponent<RouteButtonItem>().CheckButtonActive();
            }
            if (i == 3)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
            }
        }

    }
    public void OnFiveRouteButtonActive()
    {
        ButtonInfo();

        for (int i = 0; i < routeButtons.Length; i++)
        {
            if (i == 0 || i == 1 || i == 2 || i == 3) //1,2,3
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorGrayButton();
                routeButtons[i].GetComponent<RouteButtonItem>().CheckButtonActive();
            }
            if (i == 4)
            {
                routeButtons[i].GetComponent<RouteButtonItem>().ColorWhiteButton();
            }
        }

    }

    private void ButtonInfo()
    {
        for (int i = 0; i < routeButtons.Length; i++)
        {
            Debug.Log($"인덱스 : {i} - 버튼 이름 : {routeButtons[i].gameObject.name}");
        }
    }
    #endregion

    #region 버튼 콜백
    private void OnMissionButtonClicked(string routeID)
    {

        StartCoroutine(WaitForNextArea(routeID));

    }

    IEnumerator WaitForNextArea(string routeID)
    {

        currentRouteID = routeID;

        Debug.Log($"[클릭] routeID: {routeID} / routeMissionDic 보유 키: {string.Join(", ", routeMissionDic.Keys)}");

        if (!routeMissionDic.TryGetValue(routeID, out var missions))
        {
            Debug.LogWarning($"{routeID} 키가 routeMissionDic에 없음");
            yield break;
        }

        WaitForNextAreaImage.gameObject.SetActive(true);

        OnButtonEvent?.Invoke(false);

        int currentLoadingCount = 0;

        while (currentLoadingCount < loadingMaxCount)
        {
            currentLoadingCount++;

            string tmp = new string('.', currentLoadingCount);

            lodingNextTMP.text = tmp;

            yield return new WaitForSeconds(0.5f);
        }

        currentLoadingCount = 0;
        lodingNextTMP.text = "";

        OnButtonEvent?.Invoke(true);
        WaitForNextAreaImage.gameObject.SetActive(false);

        OnMissionView(routeID, missions);
    }

    private async void OnTeacherSendBtn()
    {
        if (string.IsNullOrEmpty(currentMissionID)) return;

        teacherSendBtn.interactable = false;

        string studentPrefix = AuthManager.Instance.LoginUserID.Split('@')[0]; //wls6189
        string teamId = await AuthManager.Instance.GetUserTeamName();

        await FirebaseManager.Instance.ClearMyPendingRequest(studentPrefix);

        // [Analytics] 승인 요청 이벤트 (Funnel 6단계)
        AnalyticsManager.Instance?.LogApprovalRequested(
            currentMissionID,
            missionDic[currentMissionID].MissionReward);

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

    /// <summary>
    /// 미션 대기 화면 뒤로가기 버튼
    /// </summary>
    private void OnBackButtonClicked()
    {
        HandleForceQuit();
    }
    #endregion

    #region Firebase 콜백
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
                await FirebaseManager.Instance.UpdateUserScore(AuthManager.Instance?.LoginUserID, mission.MissionReward);
                await FirebaseManager.Instance.IncrementCompletedMissionCount(AuthManager.Instance?.LoginUserID);

                // [Analytics] 전환 이벤트: 보상 수령 확정 (Funnel 최종 단계)
                AnalyticsManager.Instance?.LogRewardClaimed(currentMissionID, mission.MissionReward);



            }

            AllMissionClear();
        }

        teacherSendBtn.interactable = true;
    }

    /// <summary>
    /// AuthManger에서 어플 강제 종료 이벤트 발생 시, 대기화면 비활성화 및 승인 요청 중인 경우 status를 rejected로 변경
    /// </summary>
    private async void HandleForceQuit()
    {
        bool isWaiting = (waitCanvas != null && waitCanvas.activeSelf)
                      || (WaitForNextAreaImage != null && WaitForNextAreaImage.activeSelf);

        if (!isWaiting) return;

        StopAllCoroutines();

        if (waitCanvas != null)
        {
            waitCanvas.gameObject.SetActive(false);

            string studentPrefix = AuthManager.Instance.LoginUserID.Split('@')[0];
            await FirebaseManager.Instance.AppLogout(studentPrefix);
        }

        if (WaitForNextAreaImage != null) WaitForNextAreaImage.gameObject.SetActive(false);
    }
    #endregion
}
