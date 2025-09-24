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
        m_IsGameOver = false;
    }
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
        
    }

    private void Update()
    {
        if (m_IsGameOver) 
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }
            return;
        }
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
        newCellTarget = m_CellPosition;
        hasMoved = false;
        
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            //newCellTarget.y++;
            //hasMoved = true;
            MoveUp();
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            //newCellTarget.y--;
            //hasMoved = true;
            MoveDown();
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            //newCellTarget.x++;
            //hasMoved = true;
            MoveRight();
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            //newCellTarget.x--;
            //hasMoved = true;
            MoveLeft();
        }
    }    
    private void UpdatePlayer()
    {
        if (hasMoved)
        {
            // 셀이 움직일 수 있으면 움직여라
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);
            if (cellData != null && cellData.Passable)
            {
                
                //m_CellPosition = newCellTarget;
                if (cellData.ContainedObject == null)  // 들어가려고 할때
                {
                    MoveTo(newCellTarget);
                    
                }
                else
                {
                    if (cellData.ContainedObject.PlayerWantsToEnter())
                    {
                        //GameManager.Instance.UpdateHPBar(-2);
                        MoveTo(newCellTarget);  // 플레이어를 먼저 셀로 이동 시킨 후 호출

                    }
                    else 
                    {
                        //GameManager.Instance.UpdateHPBar();
                        m_Animator.SetTrigger(hashAttack);
                    }
                }
            GameManager.Instance.TurnManager.Tick();
            }
            
        }
    }
    public void MoveSkip()
    {
        if(m_IsGameOver)
        {
            GameManager.Instance.StartNewGame();
            return;
        }
        if (m_IsMoving) return;
        hasMoved = true;
        UpdatePlayer();
    }
    public void MoveUp()
    {
        if (m_IsMoving) return;
        newCellTarget.y++;
        hasMoved = true;
        UpdatePlayer();
    }
    public void MoveDown()
    {
        if (m_IsMoving) return;
        newCellTarget.y--;
        hasMoved = true;
        UpdatePlayer();
    }
    public void MoveRight()
    {
        if (m_IsMoving) return;
        newCellTarget.x++;
        hasMoved = true;
        UpdatePlayer();
    }
    public void MoveLeft()
    {
        if (m_IsMoving) return;
        newCellTarget.x--;
        hasMoved = true;
        UpdatePlayer();
    }
}
