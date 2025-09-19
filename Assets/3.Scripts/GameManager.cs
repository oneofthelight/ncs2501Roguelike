using System;
using System.Collections;
using System.Collections.Generic;
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
    public PlayerController PlayerController;
    public TurnManager TurnManager { get; private set; }
    public UIDocument UIDoc;
    public GameObject AndroidPanel;
    public AudioSource audioSource;
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float maxTextHP = 100f;
    public float currentTextHP = 100f;
    //public HighScoreManager highScoreManager;
    #endregion

    #region Private
    private const string GOS1 = "Game Over!\n\nYou traveled through";
    private const string GOS2 = "levels \n\n(Press Enter to New Game)";
    private VisualElement hpFill;
    private VisualElement m_GameOverPanel;
    private Label hp_Text;
    private Label m_GameOverMessage;
    private Label stageLabel;
    #endregion

    private int m_CurrentLevel = 0;
    public int CurrentLevel 
    {
        get { return m_CurrentLevel; }
        set
        {
            m_CurrentLevel = value;
            stageLabel.text = $"Stage [{m_CurrentLevel + 1}]";
        }
    
    }

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
        TurnManager = new TurnManager();           // 턴 매니저 지정
        TurnManager.OnTick += OnTurnHappen;       // OnTick 메소드로 OnTurnHappen넣기

        // Find the HP bar and other UI elements
        var root = UIDoc.rootVisualElement;
        hpFill = root.Q<VisualElement>("HP_bar");
        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");  // 게임오버 패널 호출
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");     // 게임오버 메시지 가져오기
        hp_Text = hpFill.Q<Label>("HP_Text");
        stageLabel = root.Q<Label>("StageTxt");  // 게임오버 패널 호출

        // Load high scores using the new manager
        // highScoreManager.LoadHighScores();

        m_GameOverPanel.style.visibility = Visibility.Hidden;

        StartNewGame();                          // 새 게임 불러오기
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            CurrentLevel++;
            NewLevel();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            CurrentLevel--;
            NewLevel();
        }
    }

    public void StartNewGame()
    {
        m_GameOverPanel.style.visibility = Visibility.Hidden;
        CurrentLevel = 0;
        currentHP = maxHP; // Reset HP for a new game
        UpdateHPBar();
        PlayerController.Init();
        NewLevel();
    }

    public void NewLevel()
    {
        // Clean the previous board and initialize a new one with the correct level
        BoardManager.Clean();
        //CurrentLevel++;
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }

    void OnTurnHappen()            // 턴 소비
    {
        //UpdateHPBar(-1); // Decrease HP by 1 on each turn
    }

    void OnEnable()
    {
        // This is not needed anymore as the UI elements are found in Start()
    }

    public void UpdateHPBar(int amount = 0)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP); // Ensure HP stays within bounds

        float hpPercent = currentHP / maxHP;
        
        Length length = new Length(hpPercent * 100, LengthUnit.Percent);
        hpFill.style.width = length;
        hp_Text.text = $"{currentHP}/{maxHP}";

        // 
        if (currentHP <= 0)
        {
            // Game Over logic
            m_GameOverPanel.style.visibility = Visibility.Visible;
            m_GameOverMessage.text = GOS1 + CurrentLevel + GOS2;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
