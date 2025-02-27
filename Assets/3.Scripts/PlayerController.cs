using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
   
    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        m_CellPosition = cell;
        // 보드 에서의 player위치 지정 => 화면에서 제대로된 위치에 표시
        MoveTo(cell);
    }
    public void MoveTo(Vector2Int cell)
    {
        m_CellPosition = cell;
        transform.position = m_Board.CellToWorld(m_CellPosition);
    }
    void Start()
    {
        
    }

    private void Update()
    {
        Vector2Int newCellTarget = m_CellPosition;
        bool hasMoved = false;
        
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y++;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y--;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x++;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x--;
            hasMoved = true;
        }

        if (hasMoved)
        {
            // 셀이 움직일 수 있으면 움직여라
            BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

            if (cellData != null && cellData.Passable)
            {
                //m_CellPosition = newCellTarget;
                GameManager.Instance.TurnManager.Tick();
                MoveTo(newCellTarget);
                if (cellData.ContainedObject != null)
                {
                    cellData.ContainedObject.PlayerEntered();
                }
            }
        }
    }
}
