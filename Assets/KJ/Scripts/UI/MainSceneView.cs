using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IMainSceneView
{
    public void OnRouteFirstButton();
    public void OnRouteSecondButton();
    public void OnRouteThirdButton();

}
public class MainSceneView : MonoBehaviour , IMainSceneView
{
    [SerializeField]
    TextMeshProUGUI nickNameTMP;

    [SerializeField]
    TextMeshProUGUI scoreTMP;

    private MainScenePresenter presenter;

    [SerializeField]
    Button[] routeButton = new Button[4];

    private void Awake()
    {
        presenter = new MainScenePresenter(this);
    }
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
        // 닉네임 적용 및 wh 적용 
        nickNameTMP.text = AuthManager.Instance?.LoginUserName;

        scoreTMP.text = await FirebaseManager.Instance?.GetUserIDToScore(AuthManager.Instance?.LoginUserID);
        
        // 팀에 맞는 루트 버튼 동적 생성

        
    }

    /// <summary>
    /// 업데이트 된 wH 적용
    /// </summary>
    /// <param name="score"></param>
    private void OnScoreUpdate(long score)
    {
        scoreTMP.text = score.ToString();
    }

    public void OnRouteFirstButton()
    {
        
    }

    public void OnRouteSecondButton()
    {
        
    }

    public void OnRouteThirdButton()
    {
        
    }
}
