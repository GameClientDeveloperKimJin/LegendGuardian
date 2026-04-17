using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 인증 관련 관리
/// </summary>
public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;
    public static AuthManager Instance
    {
        get
        {
            return instance;
        }
    }

    public event Action<string> OnAuthInfo;

    private string loginUserID; //로그인 한 유저의 ID
    public string LoginUserID => loginUserID;

    public void SaveUserID(string userID) => this.loginUserID = userID;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 회원가입: Auth 유저 생성 -> Firestore에 유저 데이터 저장
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<(bool success, string error)> SignUpAsync(string email,string password)
    {
        if(!FirebaseManager.Instance.IsConnect)
        {
            return (false, "Firebase 초기화 되지 않음");
        }

        try
        {
            email += "@giadian.com";

            AuthResult result = await FirebaseManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);

            await SaveUserToFirestore(result.User);

            Debug.Log($"회원가입 성공 - 회원가입 한 ID : {result.User.Email}");

            return (true, null); // true : 성공 , null : 에러 없음
        }
        catch (FirebaseException e)
        {
            string msg = ParseAuthError((AuthError)e.ErrorCode);

            OnAuthInfo?.Invoke(msg);

            return (false, msg);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }

    /// <summary>
    /// 로그인
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<(bool success, string error)> SignInAsync(string email, string password)
    {
        if (!FirebaseManager.Instance.IsConnect)
        {
            return (false, "Firebase 초기화 되지 않음");
        }

        try
        {
            email += "@giadian.com";

            AuthResult result = await FirebaseManager.Instance.Auth.SignInWithEmailAndPasswordAsync(email, password);

            // Firestore에 유저 데이터가 존재하는지 확인
            DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users").Document(result.User.UserId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                FirebaseManager.Instance.Auth.SignOut();
                return (false, "존재하지 않는 사용자입니다.");
            }

            Debug.Log($"로그인 성공 - 로그인 한 ID : {result.User.Email}");

            return (true, null); // true : 성공 , null : 에러 없음
        }
        catch (FirebaseException e)
        {
            string msg = ParseAuthError((AuthError)e.ErrorCode);

            OnAuthInfo?.Invoke(msg);
            return (false, msg);
        }
        catch (Exception e)
        {
            Debug.LogError($"일반 오류: {e.GetType().Name} / {e.Message}");
            return (false, e.Message);
        }
    }

    /// <summary>
    /// 로그아웃
    /// </summary>
    public void SignOut()
    {
        FirebaseManager.Instance.Auth.SignOut();

        Debug.Log("로그아웃 완료");
    }

    private async Task SaveUserToFirestore(FirebaseUser user)
    {
        DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users").Document(user.UserId);

        Dictionary<string, object> data = new Dictionary<string, object>();

        data["userid"] = user.Email;

        await docRef.SetAsync(data);
        Debug.Log($"FireStore users/{user.UserId} 저장 완료");
    }


    /// <summary>
    /// AuthError에 맞는 오류 메시지 반환
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    private string ParseAuthError(AuthError error)
    {
        string message = "";

        switch(error)
        {
            case AuthError.EmailAlreadyInUse:
                message = "이미 사용 중인 이메일입니다.";
                break;
            case AuthError.InvalidEmail:
                message = "이메일 형식이 올바르지 않습니다.";
                break;
            case AuthError.WeakPassword:
                message = "비밀번호는 6자 이상이어야 합니다.";
                break;
            case AuthError.WrongPassword:
                message = "비밀번호가 틀렸습니다.";
                break;
            case AuthError.UserNotFound:
                message = "존재하지 않는 사용자입니다.";
                break;
            case AuthError.NetworkRequestFailed:
                message = "네트워크 상태를 확인해주세요.";
                break;
            case AuthError.TooManyRequests:
                message = "잠시 후 다시 시도해주세요.";
                break;
            default:
                message = $"오류가 발생했습니다. {error}";
                break;
        }
        return message;
     }
}
