using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneUIController : MonoBehaviour
{
    [Header("로그인 관련")]
    [SerializeField]
    TMP_InputField LoginInputField;
    [SerializeField]
    TMP_InputField PWInputField;
    [SerializeField]
    Button LoginBtn;

    [Header("정보 관련")]
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
        CreateCheckBtn.onClick.AddListener(() => OnCreateUI(CreateLoginInputField.text, CreatePWInputField.text));
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
            OnInfoUI("ID에는 @를 사용할 수 없습니다");
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
                OnHideInfoUI();
                uiRuletCanvas.enabled = true;
                AuthManager.Instance.SaveUserID(id);
            });           
        }
        else
        {
            Debug.LogWarning($"로그인 실패: {error}"); // 추가
        }

    }

    private async void OnCreateUI(string id, string pw)
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
            OnInfoUI("ID에는 @를 사용할 수 없습니다");
            return;
        }

        var (success, error) = await AuthManager.Instance.SignUpAsync(id, pw);

        if (success)
        {
            CreateLoginInputField.text = "";
            CreatePWInputField.text = "";

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
            Debug.LogWarning($"회원가입 실패: {error}"); // 추가
        }

    }

    #endregion

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
}
