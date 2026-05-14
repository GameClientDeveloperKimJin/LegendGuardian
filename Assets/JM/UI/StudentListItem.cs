using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StudentListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtTeam;
    [SerializeField] private TextMeshProUGUI txtSumReward;
    [SerializeField] private Button btnSelect;

    private Dictionary<string, object> studentData;
    private Action<Dictionary<string, object>> onClickCallback;

    public string NickName => txtName.text;

    public void Setup(Dictionary<string, object> data, Action<Dictionary<string, object>> onClick)
    {
        studentData = data;
        onClickCallback = onClick;

        txtName.text = data.ContainsKey("nickname") ? data["nickname"].ToString() : "Unknown";
        txtTeam.text = data.ContainsKey("teamId") ? data["teamId"].ToString() : "없음";

        long completedCount = data.ContainsKey("completedMissionCount") ? Convert.ToInt64(data["completedMissionCount"]) : 0;
        long score = data.ContainsKey("score") ? Convert.ToInt64(data["score"]) : 0;
        txtSumReward.text = $"완료 {completedCount}건 / 보상 : {score}wh";

        btnSelect.onClick.RemoveAllListeners();
        btnSelect.onClick.AddListener(() => onClickCallback?.Invoke(studentData));
    }
}
