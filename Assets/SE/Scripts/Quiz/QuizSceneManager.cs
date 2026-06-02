using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum QuizArea
{
    Outside,
    Floor1,
    Floor2,
    Floor3
}

public class QuizSceneManager : MonoBehaviour
{
    [Header("��Һ� ���� ��Ʈ")]
    [SerializeField] private QuizSet outsideQuizSet;
    [SerializeField] private QuizSet floor1QuizSet;
    [SerializeField] private QuizSet floor2QuizSet;
    [SerializeField] private QuizSet floor3QuizSet;

    [Header("���� UI")]
    [SerializeField] private Image questionImage;
    [SerializeField] private TMP_Text questionText;

    [Header("���� UI")]
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] optionTexts;

    [Header("����/���� ��� �̹���")]
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;

    [Header("�ϴ� ��ư")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button nextButton;

    [Header("��Ʈ �ؽ�Ʈ")]
    [SerializeField] private TMP_Text hintText;

    [Header("���� ĵ����")]
    [SerializeField] private GameObject quizCanvas;

    [Header("���� ����")]
    [SerializeField] private int correctAnswerReward = 3;

    private QuizSet currentQuizSet;
    private int currentQuestionIndex = 0;

    // [Analytics] quiz_submitted 이벤트에 퀴즈 영역 파라미터 전달용
    private QuizArea _currentArea;

    // -1 = ���� ���� �� ��
    //  0 = ����
    //  1 = ����
    private int[] answerResults;

    private void Start()
    {
        ConnectButtons();

        MainSceneView.OnQuizStarted += StartQuizByArea;

        // ���� �� ��� �̹��� ����
        if (resultImage != null)
        {
            resultImage.gameObject.SetActive(false);
        }

        // ���� �� ��Ʈ ����
        if (hintText != null)
        {
            hintText.text = "";
        }


    }

    private void OnDestroy()
    {
        MainSceneView.OnQuizStarted -= StartQuizByArea;
    }

    private void ConnectButtons()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => SelectAnswer(index));
        }

        prevButton.onClick.AddListener(ShowPrevQuestion);
        nextButton.onClick.AddListener(ShowNextQuestion);
        hintButton.onClick.AddListener(ShowHint);
    }

    private void ShowQuestion(int index)
    {
        currentQuestionIndex = index;

        QuizQuestion question = currentQuizSet.questions[currentQuestionIndex];

        questionImage.sprite = question.questionImage;
        questionText.text = question.questionText;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = question.options[i];
        }

        if (hintText != null)
            hintText.text = "";

        UpdateResultImage();
        UpdateOptionButtons();

        prevButton.interactable = currentQuestionIndex > 0;
        nextButton.interactable = currentQuestionIndex < currentQuizSet.questions.Length - 1;
    }

    private void SelectAnswer(int selectedIndex)
    {
        // �̹� ������ ������ �Է� ����
        if (answerResults[currentQuestionIndex] != -1)
        {
            return;
        }

        QuizQuestion question = currentQuizSet.questions[currentQuestionIndex];

        if (selectedIndex == question.correctAnswerIndex)
        {
            answerResults[currentQuestionIndex] = 1;

            // ���� +3
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(correctAnswerReward);
            }

            // [Analytics] 퀴즈 정답 제출 (Funnel 5단계)
            AnalyticsManager.Instance?.LogQuizSubmitted(_currentArea.ToString(), isCorrect: true);

            Debug.Log("�����Դϴ�!");
        }
        else
        {
            answerResults[currentQuestionIndex] = 0;

            // [Analytics] 퀴즈 오답 제출
            AnalyticsManager.Instance?.LogQuizSubmitted(_currentArea.ToString(), isCorrect: false);

            Debug.Log("�����Դϴ�!");
        }

        UpdateResultImage();
        UpdateOptionButtons();

        if (currentQuestionIndex == currentQuizSet.questions.Length - 1)
        {
            if (quizCanvas != null)
            {
                quizCanvas.SetActive(false);
            }
        }

    }

    private void UpdateResultImage()
    {
        if (resultImage == null)
            return;

        int result = answerResults[currentQuestionIndex];

        if (result == -1)
        {
            resultImage.gameObject.SetActive(false);
        }
        else if (result == 1)
        {
            resultImage.sprite = correctSprite;
            resultImage.gameObject.SetActive(true);
        }
        else
        {
            resultImage.sprite = wrongSprite;
            resultImage.gameObject.SetActive(true);
        }
    }

    private void UpdateOptionButtons()
    {
        bool alreadyAnswered = answerResults[currentQuestionIndex] != -1;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].interactable = !alreadyAnswered;
        }
    }

    private void ShowPrevQuestion()
    {
        if (currentQuestionIndex > 0)
        {
            ShowQuestion(currentQuestionIndex - 1);
        }
    }

    private void ShowNextQuestion()
    {
        if (currentQuestionIndex < currentQuizSet.questions.Length - 1)
        {
            ShowQuestion(currentQuestionIndex + 1);
        }
    }

    private void ShowHint()
    {
        QuizQuestion question = currentQuizSet.questions[currentQuestionIndex];

        if (hintText != null)
        {
            hintText.text = question.hintText;
        }
    }

    public void StartQuizByArea(QuizArea area)
    {
        // [Analytics] 현재 퀴즈 영역 저장 → SelectAnswer에서 파라미터로 사용
        _currentArea = area;

        switch (area)
        {
            case QuizArea.Outside:
                currentQuizSet = outsideQuizSet;
                break;

            case QuizArea.Floor1:
                currentQuizSet = floor1QuizSet;
                break;

            case QuizArea.Floor2:
                currentQuizSet = floor2QuizSet;
                break;

            case QuizArea.Floor3:
                currentQuizSet = floor3QuizSet;
                break;
        }

        if (currentQuizSet == null)
        {
            Debug.LogError($"{area} QuizSet�� ������� �ʾҽ��ϴ�.");
            return;
        }

        // ���� �ε��� �ʱ�ȭ
        currentQuestionIndex = 0;

        // ������ ���� ���� �ʱ�ȭ
        answerResults = new int[currentQuizSet.questions.Length];

        for (int i = 0; i < answerResults.Length; i++)
        {
            answerResults[i] = -1;
        }

        // ��� �̹��� �ʱ�ȭ
        if (resultImage != null)
        {
            resultImage.gameObject.SetActive(false);
        }

        // ��Ʈ �ʱ�ȭ
        if (hintText != null)
        {
            hintText.text = "";
        }

        // ù ���� ǥ��
        ShowQuestion(0);
    }
}
