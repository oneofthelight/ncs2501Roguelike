using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public int HP { get; set; } // 또는 public int HP;
    AudioSource audioSource;
    public bool IsGameOver => m_IsGameOver;
    public AudioClip Attack;
    public float MoveSpeed = 5.0f;

    public Vector2Int Cell
    {
        get { return m_CellPosition; }
        private set { m_CellPosition = value; }
    }
    private readonly int hashMoving = Animator.StringToHash("Moving");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;

    private bool m_IsGameOver;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;
    private Animator m_Animator;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Init()
    {
        m_IsMoving = false;
        m_IsGameOver = false;
        m_Animator.SetBool(hashMoving, false);
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        m_CellPosition = cell;

        // 🚨 [필수] 맵 로드 시 이동 상태 완전 초기화
        m_IsMoving = false;

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

    public void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }

        // 1. 이동 중일 경우, 이동 완료 여부만 체크
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                m_Animator.SetBool(hashMoving, false);

                var cellData = m_Board.GetCellData(m_CellPosition);

                // 이동 완료 후, 셀 오브젝트 상호작용 (Exit/Treasure 로직)
                if (cellData != null && cellData.ContainedObject != null)
                {
                    cellData.ContainedObject.PlayerEntered();
                }

                // 이동 완료 후 턴 넘김 (Exit에서 NewLevel이 호출될 수 있으므로 마지막에 처리)
                if (!GameManager.Instance.IsLoading) // NewLevel 로딩 중이 아닐 때만 턴 넘김
                {
                    GameManager.Instance.TurnManager.Tick();
                }
            }
            return;
        }

        // 2. 입력 처리 (이동 중이 아닐 때만 입력을 받습니다)
        Vector2Int inputDirection = Vector2Int.zero;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame) inputDirection = Vector2Int.up;
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame) inputDirection = Vector2Int.down;
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) inputDirection = Vector2Int.right;
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame) inputDirection = Vector2Int.left;

        if (inputDirection != Vector2Int.zero)
        {
            TryMove(inputDirection);
        }
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int targetCell = m_CellPosition + direction;
        BoardManager.CellData cellData = m_Board.GetCellData(targetCell);

        if (cellData == null || !cellData.Passable)
        {
            // 벽에 부딪힘
            m_Animator.SetTrigger(hashAttack);
            GameManager.Instance.TurnManager.Tick();
            return;
        }

        if (cellData.ContainedObject == null) // 비어있는 셀
        {
            MoveTo(targetCell);
        }
        else // 오브젝트가 있는 셀
        {
            // PlayerWantsToEnter()를 호출하여 이동/공격 여부를 결정
            if (cellData.ContainedObject.PlayerWantsToEnter())
            {
                MoveTo(targetCell); // 이동을 허용하면 이동
            }
            else
            {
                // 이동을 허용하지 않으면 공격 애니메이션
                m_Animator.SetTrigger(hashAttack);
                GameManager.Instance.TurnManager.Tick();
            }
        }
    }

    // 외부 호출을 위한 편의 함수 (TryMove 통합)
    public void MoveUp() { TryMove(Vector2Int.up); }
    public void MoveDown() { TryMove(Vector2Int.down); }
    public void MoveRight() { TryMove(Vector2Int.right); }
    public void MoveLeft() { TryMove(Vector2Int.left); }
    public void MoveSkip()
    {
        if (m_IsGameOver || m_IsMoving) return;
        GameManager.Instance.TurnManager.Tick();
    }
}