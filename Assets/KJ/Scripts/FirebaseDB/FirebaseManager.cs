using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

    #region 팀 ID를 통해 routes 컬렉션 데이터 가져오기

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
        //중복 구독 방지
        //if(_teamListeners.ContainsKey(teamID))
        //{
        //    return;
        //}

        // DatabaseReference teamRef = RealtimeDB.Child("teams").Child(teamID).Child("status");
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
        //_teamListeners[teamID] = teamRef;
     }

    /// <summary>
    /// 팀 상태 감지 해제 ( 팀 구성 완료 시, 다른 화면으로 전환 후 더 감지 불필요 )
    /// </summary>
    public void UnListenTeamStatus()
    {
        teamRef.ValueChanged -= handler;

        teamRef = null;
        handler = null;
    }
    #endregion
}
