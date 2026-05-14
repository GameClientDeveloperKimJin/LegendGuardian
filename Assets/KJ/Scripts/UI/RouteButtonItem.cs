using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인 씬에 있는 루트 버튼 아이템 프리펩 
/// </summary>
public class RouteButtonItem : MonoBehaviour
{
    private string routeID;

    private string routeName;

    private string routeIndex; //루트 버튼 고유 번호

    [SerializeField]
    GameObject checkImage;


    Button routeButton;

    private void Start()
    {
        routeButton = GetComponent<Button>();
    }
    public void Init(int index, string routeID, string routeName)
    {
        this.routeID = routeID;
        this.routeName = routeName;

        routeButton = GetComponent<Button>();

        //초기에는 첫번째 버튼만 활성화 -> 팀마다 루트 순서가 정해져 있어서 그럼
        if (index != 0)
        {
            routeButton.interactable = false;

            ColorGrayButton();
        }

    }

    public void CheckButtonActive()
    {
        checkImage.gameObject.SetActive(true);
        routeButton.interactable = false;
    }

    public void ColorGrayButton()
    {
        ColorBlock cb = routeButton.colors;
        cb.disabledColor = Color.grey;
        routeButton.colors = cb;

        routeButton.interactable = false;
    }

    public void ColorWhiteButton()
    {
        ColorBlock cb = routeButton.colors;
        cb.disabledColor = Color.white;
        routeButton.colors = cb;

        routeButton.interactable = true;
    }
}
