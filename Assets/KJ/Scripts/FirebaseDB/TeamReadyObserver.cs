using System;
using System.Collections;
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
        private FirebaseFirestore db;
        public string ConfirmedTeamId { get; private set; }

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

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => FirebaseManager.Instance != null && FirebaseManager.Instance.IsConnect);

            FirebaseAuth.DefaultInstance.StateChanged += OnAuthStateChanged;

            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                Initialize();
        }

        private void OnAuthStateChanged(object sender, EventArgs e)
        {
            if (FirebaseAuth.DefaultInstance.CurrentUser != null && statusRef == null)
            {
                Initialize();
            }
            else if (FirebaseAuth.DefaultInstance.CurrentUser == null)
            {
                Unsubscribe();
                statusRef = null;
            }
        }

        public void Initialize()
        {
            db = FirebaseFirestore.DefaultInstance;
            statusRef = FirebaseDatabase.DefaultInstance.GetReference("gameStatus/teamReady");

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
                    // 로그인 전 신호는 무시 (구독 유지 - 로그인 후 재감지 가능)
                    if (FirebaseAuth.DefaultInstance.CurrentUser == null)
                    {
                        Debug.LogWarning("TeamReadyObserver: 로그인 전 신호 감지, 무시합니다.");
                        return;
                    }

                    Debug.Log("TeamReadyObserver: 관리자의 '팀 확정' 신호를 감지했습니다!");
                    Unsubscribe(); // 성공 확정 시에만 구독 해제
                    FetchMyTeamID();
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
            FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
        }
    }
}
