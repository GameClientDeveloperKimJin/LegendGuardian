using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using Query = Firebase.Firestore.Query;

public class FirebaseManager : MonoBehaviour, IDisposable
{
    private static FirebaseManager instance;

    public static FirebaseManager Instance { get; private set; }

    public FirebaseFirestore Firestore { get; private set; }
    public DatabaseReference RealtimeDB { get; private set; }
    public FirebaseAuth Auth { get; set; }

    public bool IsConnect { get; private set; } = false;

    public event Action<long> OnScoreUpdated;
    public void Dispose()
    {
        AuthManager.Instance.SignOut();
    }

    void Awake()
    {
        // 싱글톤 설정
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    private async void InitializeFirebase()
    {
        DependencyStatus status =
            await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status == DependencyStatus.Available)
        {
            Firestore = FirebaseFirestore.DefaultInstance;
            RealtimeDB = FirebaseDatabase
     .GetInstance("https://legendguardian-d1c51-default-rtdb.firebaseio.com/")
     .GetReference("/");
            Auth = FirebaseAuth.DefaultInstance;
            IsConnect = true;
            Debug.Log("Firebase 초기화 완료!");
        }
        else
        {
            Debug.LogError($"Firebase 종속성 오류: {status}");
        }
    }

    public async Task<Dictionary<string, object>> GetMissionAllData(string missionID)
    {
        DocumentSnapshot doc = await Firestore.Collection("missions").Document(missionID).GetSnapshotAsync();

        if (doc.Exists)
        {
            return doc.ToDictionary();
        }

        return null;
    }
    public async Task<Dictionary<string, List<string>>> GetRouteIDToMission(string teamID)
    {
        Firebase.Firestore.Query query = Firestore.Collection("routes").WhereArrayContains("teamIds", teamID);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if(snapshot.Count > 0)
        {
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> zoneRaw = doc.GetValue<Dictionary<string, object>>("zones");

                //Dictionary<string, string> resultDic = new();
                Dictionary<string, List<string>> resultDic = new();

                foreach(var kvp in zoneRaw)
                {
                    if(kvp.Value is List<object> list && list.Count > 0)
                    {
                        //resultDic[kvp.Key] = list[0].ToString(); //리스트에 첫번쨰 요소만 가져오는게 아님
                        resultDic[kvp.Key] = list.Select(o => o.ToString()).ToList();

                        Debug.Log($"딕셔너리 -> {kvp.Key}에 {list[0].ToString()}를 저장");
                    }
                }

                return resultDic;
            }
        }

        Debug.LogError($"{teamID} 에 맞는 루트 zones 데이터가 없습니다. ");

