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
    //지급한 총 wh
    [SerializeField]
    private TextMeshProUGUI scoreTMP;

    private int onlineStudentCount; //온라인 중인 학생 수 

    private async void Start()
    {
        //role이 student 이고, isOnline이 true인 학생만 필터링하여 users에 데이터 가져온다.
        var snapshot = await FirebaseManager.Instance.Firestore.Collection("users").
            WhereEqualTo("role", "student").WhereEqualTo("isOnline", true).GetSnapshotAsync();

        onlineStudentCount = snapshot.Count;
    }
    /// <summary>
    /// 선생님 클래스 객체가 초기화 될 때까지 대기
    /// </summary>
    /// <returns></returns>
    private IEnumerator DealyInstance()
    {
        yield return new WaitUntil(() => teacherApprovalView != null);
        while (true)
        {
            UpdateMissionStatus();
            yield return new WaitForSeconds(1.5f); //1.5초 간격으로 상태 업데이트
        }
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
    /// 상태 업뎃
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
