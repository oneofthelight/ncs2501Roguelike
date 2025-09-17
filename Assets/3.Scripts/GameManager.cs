using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    
    // Sigleton 싱글톤이 중요한 기능을 static 처럼 사용하기 위해 쓰는 방식
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region Public
    public BoardManager BoardManager;
    public BoardManager BoardManager2;
    public PlayerController PlayerController;
    public TurnManager TurnManager {get; private set;}
    public UIDocument UIDoc;
    public GameObject AndroidPanel;
    public AudioSource audioSource;
    #endregion

    #region Private
    private const string GOS1 = "Game Over!\n\nYou traveled through" ;
    private const string GOS2 = "levels \n\n(Press Enter to New Game)";
    private Label m_FoodLabel;
    private int m_FoodAmount = 30;
    private int m_CurrentLevel = 0;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    #endregion
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
#if UNITY_ANDROID
        Camera camera = Camera.main;
        camera.orthographicSize = 12;
        camera.transform.position = new Vector3(6, 4, -10); 
        AndroidPanel.SetActive(true);
#else
        AndroidPanel.SetActive(false);
#endif
        audioSource = GetComponent<AudioSource>();
        TurnManager = new TurnManager();            // 턴 매니저 지정
        TurnManager.OnTick += OnTurnHappen;         // OnTick 메소드로 OnTurnHappen넣기

        m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");  // 게임오버 패널 호출  
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");              // 게임오버 메시지 가져오기  

        m_GameOverPanel.style.visibility = Visibility.Hidden;
       
        StartNewGame();                                                               // 새 게임 불러오기
        
    }
    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;

        m_CurrentLevel = 0;
        m_FoodAmount = 100;
        m_FoodLabel.text = "Food : " + m_FoodAmount;

        PlayerController.Init();
        //BoardManager.Clean();
        //BoardManager.Init();
        //PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        if(m_CurrentLevel > 5 )
        {
            NextLevel();
        } 
        NewLevel();
    }
    public void NewLevel()
    {
        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        m_CurrentLevel++;
    }
    void OnTurnHappen()             // 턴 소비
    {
        ChangeFood(-1);
    }
    public void  NextLevel()
    {
        BoardManager2.Clean();
        BoardManager2.Init();
        PlayerController.Spawn(BoardManager2, new Vector2Int(1, 1));

        m_CurrentLevel++;
    }
    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;
        if (m_FoodAmount <= 0)                          // 그냥 0이 아니라 작거나로 하는 이유는 혹시나하는 상황을 대비해서
        {
            m_FoodAmount = 0;
            PlayerController.GameOver();
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = $"{GOS1} {m_CurrentLevel} {GOS2}";
        }
        m_FoodLabel.text = "Food : " + m_FoodAmount;
    }
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    
}
