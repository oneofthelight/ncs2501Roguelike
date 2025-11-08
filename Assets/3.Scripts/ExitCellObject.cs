using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ExitCellObject : CellObject
{
    public Tile EndTile;

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        GameManager.Instance.BoardManager.SetCellTile(coord, EndTile);
    }

    // 플레이어가 Exit 셀로 이동하려 할 때 호출됩니다.
    public override bool PlayerWantsToEnter()
    {
        // Treasure 획득 여부(Exit 활성화)만 확인하여 이동 가능 여부를 결정합니다.
        if (GameManager.Instance != null && GameManager.Instance.IsExitActive)
        {
            return true; // 활성화되면 이동을 허용
        }
        else
        {
            Debug.Log("Treasure를 획득해야 Exit이 활성화됩니다.");
            return false; // 비활성화되면 이동을 차단
        }
    }

    // 플레이어가 셀로 이동을 완료한 후 호출됩니다.
    public override void PlayerEntered()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsExitActive)
        {
            Debug.Log("Exit 진입 완료. 다음 레벨로 이동합니다.");
            // 다음 스테이지로 이동
            GameManager.Instance.NewLevel();
        }
    }
}