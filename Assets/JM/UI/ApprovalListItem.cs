using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApprovalListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtStudentName;
    [SerializeField] private TextMeshProUGUI txtMissionName;
    [SerializeField] private Button btnDetail; // 확인하기

    private Dictionary<string, object> approvalData;

    public void Setup(Dictionary<string, object> data, Action<Dictionary<string, object>> onDetail)
    {
        approvalData = data;

        txtStudentName.text = data.ContainsKey("studentNickname") ? data["studentNickname"].ToString() : "Unknown";
        txtMissionName.text = data.ContainsKey("missionName") ? data["missionName"].ToString() : "Unknown";

        btnDetail.onClick.RemoveAllListeners();
        btnDetail.onClick.AddListener(() => onDetail?.Invoke(approvalData));
    }
}
