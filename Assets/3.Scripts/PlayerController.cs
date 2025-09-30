using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip Attack;
    public float MoveSpeed = 5.0f;
    public Vector2Int Cell 
    {
        get
        {
            return m_CellPosition;
        }
        private set{}
    }
    private readonly int hashMoving = Animator.StringToHash("Moving");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private BoardManager m_Board;
    private Vector2Int m_CellPosition; 
    
    // [기존 변수 유지] 게임 오버 상태를 추적합니다.
    private bool m_IsGameOver; 
    
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;
    private Animator m_Animator;
    private Vector2Int newCellTarget;
    private bool hasMoved;
    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }
    public void Init()
    {
        m_IsMoving = false;
        
        // [수정] 새 게임 시작 시 조작 가능하도록 m_IsGameOver를 false로 초기화합니다.
        m_IsGameOver = false; 
    }
    
    // [기존 함수 유지] GameManager에서 호출하여 조작을 중지시킬 때 사용합니다.
    public void GameOver()
    {
        m_IsGameOver = true;
    }
    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        m_CellPosition = cell;
        // 보드 에서의 player위치 지정 => 화면에서 제대로된 위치에 표시
        MoveTo(cell, true);
    }
    public void MoveTo(Vector2Int cell, bool immediate = false)
    {
        m_CellPosition = cell;

        if (immediate)
        {
            m_IsMoving = false;
            transform.position = m_Board.CellToWorld(m_CellPosition);
        }
        else
        {
            m_IsMoving = true;
            m_MoveTarget = m_Board.CellToWorld(m_CellPosition);
        }
        
        m_Animator.SetBool(hashMoving, m_IsMoving);
    }
    void Start()
    {
        // StartNewGame에서 Init()이 호출되므로 별도 처리 필요 없음
    }

    private void Update()
    {
        // [핵심] 게임 오버 상태에서는 'Enter' 입력 외의 모든 조작을 무시하고 리턴합니다.
        if (m_IsGameOver) 
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return; // 조작 중지!
        }
        
        // ... (기존 움직임 로직: m_IsMoving) ...
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                m_Animator.SetBool(hashMoving, false);
                var cellData = m_Board.GetCellData(m_CellPosition);
                if (cellData.ContainedObject != null)
                    cellData.ContainedObject.PlayerEntered();
            }
            return;
        }
        
        // ... (기존 입력 처리 로직) ...
        newCellTarget = m_CellPosition;
        hasMoved = false;
        
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            MoveUp();
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            MoveDown();
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            MoveRight();
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            MoveLeft();
        }
    }    
    private void UpdatePlayer()
    {
        if (hasMoved)
        {
            // ... (기존 UpdatePlayer 로직) ...
            GameManager.Instance.TurnManager.Tick();
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);
            if (cellData != null && cellData.Passable)
            {
                if (cellData.ContainedObject == null)  // 들어가려고 할때
                {
                    MoveTo(newCellTarget);
                }
                else
                {
                    if (cellData.ContainedObject.PlayerWantsToEnter())
                    {
                        MoveTo(newCellTarget);  // 플레이어를 먼저 셀로 이동 시킨 후 호출
                    }
                    else 
                    {
                        m_Animator.SetTrigger(hashAttack);
                    }
                }
            }
        }
    }
    
    // [핵심] MoveSkip에서도 m_IsGameOver를 확인합니다.
    public void MoveSkip()
    {
        // 🚨 [수정] m_IsGameOver 상태에서는 New Game 시작 처리만 하고 조작을 막습니다.
        if(m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame) // (MoveSkip은 보통 키 입력이 아니므로, 이 부분은 Update의 Enter키 로직으로 대체될 수 있습니다.)
            {
                GameManager.Instance.StartNewGame();
            }
            return; 
        }
        if (m_IsMoving) return;
        hasMoved = true;
        UpdatePlayer();
    }
    
    // [핵심] 각 이동 함수에서도 m_IsGameOver를 확인하여 조작을 막습니다.
    public void MoveUp()
    {
        if (m_IsGameOver || m_IsMoving) return; // 🚨 조작 차단
        newCellTarget.y++;
        hasMoved = true;
        UpdatePlayer();
    }
    public void MoveDown()
    {
        if (m_IsGameOver || m_IsMoving) return; // 🚨 조작 차단
        newCellTarget.y--;
        hasMoved = true;
        UpdatePlayer();
    }
    public void MoveRight()
    {
        if (m_IsGameOver || m_IsMoving) return; // 🚨 조작 차단
        newCellTarget.x++;
        hasMoved = true;
        UpdatePlayer();
    }
    public void MoveLeft()
    {
        if (m_IsGameOver || m_IsMoving) return; // 🚨 조작 차단
        newCellTarget.x--;
        hasMoved = true;
        UpdatePlayer();
    }
}