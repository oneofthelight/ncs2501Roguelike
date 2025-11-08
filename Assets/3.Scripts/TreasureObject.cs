using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureObject : CellObject
{
    // ... (PlayerWantsToEnter 함수는 그대로 유지) ...
    public override bool PlayerWantsToEnter()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsExitActive)
        {
            return true;
        }
        return true;
    }

    // 플레이어가 셀에 완전히 진입했을 때 호출됩니다.
    public override void PlayerEntered()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsExitActive)
        {
            const int HEALTH_RECOVERED = 30; // 회복량 상수화

            // 🚨 [핵심 수정] 1. HP 회복 로직을 GameManager에 요청 (가장 안전한 방법)
            // GameManager에 플레이어 HP 회복을 위한 헬퍼 함수가 있다고 가정하고 호출합니다.
            GameManager.Instance.RecoverPlayerHealth(HEALTH_RECOVERED);

            // 2. Exit 활성화
            GameManager.Instance.ActivateExit();

            // 3. 오브젝트 파괴 및 셀 비우기
            if (GameManager.Instance.BoardManager.GetCellData(m_Cell).ContainedObject == this)
            {
                GameManager.Instance.BoardManager.GetCellData(m_Cell).ContainedObject = null;
            }
            Destroy(gameObject);
        }
    }
}