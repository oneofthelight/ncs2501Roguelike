using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObject : CellObject

{
    public int Health = 3;
    public int Amount = 3;
    public int healAmountOnDeath = 10;
    private int m_CurrentHealth;

    private void Awake()
    {
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
    }

    private void OnDestroy()
    {
        GameManager.Instance.TurnManager.OnTick -= TurnHappened;
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;
        Debug.Log(m_CurrentHealth);
        if (m_CurrentHealth <= 0)
        {
            GameManager.Instance.UpdateHPBar((int)healAmountOnDeath);
            Destroy(gameObject);
            GameManager.Instance.UpdateHPBar(Amount);
        }

        return false;
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.Passable
            || targetCell.ContainedObject != null)
        {
            return false;
        }

        // 현재 셀에서 적 제거
        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        // 셀 추가
        targetCell.ContainedObject = this;
        m_Cell = coord;
        transform.position = board.CellToWorld(coord);

        return true;
    }

    void TurnHappened()
    {
        //We added a public property that return the player current cell!
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = playerCell.x - m_Cell.x;    // 적 캐릭터와 주인공 캐릭터의 위치 빼기
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

         Debug.Log($"몬스터 플레이어와 거리 x:{absXDist}. y:{absYDist}");
        //if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
        if ((absXDist == 0 && absYDist == 1) || (absYDist == 0 && absXDist == 1))
        {
            //we are adjacent to the player, attack!
            GameManager.Instance.UpdateHPBar(-2);
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    //if our move was not successful (so no move and not attack)
                    //we try to move along Y
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    bool TryMoveInX(int xDist)
    {

        //player to our right
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }

        //player to our left
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        //try to get closer in y

        //player on top
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }

        //player below
        return MoveTo(m_Cell + Vector2Int.down);
    }
}