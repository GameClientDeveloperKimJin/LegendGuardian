using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

/// <summary>
/// 타이틀 씬에 있는 인증 관련 캔버스 UI 관리
/// </summary>
public class TitleSceneUIController : MonoBehaviour
{
    [Header("로그인 관련")]
    [SerializeField]
    TMP_InputField LoginInputField;
    [SerializeField]
    TMP_InputField PWInputField;
    [SerializeField]
    Button LoginBtn;
    [SerializeField]
    Toggle SaveIdToggle;

    const string PREF_SAVED_ID = "SavedLoginId";
    const string PREF_SAVE_ID_CHECKED = "SaveIdChecked";

    [Header("안내 관련")]
    [SerializeField]
    Image InfoImage;
    [SerializeField]
    TextMeshProUGUI InfoTMP;
    [SerializeField]
    Button InfoCheckBtn;

    [Header("회원가입 관련")]
    [SerializeField]
    Button CreateBtn;
    [SerializeField]
    Button CreateCheckBtn;
    [SerializeField]
    Image CreateImage;
    [SerializeField]
    TMP_InputField CreateLoginInputField;
    [SerializeField]
    TMP_InputField CreatePWInputField;
    [SerializeField]
    TMP_InputField CreateNickNameInputField;

    [Header("팀 로딩 관련")]
    [SerializeField]
    Canvas teamLoadingCanvas;
    [SerializeField]
    TextMeshProUGUI LodingTMP;
    [Header("룰렛 관련")]
    [SerializeField]
    Canvas uiRuletCanvas;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; //꺼지지 않게
    }
    private void OnEnable()
    {
        if(AuthManager.Instance != null)
        {
            AuthManager.Instance.OnAuthInfo += OnInfoUI;
        }

        LoginBtn.onClick.AddListener(() => OnLoginUI(LoginInputField.text , PWInputField.text));

        CreateBtn.onClick.AddListener(() => CreateImage.gameObject.SetActive(true));
        CreateCheckBtn.onClick.AddListener(() => OnCreateUI(CreateLoginInputField.text, CreatePWInputField.text , CreateNickNameInputField.text));

        LoadSavedId();
        if (SaveIdToggle != null)
            SaveIdToggle.onValueChanged.AddListener(OnSaveIdToggleChanged);
    }
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => AuthManager.Instance != null);

        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnAuthInfo += OnInfoUI;
        }
    }

    private void OnDisable()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnAuthInfo -= OnInfoUI;
        }

        LoginBtn.onClick.RemoveAllListeners();
        InfoCheckBtn.onClick.RemoveAllListeners();
        CreateBtn.onClick.RemoveAllListeners();

        if (SaveIdToggle != null)
            SaveIdToggle.onValueChanged.RemoveListener(OnSaveIdToggleChanged);
    }

    #region 로그인 / 회원가입
    private async void OnLoginUI(string id , string pw)
    {
        if(!FirebaseManager.Instance.IsConnect)
        {
            return;
        }

        if(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            return;
        }

        if (id.Contains('@'))
        {
            OnInfoUI("ID에는 @를 포함할 수 없습니다");
            return;
        }

        // [Analytics] 로그인 시도 이벤트
        AnalyticsManager.Instance?.LogLoginAttempt();

       var (success,error) = await AuthManager.Instance.SignInAsync(id, pw);

        if(success)
        {
            if(id.Contains("teacher"))
            {
                // [Analytics] 선생님 로그인 성공
                AnalyticsManager.Instance?.LogLoginSuccess("teacher");
                SceneManager.LoadScene("TeacherScene_KJ");
                return;
            }
            LoginInputField.text = "";
            PWInputField.text = "";

            // [Analytics] 학생 로그인 성공
            AnalyticsManager.Instance?.LogLoginSuccess("student");

            OnInfoUI("로그인 성공");

            InfoCheckBtn.onClick.RemoveAllListeners();
            InfoCheckBtn.onClick.AddListener(() =>
            {
                AuthManager.Instance.SaveLoginUser(id);

                if (SaveIdToggle != null && SaveIdToggle.isOn)
                {
                    PlayerPrefs.SetString(PREF_SAVED_ID, id);
                    PlayerPrefs.SetInt(PREF_SAVE_ID_CHECKED, 1);
                    PlayerPrefs.Save();
                }

                OnHideInfoUI();
                TeamLoading();
            });
        }
        else
        {
            Debug.LogWarning($"로그인 실패: {error}");
        }

    }

    private async void OnCreateUI(string id, string pw , string nickName)
    {
        if (!FirebaseManager.Instance.IsConnect)
        {
            return;
        }

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            return;
        }

        if (id.Contains('@'))
        {
            OnInfoUI("ID에는 @를 포함할 수 없습니다");
            return;
        }
        
        if(string.IsNullOrEmpty(nickName))
        {
            OnInfoUI("닉네임을 입력하세요.");
            return;
        }

        var (success, error) = await AuthManager.Instance.SignUpAsync(id, pw, nickName);

        if (success)
        {
            CreateLoginInputField.text = "";
            CreatePWInputField.text = "";
            CreateNickNameInputField.text = "";

            OnInfoUI("회원가입 성공");

            InfoCheckBtn.onClick.RemoveAllListeners();
            InfoCheckBtn.onClick.AddListener(() =>
            {
                OnHideInfoUI();
                CreateImage.gameObject.SetActive(false);
            });
        }
        else
        {
            Debug.LogWarning($"회원가입 실패: {error}");
        }

    }

    #endregion


    /// <summary>
    /// 팀 로딩 화면
    /// </summary>
    private void TeamLoading()
    {
        teamLoadingCanvas.enabled = true;

        StartCoroutine(LoadingTMP());
    }

    [SerializeField]
    int loadingMaxCount = 3; //점 최대 개수
    IEnumerator LoadingTMP()
    {

        AuthManager authManager = AuthManager.Instance;

        LodingTMP.text = "";

        int currentLoadingCount = 0;

        while(!authManager.IsTeamReady)
        {
            if(authManager.IsSkip)
            {
                SceneManager.LoadScene("KJ_MainPlayScene_ver2");
                yield break;
            }
            currentLoadingCount++;

            if (currentLoadingCount > loadingMaxCount)
            {
                currentLoadingCount = 0;
                LodingTMP.text = "";
            }
            else
            {
                string tmp = new string('.', currentLoadingCount);

                LodingTMP.text = tmp;
            }

            yield return new WaitForSeconds(0.5f);
        }

        teamLoadingCanvas.enabled = false;

        // [Analytics] 팀 구성 완료 → 룰렛 진입 (Funnel 3단계)
        AnalyticsManager.Instance?.LogTeamLoadingComplete();

        uiRuletCanvas.enabled = true;
        Debug.Log("팀 로딩 완료! 룰렛 시작");
    }


    #region 아이디 저장
    private void LoadSavedId()
    {
        if (SaveIdToggle == null) return;

        bool wasSaved = PlayerPrefs.GetInt(PREF_SAVE_ID_CHECKED, 0) == 1;
        SaveIdToggle.isOn = wasSaved;

        if (wasSaved)
        {
            string savedId = PlayerPrefs.GetString(PREF_SAVED_ID, "");
            if (!string.IsNullOrEmpty(savedId))
                LoginInputField.text = savedId;
        }
    }

    private void OnSaveIdToggleChanged(bool isOn)
    {
        if (!isOn)
        {
            PlayerPrefs.DeleteKey(PREF_SAVED_ID);
            PlayerPrefs.SetInt(PREF_SAVE_ID_CHECKED, 0);
            PlayerPrefs.Save();
        }
    }
    #endregion

    #region 안내창
    private void OnInfoUI(string message)
    {
        InfoImage.gameObject.SetActive(true);

        InfoTMP.text = message;

        InfoCheckBtn.onClick.AddListener(() => InfoImage.gameObject.SetActive(false));
    }

    private void OnHideInfoUI()
    {
        InfoImage.gameObject.SetActive(false);

        InfoTMP.text = "";

        InfoCheckBtn.onClick.RemoveAllListeners();
    }
    #endregion


}
