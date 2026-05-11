using System.Threading.Tasks;
using UnityEngine;

public class RankingData
{
    public string NickName;
    public long score; 
}
public class RankingView : MonoBehaviour
{
    private void OnEnable()
    {
        UserRanking();
    }

    [SerializeField]
    GameObject RankItemPrefab;

    [SerializeField]
    Transform RankContent;

    private async void UserRanking()
    {
        var rankList = await FirebaseManager.Instance?.GetRanking();

        Debug.Log(rankList != null);

        int rank = 0;
        foreach (var rankingData in rankList)
        {
            rank++;

            GameObject rankItem = Instantiate(RankItemPrefab, RankContent);
            rankItem.GetComponent<RankItem>().Init(rank.ToString(), rankingData.NickName, rankingData.score.ToString());

            Debug.Log($"{rankingData.NickName} , {rankingData.score.ToString()}");
        }
    }
}
