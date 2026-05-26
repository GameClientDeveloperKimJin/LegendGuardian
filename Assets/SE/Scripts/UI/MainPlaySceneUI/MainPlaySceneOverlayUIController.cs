using UnityEngine;
using UnityEngine.UI;

public class MainPlaySceneOverlayUIController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Target Panel")]
    [SerializeField] private GameObject targetPanel;

    private void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void OpenPanel()
    {
        if (targetPanel == null) return;

        targetPanel.SetActive(true);
    }

    private void ClosePanel()
    {
        if (targetPanel == null) return;

        targetPanel.SetActive(false);
    }
}