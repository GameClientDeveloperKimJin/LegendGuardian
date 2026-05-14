using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 승인 요청 UI 
/// </summary>
public class ApproveRequestItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI studentNameTMP; //요청한 학생 이름
    [SerializeField] private TextMeshProUGUI missionNameTMP; //요청한 미션 이름
    [SerializeField] private Button approveButton; //수락
    [SerializeField] private Button rejectButton; //거절

    private MissionRequestData requestData; //미션 요청 데이터

    public void Init(MissionRequestData requestData)
    {
        this.requestData = requestData;

        studentNameTMP.text = this.requestData.StudentName;
        missionNameTMP.text = this.requestData.MissionName;

        approveButton.onClick.AddListener(OnApprove);
        rejectButton.onClick.AddListener(OnReject);
    }

    private async void OnApprove()
    {
        approveButton.interactable = false;
        rejectButton.interactable = false;

        await FirebaseManager.Instance.ApproveRequest(requestData.StudentPrefix);

        Destroy(this.gameObject);
    }

    private async void OnReject()
    {
        approveButton.interactable = false;
        rejectButton.interactable = false;

        await FirebaseManager.Instance.RejectRequest(requestData.StudentPrefix);

        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        approveButton.onClick.RemoveAllListeners();
        rejectButton.onClick.RemoveAllListeners();
    }

}
