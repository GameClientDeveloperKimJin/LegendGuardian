    using UnityEngine;

public class UICanvasSwitcher : MonoBehaviour
{
    [Header("¹öÆ° ´­·¶À» ¶§ ÄÑµÑ Canvas")]
    [SerializeField] private GameObject[] canvasesToShow;

    [Header("¹öÆ° ´­·¶À» ¶§ ²ø Canvas")]
    [SerializeField] private GameObject[] canvasesToHide;



    public void SwitchCanvas()
    {
        //²ø Äµ¹ö½ºµé ²ô±â
        foreach (GameObject canvas in canvasesToHide)
        {
            if (canvas != null)
                canvas.SetActive(false);
        }

        //ÄÓ Äµ¹ö½ºµé ÄÑ±â
        foreach (GameObject canvas in canvasesToShow)
        {
            if (canvas != null)
                canvas.SetActive(true);
        }
    }
}
