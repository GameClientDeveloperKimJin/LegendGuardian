using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankTMP;
    [SerializeField] private TextMeshProUGUI nickNameTMP;
    [SerializeField] private TextMeshProUGUI scoreTMP;
    [SerializeField] private TextMeshProUGUI titleTMP;

    [SerializeField] private Image rankImage;
    [SerializeField] private Image medalImage;

    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite silverSprite;
    [SerializeField] private Sprite bronzeSprite;

    public void Init(int rank, string nickName, string score)
    {
        nickNameTMP.text = nickName;
        scoreTMP.text = $"{score} wh";

        int.TryParse(score, out int scoreValue);
        bool isMedal = rank <= 3 && scoreValue > 0;

        if (isMedal)
        {
            rankImage.gameObject.SetActive(false);
            rankTMP.gameObject.SetActive(false);
            titleTMP.gameObject.SetActive(rank == 1);
            medalImage.gameObject.SetActive(true);
            medalImage.sprite = rank switch
            {
                1 => goldSprite,
                2 => silverSprite,
                _ => bronzeSprite
            };
        }
        else
        {
            rankImage.gameObject.SetActive(true);
            rankTMP.gameObject.SetActive(true);
            rankTMP.text = scoreValue > 0 ? rank.ToString() : "-";
            titleTMP.gameObject.SetActive(false);
            medalImage.gameObject.SetActive(false);
        }
    }
}
