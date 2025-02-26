using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Sigleton 싱글톤이 중요한 기능을 static 처럼 사용하기 위해 쓰는 방식
    public static GameManager Instance { get; private set; }
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public TurnManager TurnManager {get; private set;}
    private int m_FoodAmount = 100;
    private void Awake()
    {
        if (Instance != null)  // 여기 있는 if 내용이 게임 매니저를 static처럼 사용하기 위한 것
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }
    void OnTurnHappen()
    {
        m_FoodAmount--;
        Debug.Log($"Current amount of food : {m_FoodAmount}");
    }
}
