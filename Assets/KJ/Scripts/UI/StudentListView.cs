using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Canvas_Student에서 학생 목록 표시 및 검색 기능 담당
/// </summary>
public class StudentListView : MonoBehaviour
{
    [Header("검색")]
    [SerializeField] private InputField searchInputField;

    [Header("학생 목록")] 
    [SerializeField] private GameObject studentCanvas;
    [SerializeField] private Transform studentContainer; // Student_Container
    [SerializeField] private GameObject studentItemPrefab; // Img_Student 프리팹

    [Header("학생 상세 정보")]
    [SerializeField] private GameObject canvasStudentInfo; // Canvas_StudentInfo
    [SerializeField] private TextMeshProUGUI infoNameTMP;
    [SerializeField] private TextMeshProUGUI infoTeamTMP;
    [SerializeField] private TextMeshProUGUI infoRewardTMP;
    [SerializeField] private Button backButton; // 뒤로가기 버튼

    private List<Dictionary<string, object>> cachedStudents = new();

    private void Awake()
    {
        // UI 리스너는 Firebase와 무관하므로 Awake에서 바로 등록
        if (searchInputField != null)
            searchInputField.onValueChanged.AddListener(OnSearchValueChanged);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButton);

        if (canvasStudentInfo != null)
            canvasStudentInfo.SetActive(false);
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance.IsConnect);

        LoadStudents();
    }

    private void OnEnable()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsConnect)
            LoadStudents();
    }

    /// <summary>
    /// Firestore에서 학생 데이터를 가져와 캐싱 후 표시
    /// </summary>
    public async void LoadStudents()
    {
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsConnect) return;

        cachedStudents = await FirebaseManager.Instance.GetAllStudents();

        string searchText = searchInputField != null ? searchInputField.text : "";

        if (string.IsNullOrEmpty(searchText))
            DisplayStudents(cachedStudents);
        else
            FilterAndDisplay(searchText);
    }

    /// <summary>
    /// 학생 목록 UI 생성
    /// </summary>
    private void DisplayStudents(List<Dictionary<string, object>> students)
    {
        foreach (Transform child in studentContainer)
            Destroy(child.gameObject);

        foreach (var student in students)
        {
            GameObject item = Instantiate(studentItemPrefab, studentContainer);
            item.GetComponent<StudentListItem>().Setup(student, OnStudentClicked);
        }
    }

    /// <summary>
    /// 검색어 변경 시 필터링
    /// </summary>
    private void OnSearchValueChanged(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            DisplayStudents(cachedStudents);
            return;
        }

        FilterAndDisplay(searchText);
    }

    private void FilterAndDisplay(string searchText)
    {
        var filtered = cachedStudents.FindAll(s =>
        {
            string nickname = s.ContainsKey("nickname") ? s["nickname"].ToString() : "";
            return nickname.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        });

        DisplayStudents(filtered);
    }

    /// <summary>
    /// 학생 아이템 클릭 시 호출
    /// </summary>
    private void OnStudentClicked(Dictionary<string, object> studentData)
    {
        string nickname = studentData.ContainsKey("nickname") ? studentData["nickname"].ToString() : "";
        string teamId = studentData.ContainsKey("teamId") ? studentData["teamId"].ToString() : "없음";
        long completedCount = studentData.ContainsKey("completedMissionCount") ? Convert.ToInt64(studentData["completedMissionCount"]) : 0;
        long score = studentData.ContainsKey("score") ? Convert.ToInt64(studentData["score"]) : 0;

        if (infoNameTMP != null) infoNameTMP.text = nickname;
        if (infoTeamTMP != null) infoTeamTMP.text = teamId;
        if (infoRewardTMP != null) infoRewardTMP.text = $" {score}wh";

        canvasStudentInfo.SetActive(true);
    }

    private void OnBackButton()
    {
        Debug.Log("[StudentListView] OnBackButton 호출됨");
        if (canvasStudentInfo != null)
            canvasStudentInfo.SetActive(false);
        else
            Debug.LogWarning("[StudentListView] canvasStudentInfo가 null입니다!");
    }

    private void OnDestroy()
    {
        if (searchInputField != null)
            searchInputField.onValueChanged.RemoveListener(OnSearchValueChanged);
        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
    }
}
