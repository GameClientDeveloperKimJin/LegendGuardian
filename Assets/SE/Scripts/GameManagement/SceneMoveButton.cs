using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMoveButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName;

    public void MoveToScene()
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning($"{gameObject.name}: 이동할 씬 이름이 비어 있습니다.");
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}
