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
            Debug.LogError($"Firebase 의존성 오류: {status}");
        }
    }



    /// <summary>
    /// 유저 ID를 통해 DB에 저장된 닉네임 가져오기
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    public async Task<string> GetUserIDToNickName(string userID)
    {
        // users 컬렉션에서 userid 필드값이 userID와 같은 문서를 찾아라.  
        Firebase.Firestore.Query query = FirebaseManager.Instance.Firestore
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
    #region 팀 구성
    //private Dictionary<string, DatabaseReference> _teamListeners = new();
    EventHandler<ValueChangedEventArgs> handler = null;
    DatabaseReference teamRef;
    /// <summary>
    /// 팀 상태 구독 시작 ( RealTimeDB에 stauts 값 있음. 해당 값 바뀔 때 onChanged 발행 ) 
    /// </summary>
    /// <param name="teamID"></param>
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
            if(args.DatabaseError != null) //데이터에서 db 에러 날 경우 return
            {
                Debug.LogError($"팀 상태 리스너 오류: {args.DatabaseError.Message}");
                return;
            }

            string status = args.Snapshot.Value?.ToString();  //2. stauts 값 읽음
            onChanged?.Invoke(status); //3. 콜백 함수에게 문자열 넘김
        };

        teamRef.ValueChanged += handler; //1. status 값 변경 되면 위 handler 람다식 실행
        //_teamListeners[teamID] = teamRef;
     }

    /// <summary>
    /// 팀 상태 구독 해제 ( 팀 구성 완료 후, 게임 씬으로 전환 될 때 해제 필요함 ) 
    /// </summary>
    /// <param name="teamID"></param>
    public void UnListenTeamStatus()
    {
        teamRef.ValueChanged -= handler;
    
        teamRef = null;
        handler = null;
    }
    #endregion
}
