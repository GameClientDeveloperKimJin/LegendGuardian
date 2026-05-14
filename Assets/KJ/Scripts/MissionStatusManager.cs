using Firebase.Firestore;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 미션 현황 매니저
/// </summary>
public class MissionStatusManager : MonoBehaviour
{
    [SerializeField]
    private TeacherApprovalView teacherApprovalView;

    //전체 학생
    [SerializeField]
    private TextMeshProUGUI allStudentTMP;
    //승인 대기 건수
    [SerializeField]
    private TextMeshProUGUI requestWaitTMP;
    //보상점수 wh
    [SerializeField]
    private TextMeshProUGUI scoreTMP;

    private int onlineStudentCount; //온라인 접속 학생 수

    private ListenerRegistration _listener;

    private async void Start()
    {
        //실시간으로 온라인 학생 수를 업데이트 하기 위해서 Listen() 사용
        _listener = FirebaseManager.Instance.Firestore.Collection("users").WhereEqualTo("role", "student").WhereEqualTo("isOnline", true)
          .Listen(snapshot =>
          {
              onlineStudentCount = snapshot.Count;
          });
    }
    private void OnEnable()
    {
        StartCoroutine(DealyInstance());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 클래스의 전체가 초기화 될 때까지 대기
    /// </summary>
    /// <returns></returns>
    private IEnumerator DealyInstance()
    {
        yield return new WaitUntil(() => teacherApprovalView != null);
        while (true)
        {
            UpdateMissionStatus();
            yield return new WaitForSeconds(1.5f); //1.5초 간격으로 현황 업데이트
        }
    }

    /// <summary>
    /// 현황 갱신
    /// </summary>
    private void UpdateMissionStatus()
    {

        allStudentTMP.text = $"{onlineStudentCount}명";
        requestWaitTMP.text = $"{teacherApprovalView.AllRequestCount}건";

        int score = 0;

        foreach(var rewardScore in teacherApprovalView.rewardedUserDic.Values)
        {
            score += rewardScore;
        }
        scoreTMP.text = $"{score.ToString()}wh";
    }
}
