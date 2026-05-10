using UnityEngine;

[System.Serializable]
public class QuizQuestion //퀴즈 문제 데이터 클래스
{
    [Header("문제 이미지")]
    public Sprite questionImage;

    [Header("문제 텍스트")]
    [TextArea(2, 5)]
    public string questionText;

    [Header("보기 4개")]
    public string[] options = new string[4];

    [Header("정답 번호")]
    [Range(0, 3)]
    public int correctAnswerIndex;

    [Header("힌트")]
    [TextArea(1, 3)]
    public string hintText;
}
