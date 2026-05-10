using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizSceneManager : MonoBehaviour
{
    [Header("현재 사용할 퀴즈 세트")]
    [SerializeField] private QuizSet quizSet;

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

    private int currentQuestionIndex = 0;

    // -1 = 아직 선택 안 함
    //  0 = 오답
    //  1 = 정답
    private int[] answerResults;

    private void Start()
    {
        if (quizSet == null)
        {
            Debug.LogError("QuizSet이 연결되지 않았습니다.");
            return;
        }

        answerResults = new int[quizSet.questions.Length];

        for (int i = 0; i < answerResults.Length; i++)
        {
            answerResults[i] = -1;
        }

        ConnectButtons();
        ShowQuestion(0);
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

        QuizQuestion question = quizSet.questions[currentQuestionIndex];

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
        nextButton.interactable = currentQuestionIndex < quizSet.questions.Length - 1;
    }

    private void SelectAnswer(int selectedIndex)
    {
        // 이미 선택한 문제면 입력 무시
        if (answerResults[currentQuestionIndex] != -1)
        {
            return;
        }

        QuizQuestion question = quizSet.questions[currentQuestionIndex];

        if (selectedIndex == question.correctAnswerIndex)
        {
            answerResults[currentQuestionIndex] = 1;
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
        if (currentQuestionIndex < quizSet.questions.Length - 1)
        {
            ShowQuestion(currentQuestionIndex + 1);
        }
    }

    private void ShowHint()
    {
        QuizQuestion question = quizSet.questions[currentQuestionIndex];

        if (hintText != null)
        {
            hintText.text = question.hintText;
        }
    }
}