        return null;
    }



    #region 팀 ID를 통해 routes 컬렉션 데이터 가져오기
    public async Task<(string[] zoneOrder, string[] zoneOrderNames)> GetTeamIDToRoutes(string teamID)
    {
        Firebase.Firestore.Query query = Firestore.Collection("routes").WhereArrayContains("teamIds", teamID);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Count > 0)
        {
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                string[] zoneOrder = doc.GetValue<List<object>>("zoneOrder").Select(o => o.ToString()).ToArray();
                string[] zoneOrderNames = doc.GetValue<List<object>>("zoneOrderNames").Select(o => o.ToString()).ToArray();

                return (zoneOrder, zoneOrderNames);
            }
        }

        Debug.LogError($"{teamID} 에 맞는 루트가 없습니다. ");

        return (null, null);
    }



    #endregion

    #region users 컬렉션 데이터 가져오기
    /// <summary>
    /// 호출 시, userID 인자를 AuthManager.Instance?.LoginUserID로 넘기시길 바랍니다. 
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    public async Task UpdateUserScore(string userID, long score) //특정 유저에 점수 업데이트
    {
        Firebase.Firestore.Query query = Firestore
           .Collection("users").WhereEqualTo("email", userID);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Count > 0)
        {
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                //클라가 계산하면 스레드 간의 Race Condition 발생 위험이 있음. 따라서 서버가 직접 계산하여 위험을 방지한다.
                await doc.Reference.UpdateAsync("score", FieldValue.Increment(score));

                //long currentScore = doc.GetValue<long>("score");
                //await doc.Reference.UpdateAsync("score", currentScore + score);

                OnScoreUpdated?.Invoke(score);
                return;
            }
        }

        Debug.LogError($"{userID} 에 맞는 유저가 없습니다. ");
    }

    /// <summary>
    /// 유저 ID를 통해 users 컬렉션에 있는 데이터들을 딕셔너리로 가져오기
    /// 방식 : var data  = await FirebaseManager.Instance.GetUserAllData(유저id);
    /// string nickname = data["nickname"].ToString();
    /// 
    /// 유저id 넘길 때, @giadian.com 붙여야 함
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    public async Task<Dictionary<string,object>> GetUserAllData(string userID)
    {
        DocumentSnapshot doc = await Firestore.Collection("users").Document(userID).GetSnapshotAsync();

        if(doc.Exists)
        {
            return doc.ToDictionary();
        }

        return null;
    }

    /// <summary>
    /// 상위 랭킹 10위 이내 조회
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    public async Task<List<RankingData>> GetRanking(int limit = 10)
    {
        QuerySnapshot snapShot = await Firestore.Collection("users").OrderByDescending("score").Limit(limit).GetSnapshotAsync();

        var scoreList = new List<RankingData>();

        string teacher = "teacher";

        foreach(DocumentSnapshot doc in snapShot.Documents)
        {
            if(doc.TryGetValue("role",out string type) && type == teacher) //선생님 제외
            {
                continue;
            }

            long scoreValue = 0;
            if (doc.TryGetValue("score", out object obj))
                scoreValue = Convert.ToInt64(obj); // string이든 int64든 자동 변환

            scoreList.Add(new RankingData()
            {
                NickName = doc.TryGetValue("nickname", out string userName) ? userName : "닉네임 없음",
                score = scoreValue,
            });

        }
        return scoreList;
    }
    /// <summary>
    /// 유저 ID로 유저 DB에 저장된 닉네임을 가져옵니다
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    public async Task<string> GetUserIDToNickName(string userID)
    {
        // users 컬렉션에서 userid 필드값이 userID와 같은 문서를 찾아라.
        Firebase.Firestore.Query query = Firestore
            .Collection("users").WhereEqualTo("email", userID);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Count > 0)
        {
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                return doc.GetValue<string>("nickname");
            }
        }

        Debug.LogError($"{userID} 에 맞는 닉네임이 없습니다. ");
        return null;
    }

    /// <summary>
    /// 유저 ID로 유저 DB에 저장된 점수를 가져옵니다
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    public async Task<string> GetUserIDToScore(string userID)
    {
        // users 컬렉션에서 userid 필드값이 userID와 같은 문서를 찾아라.
        Firebase.Firestore.Query query = Firestore
            .Collection("users").WhereEqualTo("email", userID);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Count > 0)
        {
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                return doc.GetValue<long>("score").ToString();
            }
        }

        Debug.LogError($"{userID} 에 맞는 점수가 없습니다. ");
        return null;
    }

    /// <summary>
    /// 유저 ID로 유저 DB에 저장된 팀 ID 가져옵니다
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    public async Task<string> GetUserIDToTeamID(string userID)
    {
        userID += "@giadian.com";

        // users 컬렉션에서 userid 필드값이 userID와 같은 문서를 찾아라.
        Firebase.Firestore.Query query = FirebaseManager.Instance.Firestore
            .Collection("users").WhereEqualTo("email", userID);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Count > 0)
        {
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                return doc.GetValue<string>("teamId");
            }
        }

        Debug.LogError($"{userID} 에 맞는 팀 ID가 없습니다. ");
        return null;
    }
    /// <summary>
    /// role이 "student"인 모든 유저를 가져옵니다
    /// </summary>
    public async Task<List<Dictionary<string, object>>> GetAllStudents()
    {
        Query query = Firestore.Collection("users").WhereEqualTo("role", "student");
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        List<Dictionary<string, object>> students = new();
        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            var data = doc.ToDictionary();
            data["docId"] = doc.Id;
            students.Add(data);
        }
        return students;
    }

    /// <summary>
    /// status가 "pending"인 승인 요청을 모두 가져옵니다
    /// </summary>
    public async Task<List<Dictionary<string, object>>> GetPendingApprovals()
    {
        Query query = Firestore.Collection("approvals").WhereEqualTo("status", "pending");
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        List<Dictionary<string, object>> approvals = new();
        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            var data = doc.ToDictionary();
            data["docId"] = doc.Id;
            approvals.Add(data);
        }
        return approvals;
    }

    /// <summary>
    /// 승인 요청을 승인 처리하고 학생에게 점수를 부여합니다
    /// </summary>
    public async Task ApproveRequest(string docId, string studentEmail, long reward)
    {
        DocumentReference docRef = Firestore.Collection("approvals").Document(docId);
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "status", "approved" },
            { "reviewedAt", FieldValue.ServerTimestamp },
            { "reviewedBy", AuthManager.Instance.LoginUserID ?? "teacher" }
        });

        await UpdateUserScore(studentEmail, reward);
        await IncrementCompletedMissionCount(studentEmail);
        Debug.Log($"승인 완료: {docId}, 보상: {reward}");
    }

    /// <summary>
    /// 유저의 완료 미션 수를 1 증가시킵니다
    /// </summary>
    public async Task IncrementCompletedMissionCount(string userEmail)
    {
        Query query = Firestore.Collection("users").WhereEqualTo("email", userEmail);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Count > 0)
        {
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                await doc.Reference.UpdateAsync("completedMissionCount", FieldValue.Increment(1));
                return;
            }
        }

        Debug.LogError($"{userEmail} 에 맞는 유저가 없습니다. (completedMissionCount)");
    }

    /// <summary>
    /// 유저가 현재 로그인 중인지 여부 반환
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    public async Task<bool> GetIsExitUser(string userID)
    {
        Query query = Firestore.Collection("users").WhereEqualTo("email", userID);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Count == 0) return false;

        DocumentSnapshot doc = snapshot.Documents.First();
        return doc.TryGetValue("isOnline", out bool isOnline) && isOnline;
    }

    /// <summary>
    /// 승인 요청을 거절 처리합니다
    /// </summary>
    // public async Task RejectRequest(string docId)
    // {
    //     DocumentReference docRef = Firestore.Collection("approvals").Document(docId);
    //     await docRef.UpdateAsync(new Dictionary<string, object>
    //     {
    //         { "status", "rejected" },
    //         { "reviewedAt", FieldValue.ServerTimestamp },
    //         { "reviewedBy", AuthManager.Instance.LoginUserID ?? "teacher" }
    //     });
    //     Debug.Log($"거절 완료: {docId}");
    // }

    /// <summary>
    /// 학생이 미션 완료 시 승인 요청을 생성합니다
    /// </summary>
    public async Task SubmitMissionApproval(string studentEmail, string studentNickname,
        string missionId, string missionName, string teamId, long reward)
    {
        var data = new Dictionary<string, object>
        {
            { "studentId", Auth.CurrentUser?.UserId ?? "" },
            { "studentEmail", studentEmail },
            { "studentNickname", studentNickname },
            { "missionId", missionId },
            { "missionName", missionName },
            { "teamId", teamId },
            { "status", "pending" },
            { "submittedAt", FieldValue.ServerTimestamp },
            { "reviewedAt", null },
            { "reviewedBy", null },
            { "reward", reward }
        };

        await Firestore.Collection("approvals").AddAsync(data);
        Debug.Log($"승인 요청 생성: {missionName} by {studentNickname}");
    }
    #endregion

    #region 팀 상태
    //private Dictionary<string, DatabaseReference> _teamListeners = new();
    EventHandler<ValueChangedEventArgs> handler = null;
    DatabaseReference teamRef;
    /// <summary>
    /// 팀 상태 변경 감지 ( RealTimeDB의 status 값 감지. 해당 값 바뀔 때 onChanged 호출 )
    /// </summary>
    /// <param name="onChanged"></param>
    public void ListenTeamStatus(Action<string> onChanged)
    {
        //DatabaseReference teamRef = RealtimeDB.Child("teams").Child(teamID).Child("status");
        teamRef = RealtimeDB.Child("teams").Child("status");

        handler = (sender, args) => //args : Firebase가 넘겨주는 데이터
        {
            if(args.DatabaseError != null) //데이터에서 db 오류 시 로그 return
            {
                Debug.LogError($"팀 상태 감지 오류: {args.DatabaseError.Message}");
                return;
            }

            string status = args.Snapshot.Value?.ToString();  //2. status 값 읽기
            onChanged?.Invoke(status); //3. 콜백 함수에게 문자열 넘기기
        };

        teamRef.ValueChanged += handler; //1. status 값 변경 되면 위 handler 재호출 설정
    }

    /// <summary>
    /// 팀 상태 감지 해제 ( 팀 구성 완료 시, 다른 화면으로 전환 후 더 감지 불필요 )
    /// </summary>
    public async Task UnListenTeamStatus()
    {
        try
        {
            if (teamRef != null)
            {
                teamRef.ValueChanged -= handler;
                await teamRef.SetValueAsync("waiting");
                teamRef = null;
            }
            handler = null;
        }
        catch (Exception e)
        {
            Debug.LogError($"UnListenTeamStatus 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 해당 유저가 팀이 있는지 없는지 확인
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsTeam(string uid)
    {
        DocumentSnapshot snapShot = await FirebaseManager.Instance.Firestore.Collection("users").Document(uid).GetSnapshotAsync();

        bool isFirstTeamSsetup = snapShot.TryGetValue("isFirstTeamSetup", out bool value) && value;

        return isFirstTeamSsetup;
    }
    #endregion

    #region 보상 처리 결과
    /// <summary>
    /// 보상 처리 결과를 RealtimeDB에 저장
    /// </summary>
    /// <param name="studentPrefix"></param>
    /// <param name="studentName"></param>
    /// <param name="rewardedName"></param>
    /// <returns></returns>
    public async Task RewardResult(string studentPrefix, string studentName, string rewardedName)
    {
        DatabaseReference reqRef = RealtimeDB.Child("rewardedUser").Child($"{studentPrefix}_{rewardedName}");

        await reqRef.OnDisconnect().RemoveValue();

        await reqRef.SetValueAsync(new Dictionary<string, object>
        {
            ["rewardedStudentID"] = studentPrefix,
            ["rewardedStudentName"] = studentName,
            ["rewardedName"] = rewardedName,

        });
    }

    private DatabaseReference rewardResultRef;
    private EventHandler<ValueChangedEventArgs> rewardResultHandler;

    /// <summary>
    /// 선생님이 보상 처리 결과를 감지
    /// </summary>
    /// <param name="onChanged"></param>
    public void ListenRewardReslt (Action<List<RewardResultData>> onChanged)
    {
        rewardResultRef = RealtimeDB.Child("rewardedUser");

        rewardResultHandler = (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"미션 요청 감지 오류: {args.DatabaseError.Message}");
                return;
            }

            List<RewardResultData> requests = new();

            if (args.Snapshot.Value == null)
            {
                onChanged?.Invoke(requests);
                return;
            }

            foreach (Firebase.Database.DataSnapshot child in args.Snapshot.Children)
            {
                requests.Add(new RewardResultData
                {
                    RewardStudentID = child.Child("rewardedStudentID").Value?.ToString() ?? "",
                    RewardStudentName = child.Child("rewardedStudentName").Value?.ToString() ?? "",
                    RewardName = child.Child("rewardedName").Value?.ToString() ?? "",
                });
            }

            onChanged?.Invoke(requests);
        };

        rewardResultRef.ValueChanged += rewardResultHandler;
    }
    public void StopListenRewardResult()
    {
        if (rewardResultRef != null)
        {
            rewardResultRef.ValueChanged -= rewardResultHandler;
            rewardResultRef = null;
        }
        rewardResultHandler = null;
    }
    #endregion

    #region 미션 요청/수락
   /// <summary>
   /// 학생 미션 요청 
   /// </summary>
   /// <param name="studentPrefix"></param>
   /// <param name="studentId"></param>
   /// <param name="studentName"></param>
   /// <param name="teamId"></param>
   /// <param name="missionId"></param>
   /// <param name="missionName"></param>
   /// <param name="missionReward"></param>
   /// <param name="routeId"></param>
   /// <returns></returns>
    public async Task SendMissionApprovalRequest(
        string studentPrefix, string studentId, string studentName,
        string teamId, string missionId, string missionName, long missionReward, string routeId)
    {
        Debug.Log(missionReward);

        DatabaseReference reqRef = RealtimeDB.Child("missionRequests").Child(studentPrefix);

        await reqRef.OnDisconnect().RemoveValue();

        await reqRef.SetValueAsync(new System.Collections.Generic.Dictionary<string, object>
        {
            ["studentId"] = studentId,
            ["studentName"] = studentName,
            ["teamId"] = teamId,
            ["missionId"] = missionId,
            ["missionName"] = missionName,
            ["missionReward"] = missionReward,
            ["routeId"] = routeId,
            ["status"] = "pending",
        });
    }

    /// <summary>
    /// 승인 요청 실시간 감지
    /// </summary>
    private DatabaseReference _missionRequestsRef;
    private EventHandler<ValueChangedEventArgs> _pendingRequestsHandler;

    public void ListenPendingRequests(Action<List<MissionRequestData>> onChanged)
    {
        _missionRequestsRef = RealtimeDB.Child("missionRequests");

        _pendingRequestsHandler = (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"미션 요청 감지 오류: {args.DatabaseError.Message}");
                return;
            }

            List<MissionRequestData> requests = new();

            if (args.Snapshot.Value == null)
            {
                onChanged?.Invoke(requests);
                return;
            }

            foreach (Firebase.Database.DataSnapshot child in args.Snapshot.Children)
            {
                string status = child.Child("status").Value?.ToString();
                if (status != "pending") continue;

                requests.Add(new MissionRequestData
                {
                    StudentPrefix = child.Key,
                    StudentID = child.Child("studentId").Value?.ToString() ?? "",
                    StudentName = child.Child("studentName").Value?.ToString() ?? "",
                    TeamID = child.Child("teamId").Value?.ToString() ?? "",
                    MissionID = child.Child("missionId").Value?.ToString() ?? "",
                    MissionName = child.Child("missionName").Value?.ToString() ?? "",
                    MissionReward = child.Child("missionReward").Value?.ToString() ?? "",
                    RouteID = child.Child("routeId").Value?.ToString() ?? "",
                    Status = status,
                });
            }

            onChanged?.Invoke(requests);
        };

        _missionRequestsRef.ValueChanged += _pendingRequestsHandler;
    }

    public void StopListenPendingRequests()
    {
        if (_missionRequestsRef != null)
        {
            _missionRequestsRef.ValueChanged -= _pendingRequestsHandler;
            _missionRequestsRef = null;
        }
        _pendingRequestsHandler = null;
    }

    // 선생님: 승인
    public async Task ApproveRequest(string studentPrefix)
    {
        await RealtimeDB.Child("missionRequests").Child(studentPrefix)
            .Child("status").SetValueAsync("approved");

        //await DeleteMissionRequest(studentPrefix);
    }

    // 선생님: 거절
    public async Task RejectRequest(string studentPrefix)
    {
        await RealtimeDB.Child("missionRequests").Child(studentPrefix)
            .Child("status").SetValueAsync("rejected");

        
    }

    // ──────────────────────────────────────────
    // 학생: 자신의 요청 status 실시간 감지
    // ──────────────────────────────────────────
    private DatabaseReference _myRequestRef;
    private EventHandler<ValueChangedEventArgs> _myRequestHandler;

    public void ListenMyRequest(string studentPrefix, Action<string> onStatusChanged)
    {
        StopListenMyRequest();

        _myRequestRef = RealtimeDB.Child("missionRequests").Child(studentPrefix).Child("status");

        _myRequestHandler = (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"요청 상태 감지 오류: {args.DatabaseError.Message}");
                return;
            }

            string status = args.Snapshot.Value?.ToString();
            if (!string.IsNullOrEmpty(status))
                onStatusChanged?.Invoke(status);
        };

        _myRequestRef.ValueChanged += _myRequestHandler;
    }

    public void StopListenMyRequest()
    {
        if (_myRequestRef != null)
        {
            _myRequestRef.ValueChanged -= _myRequestHandler;
            _myRequestRef = null;
        }
        _myRequestHandler = null;
    }

    // 학생: 처리 완료 후 노드 삭제
    public async Task DeleteMissionRequest(string studentPrefix)
    {
        await RealtimeDB.Child("missionRequests").Child(studentPrefix).RemoveValueAsync();
    }

    // ──────────────────────────────────────────
    // 유저 role 조회 (Firestore users 컬렉션)
    // ──────────────────────────────────────────
    public async Task<string> GetUserRole(string email)
    {
        Firebase.Firestore.Query query = Firestore.Collection("users").WhereEqualTo("email", email);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Count > 0)
        {
            foreach (DocumentSnapshot doc in snapshot.Documents)
                return doc.TryGetValue("role", out string role) ? role : "student";
        }
        return "student";
    }

    /// <summary>
    /// 최초 1회, 자신의 승인 요청 노드가 존재한다면 삭제. ( 노드 엉켜지는 경우를 방지 )
    /// </summary>
    /// <param name="studentPrefix"></param>
    /// <returns></returns>
    public async Task ClearMyPendingRequest(string studentPrefix)
    {
        DataSnapshot snapshot = await RealtimeDB.Child("missionRequests").Child(studentPrefix).GetValueAsync();
        if (snapshot.Exists)
            await RealtimeDB.Child("missionRequests").Child(studentPrefix).RemoveValueAsync();
    }
    #endregion
}
