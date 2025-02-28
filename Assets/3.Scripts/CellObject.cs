using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellObject : MonoBehaviour
{
    protected Vector2Int m_Cell;

    public virtual void Init(Vector2Int cell)
    {
        m_Cell = cell;
    }

    public virtual void PlayerEntered()
    {

    }
    public virtual bool PlayerWantsToEnter()  // 들어갈 수 있는지 없는지 여부를 여기서 확인
    {
        return true;
    }
}
