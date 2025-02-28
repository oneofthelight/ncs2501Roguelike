using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    // Sigleton 싱글톤이 중요한 기능을 static 처럼 사용하기 위해 쓰는 방식
    public static GameManager Instance { get; private set; }
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public TurnManager TurnManager {get; private set;}
    public UIDocument UIDoc;
    private Label m_FoodLabel;
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
        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        TurnManager = new TurnManager();  // 턴 매니저 지정
        TurnManager.OnTick += OnTurnHappen;  // OnTick 메소드로 OnTurnHappen넣기

        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }
    void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        m_FoodLabel.text = "Food : " + m_FoodAmount;
    }
}
