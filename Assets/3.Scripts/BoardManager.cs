using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public AudioClip collectedClip;
    public int Width;

    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;  // 테두리
    public FoodObject[] FoodPrefab;
    public WallObject[] WallPrefab; // 벽
    public ExitCellObject ExitPrefab;
    public EnemyObject[] EnemyPrefab;
    public int minFood;
    public int maxFood;
    public int minWall;
    public int maxWall;
    public int minEnemy;
    public int maxEnemy;

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }
    //public PlayerController Player;
    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    private Tilemap m_Tilemap;
    private CellData[,] m_BoardData;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;
   
    public void Init()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_BoardData = new CellData[Width, Height];  // 이 내용은 셀데이터의 전체의 내용
        //m_Grid = GetComponentInChildren<Grid>(); 현재 자기 탭에 존재하기 때문에 이 코드가 좀더 안전하게 코드 사용 밑과 성능 차이 x
        m_Grid = GetComponent<Grid>();  
        m_EmptyCellsList = new List<Vector2Int>(); 
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
                    // 비어 있는 타일이므로, 빈 타일 리스트에 넣어준다
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        // 플레이어가 등장하는 위치는 빈타일이 아니므로 빼준다
        m_EmptyCellsList.Remove(new Vector2Int(1, 1));

        // Exit
        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);            // 이걸 빼는 이유는 음식이나 벽이 생성 되면 안되기 때문

        GenerateWall();
        GenerateFood();
        GenerateEnemy();
    }
    public void Clean()
    {
        //no board data, so exit early, nothing to clean
        if (m_BoardData == null) return;

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
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
    private void GenerateFood()
    {
        int foodCount = Random.Range(minFood, maxFood + 1);
        for (int i = 0; i < foodCount; ++i)
        {
            //int randomY = Random.Range(1, Height - 1);  // 벽에는 생성되지 않아야 하므로 x,y 둘다 -1
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);  //RemoveAt에는 int index 값이 들어감
            
            int foodType = Random.Range(0, FoodPrefab.Length);  
            FoodObject newFood = Instantiate(FoodPrefab[foodType]);  // FoodPreFab에 인덱스 값을 추가함(프리팝을 배열로 만듦)
            AddObject(newFood, coord);
        }
    }
    void GenerateWall()
    {
        int wallCount = Random.Range(minWall, maxWall + 1);
        for (int i = 0; i < wallCount; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];  // 랜덤으로 빈 셀 선택

            m_EmptyCellsList.RemoveAt(randomIndex);            // 벽 제거   

            int wallType = Random.Range(0, WallPrefab.Length);
            WallObject newWall = Instantiate(WallPrefab[wallType]);      // 벽 생성
            AddObject(newWall, coord);
        }
    }
    void GenerateEnemy()
    {
        int enemyCount = Random.Range(minEnemy, maxEnemy + 1);
        for (int i = 0; i < enemyCount; i++)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];  // 랜덤으로 빈 셀 선택

            m_EmptyCellsList.RemoveAt(randomIndex);            // 벽 제거   

            int enemyType = Random.Range(0, EnemyPrefab.Length);
            EnemyObject newEnemy = Instantiate(EnemyPrefab[enemyType]);      // 벽 생성
            AddObject(newEnemy, coord);
        }
    }
    void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }
    
}
