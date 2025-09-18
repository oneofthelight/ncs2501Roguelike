using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager
{
    public event System.Action OnTick;
    private int m_TurnCount;

    public TurnManager()
    {
        m_TurnCount = 1;
    }

    public void Tick()
    {
        m_TurnCount++;
        Debug.Log($"Current turn count : + {m_TurnCount}");
        OnTick?.Invoke();
    }
}
