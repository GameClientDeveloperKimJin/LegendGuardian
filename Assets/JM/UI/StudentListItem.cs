using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StudentListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtScore;
    [SerializeField] private Button btnSelect;

    private Dictionary<string, object> studentData;
    private Action<Dictionary<string, object>> onClickCallback;

    public void Setup(Dictionary<string, object> data, Action<Dictionary<string, object>> onClick)
    {
        studentData = data;
        onClickCallback = onClick;

        txtName.text = data.ContainsKey("nickname") ? data["nickname"].ToString() : "Unknown";
        txtScore.text = data.ContainsKey("score") ? data["score"].ToString() : "0";

        btnSelect.onClick.RemoveAllListeners();
        btnSelect.onClick.AddListener(() => onClickCallback?.Invoke(studentData));
    }
}
