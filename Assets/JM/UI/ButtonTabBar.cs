using UnityEngine;
using UnityEngine.UI;

public class ButtonTapBar : MonoBehaviour
{
   [SerializeField] private TabItem[] tabItems;

   [SerializeField] private GameObject[] tabPanels;
   
 
   [Header("시작 시 선택할 탭 (0부터 시작)")]
   [SerializeField] private int defaultIndex = 0;
 
   private int currentIndex = -1;

   private void Start()
   {
      for(int i=0; i < tabItems.Length; i++)
      {
         int index = i;
         var btn = tabItems[i].GetComponentInChildren<Button>();
         btn.onClick.AddListener(()=>SelectTab(index));

      }
      
      SelectTab(defaultIndex);
   }
   
   public void SelectTab(int index)
   {
      if (index == currentIndex) return;
 
      currentIndex = index;
 
      // 탭 색상 전환
      for (int i = 0; i < tabItems.Length; i++)
      {
         tabItems[i].SetSelected(i == currentIndex);
      }
 
      // 패널 전환: 선택된 것만 켜고 나머지 전부 끄기
      for (int i = 0; i < tabPanels.Length; i++)
      {
         tabPanels[i].SetActive(i == currentIndex);
      }
   }
}
