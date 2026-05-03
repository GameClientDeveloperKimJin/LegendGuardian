using UnityEngine;
using UnityEngine.UI;

public class FloorMapUIController : MonoBehaviour
{
    [System.Serializable]
    public class FloorMapData
    {
        public Button button;
        public Sprite mapSprite;
    }

    [Header("Map Image")]
    [SerializeField] private Image mapImage;

    [Header("Floor Data")]
    [SerializeField] private FloorMapData[] floorMapDataList;

    private void Awake()
    {
        if (mapImage != null)
            mapImage.gameObject.SetActive(false);

        BindButtons();
    }

    private void BindButtons()
    {
        foreach (var data in floorMapDataList)
        {
            if (data.button != null && data.mapSprite != null)
            {
                data.button.onClick.AddListener(() => ShowMap(data.mapSprite));
            }
        }
    }

    private void ShowMap(Sprite sprite)
    {
        if (mapImage == null || sprite == null)
            return;

        mapImage.sprite = sprite;
        mapImage.gameObject.SetActive(true);
    }
}