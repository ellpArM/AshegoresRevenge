using System.Collections.Generic;
using UnityEngine;

public class DungeonMapGenerator : MonoBehaviour, IMapGenerator
{
    public GameObject playerPrefab;
    public GameObject bossPrefab;
    public GameObject enemyPrefab;

    public int roomCount = 12;
    public int minRoomSize = 4;
    public int maxRoomSize = 7;

    public int corridorWidth = 2;
    public int corridorLength = 6;

    public int floorVoxelId = 1;
    public int wallVoxelId = 2;
    public HeroSelectionData startingHeroes;
    int nextEnemyId = 1;

    private List<Room> rooms = new();

    class Room
    {
        public RectInt rect;
        public Vector2Int center;

        public Room(RectInt rect)
        {
            this.rect = rect;
            center = new Vector2Int(
                rect.x + rect.width / 2,
                rect.y + rect.height / 2
            );
        }
    }

    public void GenerateMap()
    {
        // if has path we are returning from Battle scene and BattleTransitionManager will handle restoring world
        // else assume we have to generate new world
        if (BattleTransitionManager.Instance.GetPath() != null)
            return;

        World.instance.BeginVoxelBatch();

        rooms.Clear();

        GenerateLinearRooms();
        GenerateBranches();
        BuildWalls();
        SpawnEnemies();
        PlaceActors();
        GenerateHeroData();

        World.instance.EndVoxelBatch();
    }

    void GenerateLinearRooms()
    {
        Room start = CreateRoom(Vector2Int.zero);
        rooms.Add(start);

        Room previous = start;

        for (int i = 1; i < roomCount; i++)
        {
            Room next = TryCreateConnectedRoom(previous);

            if (next == null)
                break;

            rooms.Add(next);

            Vector2Int startEdge = GetEdgePoint(previous, next);
            Vector2Int endEdge = GetEdgePoint(next, previous);

            CarveCorridor(startEdge, endEdge);

            previous = next;
        }
    }
    public void GenerateHeroData()
    {
        foreach (var hero in startingHeroes.selectedHeroes)
            PlayerDataManager.instance.AddToParty(hero.GetComponent<HeroEntity>());
        //startingHeroes.selectedHeroes;
    }

    Room TryCreateConnectedRoom(Room previous)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2Int dir = RandomDirection();

            Vector2Int center =
                previous.center + dir * corridorLength;

            Room candidate = CreateRoom(center, false);

            if (Overlaps(candidate))
                continue;

            CarveRoom(candidate.rect);

