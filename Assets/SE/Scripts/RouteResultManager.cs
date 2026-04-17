using UnityEngine;

public class RouteResultManager : MonoBehaviour
{
    public static RouteResultManager Instance { get; private set; }

    public RouteResultData CurrentResult { get; private set; }

    public bool HasResult => CurrentResult != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetResult(RouteResultData resultData)
    {
        CurrentResult = resultData;
    }

    public void ClearResult()
    {
        CurrentResult = null;
    }
}
