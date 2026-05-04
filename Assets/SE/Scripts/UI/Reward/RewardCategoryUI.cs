using TMPro;
using UnityEngine;

public class RewardCategoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text categoryTitleText;

    public void Setup(string categoryName)
    {
        if (categoryTitleText != null)
            categoryTitleText.text = categoryName;
    }
}