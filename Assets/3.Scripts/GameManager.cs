using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit
using TMPro;                 // TextMeshPro
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    // --- 1. 싱글톤 설정 ---
    public static GameManager Instance { get; private set; }
    public static GameManager instance => Instance;

    // --- 2. 인스펙터 노출 필드 (로그라이크) ---
    [Header("Roguelike System")]
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public UIDocument UIDoc;
    public float maxHP = 200;
    public float currentHP = 200f;

    // --- 3. 인스펙터 노출 필드 (슈팅 & 보스전) ---
    [Header("Shooting & Boss System")]
    public GameObject panelGameOver; // 로그라이크 씬용 uGUI 패널
    public GameObject monsterPrefab;
    public List<GameObject> monsterPool = new List<GameObject>();
    public int maxMonsters = 10;
    public float createTime = 3.0f;
    public TMP_Text scoreText;
    public TMP_Text killText;
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Common Assets")]
    public GameObject AndroidPanel;
    public AudioSource audioSource;

    // --- 4. 내부 상태 변수 ---
    private int m_CurrentLevel = 0;
    private int totScore = 0;
    private int killcount;
    private bool _isGameOver;

    public TurnManager TurnManager { get; private set; }
    public bool IsExitActive { get; private set; }
    public bool IsLoading { get; private set; }

    // --- 5. UI 요소 (UIToolkit - 로그라이크) ---
    private VisualElement hpFill;
    private VisualElement m_GameOverPanel;
    private VisualElement background;
    private Label hp_Text;
    private Label stageLabel;

    // --- 6. UI 요소 (UIToolkit - 보스 씬) ---
    [Header("Boss Scene UI (UI Toolkit)")]
    private VisualElement bossUI_Back;
    private VisualElement bossUI_GameOver;
    private VisualElement bossUI_GameClear;
    private Label bossUI_GameOverText;
    private Label bossUI_GameClearText;

    // --- 7. 속성 (Properties) ---
    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            _isGameOver = value;
            if (_isGameOver)
            {
                CancelInvoke("CreateMonster");
                // 게임이 멈추길 원한다면 주석 해제
                // Time.timeScale = 0f; 
            }
        }
    }

    public int CurrentLevel
    {
        get => m_CurrentLevel;
        set
        {
            m_CurrentLevel = value;
            if (stageLabel != null) stageLabel.text = $"Stage [{m_CurrentLevel}]";
        }
    }

    public int KillCount
    {
        get => killcount;
        set
        {
            killcount = Mathf.Min(value, 99);
            DisplayKillCount();
        }
    }

    // --- 8. 초기화 및 생명주기 ---
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializePlatformSettings();
        audioSource = GetComponent<AudioSource>();
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        SetupUIToolkit();

        if (panelGameOver != null) panelGameOver.SetActive(false);
        totScore = PlayerPrefs.GetInt("TOT_SCORE", 0);
        DisplayerScore(0);

        StartNewGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) { CurrentLevel++; NewLevel(); }
    }

    // --- 9. 게임 흐름 제어 ---
    public void StartNewGame()
    {
        Time.timeScale = 1f; // 시간 초기화
        ResetGameState();
        NewLevel();
    }

    public void NewLevel()
    {
        if (CurrentLevel >= 36)
        {
            Debug.Log("최종 스테이지 클리어! 보스 스테이지로 이동합니다.");
            SceneManager.LoadScene("SpaceShooterScene");
            return;
        }

        IsLoading = true;
        BoardManager.Clean();
        CurrentLevel++;
        IsExitActive = false;
        BoardManager.Init();

        if (PlayerController != null)
            PlayerController.Spawn(BoardManager, BoardManager.PlayerStartCoord);

        UpdateCameraPosition();
        IsLoading = false;
    }

    // --- 10. 체력 및 전투 로직 (로그라이크 필수 함수 포함) ---
    void OnTurnHappen() => UpdateHPBar(-1);

    public void UpdateHPBar(int amount = 0)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);

        if (hpFill != null) hpFill.style.width = Length.Percent((currentHP / maxHP) * 100);
        if (hp_Text != null) hp_Text.text = $"{currentHP}/{maxHP}";

        if (currentHP <= 0 && !IsGameOver) DisplayerGameOver();
    }

    public void RecoverPlayerHealth(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        UpdateHPBar(0);
    }

    public void ActivateExit() => IsExitActive = true;
    public void PlaySound(AudioClip clip) { if (clip != null) audioSource.PlayOneShot(clip); }

    // --- 11. 통합 사망 처리 (핵심 수정 부분) ---
    public void DisplayerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"사망 발생 - 현재 씬: {currentScene}");

        // 1. 보스 씬일 경우 (UI Toolkit 버전 호출)
        if (currentScene == "SpaceShooterScene")
        {
            ShowGameOverUI();
        }
        // 2. 로그라이크 씬일 경우 (uGUI 또는 기존 UI Toolkit 패널)
        else
        {
            ShowLegacyGameOverUI();
        }

        // 공통: 시간 정지 (필요 시)
        // Time.timeScale = 0f;
    }

    private void ShowLegacyGameOverUI()
    {
        // UI Toolkit 로그라이크용 패널 활성화
        if (m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Visible;
        if (background != null) background.style.display = DisplayStyle.Flex;

        // 기존 uGUI 패널이 있다면 활성화 (코드에 Find 로직 유지)
        GameObject activePanel = GameObject.Find("Panel_GameOver");
        if (activePanel == null && panelGameOver != null)
        {
            activePanel = Instantiate(panelGameOver);
            activePanel.name = "Panel_GameOver";
            panelGameOver = activePanel;
        }

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
    }

    // --- 12. 보스 씬 UI Toolkit 전용 함수 ---
    public void SetupBossSceneUI(UIDocument bossDoc)
    {
        if (bossDoc == null) return;
        var root = bossDoc.rootVisualElement;

        bossUI_Back = root.Q<VisualElement>("Back");
        bossUI_GameOver = root.Q<VisualElement>("GameOverGroup");
        bossUI_GameClear = root.Q<VisualElement>("GameClearGroup");
        bossUI_GameOverText = bossUI_GameOver?.Q<Label>("GameOverText");
        bossUI_GameClearText = bossUI_GameClear?.Q<Label>("GameClearText");

        // 초기 상태: 숨김
        if (bossUI_Back != null) bossUI_Back.style.display = DisplayStyle.None;
        if (bossUI_GameOver != null) bossUI_GameOver.style.display = DisplayStyle.None;
        if (bossUI_GameClear != null) bossUI_GameClear.style.display = DisplayStyle.None;

        Debug.Log("보스 씬 UI Toolkit 연결 완료");
    }

    public void ShowGameOverUI()
    {
        Debug.Log("<color=cyan>보스 씬 GameOver UI 출력</color>");
        if (bossUI_Back != null) bossUI_Back.style.display = DisplayStyle.Flex;
        if (bossUI_GameOver != null) bossUI_GameOver.style.display = DisplayStyle.Flex;
    }

    public void ShowGameClearUI()
    {
        IsGameOver = true;
        Debug.Log("<color=green>보스 씬 Game Clear UI 출력</color>");
        if (bossUI_Back != null) bossUI_Back.style.display = DisplayStyle.Flex;
        if (bossUI_GameClear != null) bossUI_GameClear.style.display = DisplayStyle.Flex;
    }

    // --- 13. 몬스터 풀링 및 기타 기능 (기존 유지) ---
    public void CreateMonster()
    {
        if (SceneManager.GetActiveScene().name != "SpaceShooterScene") return;
        if (IsGameOver) return;

        if (monsterPool != null)
        {
            foreach (GameObject m in monsterPool)
            {
                if (m != null && m.activeSelf) return;
            }
        }

        if (spawnPoints == null || spawnPoints.Count == 0 || (spawnPoints.Count > 0 && spawnPoints[0] == null))
            SetupSpawnPoints();

        GameObject mon = GetMonsterInPool();
        if (mon != null && spawnPoints.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, spawnPoints.Count);
            mon.transform.position = spawnPoints[idx].position;
            mon.transform.rotation = spawnPoints[idx].rotation;
            mon.SetActive(true);
        }
    }

    public GameObject GetMonsterInPool()
    {
        if (monsterPool == null) monsterPool = new List<GameObject>();
        foreach (var mon in monsterPool)
        {
            if (mon != null && !mon.activeSelf) return mon;
        }
        if (monsterPrefab != null)
        {
            GameObject newMon = Instantiate(monsterPrefab);
            newMon.SetActive(false);
            monsterPool.Add(newMon);
            return newMon;
        }
        return null;
    }

    public void DisplayerScore(int score)
    {
        totScore = Mathf.Min(totScore + score, 99999);
        if (scoreText != null) scoreText.text = $"SCORE: {totScore:#,##0}";
        PlayerPrefs.SetInt("TOT_SCORE", totScore);
    }

    public void DisplayKillCount() { if (killText != null) killText.text = $"{killcount:00}"; }

    private void SetupUIToolkit()
    {
        if (UIDoc == null) return;
        var root = UIDoc.rootVisualElement;
        hpFill = root.Q<VisualElement>("HP_bar");
        hp_Text = hpFill?.Q<Label>("HP_Text");
        stageLabel = root.Q<Label>("StageTxt");
        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");
        background = root.Q<VisualElement>("Back");
    }

    private void ResetGameState()
    {
        if (m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Hidden;
        if (background != null) background.style.display = DisplayStyle.None;
        if (panelGameOver != null) panelGameOver.SetActive(false);

        CurrentLevel = 0;
        currentHP = maxHP;
        IsGameOver = false;
        UpdateHPBar();

        if (PlayerController != null) PlayerController.Init();
        CancelInvoke("CreateMonster");
    }

    public void SetupSpawnPoints()
    {
        GameObject g = GameObject.Find("SpawnPointGroup");
        if (g != null)
        {
            spawnPoints.Clear();
            foreach (Transform t in g.transform) spawnPoints.Add(t);
        }
    }

    private void InitializePlatformSettings()
    {
#if UNITY_ANDROID
        if(Camera.main != null) { Camera.main.orthographicSize = 12; Camera.main.transform.position = new Vector3(6, 4, -10); }
        if(AndroidPanel != null) AndroidPanel.SetActive(true);
#else
        if (AndroidPanel != null) AndroidPanel.SetActive(false);
#endif
    }

    private void UpdateCameraPosition()
    {
        if (Camera.main != null && BoardManager != null)
        {
            Vector3 targetPos = BoardManager.CellToWorld(BoardManager.PlayerStartCoord);
            Camera.main.transform.position = new Vector3(targetPos.x, targetPos.y, Camera.main.transform.position.z);
        }
    }

#if UNITY_EDITOR
    [MenuItem("MyMenu/SpaceShooter/Reset score")]
    public static void ResetScore() { PlayerPrefs.SetInt("TOT_SCORE", 0); Debug.Log("Score Reset Done."); }
#endif
}