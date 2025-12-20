using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    public float spawnRate = 2.0f;

    void Start()
    {
        // 씬이 시작되자마자 GameManager에게 스폰 포인트 정보를 강제로 새로고침 시킵니다.
        if (GameManager.Instance != null)
        {
            // 1. 기존 Invoke 모두 중지 (꼬임 방지)
            GameManager.Instance.CancelInvoke("CreateMonster");

            // 2. 현재 씬의 스폰 포인트를 다시 잡으라고 명령
            // GameManager 내부의 SetupSpawnPoints()가 public이어야 합니다.
            GameManager.Instance.Invoke("CreateMonster", 1.0f);

            // 3. 반복 생성 시작
            InvokeRepeating("RequestSpawn", 1.0f, spawnRate);
        }
    }

    void RequestSpawn()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            // GameManager의 CreateMonster를 직접 호출
            GameManager.Instance.SendMessage("CreateMonster", SendMessageOptions.DontRequireReceiver);
        }
    }
}