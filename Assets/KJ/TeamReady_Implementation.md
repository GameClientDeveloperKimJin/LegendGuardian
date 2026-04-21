# 팀 구성 완료 알림 구현 가이드
> Firebase Realtime Database를 사용해 관리자 웹 → Unity 클라이언트로 팀 구성 완료를 전달한다.

---

## 1. Firebase Realtime DB 데이터 구조

```
teams/
  {teamId}/
    status: "waiting" | "ready"
    players/
      {userId}: true
```

- `status` 값이 `"ready"` 로 바뀌는 순간 Unity가 감지한다.
- `players`는 관리자가 팀 구성 시 함께 기록한다.

---

## 1-1. Firebase 콘솔에서 구조 직접 만드는 방법 (초기 세팅)

> 코드를 짜기 전에 Firebase 콘솔에서 먼저 구조를 잡아두면 확인이 편하다.

### 절차

**① Firebase 콘솔 접속**
- [console.firebase.google.com](https://console.firebase.google.com) → 프로젝트(`legendguardian-d1c51`) 선택

**② Realtime Database 메뉴로 이동**
- 왼쪽 사이드바 → `빌드` → `Realtime Database`

**③ 데이터 탭에서 루트(`/`) 클릭**
- 현재 DB 트리가 보인다.
- 우측 `+` 버튼 클릭 → 새 노드 추가 시작

**④ `teams` 노드 생성**
- 이름: `teams`
- 값: 비워두고 `+` 클릭해 자식 노드로 이동

**⑤ `{teamId}` 노드 생성**
- 이름: `team_001` (테스트용 팀 ID)
- 값: 비워두고 `+` 클릭해 자식 노드로 이동

**⑥ `status` 필드 추가**
- 이름: `status`
- 값: `waiting`
- 확인(체크) 클릭

**⑦ `players` 노드 추가**
- `team_001` 노드에서 다시 `+` 클릭
- 이름: `players`
- 값: 비워두고 `+` 클릭해 자식 노드로 이동

**⑧ `{userId}` 추가**
- 이름: `user_a` (테스트용 유저 ID)
- 값: `true`
- 확인(체크) 클릭

### 완성된 콘솔 트리 모습

```
legendguardian-d1c51-default-rtdb
└── teams
    └── team_001
        ├── status: "waiting"
        └── players
            └── user_a: true
```

> **팁:** 콘솔에서 직접 만든 구조는 테스트용이다.  
> 실제 운영에서는 섹션 4의 관리자 웹 코드가 이 구조를 자동으로 생성한다.

---

## 2. 전체 흐름

```
[관리자 웹]
  팀 구성 완료 버튼 클릭
        │
        ▼
  teams/{teamId}/status = "ready"  ← Realtime DB에 SET
        │
        ▼ (ValueChanged 이벤트)
[Unity 클라이언트]
  FirebaseManager.ListenTeamStatus()가 감지
        │
        ▼
  AuthManager.OnTeamReady 이벤트 발생
        │
        ▼
  씬 전환 / UI 처리
```

---

## 3. Unity 구현

### 3-1. FirebaseManager.cs — 구독 메서드 추가

```csharp
// 구독 중인 리스너를 관리 (teamId → DatabaseReference)
private Dictionary<string, DatabaseReference> _teamListeners = new();

/// <summary>
/// 팀 상태 구독 시작. status 값이 바뀔 때마다 onChanged 호출.
/// </summary>
public void ListenTeamStatus(string teamId, Action<string> onChanged)
{
    if (_teamListeners.ContainsKey(teamId)) return; // 중복 방지

    DatabaseReference teamRef = RealtimeDB
        .Child("teams").Child(teamId).Child("status");

    EventHandler<ValueChangedEventArgs> handler = null;
    handler = (sender, args) =>
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"팀 상태 리스너 오류: {args.DatabaseError.Message}");
            return;
        }
        string status = args.Snapshot.Value?.ToString();
        onChanged?.Invoke(status);
    };

    teamRef.ValueChanged += handler;
    _teamListeners[teamId] = teamRef;
}

/// <summary>
/// 팀 상태 구독 해제. 씬 전환 전 반드시 호출.
/// </summary>
public void UnlistenTeamStatus(string teamId)
{
    if (_teamListeners.TryGetValue(teamId, out DatabaseReference teamRef))
    {
        // ValueChanged 핸들러는 람다로 등록했으므로, 레퍼런스를 끊으면 GC가 처리
        _teamListeners.Remove(teamId);
    }
}
```

> **주의:** `ValueChanged`는 구독 즉시 현재 값도 한 번 호출된다.
> `"waiting"` 상태는 무시하고 `"ready"` 일 때만 처리할 것.

---

### 3-2. AuthManager.cs — 팀 상태 이벤트 연결

```csharp
// 팀 구성 완료 이벤트
public event Action OnTeamReady;

/// <summary>
/// 팀 ID를 알게 된 시점에 호출. Realtime DB 구독 시작.
/// </summary>
public void StartListenTeam(string teamId)
{
    AuthManager.Instance.userDictionary[LoginUserID]?.JoinTeam(teamId);

    FirebaseManager.Instance.ListenTeamStatus(teamId, (status) =>
    {
        if (status == "ready")
        {
            Debug.Log($"팀 {teamId} 구성 완료!");
            OnTeamReady?.Invoke();
        }
    });
}
```

---

### 3-3. 호출 예시 (대기 씬 등)

```csharp
// 대기 씬 진입 시
void Start()
{
    string teamId = "team_001"; // 서버에서 받아온 팀 ID
    AuthManager.Instance.OnTeamReady += HandleTeamReady;
    AuthManager.Instance.StartListenTeam(teamId);
}

void HandleTeamReady()
{
    // 구독 해제 후 씬 전환
    AuthManager.Instance.OnTeamReady -= HandleTeamReady;
    SceneManager.LoadScene("GameScene");
}

void OnDestroy()
{
    // 혹시 씬이 먼저 파괴될 경우를 대비한 정리
    AuthManager.Instance.OnTeamReady -= HandleTeamReady;
}
```

---

## 4. 관리자 웹 구현 (JavaScript)

```javascript
import { getDatabase, ref, set, update } from "firebase/database";

const db = getDatabase();

// 팀 구성 완료 처리
async function completeTeamSetup(teamId, playerIds) {
  const teamRef = ref(db, `teams/${teamId}`);

  // 팀원 목록과 상태를 한 번에 기록
  await update(teamRef, {
    status: "ready",
    players: Object.fromEntries(playerIds.map(id => [id, true]))
  });

  console.log(`팀 ${teamId} 구성 완료 알림 전송`);
}

// 사용 예시
completeTeamSetup("team_001", ["user_a", "user_b", "user_c", "user_d"]);
```

---

## 5. Firebase 보안 규칙 (Realtime DB Rules)

```json
{
  "rules": {
    "teams": {
      "$teamId": {
        ".read": "auth != null",
        ".write": "auth.token.admin == true"
      }
    }
  }
}
```

- **읽기**: 로그인한 유저만 가능
- **쓰기**: 관리자 커스텀 클레임(`admin: true`)이 있는 계정만 가능

---

## 6. 미결 사항 (구현 전 확인 필요)

| 항목 | 질문 |
|------|------|
| teamId 취득 시점 | 로그인 직후? 대기 화면 진입 시? |
| teamId 저장 위치 | Firestore `users` 컬렉션? 별도 경로? |
| 대기 UI | 팀 구성 대기 중 표시할 화면 있나요? |
| 타임아웃 처리 | 관리자가 오래 안 누를 경우 재시도 로직 필요? |
