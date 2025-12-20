using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit
using TMPro;                // TextMeshPro
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    // --- 1. 싱글톤 설정 ---
    public static GameManager Instance { get; private set; }
    public static GameManager instance => Instance; // 소문자 호출 대응 (호환성)

    // --- 2. 인스펙터 노출 필드 (로그라이크) ---
    [Header("Roguelike System")]
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public UIDocument UIDoc;
    public float maxHP = 200;
    public float currentHP = 200f;

    // --- 3. 인스펙터 노출 필드 (슈팅 & 보스전) ---
    [Header("Shooting & Boss System")]
    public GameObject panelGameOver;
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

    // --- 5. UI 요소 (UIToolkit) ---
    private VisualElement hpFill;
    private VisualElement m_GameOverPanel;
    private VisualElement background;

    private Label hp_Text;
    private Label m_GameOverMessage;
    private Label stageLabel;

    private const string GOS1 = "Game Over!\n\nYou traveled through ";
    private const string GOS2 = " levels \n\n(Press Enter to New Game)";

    // --- 6. 속성 (Properties) ---
    // [해결] EnemyObject(함수형)와 PlayerCtrl(변수형) 호출 모두 대응
    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            _isGameOver = value;
            if (_isGameOver) CancelInvoke("CreateMonster");
        }
    }

    // 함수 형태로 호출하는 EnemyObject를 위한 래퍼 함수

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

    // --- 7. 초기화 및 생명주기 ---
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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

        // 🚨 몬스터 풀 생성은 여기서 하지 않고 StartNewGame 내부 또는 보스 씬 진입 시 수행합니다.

        totScore = PlayerPrefs.GetInt("TOT_SCORE", 0);
        DisplayerScore(0);

        // ✅ [복구] 게임 시작 시 첫 로그라이크 스테이지를 생성하기 위해 호출합니다.
        StartNewGame();
    }

    void Update()
    {
        // 디버그용 레벨 이동 (필요 없으면 삭제 가능)
        if (Input.GetKeyDown(KeyCode.F1)) { CurrentLevel++; NewLevel(); }
    }

    // --- 8. 게임 흐름 제어 (핵심) ---
    public void StartNewGame()
    {
        ResetGameState();
        NewLevel(); 
    }

    public void NewLevel()
    {
        // 🚨 수정 포인트: CurrentLevel이 36인 상태에서 '출구'를 밟아 NewLevel이 호출되면 씬 전환
        if (CurrentLevel >= 36)
        {
            Debug.Log("최종 스테이지 클리어! 보스 스테이지로 이동합니다.");
            SceneManager.LoadScene("SpaceShooterScene"); // 👈 유니티 Project 창의 씬 이름과 정확히 일치해야 함
            return;
        }

        IsLoading = true;
        BoardManager.Clean();

        // 스테이지 증가 (전환 조건 뒤에 배치하여 36 스테이지 플레이를 보장)
        CurrentLevel++;

        IsExitActive = false;
        BoardManager.Init();

        if (PlayerController != null)
            PlayerController.Spawn(BoardManager, BoardManager.PlayerStartCoord);

        UpdateCameraPosition();
        IsLoading = false;
    }

    // --- 9. 체력 및 전투 로직 ---
    void OnTurnHappen() => UpdateHPBar(-1); // 턴마다 체력 감소

    public void UpdateHPBar(int amount = 0)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);

        if (hpFill != null) hpFill.style.width = Length.Percent((currentHP / maxHP) * 100);
        if (hp_Text != null) hp_Text.text = $"{currentHP}/{maxHP}";

        if (currentHP <= 0) DisplayerGameOver();
    }

    public void RecoverPlayerHealth(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        UpdateHPBar(0);
    }

    public void DisplayerGameOver()
    {
        IsGameOver = true;
        if (PlayerController != null) PlayerController.GameOver();

        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.style.visibility = Visibility.Visible;
            if (m_GameOverMessage != null) m_GameOverMessage.text = GOS1 + CurrentLevel + GOS2;
        }
        if (panelGameOver != null) panelGameOver.SetActive(true);
    }

    // --- 10. 몬스터 풀링 및 스코어 (슈팅 시스템) ---
    private void CreateMonsterPool()
    {
        if (monsterPrefab == null) return;
        for (int i = 0; i < maxMonsters; i++)
        {
            GameObject obj = Instantiate(monsterPrefab);
            obj.SetActive(false);
            monsterPool.Add(obj);
        }
    }

    public void CreateMonster()
    {
        // 1. 씬 이름 체크
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SpaceShooterScene") return;

        // 2. 🚨 [강력한 중복 체크] 풀 안에 활성화된 몬스터가 하나라도 있는지 검사
        if (monsterPool != null)
        {
            foreach (GameObject m in monsterPool)
            {
                // 리스트 안의 몬스터가 씬에서 켜져 있다면(Active) 함수를 즉시 종료
                if (m != null && m.activeSelf)
                {
                    return;
                }
            }
        }

        // 3. 스폰 포인트 리스트 재수집 및 예외 처리
        if (spawnPoints == null || spawnPoints.Count == 0 || (spawnPoints.Count > 0 && spawnPoints[0] == null))
        {
            SetupSpawnPoints();
        }

        if (spawnPoints == null || spawnPoints.Count == 0) return;

        // 4. 몬스터 소환 로직
        GameObject mon = GetMonsterInPool();
        if (mon != null)
        {
            int idx = UnityEngine.Random.Range(0, spawnPoints.Count);
            if (spawnPoints[idx] == null) { SetupSpawnPoints(); return; }

            mon.transform.position = spawnPoints[idx].position;
            mon.transform.rotation = spawnPoints[idx].rotation;

            mon.SetActive(true);

            var agent = mon.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
                agent.enabled = true;
            }
        }
    }

    // 1. 몬스터를 가져오는 함수 (비어있으면 새로 생성하도록 보강)
    public GameObject GetMonsterInPool()
    {
        // 리스트가 없거나 파괴되었다면 새로 생성
        if (monsterPool == null) monsterPool = new List<GameObject>();

        // 사용할 수 있는 비활성 객체 찾기
        foreach (var mon in monsterPool)
        {
            if (mon != null && !mon.activeSelf) return mon;
        }

        // 🚨 [핵심] 만약 사용할 객체가 없다면 프리팹을 새로 생성해서 리스트에 추가
        if (monsterPrefab != null)
        {
            GameObject newMon = Instantiate(monsterPrefab);
            newMon.SetActive(false);
            monsterPool.Add(newMon);
            return newMon;
        }

        Debug.LogError("🚨 GameManager: monsterPrefab이 연결되어 있지 않아 생성에 실패했습니다!");
        return null;
    }

    public void DisplayerScore(int score)
    {
        totScore = Mathf.Min(totScore + score, 99999);
        if (scoreText != null) scoreText.text = $"SCORE: {totScore:#,##0}";
        PlayerPrefs.SetInt("TOT_SCORE", totScore);
    }

    public void DisplayKillCount()
    {
        if (killText != null) killText.text = $"{killcount:00}";
    }

    // --- 11. 유틸리티 함수 ---
    private void InitializePlatformSettings()
    {
#if UNITY_ANDROID
        if(Camera.main != null) { Camera.main.orthographicSize = 12; Camera.main.transform.position = new Vector3(6, 4, -10); }
        if(AndroidPanel != null) AndroidPanel.SetActive(true);
#else
        if (AndroidPanel != null) AndroidPanel.SetActive(false);
#endif
    }

    private void SetupUIToolkit()
    {
        if (UIDoc == null) return;
        var root = UIDoc.rootVisualElement;
        hpFill = root.Q<VisualElement>("HP_bar");
        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");
        hp_Text = hpFill?.Q<Label>("HP_Text");
        stageLabel = root.Q<Label>("StageTxt");
        background = root.Q<VisualElement>("Back");

        if (m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Hidden;
    }

    private void ResetGameState()
    {
        if (m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Hidden;
        if (panelGameOver != null) panelGameOver.SetActive(false);

        CurrentLevel = 0;
        currentHP = maxHP;
        IsGameOver = false;
        UpdateHPBar();

        if (PlayerController != null) PlayerController.Init();

        // 🚨 중요: 로그라이크 씬에서 몬스터가 생성되지 않도록 Invoke를 여기서 예약하지 않습니다.
        CancelInvoke("CreateMonster");
    }

    private void UpdateCameraPosition()
    {
        if (Camera.main != null && BoardManager != null)
        {
            Vector3 targetPos = BoardManager.CellToWorld(BoardManager.PlayerStartCoord);
            Camera.main.transform.position = new Vector3(targetPos.x, targetPos.y, Camera.main.transform.position.z);
        }
    }

    public void SetupSpawnPoints()
    {
        // 현재 활성화된 씬에서 "SpawnPointGroup"을 검색
        GameObject g = GameObject.Find("SpawnPointGroup");

        if (g != null)
        {
            spawnPoints.Clear();
            foreach (Transform t in g.transform)
            {
                spawnPoints.Add(t);
            }
            Debug.Log($"[GameManager] {spawnPoints.Count}개의 스폰 포인트를 갱신했습니다.");
        }
    }

    public void ActivateExit() => IsExitActive = true;
    public void PlaySound(AudioClip clip) { if (clip != null) audioSource.PlayOneShot(clip); }

#if UNITY_EDITOR
    [MenuItem("MyMenu/SpaceShooter/Reset score")]
    public static void ResetScore() { PlayerPrefs.SetInt("TOT_SCORE", 0); Debug.Log("Score Reset Done."); }
#endif
}