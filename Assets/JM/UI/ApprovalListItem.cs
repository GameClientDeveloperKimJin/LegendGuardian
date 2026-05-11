using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApprovalListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtStudentName;
    [SerializeField] private TextMeshProUGUI txtMissionName;
    [SerializeField] private Button btnApprove;
    [SerializeField] private Button btnReject;
    [SerializeField] private Button btnDetail;

    private Dictionary<string, object> approvalData;

    public void Setup(Dictionary<string, object> data,
        Action<Dictionary<string, object>> onApprove,
        Action<Dictionary<string, object>> onReject,
        Action<Dictionary<string, object>> onDetail = null)
    {
        approvalData = data;

        txtStudentName.text = data.ContainsKey("studentNickname") ? data["studentNickname"].ToString() : "Unknown";
        txtMissionName.text = data.ContainsKey("missionName") ? data["missionName"].ToString() : "Unknown";

        btnApprove.onClick.RemoveAllListeners();
        btnApprove.onClick.AddListener(() => onApprove?.Invoke(approvalData));

        btnReject.onClick.RemoveAllListeners();
        btnReject.onClick.AddListener(() => onReject?.Invoke(approvalData));

        if (btnDetail != null && onDetail != null)
        {
            btnDetail.onClick.RemoveAllListeners();
            btnDetail.onClick.AddListener(() => onDetail?.Invoke(approvalData));
        }
    }
}
