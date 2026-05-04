using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MainSceneView : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI nickNameTMP;

    [SerializeField]
    TextMeshProUGUI scoreTMP;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => AuthManager.Instance != null);
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
       
        Init();

        FirebaseManager.Instance.OnScoreUpdated += OnScoreUpdate;
    }

    private void OnDestroy()
    {
        FirebaseManager.Instance.OnScoreUpdated -= OnScoreUpdate;
    }

    /// <summary>
    /// 메인 씬 전환 시, 초기화 할 내용 ( 1회성)
    /// </summary>
    private async void Init()
    {
        nickNameTMP.text = AuthManager.Instance?.LoginUserName;

        scoreTMP.text = await FirebaseManager.Instance?.GetUserIDToScore(AuthManager.Instance?.LoginUserID);
    }

    private void OnScoreUpdate(long score)
    {
        scoreTMP.text = score.ToString();
    }
}
