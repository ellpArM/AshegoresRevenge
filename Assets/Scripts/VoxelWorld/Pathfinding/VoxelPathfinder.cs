using System.Collections.Generic;
using UnityEngine;

public static class VoxelPathfinder
{
    class Node
    {
        public Vector3Int pos;
        public Node parent;
        public int gCost;
        public int hCost;
        public int FCost => gCost + hCost;

        public Node(Vector3Int pos)
        {
            this.pos = pos;
        }
    }

    static readonly Vector3Int[] directions =
    {
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.forward,
        Vector3Int.back,

        new Vector3Int(1,0,1),
        new Vector3Int(1,0,-1),
        new Vector3Int(-1,0,1),
        new Vector3Int(-1,0,-1)
    };

    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int target)
    {
        List<Node> open = new();
        HashSet<Vector3Int> closed = new();

        Node startNode = new(start);
        Node targetNode = new(target);

        open.Add(startNode);

        while (open.Count > 0)
        {
            Node current = open[0];

            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].FCost < current.FCost ||
                   (open[i].FCost == current.FCost && open[i].hCost < current.hCost))
                {
                    current = open[i];
                }
            }

            open.Remove(current);
            closed.Add(current.pos);

            if (current.pos == targetNode.pos)
                return RetracePath(current);

            foreach (Vector3Int dir in directions)
            {
                foreach (int yOffset in new int[] { 0, 1, -1 })
                {
                    Vector3Int next = current.pos + dir + Vector3Int.up * yOffset;

                    if (closed.Contains(next))
                        continue;

                    if (IsDiagonal(dir))
                    {
                        if (!CanMoveDiagonal(current.pos, dir))
                            continue;
                    }

                    if (!IsWalkable(next, current.pos))
                        continue;

                    int newCost = current.gCost + (IsDiagonal(dir) ? 14 : 10);

                    Node existing = open.Find(n => n.pos == next);

                    if (existing == null)
                    {
                        Node node = new(next);
                        node.gCost = newCost;
                        node.hCost = Heuristic(next, target);
                        node.parent = current;
                        open.Add(node);
                    }
                    else if (newCost < existing.gCost)
                    {
                        existing.gCost = newCost;
                        existing.parent = current;
                    }
                }
            }
        }

        return null;
    }

    static bool IsWalkable(Vector3Int pos, Vector3Int from)
    {
        int body = World.instance.GetVoxel(pos).VoxelId;
        int ground = World.instance.GetVoxel(pos + Vector3Int.down).VoxelId;

        if (body != 0)
            return false;

        if (ground == 0)
            return false;

        if (Mathf.Abs(pos.y - from.y) > 1)
            return false;

        return true;
    }

    static int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) +
               Mathf.Abs(a.y - b.y) +
               Mathf.Abs(a.z - b.z);
    }

    static List<Vector3Int> RetracePath(Node endNode)
    {
        List<Vector3Int> path = new();

        Node current = endNode;

        while (current != null)
        {
            path.Add(current.pos);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }
    static bool IsDiagonal(Vector3Int dir)
    {
        return Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.z) == 1;
    }

    static bool CanMoveDiagonal(Vector3Int current, Vector3Int dir)
    {
        Vector3Int side1 = current + new Vector3Int(dir.x, 0, 0);
        Vector3Int side2 = current + new Vector3Int(0, 0, dir.z);

        if (!IsWalkable(side1, current))
            return false;

        if (!IsWalkable(side2, current))
            return false;

        return true;
    }
}