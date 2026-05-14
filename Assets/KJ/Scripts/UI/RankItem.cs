using TMPro;
using UnityEngine;

public class RankItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI rankTMP;
    [SerializeField]
    private TextMeshProUGUI nickNameTMP;
    [SerializeField]
    private TextMeshProUGUI scoreTMP;

    public void Init(string rank , string nickName, string score)
    {
        rankTMP.text = rank;
        nickNameTMP.text = nickName;
        scoreTMP.text = $"{score} wh";
    }
}
