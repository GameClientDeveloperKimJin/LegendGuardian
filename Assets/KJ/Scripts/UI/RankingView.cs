using System.Threading.Tasks;
using UnityEngine;

public class RankingData
{
    public string NickName;
    public long score;
}

public class TeamRankingData
{
    public string TeamName;
    public long score;
}

public enum RankingMode { User, Team }

public class RankingView : MonoBehaviour
{
    [SerializeField]
    RankingMode mode;

    [SerializeField]
    GameObject RankItemPrefab;

    [SerializeField]
    Transform RankContent;

    private void OnEnable()
    {
        if (mode == RankingMode.User)
            UserRanking();
        else
            TeamRanking();
    }

    private async void UserRanking()
    {
        var rankList = await FirebaseManager.Instance?.GetRanking();

        if (rankList == null) return;

        int rank = 0;
        foreach (var rankingData in rankList)
        {
            rank++;

            GameObject rankItem = Instantiate(RankItemPrefab, RankContent);
            rankItem.GetComponent<RankItem>().Init(rank, rankingData.NickName, rankingData.score.ToString());

            Debug.Log($"{rankingData.NickName} , {rankingData.score.ToString()}");
        }
    }

    private async void TeamRanking()
    {
        var rankList = await FirebaseManager.Instance?.GetTeamRanking();

        if (rankList == null) return;

        int rank = 0;
        foreach (var rankingData in rankList)
        {
            rank++;

            GameObject rankItem = Instantiate(RankItemPrefab, RankContent);
            rankItem.GetComponent<RankItem>().Init(rank, rankingData.TeamName, rankingData.score.ToString());
        }
    }

    private void OnDisable()
    {

        foreach (Transform rankitem in RankContent)
        {
            Destroy(rankitem.gameObject);
        }
    }

    private void UserRankDestroy()
    {
        foreach (Transform rankitem in RankContent)
        {
            Destroy(rankitem.gameObject);
        }
    }


}
