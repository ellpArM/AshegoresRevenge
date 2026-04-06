using System.Collections.Generic;
using UnityEngine;

public struct VoxelChange
{
    public Vector3Int worldPos;
    public int beforeId;
    public int afterId;
}

public class VoxelAction
{
    public readonly List<VoxelChange> Changes = new();

    public bool IsEmpty => Changes.Count == 0;

    public void AddChange(Vector3Int pos, int before, int after)
    {
        if (before == after)
            return; // no-op, do not pollute history

        Changes.Add(new VoxelChange
        {
            worldPos = pos,
            beforeId = before,
            afterId = after
        });
    }
}
public class VoxelUndoManager
{
    readonly int maxActions;
    readonly Stack<VoxelAction> undoStack = new();
    readonly Stack<VoxelAction> redoStack = new();

    public VoxelUndoManager(int maxActions = 10)
    {
        this.maxActions = maxActions;
    }

    public void Push(VoxelAction action)
    {
        if (action == null || action.IsEmpty)
            return;

        undoStack.Push(action);
        redoStack.Clear();

        while (undoStack.Count > maxActions)
            TrimBottom(undoStack);
    }

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    public VoxelAction PopUndo()
    {
        if (!CanUndo) return null;
        var a = undoStack.Pop();
        redoStack.Push(a);
        return a;
    }

    public VoxelAction PopRedo()
    {
        if (!CanRedo) return null;
        var a = redoStack.Pop();
        undoStack.Push(a);
        return a;
    }

    static void TrimBottom(Stack<VoxelAction> stack)
    {
        var tmp = stack.ToArray();
        stack.Clear();
        for (int i = tmp.Length - 2; i >= 0; i--)
            stack.Push(tmp[i]);
    }
}
