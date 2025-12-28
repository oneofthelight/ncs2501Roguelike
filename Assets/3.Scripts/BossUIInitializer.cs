using UnityEngine;
using UnityEngine.UIElements;

public class BossUIInitializer : MonoBehaviour
{
    void Start()
    {
        // 씬에 있는 UIDocument를 직접 찾아서 GameManager에 주입
        UIDocument doc = FindObjectOfType<UIDocument>();
        if (doc != null && GameManager.instance != null)
        {
            GameManager.instance.SetupBossSceneUI(doc);
            Debug.Log("<color=green>3. BossUIInitializer: UI 연결 성공!</color>");
        }
    }
}