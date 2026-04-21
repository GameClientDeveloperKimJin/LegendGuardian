using System.Collections;
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

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    [Header("Flash Effect")]
    [SerializeField] private Image flashImage;
    [SerializeField] private RectTransform flashRect;
    [SerializeField] private float flashDuration = 1.2f;
    [SerializeField] private float flashStartScale = 0.15f;
    [SerializeField] private float flashEndScale = 6.5f;
    [SerializeField] private AnimationCurve flashAlphaCurve;
    [SerializeField] private AnimationCurve flashScaleCurve;
    [SerializeField] private Image whiteFlashOverlay;
    [SerializeField] private float whiteFlashFadeInTime = 0.8f;
    [SerializeField] private float whiteFlashHoldTime = 0.7f;
    [SerializeField] private float whiteFlashFadeOutTime = 0.8f;
    [SerializeField] private float whiteFlashMaxAlpha = 1f;

    private void Awake()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(OnClickSpin);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClickCloseResult);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
            flashImage.gameObject.SetActive(false);
        }

        if (flashRect != null)
        {
            flashRect.localScale = Vector3.one * flashStartScale;
        }

        if (whiteFlashOverlay != null)
        {
            Color overlayColor = whiteFlashOverlay.color;
            overlayColor.a = 0f;
            whiteFlashOverlay.color = overlayColor;
            whiteFlashOverlay.gameObject.SetActive(false);
        }
    }

    [SerializeField]
    int routeIndex;
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

        if (spinButton != null)
            spinButton.gameObject.SetActive(true);

        wheelController.SpinToResult(routeIndex, OnSpinComplete);
    }

    private void OnSpinComplete()
    {
        ShowResultPanel();
    }


    private IEnumerator PlayFlashEffectCoroutine()
    {
        if (flashImage == null || flashRect == null || whiteFlashOverlay == null)
        {
            Debug.LogWarning("FlashImage, FlashRect, WhiteFlashOverlay 중 연결되지 않은 것이 있습니다.");
            yield break;
        }

        flashImage.gameObject.SetActive(true);
        whiteFlashOverlay.gameObject.SetActive(true);

        Color radialColor = flashImage.color;
        radialColor.a = 0f;
        flashImage.color = radialColor;

        Color overlayColor = whiteFlashOverlay.color;
        overlayColor.a = 0f;
        whiteFlashOverlay.color = overlayColor;

        flashRect.localScale = Vector3.one * flashStartScale;


        float totalDuration = whiteFlashFadeInTime + whiteFlashHoldTime;
        float t = 0f;

        while (t < totalDuration)
        {
            t += Time.deltaTime;

            float radialNormalized = Mathf.Clamp01(t / flashDuration);
            float radialAlpha = flashAlphaCurve != null ? flashAlphaCurve.Evaluate(radialNormalized) : (1f - radialNormalized);
            float radialScaleValue = flashScaleCurve != null ? flashScaleCurve.Evaluate(radialNormalized) : radialNormalized;

            radialColor.a = radialAlpha;
            flashImage.color = radialColor;

            float scale = Mathf.Lerp(flashStartScale, flashEndScale, radialScaleValue);
            flashRect.localScale = Vector3.one * scale;

            // 화면 전체 천천히 하얘지는 연출
            if (t <= whiteFlashFadeInTime)
            {
                float fadeInNormalized = Mathf.Clamp01(t / whiteFlashFadeInTime);
                overlayColor.a = Mathf.Lerp(0f, whiteFlashMaxAlpha, fadeInNormalized);
            }
            else
            {
                overlayColor.a = whiteFlashMaxAlpha;
            }

            whiteFlashOverlay.color = overlayColor;

            yield return null;
        }

        overlayColor.a = whiteFlashMaxAlpha;
        whiteFlashOverlay.color = overlayColor;
    }

    private void ShowResultPanel()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (routeNameText != null)
        {
            routeNameText.text = AuthManager.Instance?.GetUserTeamName();
        }

        if (flashImage != null)
        {
            Color radialColor = flashImage.color;
            radialColor.a = 0f;
            flashImage.color = radialColor;
            flashRect.localScale = Vector3.one * flashStartScale;
            flashImage.gameObject.SetActive(false);
        }

        if (whiteFlashOverlay != null)
        {
            StartCoroutine(FadeOutWhiteOverlayAfterResult());
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

    private IEnumerator FadeOutWhiteOverlayAfterResult()
    {
        if (whiteFlashOverlay == null)
            yield break;

        Color overlayColor = whiteFlashOverlay.color;
        float startAlpha = overlayColor.a;
        float t = 0f;

        while (t < whiteFlashFadeOutTime)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / whiteFlashFadeOutTime);

            overlayColor.a = Mathf.Lerp(startAlpha, 0f, normalized);
            whiteFlashOverlay.color = overlayColor;

            yield return null;
        }

        overlayColor.a = 0f;
        whiteFlashOverlay.color = overlayColor;
        whiteFlashOverlay.gameObject.SetActive(false);
    }
    public void StartFlashEarly()
    {
        StartCoroutine(PlayFlashEffectCoroutine());
    }
}
