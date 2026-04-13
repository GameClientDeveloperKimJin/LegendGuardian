using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteUIController : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private RouletteWheelController wheelController;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text routeNameText;
    [SerializeField] private Button closeButton;

    [Header("Auto Spin")]
    [SerializeField] private float autoSpinDelay = 1f;

    [Header("Test Result")]
    [SerializeField] private RouteResultData testResultData;

    [Header("Wheel Objects")]
    [SerializeField] private GameObject rouletteWheel;
    [SerializeField] private GameObject roulettepointer;

    private RouteResultData currentResultData;
    private bool hasStarted;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnClickCloseResult);

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void Start()
    {
        if (!hasStarted)
        {
            StartCoroutine(AutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        hasStarted = true;

        yield return new WaitForSeconds(autoSpinDelay);

        // 나중에 Firebase/DB 결과가 들어오면 이 부분만 바꾸면 됨. 현재는 테스트용
        currentResultData = testResultData;

        if (currentResultData == null)
        {
            Debug.LogError("testResultData가 비어 있습니다.");
            yield break;
        }

        if (wheelController == null)
        {
            Debug.LogError("wheelController가 연결되지 않았습니다.");
            yield break;
        }

        wheelController.SpinToResult(currentResultData.routeIndex, OnSpinComplete);
    }

    private void OnSpinComplete()
    {
        if (RouteResultManager.Instance != null)
        {
            RouteResultManager.Instance.SetResult(currentResultData);
        }

        if (routeNameText != null)
        {
            routeNameText.text = currentResultData.routeName;
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
    }

    private void OnClickCloseResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (rouletteWheel != null)
            rouletteWheel.SetActive(false);

        if (roulettepointer != null)
            roulettepointer.SetActive(false);
    }

    // 나중에 외부(DB/Firebase)에서 결과를 넣을 때 사용할 예정
    public void SetResultData(RouteResultData resultData)
    {
        currentResultData = resultData;
    }
}
