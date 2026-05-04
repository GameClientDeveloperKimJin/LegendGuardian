using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        FirebaseManager.Instance.OnScoreUpdated += OnScoreUpdate;
    }

    private void OnDestroy()
    {
        FirebaseManager.Instance.OnScoreUpdated -= OnScoreUpdate;
    }

    Button[] routeButtons;

    /// <summary>
    /// 메인 씬 전환 시, 초기화 할 내용 ( 1회성)
    /// </summary>
    private async void Init()
    {
        // 닉네임 적용 및 wh 적용 
        nickNameTMP.text = AuthManager.Instance.LoginUserName;

        scoreTMP.text = await FirebaseManager.Instance.GetUserIDToScore(AuthManager.Instance?.LoginUserID);

        // 팀에 맞는 루트 버튼 동적 생성
        string teamID = await AuthManager.Instance.GetUserTeamName();

        var (routesID,routesName) = await FirebaseManager.Instance.GetTeamIDToRoutes(teamID);

        routeButtons = new Button[routesID.Length];

        for (int i = 0; i < routesID.Length; i++)
        {
            routeButtons[i] = Instantiate(routeButtonPrefab, routeButtonContent);
            routeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = routesName[i];

            routeButtons[i].GetComponent<RouteButtonItem>().Init(i,routesID[i], routesName[i]); //버튼 인덱스 번호, 루트 ID , 루트 네임
        }


    }

    /// <summary>
    /// 두번째 루트 버튼 활성화 
    /// </summary>
    public void OnSecondRouteButtonActive()
    {
        for (int i = 0; i < routeButtons.Length; i++)
        {

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


    /// <summary>
    /// 업데이트 된 wH 적용
    /// </summary>
    /// <param name="score"></param>
    private void OnScoreUpdate(long score)
    {
        scoreTMP.text = score.ToString();
    }

}
