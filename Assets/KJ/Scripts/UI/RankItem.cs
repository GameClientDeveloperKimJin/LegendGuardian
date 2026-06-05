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

        bool isMedal = rank <= 3;

        if(isMedal)
        {
            rankImage.gameObject.SetActive(false);
            rankTMP.gameObject.SetActive(false);
            titleTMP.gameObject.SetActive(rank == 1);
            medalImage.gameObject.SetActive(isMedal);
        }
        else
        {
            rankTMP.gameObject.SetActive(true);
            rankTMP.text = rank.ToString();
        }

        medalImage.sprite = rank switch
        {
            1 => goldSprite,
            2 => silverSprite,
            _ => bronzeSprite
        };
    }
}
