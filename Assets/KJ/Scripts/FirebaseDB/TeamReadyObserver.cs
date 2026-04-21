using System;
using UnityEngine;
using Firebase.Database;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;

namespace KJ.FirebaseDB
{
    public class TeamReadyObserver : MonoBehaviour
    {
        public static TeamReadyObserver Instance { get; private set; }

        private DatabaseReference statusRef;
        private FirebaseFirestore db; // 실수로 지워진 변수 복구
        // 이미 확정된 팀 아이디를 캐싱 (Start 시점에 늦게 구독해도 탈 수 있도록)
        public string ConfirmedTeamId { get; private set; }

        // 팀 정보를 가져왔을 때 발동하는 이벤트 (매개변수: 새로 할당된 팀 ID)
        public event Action<string> OnTeamConfirmed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Firebase 초기화 후 호출되어야 합니다.
            // 보통 FirebaseManager 등에서 FirebaseApp.CheckAndFixDependenciesAsync() 이후에 이 스크립트를 활성화하는 것이 좋습니다.
            Initialize();
        }

        public void Initialize()
        {
             db = FirebaseFirestore.DefaultInstance;
            statusRef = FirebaseDatabase.DefaultInstance.GetReference("gameStatus/teamReady");

            // 상태 변경 리스너 등록
            statusRef.ValueChanged += HandleTeamReadyStatusChanged;
            Debug.Log("TeamReadyObserver: RTDB 'gameStatus/teamReady' 구독 시작");
        }

        private void HandleTeamReadyStatusChanged(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Database Error: " + args.DatabaseError.Message);
                return;
            }

            if (args.Snapshot != null && args.Snapshot.Exists)
            {
                var isReadyObj = args.Snapshot.Child("isReady").Value;
                if (isReadyObj != null && (bool)isReadyObj == true)
                {
                    Debug.Log("TeamReadyObserver: 관리자의 '팀 확정' 신호를 감지했습니다!");
                    FetchMyTeamID();
                    
                    // 신호를 한 번 받은 후에는 데이터를 소비(초기화)하거나 구독을 해지할 수 있습니다.
                    // 여기서는 구독 해지:
                    Unsubscribe();
                }
            }
        }

        private void FetchMyTeamID()
        {
            // 현재 로그인된 유저 가져오기
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser == null)
            {
                Debug.LogError("TeamReadyObserver: 로그인된 유저가 없어 팀 정보를 가져올 수 없습니다.");
                return;
            }

            string uid = auth.CurrentUser.UserId;
            DocumentReference userDoc = db.Collection("users").Document(uid);

            userDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("TeamReadyObserver: Firestore에서 유저 문서를 가져오는 데 실패했습니다.");
                    return;
                }

                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField("teamId"))
                {
                    string myTeamId = snapshot.GetValue<string>("teamId");
                    Debug.Log($"TeamReadyObserver: 성공적으로 내 팀({myTeamId}) 정보를 가져왔습니다.");
                    
                    ConfirmedTeamId = myTeamId; // 캐싱 저장
                    // 이벤트 발생시켜 룰렛 등 시각화 처리 로직으로 연결
                    OnTeamConfirmed?.Invoke(myTeamId);
                }
                else
                {
                    Debug.LogWarning("TeamReadyObserver: 유저 문서가 없거나 teamId 필드가 아직 없습니다.");
                }
            });
        }

        public void Unsubscribe()
        {
            if (statusRef != null)
            {
                statusRef.ValueChanged -= HandleTeamReadyStatusChanged;
                Debug.Log("TeamReadyObserver: RTDB 구독 해제");
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
