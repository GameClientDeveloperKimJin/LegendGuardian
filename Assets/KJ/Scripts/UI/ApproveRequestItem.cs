using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ��û ������ ������ , UI �ð�ȭ
/// </summary>
public class ApproveRequestItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI studentNameTMP; //��û�� �л� �̸�
    [SerializeField] private TextMeshProUGUI missionNameTMP; //��û�� �̼� �̸�
    [SerializeField] private TextMeshProUGUI missionRewardTMP; //��û�� �̼� ����

    [SerializeField] private Button approveButton; //����
    [SerializeField] private Button rejectButton; //����

    private MissionRequestData requestData; //�̼� ��û ������

    public  void Init(TeacherApprovalView approvalView, MissionRequestData requestData)
    {
        this.requestData = requestData;

        studentNameTMP.text = $"�̸�: {this.requestData.StudentName}";
        missionNameTMP.text = $"�̼�: {this.requestData.MissionName}";
        missionRewardTMP.text = $"����: {this.requestData.MissionReward}";

        //���� ��ư
        approveButton.onClick.AddListener(async () =>
        {
            bool isOnline = await FirebaseManager.Instance.GetIsExitUser(this.requestData.StudentID);
            Debug.Log("�ش� ���� �¶��� ����" + isOnline);
            if (!isOnline)
            {
                approvalView.ClearRequestItem();
                approvalView.NotUser();
                return;
            }

            OnApprove();
            approvalView.ClearRequestItem();

            approvalView.CompensationPaid(requestData.StudentPrefix, int.Parse(this.requestData.MissionReward)); 
        });
        //���� ��ư
        rejectButton.onClick.AddListener(async  () =>
        {
            bool isOnline = await FirebaseManager.Instance.GetIsExitUser(this.requestData.StudentID);
            if (!isOnline)
            {
                approvalView.ClearRequestItem();
                approvalView.NotUser();
                return;
            }

            OnReject();
            approvalView.ClearRequestItem();
        });
    }

    private async void OnApprove()
    {
        approveButton.interactable = false;
        rejectButton.interactable = false;

        // [Analytics] 선생님 승인 행동 이벤트
        AnalyticsManager.Instance?.LogApprovalAction("approved", requestData.MissionID, requestData.MissionReward);

        await FirebaseManager.Instance.ApproveRequest(requestData.StudentPrefix); // wls6189
    }

    private async void OnReject()
    {
        approveButton.interactable = false;
        rejectButton.interactable = false;

        // [Analytics] 선생님 거절 행동 이벤트
        AnalyticsManager.Instance?.LogApprovalAction("rejected", requestData.MissionID, requestData.MissionReward);

        await FirebaseManager.Instance.RejectRequest(requestData.StudentPrefix);

    }

    private void OnDestroy()
    {
        approveButton.onClick.RemoveAllListeners();
        rejectButton.onClick.RemoveAllListeners();
    }

}
