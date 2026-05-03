using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;
  
    [Header("색상 설정")]
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.5f, 1f);   // 파란색
    [SerializeField] private Color defaultColor = new Color(0.6f, 0.6f, 0.6f);  // 회색

    public void SetSelected(bool isSelected)
    {
        Color color = isSelected ? selectedColor : defaultColor;
        image.color = color;
        text.color = color;
    }
}
