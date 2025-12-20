using UnityEngine;
using System.Collections;

public class BossSceneInitializer : MonoBehaviour
{
    void Start()
    {
        // 씬 시작 시 GameManager 설정 및 생성 루틴 시작
        StartCoroutine(SpawnRoutine());
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