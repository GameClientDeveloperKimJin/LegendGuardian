using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KJ.UI
{
    public class TeamRouletteController : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject roulettePanel;
        public TextMeshProUGUI resultText;
        public Image rouletteWheel; // 빙글빙글 도는 이미지 
        
        [Header("Settings")]
        public float spinDuration = 3f;

        private void Start()
        {
            // 처음에 룰렛 패널은 꺼둡니다.
            roulettePanel.SetActive(false);

            // 옵저버 이벤트에 구독
            if (KJ.FirebaseDB.TeamReadyObserver.Instance != null)
            {
                KJ.FirebaseDB.TeamReadyObserver.Instance.OnTeamConfirmed += HandleTeamConfirmed;
                
                // 만약 스크립트가 켜지기 전에 이미 팀 확정 이벤트가 지나갔다면 캐시된 값을 확인하여 바로 실행
                if (!string.IsNullOrEmpty(KJ.FirebaseDB.TeamReadyObserver.Instance.ConfirmedTeamId))
                {
                    HandleTeamConfirmed(KJ.FirebaseDB.TeamReadyObserver.Instance.ConfirmedTeamId);
                }
            }
            else
            {
                Debug.LogWarning("TeamRouletteController: TeamReadyObserver 인스턴스를 찾을 수 없습니다.");
            }
        }

        private void OnDestroy()
        {
            if (KJ.FirebaseDB.TeamReadyObserver.Instance != null)
            {
                KJ.FirebaseDB.TeamReadyObserver.Instance.OnTeamConfirmed -= HandleTeamConfirmed;
            }
        }

        // Firebase에서 확정 신호가 오고, 내 teamID까지 가져왔을 때 호출됨
        private void HandleTeamConfirmed(string myTeamId)
        {
            Debug.Log($"TeamRouletteController: 확정된 팀 ID({myTeamId}) 수신 완료. 룰렛 시작!");
            roulettePanel.SetActive(true);
            resultText.text = "팀 편성 중...";
            
            // 룰렛 코루틴 시작 (결과는 무조건 myTeamId 로 고정된 '주작' 룰렛)
            StartCoroutine(SpinRouletteRoutine(myTeamId));
        }

        private IEnumerator SpinRouletteRoutine(string targetTeamId)
        {
            float elapsed = 0f;
            float spinSpeed = 1000f; // 초기 회전 속도

            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                
                // 회전 속도를 서서히 줄입니다 (감속 효과)
                spinSpeed = Mathf.Lerp(1000f, 0f, elapsed / spinDuration);
                
                if(rouletteWheel != null)
                {
                    rouletteWheel.rectTransform.Rotate(Vector3.forward, -spinSpeed * Time.deltaTime);
                }

                // 중간중간 긴장감을 위해 임시 텍스트 변경도 가능합니다.
                // 텍스트를 빠르게 A, B, C 로 바꾸는 등의 효과...

                yield return null;
            }

            // 룰렛 정지 및 결과 도출! (이것이 100% targetTeamId에 매핑됨)
            resultText.text = $"당신은 [{targetTeamId}팀] 입니다!";
            
            // 목표한 팀 위치로 각도를 딱 맞춰주거나 이펙트를 터뜨릴 수 있습니다.
            PlaySuccessEffect();

            // 3초 뒤에 실제 게임 씬 등으로 이동하는 로직 추가
            Invoke(nameof(MoveToGameScene), 3f);
        }

        private void PlaySuccessEffect()
        {
            // 파티클 생성, 사운드 재생 등
            Debug.Log("주작 룰렛 성공! 이펙트 펑!");
        }

        private void MoveToGameScene()
        {
            Debug.Log("게임 씬으로 이동합니다...");
            // TODO: 아래 "GameScene"을 개발자님의 실제 인게임 씬 이름(예: "Stage1", "MainMap")으로 바꿔주세요!
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
        }
    }
}
