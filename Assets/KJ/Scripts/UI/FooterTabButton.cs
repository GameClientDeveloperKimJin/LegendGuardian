using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인화면 하단 버튼 3개 생명주기 관리
/// </summary>
public class FooterTabButton : MonoBehaviour
{
    [SerializeField]
    Button[] mainButtons;


    private void Start()
    {
        MainSceneView.OnButtonEvent += OnButtonEvent;

    }

    private void OnDestroy()
    {
        MainSceneView.OnButtonEvent -= OnButtonEvent;
    }
    private void OnButtonEvent(bool IsActive)
    {
        for(int i = 0; i< mainButtons.Length; i++)
        {
            mainButtons[i].interactable = IsActive;
        }
    }
}
