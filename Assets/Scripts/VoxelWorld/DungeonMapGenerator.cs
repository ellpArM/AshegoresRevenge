using System.Collections.Generic;
using UnityEngine;

public class DungeonMapGenerator : MonoBehaviour, IMapGenerator
{
    public Transform player;
    public Transform boss;

    public int roomCount = 12;
    public int minRoomSize = 4;
    public int maxRoomSize = 7;

    public int corridorWidth = 2;
    public int corridorLength = 6;

    public int floorVoxelId = 1;
    public int wallVoxelId = 2;

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
        World.instance.BeginVoxelBatch();

        rooms.Clear();

        GenerateLinearRooms();
        GenerateBranches();
        BuildWalls();
        PlaceActors();

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

            CarveCorridor(previous.center, next.center);

            previous = next;
        }
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

        while (current.x != b.x)
        {
            CarveCorridorSegment(current, true);
            current.x += (b.x > current.x) ? 1 : -1;
        }

        while (current.y != b.y)
        {
            CarveCorridorSegment(current, false);
            current.y += (b.y > current.y) ? 1 : -1;
        }
    }
    void CarveCorridorSegment(Vector2Int pos, bool horizontal)
    {
        int start = -(corridorWidth / 2);
        int end = start + corridorWidth - 1;

        for (int i = start; i <= end; i++)
        {
            Vector3Int voxel;

            if (horizontal)
                voxel = new Vector3Int(pos.x, 0, pos.y + i);
            else
                voxel = new Vector3Int(pos.x + i, 0, pos.y);

            World.instance.SetVoxelBatched(voxel, floorVoxelId);
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

        if (player != null)
        {
            player.position = new Vector3(
                start.center.x + 0.5f,
                1,
                start.center.y + 0.5f
            );
        }

        if (boss != null)
        {
            boss.position = new Vector3(
                end.center.x + 0.5f,
                1,
                end.center.y + 0.5f
            );
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
                CarveCorridor(source.center, branch.center);
            }
        }
    }
}