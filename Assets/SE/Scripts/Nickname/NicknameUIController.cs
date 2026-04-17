using System.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NicknameUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button checkButton;
    [SerializeField] private Button enterButton;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text checkText;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private string nextSceneName = "Sieun_Main";

    private bool isChecked = false;
    private string lastCheckedNickname = "";

    private void Awake()
    {
        checkButton.onClick.AddListener(OnClickCheck);
        enterButton.onClick.AddListener(OnClickEnter);
        nicknameInput.onValueChanged.AddListener(_ => OnNicknameChanged());

        ShowError("");
        ShowCheck("");
        SetLoading(false);
        RefreshUI();
    }

    void OnNicknameChanged()
    {
        if (nicknameInput.text.Trim() != lastCheckedNickname)
        {
            isChecked = false;
            ShowCheck("");
        }

        ShowError("");
        RefreshUI();
    }

    void RefreshUI()
    {
        bool hasText = !string.IsNullOrWhiteSpace(nicknameInput.text);

        checkButton.interactable = hasText;
        enterButton.interactable = hasText && isChecked;
    }

    void SetLoading(bool on)
    {
        loadingPanel.SetActive(on);
    }

    void ShowError(string msg)
    {
        errorText.text = msg;
        errorText.gameObject.SetActive(!string.IsNullOrEmpty(msg));

        if (!string.IsNullOrEmpty(msg))
        {
            StartCoroutine(HideTextAfterDelay(errorText, 2f));
        }
    }

    void ShowCheck(string msg)
    {
        checkText.text = msg;
        checkText.gameObject.SetActive(!string.IsNullOrEmpty(msg));

        if (!string.IsNullOrEmpty(msg))
        {
            StartCoroutine(HideTextAfterDelay(checkText, 2f));
        }
    }

    private IEnumerator HideTextAfterDelay(TMP_Text text, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (text != null)
        {
            text.gameObject.SetActive(false);
        }
    }

    async void OnClickCheck()
    {
        string nickname = nicknameInput.text.Trim();

        if (!NicknameValidator.Validate(nickname, out string error))
        {
            ShowError(error);
            return;
        }

        ShowError("");
        SetLoading(true);

        // 추후 Firebase 연결 지점
        bool available = await CheckNicknameAvailable(nickname);

        SetLoading(false);

        if (available)
        {
            isChecked = true;
            lastCheckedNickname = nickname;
            ShowCheck("사용 가능한 닉네임입니다.");
        }
        else
        {
            isChecked = false;
            ShowError("이미 사용 중인 닉네임입니다.");
        }

        RefreshUI();
    }

    async void OnClickEnter()
    {
        string nickname = nicknameInput.text.Trim();

        if (!NicknameValidator.Validate(nickname, out string error))
        {
            ShowError(error);
            return;
        }

        if (!isChecked)
        {
            ShowError("중복 확인을 먼저 해주세요.");
            return;
        }

        SetLoading(true);

        // 추후 Firebase 연결 지점
        bool success = await RegisterNickname(nickname);

        SetLoading(false);

        if (success)
        {
            PlayerPrefs.SetString("Nickname", nickname);
            PlayerPrefs.Save();

            await LoadNextSceneAsync();
        }
        else
        {
            ShowError("입장 실패");
        }
    }

// Firebase 추가

    async Task<bool> CheckNicknameAvailable(string nickname) //Firebase 붙일 때 수정
    {
        await Task.Delay(500);
        return true;
    }

    async Task<bool> RegisterNickname(string nickname) //Firebase 붙일 때 수정
    {
        await Task.Delay(500);
        return true;
    }

    private async Task LoadNextSceneAsync()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            SetLoading(false);
            ShowError("다음 씬 이름이 비어 있습니다.");
            return;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);

        if (op == null)
        {
            SetLoading(false);
            ShowError($"씬 '{nextSceneName}' 을(를) 불러오지 못했습니다.");
            return;
        }

        while (!op.isDone)
        {
            await Task.Yield();
        }
    }


}
