using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class TitleSceneUIController : MonoBehaviour
{
    [Header("로그인 관련")]
    [SerializeField]
    TMP_InputField LoginInputField;
    [SerializeField]
    TMP_InputField PWInputField;
    [SerializeField]
    Button LoginBtn;

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

    private void OnEnable()
    {
        if(AuthManager.Instance != null)
        {
            AuthManager.Instance.OnAuthInfo += OnInfoUI;
        }

        LoginBtn.onClick.AddListener(() => OnLoginUI(LoginInputField.text , PWInputField.text));

        CreateBtn.onClick.AddListener(() => CreateImage.gameObject.SetActive(true));
        CreateCheckBtn.onClick.AddListener(() => OnCreateUI(CreateLoginInputField.text, CreatePWInputField.text , CreateNickNameInputField.text));
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

       var (success,error) = await AuthManager.Instance.SignInAsync(id, pw);
       
        if(success)
        {
            LoginInputField.text = "";
            PWInputField.text = "";

            OnInfoUI("로그인 성공");

            InfoCheckBtn.onClick.RemoveAllListeners();
            InfoCheckBtn.onClick.AddListener(() =>
            {
                AuthManager.Instance.SaveLoginUser(id);

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

        uiRuletCanvas.enabled = true;
        Debug.Log("팀 로딩 완료! 룰렛 시작");
    }


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
