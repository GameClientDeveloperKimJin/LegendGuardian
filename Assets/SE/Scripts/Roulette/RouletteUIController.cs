using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RouletteUIController : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private RouletteWheelController wheelController;

    [Header("Buttons")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button closeButton;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text routeNameText;

    [Header("Test Result")]
    [SerializeField] private RouteResultData testResultData;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    private RouteResultData currentResultData;

    private void Awake()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(OnClickSpin);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClickCloseResult);

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void SetResultData(RouteResultData resultData)
    {
        currentResultData = resultData;
    }

    private void OnClickSpin()
    {
        if (wheelController == null)
        {
            Debug.LogError("wheelController가 연결되지 않았습니다.");
            return;
        }

        if (wheelController.IsSpinning)
            return;

        if (spinButton != null)
            spinButton.gameObject.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        // 외부에서 결과를 넣지 않으면 테스트 데이터 사용
        if (currentResultData == null)
            currentResultData = testResultData;

        if (currentResultData == null)
        {
            Debug.LogError("currentResultData와 testResultData가 모두 비어 있습니다.");

            if (spinButton != null)
                spinButton.interactable = true;

            return;
        }

        wheelController.SpinToResult(currentResultData.routeIndex, OnSpinComplete);
    }

    private void OnSpinComplete()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (routeNameText != null)
        {
            if (currentResultData != null)
                routeNameText.text = currentResultData.routeName;
            else
                routeNameText.text = "결과 없음";
        }

    }

    private void OnClickCloseResult()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("다음 씬 이름이 설정되지 않았습니다.");
        }
    }
}
