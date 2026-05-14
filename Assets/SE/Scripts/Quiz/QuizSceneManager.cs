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
    [Header("장소별 퀴즈 세트")]
    [SerializeField] private QuizSet outsideQuizSet;
    [SerializeField] private QuizSet floor1QuizSet;
    [SerializeField] private QuizSet floor2QuizSet;
    [SerializeField] private QuizSet floor3QuizSet;

    [Header("문제 UI")]
    [SerializeField] private Image questionImage;
    [SerializeField] private TMP_Text questionText;

    [Header("보기 UI")]
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] optionTexts;

    [Header("정답/오답 결과 이미지")]
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;

    [Header("하단 버튼")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button hintButton;
    [SerializeField] private Button nextButton;

    [Header("힌트 텍스트")]
    [SerializeField] private TMP_Text hintText;

    private QuizSet currentQuizSet;
    private int currentQuestionIndex = 0;

    // -1 = 아직 선택 안 함
    //  0 = 오답
    //  1 = 정답
    private int[] answerResults;

    private void Start()
    {
        ConnectButtons();

        MainSceneView.OnQuizStarted += StartQuizByArea;

        // 시작 시 결과 이미지 숨김
        if (resultImage != null)
        {
            resultImage.gameObject.SetActive(false);
        }

        // 시작 시 힌트 비우기
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
        // 이미 선택한 문제면 입력 무시
        if (answerResults[currentQuestionIndex] != -1)
        {
            return;
        }

        QuizQuestion question = currentQuizSet.questions[currentQuestionIndex];

        if (selectedIndex == question.correctAnswerIndex)
        {
            answerResults[currentQuestionIndex] = 1;

            // 점수 +3
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(3);
            }

            Debug.Log("정답입니다!");
        }
        else
        {
            answerResults[currentQuestionIndex] = 0;
            Debug.Log("오답입니다!");
        }

        UpdateResultImage();
        UpdateOptionButtons();
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
            Debug.LogError($"{area} QuizSet이 연결되지 않았습니다.");
            return;
        }

        // 문제 인덱스 초기화
        currentQuestionIndex = 0;

        // 문제별 정답 상태 초기화
        answerResults = new int[currentQuizSet.questions.Length];

        for (int i = 0; i < answerResults.Length; i++)
        {
            answerResults[i] = -1;
        }

        // 결과 이미지 초기화
        if (resultImage != null)
        {
            resultImage.gameObject.SetActive(false);
        }

        // 힌트 초기화
        if (hintText != null)
        {
            hintText.text = "";
        }

        // 첫 문제 표시
        ShowQuestion(0);
    }
}
