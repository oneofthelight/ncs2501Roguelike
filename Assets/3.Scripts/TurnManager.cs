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
        /*
        if (OnTick != null)   이 주석 내용을 한줄로 줄인게 위에 내용
        {
            OnTick.Invoke();
        }
        */
    }
}