            return candidate;
        }

        return null;
    }

    Room CreateRoom(Vector2Int center, bool carve = true)
    {
        int w = Random.Range(minRoomSize, maxRoomSize + 1);
        int h = Random.Range(minRoomSize, maxRoomSize + 1);

        RectInt rect = new RectInt(
            center.x - w / 2,
            center.y - h / 2,
            w,
            h
        );

        if (carve)
            CarveRoom(rect);

        return new Room(rect);
    }

    void CarveRoom(RectInt rect)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
        {
            for (int z = rect.yMin; z < rect.yMax; z++)
            {
                World.instance.SetVoxelBatched(
                    new Vector3Int(x, 0, z),
                    floorVoxelId
                );
            }
        }
    }

    void CarveCorridor(Vector2Int a, Vector2Int b)
    {

        Vector2Int current = a;

        // --- X phase ---
        if (a.x < b.x)
        {
            for (int x = a.x; x <= b.x; x++)
            {
                current.x = x;
                CarveCorridorSegment(current, Vector2Int.right);
            }
        }
        else if (a.x > b.x)
        {
            for (int x = a.x; x >= b.x; x--)
            {
                current.x = x;
                CarveCorridorSegment(current, Vector2Int.left);
            }
        }

        // --- Y phase ---
        if (a.y < b.y)
        {
            for (int y = current.y; y <= b.y; y++)
            {
                current.y = y;
                CarveCorridorSegment(current, Vector2Int.up);
            }
        }
        else if (a.y > b.y)
        {
            for (int y = current.y; y >= b.y; y--)
            {
                current.y = y;
                CarveCorridorSegment(current, Vector2Int.down);
            }
        }
    }
    void CarveCorridorSegment(Vector2Int pos, Vector2Int direction)
    {
        int floorY = 0;
        int wallHeight = 2;

        Vector2Int perp = new Vector2Int(-direction.y, direction.x);

        // --- DEFINE CORRIDOR BOUNDS ---
        int half = corridorWidth / 2;

        int minOffset = -half;
        int maxOffset = half;

        // Fix asymmetry for even widths
        if (corridorWidth % 2 == 0)
        {
            maxOffset -= 1;
        }

        // --- FLOOR + CLEAR ---
        for (int o = minOffset; o <= maxOffset; o++)
        {
            Vector2Int offset = pos + perp * o;

            // floor
            World.instance.SetVoxelBatched(new Vector3Int(offset.x, floorY, offset.y),floorVoxelId);

            // clear space
            for (int h = 1; h <= wallHeight; h++)
            {
                World.instance.SetVoxelBatched(new Vector3Int(offset.x, floorY + h, offset.y), 0);
            }
        }

        // --- WALLS (based on SAME bounds) ---
        Vector2Int leftWall = pos + perp * (minOffset - 1);
        Vector2Int rightWall = pos + perp * (maxOffset + 1);

        BuildWall(leftWall, floorY, wallHeight);
        BuildWall(rightWall, floorY, wallHeight);
    }
    void BuildWall(Vector2Int pos, int baseY, int height)
    {
        for (int h = 1; h <= height; h++)
        {
            World.instance.SetVoxelBatched(
                new Vector3Int(pos.x, baseY + h, pos.y),
                wallVoxelId
            );
        }
    }
    void BuildWalls()
    {
        HashSet<Vector3Int> walls = new();

        foreach (var room in rooms)
        {
            for (int x = room.rect.xMin - 2; x <= room.rect.xMax + 2; x++)
            {
                for (int z = room.rect.yMin - 2; z <= room.rect.yMax + 2; z++)
                {
                    if (World.instance.GetVoxelId(new Vector3Int(x, 0, z)) == 0)
                    {
                        walls.Add(new Vector3Int(x, 1, z));
                        walls.Add(new Vector3Int(x, 2, z));
                    }
                }
            }
        }

        foreach (var wall in walls)
        {
            World.instance.SetVoxelBatched(wall, wallVoxelId);
        }
    }
    void PlaceActors()
    {
        if (rooms.Count == 0)
            return;

        Room start = rooms[0];
        Room end = rooms[rooms.Count - 1];

        if (playerPrefab != null)
        {
            Instantiate(playerPrefab,
                new Vector3(start.center.x + 0.5f, 1, start.center.y + 0.5f),
                Quaternion.identity);
        }

        if (bossPrefab != null)
        {
            var bossObj = Instantiate(bossPrefab,
                new Vector3(end.center.x + 0.5f, 1, end.center.y + 0.5f),
                Quaternion.identity);

            var enemy = bossObj.GetComponent<EnemyWorldEntity>();
            if (enemy != null)
            {
                enemy.enemyId = GenerateUniqueEnemyId();
            }
        }
    }
    int GenerateUniqueEnemyId()
    {
        return nextEnemyId++;
    }
    void SpawnEnemies()
    {
        if (enemyPrefab == null)
            return;

        for (int i = 1; i < rooms.Count - 1; i++)
        {
            Room room = rooms[i];

            Vector3 spawnPos = new Vector3(
                room.center.x + 0.5f,
                1f,
                room.center.y + 0.5f
            );

            var enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            var enemy = enemyObj.GetComponent<EnemyWorldEntity>();
            if (enemy != null)
                enemy.enemyId = GenerateUniqueEnemyId();
        }
    }
    bool Overlaps(Room candidate)
    {
        foreach (var room in rooms)
        {
            RectInt expanded = room.rect;
            expanded.xMin -= 2;
            expanded.xMax += 2;
            expanded.yMin -= 2;
            expanded.yMax += 2;

            if (expanded.Overlaps(candidate.rect))
                return true;
        }

        return false;
    }

    Vector2Int RandomDirection()
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        return dirs[Random.Range(0, dirs.Length)];
    }
    void GenerateBranches()
    {
        List<Room> mainRooms = new List<Room>(rooms);

        for (int i = 1; i < mainRooms.Count - 1; i++)
        {
            if (Random.value < 0.5f)
                continue;

            Room source = mainRooms[i];

            Room branch = TryCreateConnectedRoom(source);

            if (branch != null)
            {
                rooms.Add(branch);
                //CarveCorridor(source.center, branch.center);

                Vector2Int startEdge = GetEdgePoint(source, branch);
                Vector2Int endEdge = GetEdgePoint(branch, source);

                CarveCorridor(startEdge, endEdge);
            }
        }
    }
    Vector2Int GetEdgePoint(Room from, Room to)
    {
        Vector2Int fromCenter = from.center;
        Vector2Int toCenter = to.center;

        Vector2Int delta = toCenter - fromCenter;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0)
            {
                // RIGHT
                return new Vector2Int(from.rect.xMax, fromCenter.y);
            }
            else
            {
                // LEFT
                return new Vector2Int(from.rect.xMin - 1, fromCenter.y);
            }
        }
        else
        {
            if (delta.y > 0)
            {
                // TOP
                return new Vector2Int(fromCenter.x, from.rect.yMax);
            }
            else
            {
                // BOTTOM
                return new Vector2Int(fromCenter.x, from.rect.yMin - 1);
            }
        }
    }
}