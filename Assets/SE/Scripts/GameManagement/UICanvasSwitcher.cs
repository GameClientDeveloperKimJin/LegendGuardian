using UnityEngine;

public class UICanvasSwitcher : MonoBehaviour
{
    [Header("항상 켜둘 Canvas")]
    [SerializeField] private GameObject[] persistentCanvases;

    [Header("버튼 눌렀을 때 켜둘 Canvas")]
    [SerializeField] private GameObject[] canvasesToShow;

    public void SwitchCanvas()
    {
        // 씬 안의 모든 Canvas 찾기
        Canvas[] allCanvases = FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        // 전부 끄기
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas != null)
                canvas.gameObject.SetActive(false);
        }

        // 항상 켜둘 Canvas 다시 켜기
        foreach (GameObject canvas in persistentCanvases)
        {
            if (canvas != null)
                canvas.SetActive(true);
        }

        // 현재 버튼에서 보여줄 Canvas 켜기
        foreach (GameObject canvas in canvasesToShow)
        {
            if (canvas != null)
                canvas.SetActive(true);
        }
    }
}
