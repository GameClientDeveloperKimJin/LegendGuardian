using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Firestore;
using System;
using UnityEngine;

public class FirebaseManager : MonoBehaviour, IDisposable
{
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
}
