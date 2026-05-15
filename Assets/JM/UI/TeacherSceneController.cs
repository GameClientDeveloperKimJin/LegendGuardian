using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeacherSceneController : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private GameObject canvasMain;
    [SerializeField] private GameObject canvasStudent;
    [SerializeField] private GameObject canvasApprovalWaiting;
    [SerializeField] private GameObject canvasCheckApproval;

    [Header("Student Info Panel (Canvas_Student 내부 Cnavas_StudentInfo)")]
    [SerializeField] private GameObject studentInfoPanel;

    [Header("Student Search")]
    [SerializeField] private InputField searchInputField;

    [Header("Student List")]
    [SerializeField] private Transform studentListParent; // Grp_Middle
    [SerializeField] private GameObject studentListItemPrefab;

    [Header("Student Info")]
    [SerializeField] private TextMeshProUGUI txtInfoName;
    [SerializeField] private TextMeshProUGUI txtInfoTeam;
    [SerializeField] private Image imgStudent;

    [Header("Approval Floor Filter (Grp_High)")]
    [SerializeField] private Button btnFloorAll;    // Btn_All (전체)
    [SerializeField] private Button btnFloor1;      // Btn_Floor1 (야외)
    [SerializeField] private Button btnFloor2;      // Btn_Floor2 (1층)
    [SerializeField] private Button btnFloor3;      // Btn_Floor3 (2층)
    [SerializeField] private Button btnFloor4;      // Btn_Floor4 (3층)

    [Header("Approval List")]
    [SerializeField] private Transform approvalListParent;
    [SerializeField] private GameObject approvalListItemPrefab;

    [Header("Check Approval Detail")]
    [SerializeField] private TextMeshProUGUI txtMissionInfo;
    [SerializeField] private Image imgMissionData;
    [SerializeField] private Button btnApprove;   // Btn_Wh (승인)
    [SerializeField] private Button btnReject;    // Btn_Delete (거절)

    private Dictionary<string, object> selectedApproval;
    private Dictionary<string, object> selectedStudent;
    private List<Dictionary<string, object>> cachedApprovals = new();
    private string currentFloorFilter = "all"; // "all", "floor1"~"floor4"

    private List<Dictionary<string, object>> cachedStudents = new();

    private void Start()
    {
        LoadStudents();
        LoadPendingApprovals();

        if (btnApprove != null)
            btnApprove.onClick.AddListener(OnApproveClicked);
        if (btnReject != null)
            btnReject.onClick.AddListener(OnRejectClicked);

        // 층 필터 버튼
        if (btnFloorAll != null) btnFloorAll.onClick.AddListener(() => FilterByFloor("all"));
        if (btnFloor1 != null) btnFloor1.onClick.AddListener(() => FilterByFloor("floor1"));
        if (btnFloor2 != null) btnFloor2.onClick.AddListener(() => FilterByFloor("floor2"));
        if (btnFloor3 != null) btnFloor3.onClick.AddListener(() => FilterByFloor("floor3"));
        if (btnFloor4 != null) btnFloor4.onClick.AddListener(() => FilterByFloor("floor4"));

        // 검색 InputField 이벤트 등록
        if (searchInputField != null)
            searchInputField.onValueChanged.AddListener(OnSearchValueChanged);

        // 학생 상세 패널 초기 비활성화
        if (studentInfoPanel != null)
            studentInfoPanel.SetActive(false);
    }

    #region Student List

    public async void LoadStudents()
    {
        if (!FirebaseManager.Instance.IsConnect) return;

        cachedStudents = await FirebaseManager.Instance.GetAllStudents();
        DisplayStudents(cachedStudents);
    }

    private void DisplayStudents(List<Dictionary<string, object>> students)
    {
        foreach (Transform child in studentListParent)
            Destroy(child.gameObject);

        foreach (var student in students)
        {
            var item = Instantiate(studentListItemPrefab, studentListParent);
            var listItem = item.GetComponent<StudentListItem>();
            listItem.Setup(student, ShowStudentInfo);
        }
    }

    private void OnSearchValueChanged(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            DisplayStudents(cachedStudents);
            return;
        }

        var filtered = cachedStudents.FindAll(s =>
        {
            string nickname = s.ContainsKey("nickname") ? s["nickname"].ToString() : "";
            return nickname.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        });

        DisplayStudents(filtered);
    }

    public void ShowStudentInfo(Dictionary<string, object> studentData)
    {
        selectedStudent = studentData;

        if (txtInfoName != null)
            txtInfoName.text = studentData.ContainsKey("nickname") ? studentData["nickname"].ToString() : "";
        if (txtInfoTeam != null)
            txtInfoTeam.text = studentData.ContainsKey("teamId") ? studentData["teamId"].ToString() : "";

        // Canvas_Student 내부의 Cnavas_StudentInfo 패널을 활성화
        if (studentInfoPanel != null)
            studentInfoPanel.SetActive(true);
    }

    public void HideStudentInfo()
    {
        if (studentInfoPanel != null)
            studentInfoPanel.SetActive(false);
    }

    #endregion

    #region Approval List

    
    public async void LoadPendingApprovals()
    {
        if (!FirebaseManager.Instance.IsConnect) return;

        cachedApprovals = await FirebaseManager.Instance.GetPendingApprovals();
        DisplayApprovals();
    }

    private void FilterByFloor(string floor)
    {
        currentFloorFilter = floor;
        DisplayApprovals();
    }

    private void DisplayApprovals()
    {
        foreach (Transform child in approvalListParent)
            Destroy(child.gameObject);

        Debug.Log($"DisplayApprovals: {cachedApprovals.Count}개 승인 데이터");

        foreach (var approval in cachedApprovals)
        {
            var item = Instantiate(approvalListItemPrefab, approvalListParent);
            var listItem = item.GetComponent<ApprovalListItem>();
            if (listItem == null)
            {
                Debug.LogError("프리팹에 ApprovalListItem 컴포넌트가 없습니다!");
                continue;
            }
            listItem.Setup(approval, ShowApprovalDetail);
        }
    }

    private void ShowApprovalDetail(Dictionary<string, object> approvalData)
    {
        selectedApproval = approvalData;

        if (txtMissionInfo != null)
        {
            string missionName = approvalData.ContainsKey("missionName") ? approvalData["missionName"].ToString() : "";
            string studentName = approvalData.ContainsKey("studentNickname") ? approvalData["studentNickname"].ToString() : "";
            string reward = approvalData.ContainsKey("reward") ? approvalData["reward"].ToString() : "0";
            txtMissionInfo.text = $"학생: {studentName}\n미션: {missionName}\n보상: {reward}";
        }

        ShowCheckApproval();
    }

    public async void OnApproveClicked()
    {
        if (selectedApproval == null) return;

        string docId = selectedApproval["docId"].ToString();
        string email = selectedApproval.ContainsKey("studentEmail") ? selectedApproval["studentEmail"].ToString() : "";
        long reward = selectedApproval.ContainsKey("reward") ? (long)selectedApproval["reward"] : 0;

        await FirebaseManager.Instance.ApproveRequest(docId, email, reward);
        selectedApproval = null;
        BackToApprovalList();
    }

    public async void OnRejectClicked()
    {
        if (selectedApproval == null) return;

        string docId = selectedApproval["docId"].ToString();
        await FirebaseManager.Instance.RejectRequest(docId);
        selectedApproval = null;
        BackToApprovalList();
    }

    #endregion

    #region Canvas Navigation

    private void ShowOnly(GameObject target)
    {
        // TeacherCanvas_Main은 항상 Active 유지
        canvasMain.SetActive(true);
        canvasStudent.SetActive(target == canvasStudent);
        canvasApprovalWaiting.SetActive(target == canvasApprovalWaiting);
        canvasCheckApproval.SetActive(target == canvasCheckApproval);

        if (studentInfoPanel != null)
            studentInfoPanel.SetActive(false);
    }

    // Canvas_Main 버튼용
    public void GoToStudentCanvas()
    {
        ShowOnly(canvasStudent);
        LoadStudents();
    }

    public void GoToApprovalCanvas()
    {
        ShowOnly(canvasApprovalWaiting);
        LoadPendingApprovals();
    }

    public void BackToMain()
    {
        ShowOnly(canvasMain);
    }

    // 승인 상세 전환
    public void ShowCheckApproval()
    {
        Debug.Log($"ShowCheckApproval 호출됨 - canvasCheckApproval: {canvasCheckApproval}, null?: {canvasCheckApproval == null}");
        ShowOnly(canvasCheckApproval);
        Debug.Log($"Canvas_CheckApproval active: {canvasCheckApproval.activeSelf}");
    }

    public void BackToApprovalList()
    {
        ShowOnly(canvasApprovalWaiting);
        LoadPendingApprovals();
    }

    public void BackToStudentList()
    {
        HideStudentInfo();
    }

    #endregion
}
