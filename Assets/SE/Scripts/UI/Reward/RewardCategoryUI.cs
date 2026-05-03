using TMPro;
using UnityEngine;

public class RewardCategoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text categoryTitleText;
    [SerializeField] private Transform itemContainer;

    public Transform ItemContainer => itemContainer;

    public void Setup(string categoryName)
    {
        if (categoryTitleText != null)
            categoryTitleText.text = categoryName;
    }
}