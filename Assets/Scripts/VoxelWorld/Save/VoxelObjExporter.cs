using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelObjExporter
{
    public static void Export(string path, World world)
    {
        var chunks = world.Chunks;

        if (chunks.Count == 0)
        {
            Debug.LogWarning("No chunks to export.");
            return;
        }

        string objPath = path;
        string mtlPath = Path.ChangeExtension(path, ".mtl");
        string mtlFileName = Path.GetFileName(mtlPath);

        StringBuilder obj = new StringBuilder();
        StringBuilder mtl = new StringBuilder();

        obj.AppendLine($"mtllib {mtlFileName}");
        obj.AppendLine("o VoxelWorld");

        int vertexOffset = 0;
        int normalOffset = 0;
        int uvOffset = 0;

        HashSet<string> writtenMaterials = new HashSet<string>();

        foreach (var pair in chunks)
        {
            ChunkRenderer renderer = pair.Value;
            Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null) continue;

            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uvs = mesh.uv;

            Transform t = renderer.transform;

            // Write vertices
            foreach (var v in vertices)
            {
                Vector3 worldV = t.TransformPoint(v);
                obj.AppendLine($"v {worldV.x} {worldV.y} {worldV.z}");
            }

            // Write normals
            foreach (var n in normals)
            {
                Vector3 worldN = t.TransformDirection(n);
                obj.AppendLine($"vn {worldN.x} {worldN.y} {worldN.z}");
            }

            // Write UVs
            foreach (var uv in uvs)
            {
                obj.AppendLine($"vt {uv.x} {uv.y}");
            }
            MeshRenderer meshRenderer = renderer.GetComponent<MeshRenderer>();
            Material[] materials = meshRenderer != null
            ? meshRenderer.sharedMaterials
            : null;

            // Submeshes
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                string matName = "DefaultMaterial";

                if (materials != null && sub < materials.Length && materials[sub] != null)
                {
                    matName = materials[sub].name.Replace(" (Instance)", "");
                }

                obj.AppendLine($"usemtl {matName}");

                if (!writtenMaterials.Contains(matName))
                {
                    if (materials != null && sub < materials.Length && materials[sub] != null)
                    {
                        WriteMaterial(mtl, matName, materials[sub]);
                    }
                    else
                    {
                        WriteMaterial(mtl, matName, null);
                    }

                    writtenMaterials.Add(matName);
                }

                int[] tris = mesh.GetTriangles(sub);

                for (int i = 0; i < tris.Length; i += 3)
                {
                    int a = tris[i] + 1 + vertexOffset;
                    int b = tris[i + 1] + 1 + vertexOffset;
                    int c = tris[i + 2] + 1 + vertexOffset;

                    obj.AppendLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");
                }
            }

            vertexOffset += vertices.Length;
            normalOffset += normals.Length;
            uvOffset += uvs.Length;
        }

        File.WriteAllText(objPath, obj.ToString());
        File.WriteAllText(mtlPath, mtl.ToString());

        Debug.Log($"OBJ exported to {objPath}");
    }

    private static void WriteMaterial(StringBuilder mtl, string name, Material mat)
    {
        mtl.AppendLine($"newmtl {name}");
        mtl.AppendLine("Ka 1.000 1.000 1.000");
        mtl.AppendLine("Kd 1.000 1.000 1.000");
        mtl.AppendLine("Ks 0.000 0.000 0.000");
        mtl.AppendLine("d 1.0");
        mtl.AppendLine("illum 2");

        if (mat.mainTexture != null)
        {
            string texName = mat.mainTexture.name + ".png";
            mtl.AppendLine($"map_Kd {texName}");
        }

        mtl.AppendLine();
    }
}
