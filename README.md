# LegendGuardian
**탄소중립 에너지 교육 모바일 게임** | 동양대학교 AI빅데이터융합학과 AI 서비스러닝

> 청소년이 스마트폰으로 건물 층을 탐험하며 탄소중립·에너지 행동을 게임으로 배우는 Android 앱입니다.
> 구립신림청소년독서실에 기증 예정 (2026년 6월 12일 전달식)

---

## 프로젝트 소개

학생들이 룰렛으로 팀을 구성하고, 층별 지도를 탐험하며 퀴즈를 풀고 미션을 수행합니다.
완료된 미션은 선생님이 실시간으로 승인하고, 보상(Wh)을 획득할 수 있습니다.

## 기술 스택

| 분류 | 기술 |
|------|------|
| 게임 엔진 | Unity 2D |
| 언어 | C# |
| 인증 | Firebase Authentication |
| DB | Firebase Firestore + Realtime Database |
| 웹 호스팅 | Firebase Hosting (관리자 웹) |
| 빌드 | Android (APK) |
| AI 도구 | Claude Code (Anthropic) |

## 설치 및 실행

### 개발 환경 실행
1. Unity (LTS 버전) 설치
2. 이 저장소 클론: `git clone <repo-url>`
3. Unity Hub에서 `D:\UnityProject\2D\Giadian` 폴더를 프로젝트로 추가
4. `Assets/google-services.json` 파일을 Firebase 콘솔에서 다운로드하여 배치
5. Unity Editor에서 프로젝트 열기 → Play 버튼으로 실행

### Android APK 빌드
1. Unity → File → Build Settings → Android 선택
2. `google-services.json` 위치 확인 (`Assets/` 폴더)
3. Build 버튼 클릭

> **주의**: `google-services.json`과 Firebase API 키는 `.gitignore`로 관리합니다. 저장소에 포함되지 않습니다.

## 폴더 구조

```
Assets/
├── KJ/Scripts/          # 백엔드·Firebase·미션·랭킹·보상 (김진)
│   ├── FirebaseDB/      # FirebaseManager.cs, AuthManager.cs
│   ├── Mission/         # 미션 데이터, 승인 요청
│   └── UI/              # 랭킹, 보상, 메인 화면 뷰
├── SE/Scripts/          # UI·퀴즈·에몬·진행도 (시은)
│   ├── Quiz/            # QuizSceneManager, QuizSet, QuizQuestion
│   ├── UI/              # 룰렛, 보상, 지도, 메인
│   └── GameManagement/  # ScoreManager, SafeAreaApplier
├── JM/UI/               # 선생님 화면 컴포넌트
├── Firebase/            # Firebase Unity SDK
└── docs/                # 기증 산출물 문서
    ├── 1_소프트웨어정의서_v1.md
    ├── 2_운영매뉴얼_v1.md
    ├── 3_AI_Prompt_Book_v1.md
    └── 4_한계보고서_v1.md
```

## 주요 기능

### 관리자 웹 (https://legendguardian-d1c51.web.app/)
> 게임 시작 전 콘텐츠를 사전 설정하는 전용 대시보드

- 팀 설정 (팀 이름·속성·구성원 등록)
- 루트 순서 설정 (팀별 존 탐험 순서 지정)
- 미션 설정 (존별 미션 이름·설명·보상 Wh 등록)

### 모바일 앱 (Android)
- 로그인/회원가입 (Firebase Auth, @giadian.com 계정)
- 룰렛 팀 구성 (실시간 DB 동기화)
- 층별 지도 탐험 (실외/1층/2층/3층)
- 퀴즈 (층별 객관식, 정답 시 +3 Wh)
- 미션 시스템 (학생 완료 요청 → 앱 내 선생님 승인 → Wh 지급)
- 랭킹 (상위 10위)
- 보상 교환 (Wh → 실물 보상)
- 선생님 관리 화면 (학생 현황 실시간, 미션 승인, 보상 처리)

## 환경 변수 / 설정 파일

| 파일 | 위치 | 설명 |
|------|------|------|
| `google-services.json` | `Assets/` | Firebase Android 설정 (저장소 미포함) |
| Firebase Realtime DB URL | `FirebaseManager.cs` | `https://legendguardian-d1c51-default-rtdb.firebaseio.com/` |

## Git 브랜치 구조

| 브랜치 | 담당자 | 역할 |
|--------|--------|------|
| `main` | 전체 | 배포 버전 |
| `develop` | 전체 | 통합 개발 |
| `dev_backend_jin` | 김진 (KJ) | Firebase·미션·랭킹·보상 |
| `dev/sieun` | 시은 (SE) | UI·퀴즈·에몬 |
| `feat_Web&DataBase` | JM | 웹·데이터베이스 연동 |

## 라이선스

본 프로젝트는 동양대학교 AI빅데이터융합학과 AI 서비스러닝 수업의 산학협력 결과물로,
구립신림청소년독서실에 기증됩니다.

## 제작

동양대학교 AI빅데이터융합학과 AI 서비스러닝 팀
지도교수: 안병천 교수
제작 연도: 2026년

