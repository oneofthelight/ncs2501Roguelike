using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class BossSceneInitializer : MonoBehaviour
{
    public UIDocument bossUIDoc;

    // BossSceneInitializer.cs
    void Start()
    {
        // 씬에 배치된 UIDocument를 찾아서 GameManager에 전달
        UIDocument bossUI = FindObjectOfType<UIDocument>();
        if (GameManager.instance != null && bossUI != null)
        {
            GameManager.instance.SetupBossSceneUI(bossUI);
            Debug.Log("GameManager에게 보스 UI 전달 완료");
        }
    }

    IEnumerator SpawnRoutine()
    {
        // 씬 로딩 및 리소스 정리를 위해 잠시 대기
        yield return new WaitForSeconds(1.5f);

        if (GameManager.Instance != null)
        {
            Debug.Log("보스 씬 진입: 몬스터 생성을 시작합니다.");

            // 반복 생성 루프 (GameManager 내부에서 한 마리 체크 로직이 있으므로 안전함)
            while (true)
            {
                GameManager.Instance.CreateMonster();

                // 2초마다 몬스터가 죽었는지 체크하여 소환 시도
                yield return new WaitForSeconds(2.0f);
            }
        }
    }
} // 이 중괄호가 클래스의 끝입니다.