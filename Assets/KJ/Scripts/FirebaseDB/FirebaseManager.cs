using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseFirestore Firestore { get; private set; }
    public DatabaseReference RealtimeDB { get; private set; }
    public FirebaseAuth Auth { get; private set; }

    public bool IsReady { get; private set; } = false;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    async void InitializeFirebase()
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
            IsReady = true;
            Debug.Log("Firebase 초기화 완료!");
        }
        else
        {
            Debug.LogError($"Firebase 의존성 오류: {status}");
        }
    }
}
