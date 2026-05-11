using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// 로컬에서 유저 데이터 정의
/// </summary>
public class UserData
{
    public string UserID { get; set; }

    public string UserName { get; set; }

    public string TeamID { get; set; }

    public UserData(string userID, string userName)
    {
        this.UserID = userID;
        this.UserName = userName;
    }

    /// <summary>
    /// 팀 참가 되었을 때 팀 ID 저장 
    /// </summary>
    /// <param name="teamID"></param>
    public void JoinTeam(string teamID)
    {
        this.TeamID = teamID;
    }
}
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

    public string LoginUserID { get; private set; }
    public string LoginUserName { get; private set; }

    public bool IsTeamReady { get; private set; }

    public Dictionary<string, UserData> userDictionary = new();


    /// <summary>
    /// 유저가 로그인 했을 때, 로그인 한 유저 데이터 저장
    /// </summary>
    /// <param name="userID"></param>
    public async void SaveLoginUser(string userID)
    {
        try
        {
            this.LoginUserID = userID + "@giadian.com";
            this.LoginUserName = await FirebaseManager.Instance.GetUserIDToNickName(LoginUserID);
            UserData userData = new UserData(LoginUserID, LoginUserName);
            userDictionary[userID] = userData;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveLoginUser 오류: {e.Message}");
        }
    }


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

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => FirebaseManager.Instance != null);

        yield return new WaitUntil(() => FirebaseManager.Instance.IsConnect);

        IsTeamReady = false;

    }

    #region 회원가입 / 로그인 / 로그아웃
    /// <summary>
    /// 회원가입: Auth 유저 생성 -> Firestore에 유저 데이터 저장
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<(bool success, string error)> SignUpAsync(string email,string password , string nickname)
    {
        if(!FirebaseManager.Instance.IsConnect)
        {
            return (false, "Firebase 초기화 되지 않음");
        }

        try
        {
            email += "@giadian.com";

            AuthResult result = await FirebaseManager.Instance.Auth.CreateUserWithEmailAndPasswordAsync(email, password);

            await SaveUserToFirestore(result.User,nickname);

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

            AddTeamListener();

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

    private void OnDestroy()
    {
        SignOut();
    }
    /// <summary>
    /// 로그아웃
    /// </summary>
    public void SignOut()
    {
        try
        {
            FirebaseManager.Instance?.Auth?.SignOut();
            RemoveTeamListener();
            Debug.Log("로그아웃 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"SignOut 오류: {e.Message}");
        }
    }

    #endregion


    /// <summary>
    /// 유저 컬렉션에 UID로 문서 만들고 해당 문서에 데이터 저장까지 레스고 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="nickname"></param>
    /// <returns></returns>
    private async Task SaveUserToFirestore(FirebaseUser user , string nickname)
    {
        DocumentReference docRef = FirebaseManager.Instance.Firestore.Collection("users").Document(user.UserId);

        Dictionary<string, object> data = new Dictionary<string, object>();

        //data["userid"] = user.Email; //아이디 저장
        data["email"] = user.Email; //아이디(이메일) 저장
        data["nickname"] = nickname; //닉네임 저장
        data["score"] = 0L; //개인 wh 저장, 0L : long(int 64)
        data["role"] = "student";

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
            case AuthError.MissingEmail:
                message = "이메일을 입력해주세요.";
                break;

            case AuthError.MissingPassword:
                message = "비밀번호를 입력해주세요.";
                break;

            case AuthError.UserDisabled:
                message = "비활성화된 계정입니다.";
                break;
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "이미 다른 방식으로 가입된 이메일입니다.";
                break;

            case AuthError.SessionExpired:
                message = "세션이 만료됐습니다. 다시 로그인해주세요.";
                break;
            default:
                message = $"오류가 발생했습니다. {error}";
                break;
        }
        return message;
     }


    #region 팀 상태 이벤트 연결

    /// <summary>
    /// 팀 구성 이벤트 구독 메서드 호출
    /// </summary>
    /// <param name="teamID"></param>
    public void AddTeamListener() => FirebaseManager.Instance?.ListenTeamStatus(OnStatusChanged);

    /// <summary>
    /// 팀 구성 이벤트 구독 해제 메서드 호출
    /// </summary>
    public void RemoveTeamListener() => FirebaseManager.Instance?.UnListenTeamStatus();

    /// <summary>
    /// RealTimeDB에 status 값이 변경되었을 때 호출 -> 값이 만약 ready라면 클라측 팀 구성 완료
    /// </summary>
    /// <param name="status"></param>
    private void OnStatusChanged(string status)
    {
        if(status == "ready")
        {
            if (string.IsNullOrEmpty(LoginUserID)) return;

            string cleanID = LoginUserID.Split('@')[0]; // 이메일 제거

            if (userDictionary.TryGetValue(cleanID, out var userData))
            {
                Debug.Log($"팀 {userData.TeamID} 구성 완료! ");
                userDictionary[cleanID]?.JoinTeam(userData.TeamID); //유저 데이터 클래스에 JoinTeam 호출해서 유저의 팀 ID를 로컬로 저장
                IsTeamReady = true;
            }
        }
    }

    /// <summary>
    /// 유저의 팀 이름을 반환
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetUserTeamName()
    {
        string cleanID = LoginUserID.Split('@')[0]; // 이메일 제거

        Debug.Log(cleanID);

        var teamName = await FirebaseManager.Instance.GetUserIDToTeamID(cleanID);

        Debug.Log("팀 ID" + teamName);
        return teamName;
    }
    



    #endregion
}
