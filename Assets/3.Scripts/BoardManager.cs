using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    // 맵 데이터 구조체
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }
    private CellData[,] m_BoardData;
    // 🚨 [추가] 첫 번째 방의 좌표 목록을 저장할 변수
    private List<Vector2Int> m_FirstRoomCells;

    // 맵 생성 및 룸 배치에 필요한 상수 및 변수
    private const int MAX_MAP_WIDTH = 40; // 전체 맵의 최대 너비 고정
    private const int MAX_MAP_HEIGHT = 40; // 전체 맵의 최대 높이 고정
    private int m_RoomCount; // 현재 레벨에 따른 생성될 방 개수

    // 룸 정보를 담을 내부 구조체
    private struct Room
    {
        public RectInt bounds; // 룸의 경계 (RectInt은 유니티의 정수 기반 사각형 구조체)
    }
    private List<Room> m_Rooms;

    // 플레이어 시작 및 종료 좌표 (GameManager에서 사용될 새 좌표)
    public Vector2Int PlayerStartCoord { get; private set; }
    public Vector2Int ExitCoord { get; private set; }


    public AudioClip collectedClip;
    public int Width;
    public int Height;
    public int minFood;
    public int maxFood = 5; // 최대값은 적절히 설정
    public int minWall;
    public int maxWall = 10; // 최대값은 적절히 설정
    public int minEnemy;
    public int maxEnemy = 10; // 최대값은 적절히 설정
    public int Elitenemy;
    public int minPotion;
    public int maxPotion = 3;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;    // 테두리
    public PotionObject[] PotionPrefab;
    public FoodObject[] FoodPrefab;
    public WallObject[] WallPrefab; // 벽
    public ExitCellObject ExitPrefab;
    public EnemyObject[] EnemyPrefab;
    public EnemyObject1[] ElitenemyPrefab;
    public CellObject TreasurePrefab;

    private Tilemap m_Tilemap;

    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void Init()
    {
        // 1. 맵 크기를 고정된 최대 크기로 설정 (다중 룸 배치를 위함)
        Width = MAX_MAP_WIDTH;
        Height = MAX_MAP_HEIGHT;

        // 2. 레벨에 따른 룸 개수 계산 (최소 2개, 6스테이지마다 1개 추가)
        m_RoomCount = 2 + (GameManager.Instance.CurrentLevel / 6);
        if (m_RoomCount > 10) m_RoomCount = 10; // 최대 방 개수 제한 (필요에 따라 조절)

        // 3. 맵 오브젝트 개수 스케일링 (이제 룸의 크기 대신 룸 내 오브젝트 밀도를 조정)
        minFood = 6 + (GameManager.Instance.CurrentLevel / 10) * 2;
        minEnemy = 3 + (GameManager.Instance.CurrentLevel / 10) * 2;
        minWall = 5 + (GameManager.Instance.CurrentLevel / 6) * 2;
        minPotion = 2 + (GameManager.Instance.CurrentLevel / 10) * 1;
        Elitenemy = 1 + (GameManager.Instance.CurrentLevel / 20);

        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_BoardData = new CellData[Width, Height];
        m_Grid = GetComponent<Grid>();
        m_EmptyCellsList = new List<Vector2Int>();
        m_Rooms = new List<Room>();

        // 4. 전체 보드를 초기화 (모든 셀을 벽으로 설정)
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                // 모든 타일을 벽으로 초기화하고 통과 불가능하게 설정
                Tile wallTile = WallTiles[Random.Range(0, WallTiles.Length)];
                m_Tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                m_BoardData[x, y] = new CellData { Passable = false };
            }
        }

        // 5. 방 생성 및 복도 연결 (이 과정에서 바닥 타일이 깔리고 m_EmptyCellsList가 채워짐)
        GenerateRooms();
        m_FirstRoomCells = new List<Vector2Int>(); // 🚨 [추가] 리스트 초기화

        // 6. 플레이어 시작 위치 및 출구 설정
        if (m_Rooms.Count > 0)
        {
            // 🚨 [수정] 첫 번째 방의 모든 셀 좌표를 리스트에 추가합니다.
            for (int x = m_Rooms[0].bounds.xMin; x < m_Rooms[0].bounds.xMax; x++)
            {
                for (int y = m_Rooms[0].bounds.yMin; y < m_Rooms[0].bounds.yMax; y++)
                {
                    m_FirstRoomCells.Add(new Vector2Int(x, y));
                }
            }
            // 🚨 [추가/수정] Exit 위치를 마지막 방 끝에 설정
            ExitCoord = new Vector2Int(m_Rooms[m_Rooms.Count - 1].bounds.xMax - 2, m_Rooms[m_Rooms.Count - 1].bounds.yMax - 2);
            if (m_EmptyCellsList.Contains(ExitCoord))
            {
                // Exit 오브젝트 생성 및 배치
                AddObject(Instantiate(ExitPrefab), ExitCoord);
                m_EmptyCellsList.Remove(ExitCoord);
            }
            // 🚨 [핵심 수정 부분] RectInt.center는 Vector2(float)를 반환하므로, 정수로 변환해야 합니다.
            // 첫 번째 방의 중앙 근처를 플레이어 시작 위치로 설정
            Vector2 center = m_Rooms[0].bounds.center; // center는 Vector2 (float)
            PlayerStartCoord = new Vector2Int(Mathf.FloorToInt(center.x), Mathf.FloorToInt(center.y));

            if (m_EmptyCellsList.Contains(PlayerStartCoord))
            {
                m_EmptyCellsList.Remove(PlayerStartCoord);
            }

            // 마지막 방의 끝부분을 출구 위치로 설정
            ExitCoord = new Vector2Int(m_Rooms[m_Rooms.Count - 1].bounds.xMax - 2, m_Rooms[m_Rooms.Count - 1].bounds.yMax - 2);
            if (m_EmptyCellsList.Contains(ExitCoord))
            {
                AddObject(Instantiate(ExitPrefab), ExitCoord);
                m_EmptyCellsList.Remove(ExitCoord);
            }
        }
        else
        {
            // 방이 하나도 생성되지 않았을 경우를 대비한 기본값
            PlayerStartCoord = new Vector2Int(1, 1);
        }

        // 7. 오브젝트 배치
        if (GameManager.Instance.CurrentLevel >= 20)
        {
            GenerateElitenemy();
        }
        if (GameManager.Instance.CurrentLevel >= 10)
        {
            GeneratePotion();
        }
        GenerateTreasure(); // 이 함수는 이제 카운터 대신 Exit 활성화만 합니다.
        GenerateFood();
        GenerateWall(); // 룸 내부에 랜덤 벽 생성
        GenerateEnemy();
    }

    public void Clean()
    {
        if (m_BoardData == null) return;

        // 고정된 MAX_MAP_WIDTH/HEIGHT를 사용하여 전체 그리드를 청소
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

    // (기존의 CellToWorld, GetCellData, AddObject, GenerateFood, GenerateWall, GenerateEnemy, GenerateElitenemy 함수는 유지)

    // --- 새로운 룸 및 복도 생성 로직 ---

    private void GenerateRooms()
    {
        const int MIN_ROOM_SIZE = 6;
        const int MAX_ROOM_SIZE = 15;
        const int MAX_TRIES = 1000; // 방 생성 시도 횟수 제한

        for (int i = 0; i < MAX_TRIES && m_Rooms.Count < m_RoomCount; i++)
        {
            int roomW = Random.Range(MIN_ROOM_SIZE, MAX_ROOM_SIZE + 1);
            int roomH = Random.Range(MIN_ROOM_SIZE, MAX_ROOM_SIZE + 1);

            // 룸 위치를 맵 경계에서 충분히 떨어지게 랜덤하게 선택
            int roomX = Random.Range(2, Width - roomW - 2);
            int roomY = Random.Range(2, Height - roomH - 2);

            // 룸의 경계 (x, y, width, height)
            RectInt newBounds = new RectInt(roomX, roomY, roomW, roomH);

            // 다른 룸과 겹치는지 확인 (룸 사이에 최소 2칸의 여유 공간을 둡니다)
            bool overlaps = false;
            foreach (var room in m_Rooms)
            {
                // 기존 룸 경계에 4칸을 더하여 겹치는지 확인 (1칸 벽 + 1칸 복도 공간)
                if (newBounds.Overlaps(new RectInt(room.bounds.xMin - 2, room.bounds.yMin - 2, room.bounds.width + 4, room.bounds.height + 4)))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                Room newRoom = new Room { bounds = newBounds };
                m_Rooms.Add(newRoom);

                // 보드에 룸 배치 (바닥 타일 및 내부 벽 설정)
                for (int y = newBounds.yMin; y < newBounds.yMax; y++)
                {
                    for (int x = newBounds.xMin; x < newBounds.xMax; x++)
                    {
                        if (x == newBounds.xMin || x == newBounds.xMax - 1 || y == newBounds.yMin || y == newBounds.yMax - 1)
                        {
                            // 룸 내부를 두르는 벽 (통과 불가능)
                            Tile wallTile = WallTiles[Random.Range(0, WallTiles.Length)];
                            m_Tilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                            m_BoardData[x, y].Passable = false;
                        }
                        else
                        {
                            // 룸 내부의 바닥 (통과 가능)
                            Tile groundTile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                            m_Tilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                            m_BoardData[x, y].Passable = true;
                            m_EmptyCellsList.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
        }

        // 룸 연결 (생성된 모든 룸을 순서대로 연결)
        for (int i = 0; i < m_Rooms.Count - 1; i++)
        {
            ConnectRooms(m_Rooms[i], m_Rooms[i + 1]);
        }
    }

    private void ConnectRooms(Room roomA, Room roomB)
    {
        // 🚨 [수정된 부분] RectInt.center는 float(Vector2)를 반환하므로, 정수로 변환해야 합니다.
        // Math.FloorToInt()를 사용하여 내림하여 정수 좌표를 얻습니다.
        Vector2Int centerA = new Vector2Int(Mathf.FloorToInt(roomA.bounds.center.x), Mathf.FloorToInt(roomA.bounds.center.y));
        Vector2Int centerB = new Vector2Int(Mathf.FloorToInt(roomB.bounds.center.x), Mathf.FloorToInt(roomB.bounds.center.y));

        // 1. A의 x에서 B의 x까지 수평 복도 생성 (y는 A의 중앙 y)
        int xStart = Mathf.Min(centerA.x, centerB.x);
        int xEnd = Mathf.Max(centerA.x, centerB.x);
        for (int x = xStart; x <= xEnd; x++)
        {
            MakeCorridorCell(new Vector2Int(x, centerA.y));
        }

        // 2. B의 y에서 A의 y까지 수직 복도 생성 (x는 B의 중앙 x)
        int yStart = Mathf.Min(centerA.y, centerB.y);
        int yEnd = Mathf.Max(centerA.y, centerB.y);
        for (int y = yStart; y <= yEnd; y++)
        {
            // 🚨 [수정된 부분] y 루프에서는 centerB.x를 사용해야 합니다.
            MakeCorridorCell(new Vector2Int(centerB.x, y));
        }
    }

    // 특정 셀을 복도(바닥)로 만드는 헬퍼 함수
    private void MakeCorridorCell(Vector2Int coord)
    {
        // 맵 경계를 벗어나지 않도록
        if (coord.x <= 0 || coord.x >= Width - 1 || coord.y <= 0 || coord.y >= Height - 1) return;

        // 이미 룸의 내부 바닥인 경우 복도를 만들 필요 없음
        if (m_BoardData[coord.x, coord.y].Passable) return;

        // 벽을 바닥 타일로 변경하고 통과 가능하게 설정
        Tile groundTile = GroundTiles[Random.Range(0, GroundTiles.Length)];
        m_Tilemap.SetTile(new Vector3Int(coord.x, coord.y, 0), groundTile);
        m_BoardData[coord.x, coord.y].Passable = true;
        m_EmptyCellsList.Add(coord);

        // 복도의 너비를 3x3으로 확장하여 벽을 제거하고 플레이어가 지나가기 쉽게 만듭니다.
        // 이 셀이 룸의 내부 벽이었다면, 외부 복도를 만들어줍니다.
        Vector2Int[] neighbors = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)
        };
        foreach (var offset in neighbors)
        {
            Vector2Int neighborCoord = coord + offset;
            if (neighborCoord.x > 0 && neighborCoord.x < Width - 1 && neighborCoord.y > 0 && neighborCoord.y < Height - 1)
            {
                // 인접 셀이 통과 불가능한 벽일 때만 바닥으로 만듭니다. (룸 내부 바닥을 다시 만들지 않기 위해)
                if (!m_BoardData[neighborCoord.x, neighborCoord.y].Passable)
                {
                    Tile neighborTile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_Tilemap.SetTile(new Vector3Int(neighborCoord.x, neighborCoord.y, 0), neighborTile);
                    m_BoardData[neighborCoord.x, neighborCoord.y].Passable = true;
                    // 이 셀은 복도이므로 m_EmptyCellsList에 추가해 오브젝트가 스폰될 수 있도록 합니다.
                    m_EmptyCellsList.Add(neighborCoord);
                }
            }
        }
    }

    // (기존의 CellToWorld, GetCellData, AddObject, GenerateFood, GenerateWall, GenerateEnemy, GenerateElitenemy 함수는 여기에 붙여넣습니다.)
    // ...
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
            if (m_EmptyCellsList.Count == 0) break;
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            int foodType = Random.Range(0, FoodPrefab.Length);
            FoodObject newFood = Instantiate(FoodPrefab[foodType]);
            AddObject(newFood, coord);
        }
    }

    private void GeneratePotion()
    {
        int PotionCount = Random.Range(minPotion, maxPotion + 1);
        for (int i = 0; i < PotionCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            int foodType = Random.Range(0, PotionPrefab.Length);
            PotionObject newPotion = Instantiate(PotionPrefab[foodType]);
            AddObject(newPotion, coord);
        }
    }
    void GenerateWall()
    {
        int wallCount = Random.Range(minWall, maxWall + 1);
        for (int i = 0; i < wallCount; ++i)
        {
            if (m_EmptyCellsList.Count == 0) break;
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            int wallType = Random.Range(0, WallPrefab.Length);
            WallObject newWall = Instantiate(WallPrefab[wallType]);
            AddObject(newWall, coord);
        }
    }
    void GenerateEnemy()
    {
        int enemyCount = Random.Range(minEnemy, maxEnemy + 1);
        for (int i = 0; i < enemyCount; i++)
        {
            if (m_EmptyCellsList.Count == 0) break;
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            int enemyType = Random.Range(0, EnemyPrefab.Length);
            EnemyObject newEnemy = Instantiate(EnemyPrefab[enemyType]);
            AddObject(newEnemy, coord);
        }
    }
    public void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }
    void GenerateElitenemy()
    {
        // 1. Init()에서 계산된 Elitenemy 변수를 사용하여 개수를 설정
        int ElitenemyCount = Elitenemy;

        for (int i = 0; i < ElitenemyCount; i++)
        {
            if (m_EmptyCellsList.Count == 0 || ElitenemyPrefab.Length == 0) break;

            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];

            m_EmptyCellsList.RemoveAt(randomIndex);

            int enemyType = Random.Range(0, ElitenemyPrefab.Length);
            EnemyObject1 newElitenemy = Instantiate(ElitenemyPrefab[enemyType]);
            AddObject(newElitenemy, coord);
        }
    }
    private void GenerateTreasure()
    {
        if (TreasurePrefab == null)
        {
            Debug.LogError("TreasurePrefab이 BoardManager에 할당되지 않았습니다!");
            return;
        }

        // 🚨 [핵심 수정] 첫 번째 방을 제외한 빈 셀 목록을 만듭니다.
        List<Vector2Int> availableCells = new List<Vector2Int>(m_EmptyCellsList);

        // 첫 번째 방의 셀을 사용 가능한 목록에서 제거합니다.
        foreach (Vector2Int cell in m_FirstRoomCells)
        {
            if (availableCells.Contains(cell))
            {
                availableCells.Remove(cell);
            }
        }

        if (availableCells.Count == 0)
        {
            Debug.LogWarning("첫 번째 방을 제외한 빈 셀이 없습니다. Treasure 생성을 건너뜁니다.");
            return;
        }

        // 빈 셀 중 랜덤한 위치를 선택 (첫 번째 방 제외)
        int randomIndex = Random.Range(0, availableCells.Count);
        Vector2Int coord = availableCells[randomIndex];

        // 선택된 좌표를 원래 EmptyCellsList와 availableCells 모두에서 제거
        m_EmptyCellsList.Remove(coord);

        // Treasure 오브젝트 생성 및 배치
        CellObject newTreasure = Instantiate(TreasurePrefab);
        AddObject(newTreasure, coord);
    }
}