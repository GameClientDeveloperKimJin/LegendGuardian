using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeacherSceneController : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject canvasStudent;
    [SerializeField] private GameObject canvasStudentInfo;
    [SerializeField] private GameObject canvasApprovalWaiting;
    [SerializeField] private GameObject canvasCheckApproval;
    [SerializeField] private GameObject canvasSettings;

    [Header("Student List")]
    [SerializeField] private Transform studentListParent; // Grp_Middle
    [SerializeField] private GameObject studentListItemPrefab;

    [Header("Student Info")]
    [SerializeField] private TextMeshProUGUI txtInfoName;
    [SerializeField] private TextMeshProUGUI txtInfoScore;
    [SerializeField] private TextMeshProUGUI txtInfoTeam;
    [SerializeField] private TextMeshProUGUI txtInfoEmail;
    [SerializeField] private Image imgStudent;

    [Header("Approval List")]
    [SerializeField] private Transform approvalListParent; // ScrollView content
    [SerializeField] private GameObject approvalListItemPrefab;

    [Header("Check Approval Detail")]
    [SerializeField] private TextMeshProUGUI txtMissionInfo;
    [SerializeField] private Image imgMissionData;
    [SerializeField] private Button btnApproveDetail;
    [SerializeField] private Button btnRejectDetail;

    private Dictionary<string, object> selectedApproval;
    private Dictionary<string, object> selectedStudent;

    private void Start()
    {
        LoadStudents();
        LoadPendingApprovals();

        if (btnApproveDetail != null)
            btnApproveDetail.onClick.AddListener(OnApproveClicked);
        if (btnRejectDetail != null)
            btnRejectDetail.onClick.AddListener(OnRejectClicked);
    }

    #region Student List

    public async void LoadStudents()
    {
        if (!FirebaseManager.Instance.IsConnect) return;

        // 기존 항목 제거
        foreach (Transform child in studentListParent)
            Destroy(child.gameObject);

        var students = await FirebaseManager.Instance.GetAllStudents();

        foreach (var student in students)
        {
            var item = Instantiate(studentListItemPrefab, studentListParent);
            var listItem = item.GetComponent<StudentListItem>();
            listItem.Setup(student, ShowStudentInfo);
        }
    }

    public void ShowStudentInfo(Dictionary<string, object> studentData)
    {
        selectedStudent = studentData;

        if (txtInfoName != null)
            txtInfoName.text = studentData.ContainsKey("nickname") ? studentData["nickname"].ToString() : "";
        if (txtInfoScore != null)
            txtInfoScore.text = studentData.ContainsKey("score") ? studentData["score"].ToString() : "0";
        if (txtInfoTeam != null)
            txtInfoTeam.text = studentData.ContainsKey("teamId") ? studentData["teamId"].ToString() : "";
        if (txtInfoEmail != null)
            txtInfoEmail.text = studentData.ContainsKey("email") ? studentData["email"].ToString() : "";

        ShowCanvas(canvasStudentInfo);
    }

    #endregion

    #region Approval List

    public async void LoadPendingApprovals()
    {
        if (!FirebaseManager.Instance.IsConnect) return;

        foreach (Transform child in approvalListParent)
            Destroy(child.gameObject);

        var approvals = await FirebaseManager.Instance.GetPendingApprovals();

        foreach (var approval in approvals)
        {
            var item = Instantiate(approvalListItemPrefab, approvalListParent);
            var listItem = item.GetComponent<ApprovalListItem>();
            listItem.Setup(approval, OnApproveFromList, OnRejectFromList, ShowApprovalDetail);
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

        ShowCanvas(canvasCheckApproval);
    }

    private async void OnApproveFromList(Dictionary<string, object> data)
    {
        string docId = data["docId"].ToString();
        string email = data.ContainsKey("studentEmail") ? data["studentEmail"].ToString() : "";
        long reward = data.ContainsKey("reward") ? (long)data["reward"] : 0;

        await FirebaseManager.Instance.ApproveRequest(docId, email, reward);
        LoadPendingApprovals();
    }

    private async void OnRejectFromList(Dictionary<string, object> data)
    {
        string docId = data["docId"].ToString();
        await FirebaseManager.Instance.RejectRequest(docId);
        LoadPendingApprovals();
    }

    public async void OnApproveClicked()
    {
        if (selectedApproval == null) return;

        string docId = selectedApproval["docId"].ToString();
        string email = selectedApproval.ContainsKey("studentEmail") ? selectedApproval["studentEmail"].ToString() : "";
        long reward = selectedApproval.ContainsKey("reward") ? (long)selectedApproval["reward"] : 0;

        await FirebaseManager.Instance.ApproveRequest(docId, email, reward);
        selectedApproval = null;
        ShowCanvas(canvasApprovalWaiting);
        LoadPendingApprovals();
    }

    public async void OnRejectClicked()
    {
        if (selectedApproval == null) return;

        string docId = selectedApproval["docId"].ToString();
        await FirebaseManager.Instance.RejectRequest(docId);
        selectedApproval = null;
        ShowCanvas(canvasApprovalWaiting);
        LoadPendingApprovals();
    }

    #endregion

    #region Canvas Navigation

    public void ShowCanvas(GameObject target)
    {
        canvasStudent.SetActive(target == canvasStudent);
        canvasStudentInfo.SetActive(target == canvasStudentInfo);
        canvasApprovalWaiting.SetActive(target == canvasApprovalWaiting);
        canvasCheckApproval.SetActive(target == canvasCheckApproval);
        canvasSettings.SetActive(target == canvasSettings);
    }

    // 버튼에서 호출할 수 있는 퍼블릭 메서드들
    public void ShowStudentCanvas() => ShowCanvas(canvasStudent);
    public void ShowApprovalCanvas()
    {
        ShowCanvas(canvasApprovalWaiting);
        LoadPendingApprovals();
    }
    public void ShowSettingsCanvas() => ShowCanvas(canvasSettings);

    public void BackToStudentList() => ShowCanvas(canvasStudent);
    public void BackToApprovalList()
    {
        ShowCanvas(canvasApprovalWaiting);
        LoadPendingApprovals();
    }

    #endregion
}
