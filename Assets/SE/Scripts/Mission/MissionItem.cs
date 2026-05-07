using UnityEngine;

public class MissionItem : MonoBehaviour
{
    [Header("Mission Info")]
    [SerializeField] private string missionId;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private bool isCompleted = false;

    public string MissionId => missionId;
    public int ScoreValue => scoreValue;
    public bool IsCompleted => isCompleted;

    public void CompleteMission()
    {
        if (isCompleted)
            return;

        isCompleted = true;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        Debug.Log($"미션 완료: {missionId}, +{scoreValue}점");
    }

    public void ResetMission()
    {
        isCompleted = false;
    }
}
