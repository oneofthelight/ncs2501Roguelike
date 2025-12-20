using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public List<Transform> spawnPoints = new List<Transform>();
    public float spawnRate = 3.0f;

    void Start()
    {
        // 씬 내의 SpawnPointGroup 자식들을 자동으로 가져옵니다.
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }

        // GameManager의 몬스터 생성 주기를 여기서 제어하거나 
        // GameManager에 등록된 Invoke를 사용합니다.
        if (GameManager.Instance != null)
        {
            // GameManager의 리스트를 업데이트 해줍니다.
            GameManager.Instance.spawnPoints = this.spawnPoints;
        }
    }
}