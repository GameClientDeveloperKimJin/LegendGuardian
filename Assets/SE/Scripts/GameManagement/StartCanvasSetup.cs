using UnityEngine;

public class StartCanvasSetup : MonoBehaviour
{
    [Header("처음 켜둘 Canvas들")]
    [SerializeField] private GameObject[] canvasesToShowOnStart;

    [Header("처음 꺼둘 Canvas들")]
    [SerializeField] private GameObject[] canvasesToHideOnStart;

    private void Start()
    {
        foreach (GameObject canvas in canvasesToShowOnStart)
        {
            if (canvas != null)
                canvas.SetActive(true);
        }

        foreach (GameObject canvas in canvasesToHideOnStart)
        {
            if (canvas != null)
                canvas.SetActive(false);
        }
    }
}