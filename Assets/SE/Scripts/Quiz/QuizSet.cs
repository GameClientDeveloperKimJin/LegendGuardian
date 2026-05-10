using UnityEngine;

[CreateAssetMenu(fileName = "NewQuizSet", menuName = "Quiz/Quiz Set")]
public class QuizSet : ScriptableObject //네 문제를 한 세트 단위로 관리
{
    [Header("퀴즈 세트 이름")]
    public string setName;

    [Header("4문제 등록")]
    public QuizQuestion[] questions = new QuizQuestion[4];
}