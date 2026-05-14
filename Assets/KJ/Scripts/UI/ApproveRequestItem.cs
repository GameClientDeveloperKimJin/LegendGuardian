using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 승인 요청 아이템 프리펩 , UI 시각화
/// </summary>
public class ApproveRequestItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI studentNameTMP; //요청한 학생 이름
    [SerializeField] private TextMeshProUGUI missionNameTMP; //요청한 미션 이름
    [SerializeField] private TextMeshProUGUI missionRewardTMP; //요청한 미션 보상

    [SerializeField] private Button approveButton; //수락
    [SerializeField] private Button rejectButton; //거절

    private MissionRequestData requestData; //미션 요청 데이터

    public void Init(TeacherApprovalView approvalView, MissionRequestData requestData)
    {
        this.requestData = requestData;

        studentNameTMP.text = $"이름: {this.requestData.StudentName}";
        missionNameTMP.text = $"미션: {this.requestData.MissionName}";
        missionRewardTMP.text = $"보상: {this.requestData.MissionReward}";

        approveButton.onClick.AddListener(() =>
        {
            OnApprove();
            approvalView.ClearRequestItem();
        });
        rejectButton.onClick.AddListener(() =>
        {
            OnReject();
            approvalView.ClearRequestItem();
        });
    }

    private async void OnApprove()
    {
        approveButton.interactable = false;
        rejectButton.interactable = false;

        await FirebaseManager.Instance.ApproveRequest(requestData.StudentPrefix); // wls6189
    }

    private async void OnReject()
    {
        approveButton.interactable = false;
        rejectButton.interactable = false;

        await FirebaseManager.Instance.RejectRequest(requestData.StudentPrefix);

    }

    private void OnDestroy()
    {
        approveButton.onClick.RemoveAllListeners();
        rejectButton.onClick.RemoveAllListeners();
    }

}
