using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObject1 : CellObject
{
    public int Health = 4;
    public int Amount = 4;
    public int healAmountOnDeath = 20;
    private int m_CurrentHealth;

    // 🚨 [추가] Animator 컴포넌트 참조
    private Animator m_Animator;
    // 🚨 [추가] Animator 트리거 해시 (성능 최적화)
    private readonly int hashAttack = Animator.StringToHash("Attack");

    private void Awake()
    {
        GameManager.Instance.TurnManager.OnTick += TurnHappened;
        // 🚨 [추가] Awake에서 Animator 컴포넌트 가져오기
        m_Animator = GetComponent<Animator>();
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
        // 🚨 [핵심 수정] 플레이어와 충돌 시 공격 애니메이션 트리거 발동
        if (m_Animator != null)
        {
            m_Animator.SetTrigger(hashAttack);
        }

        m_CurrentHealth -= 1;
        Debug.Log(m_CurrentHealth);
        if (m_CurrentHealth <= 0)
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
            {
                GameManager.Instance.UpdateHPBar(healAmountOnDeath);
                GameManager.Instance.UpdateHPBar(Amount);
            }
            Destroy(gameObject);
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

        if (targetCell.ContainedObject != null && !(targetCell.ContainedObject is FoodObject))
        {
            return false;
        }

        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        targetCell.ContainedObject = this;
        m_Cell = coord;
        transform.position = board.CellToWorld(coord);

        if (targetCell == currentCell)
        {
            GameManager.Instance.UpdateHPBar(-2);
        }

        return true;
    }

    void TurnHappened()
    {
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        Debug.Log($"몬스터 플레이어와 거리 x:{xDist}. y:{yDist}");

        // 🚨 [추가] 몬스터가 플레이어를 직접 공격하는 경우 (인접했을 때) 애니메이션 트리거 발동
        if ((absXDist == 0 && absYDist == 1) || (absYDist == 0 && absXDist == 1))
        {
            // 플레이어와 인접해 공격할 때도 애니메이션을 재생할 수 있습니다.
            // 이 부분은 필요에 따라 추가하거나 제거하세요.
            if (m_Animator != null)
            {
                m_Animator.SetTrigger(hashAttack);
            }
            GameManager.Instance.UpdateHPBar(-3);
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
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
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }
        return MoveTo(m_Cell + Vector2Int.down);
    }
}