using System;
using UnityEngine;
using UnityEngine.UI;

public class RouteButtonItem : MonoBehaviour
{
    private string routeID;

    private string routeName;

    private string routeIndex; //루트 버튼 고유 번호

    Button routeButton;

    private void Start()
    {
        routeButton = GetComponent<Button>();
    }
    public void Init(int index, string routeID, string routeName)
    {
        this.routeID = routeID;
        this.routeName = routeName;

        //초기에는 첫번째 버튼만 활성화 -> 팀마다 루트 순서가 정해져 있어서 그럼
        if (index != 0)
        {
            routeButton.interactable = false;

            ColorBlock cb = routeButton.colors;
            cb.disabledColor = Color.grey;
            routeButton.colors = cb;
        }
        //TODO KJ - 버튼 클릭 이벤트 등록 및 루트 id에 해당 하는 미션 내용 가져오기
    }

}
