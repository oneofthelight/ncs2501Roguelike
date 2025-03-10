using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class WallObject : CellObject
{
    public Tile ObstacleTile;
    public Tile HP1Tile;
    public int MaxHealth = 3;
    public AudioClip Clip;
    private int m_HealthPoint;  
    private Tile m_OriginalTile;

    public override void Init(Vector2Int cell)
    {
        base.Init(cell);  // base -> 자신의 부모를 의미, 이것을 호출하는 이유는 base에서 수정하면 되기 때문
        m_HealthPoint = MaxHealth;

        m_OriginalTile = GameManager.Instance.BoardManager.GetCellTile(cell);
        GameManager.Instance.BoardManager.SetCellTile(cell, ObstacleTile);
    }
    public override void PlayerEntered()
    {
        
    }
    public override bool PlayerWantsToEnter()
    {
        m_HealthPoint -= 1;
        GameManager.Instance.PlaySound(Clip);
        if (m_HealthPoint > 0)
        {
            if(m_HealthPoint == 1)
            {
                GameManager.Instance.BoardManager.SetCellTile(m_Cell, HP1Tile);
            }
            return false;
        }

        GameManager.Instance.BoardManager.SetCellTile(m_Cell, m_OriginalTile);
        Destroy(gameObject);
        return true;
    }
}

