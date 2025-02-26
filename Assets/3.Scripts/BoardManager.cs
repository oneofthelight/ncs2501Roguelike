using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public int Width;

    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;
    public class CellData
    {
      public bool Passable;
    }
    //public PlayerController Player;

    private Tilemap m_Tilemap;
    private CellData[,] m_BoardData;
    private Grid m_Grid;

    // Start is called before the first frame update
    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_BoardData = new CellData[Width, Height];  // 이 내용은 셀데이터의 전체의 내용
        //m_Grid = GetComponentInChildren<Grid>(); 현재 자기 탭에 존재하기 때문에 이 코드가 좀더 안전하게 코드 사용 밑과 성능 차이 x
        m_Grid = GetComponent<Grid>();   
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();   // 위에서 만들었는데 이것을 또 쓴 이유 --> 셀데이터 각각의 내용
                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    // Wall tile
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    // grond tile
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }
    
    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width
            || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }
}