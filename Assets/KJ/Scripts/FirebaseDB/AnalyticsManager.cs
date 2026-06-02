using Firebase.Analytics;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 유저 역할 세그먼트 (student / teacher)
    public void SetUserRole(string role)
    {
        FirebaseAnalytics.SetUserProperty("user_role", role);
    }

    public void LogLoginAttempt()
    {
        FirebaseAnalytics.LogEvent("login_attempt");
    }

    public void LogLoginSuccess(string role)
    {
        FirebaseAnalytics.LogEvent("login_success",
            new Parameter[] { new Parameter("role", role) });
    }

    // 팀 구성 완료 후 룰렛 진입 시점
    public void LogTeamLoadingComplete()
    {
        FirebaseAnalytics.LogEvent("team_loading_complete");
    }

    public void LogMissionStarted(string missionId, string missionName, string teamId, long reward)
    {
        FirebaseAnalytics.LogEvent("mission_started",
            new Parameter[] {
                new Parameter("mission_id", missionId),
                new Parameter("mission_name", missionName),
                new Parameter("team_id", teamId),
                new Parameter("reward", reward)
            });
    }

    public void LogQuizSubmitted(string quizArea, bool isCorrect)
    {
        FirebaseAnalytics.LogEvent("quiz_submitted",
            new Parameter[] {
                new Parameter("quiz_area", quizArea),
                new Parameter("is_correct", isCorrect ? "true" : "false")
            });
    }

    public void LogApprovalRequested(string missionId, long reward)
    {
        FirebaseAnalytics.LogEvent("approval_requested",
            new Parameter[] {
                new Parameter("mission_id", missionId),
                new Parameter("reward", reward)
            });
    }

    // 전환 이벤트: 선생님 승인 완료 → 학생 보상 수령 확정
    public void LogRewardClaimed(string missionId, long reward)
    {
        FirebaseAnalytics.LogEvent("reward_claimed",
            new Parameter[] {
                new Parameter("mission_id", missionId),
                new Parameter("reward", reward)
            });
    }

    public void LogApprovalAction(string action, string missionId, string missionReward)
    {
        FirebaseAnalytics.LogEvent("approval_action",
            new Parameter[] {
                new Parameter("action", action),
                new Parameter("mission_id", missionId),
                new Parameter("mission_reward", missionReward)
            });
    }
}
