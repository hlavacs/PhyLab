
using System;
using System.Collections.Generic;
using UnityEngine;

public static class NormalSolver
{
    struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public override bool Equals(object obj)
        {
            var key = (Vertex)obj;
            return Position == key.Position && UV == key.UV;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ UV.GetHashCode();
        }
    }

    struct Triangle
    {
        public Vertex A;
        public Vertex B;
        public Vertex C;
        public Vector3 Normal;
    }


    /// <summary>
    ///     Recalculate the normals of a mesh based on an angle threshold.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="angle">
    ///     The smoothing angle. 
    /// </param>
    public static void RecalculateNormals(this Mesh mesh, float angle)
    {
        var cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var uvs = mesh.uv;
        var indices = mesh.GetIndices(0);

        if(indices.Length < 3)
        {
            return;
        }

        var triangles = new Triangle[indices.Length / 3];
        var vertexToTriangles = new Dictionary<Vertex, List<Triangle>>();

        // Compute look-up table to easily get all triangles that correspond to a specific vertex.
        for (int i = 0; i < indices.Length; i+=3)
        {
            int iA = indices[i + 0];
            int iB = indices[i + 1];
            int iC = indices[i + 2];

            Triangle tri;
            tri.A.Position = vertices[iA];
            tri.A.Normal = Vector3.zero;
            tri.A.UV = uvs[iA];
            tri.B.Position = vertices[iB];
            tri.B.UV = uvs[iB];
            tri.B.Normal = Vector3.zero;
            tri.C.Position = vertices[iC];
            tri.C.UV = uvs[iC];
            tri.C.Normal = Vector3.zero;

            tri.Normal = Vector3.Cross(tri.B.Position - tri.A.Position, tri.C.Position - tri.A.Position).normalized;

            if(!vertexToTriangles.ContainsKey(tri.A))
            {
                var list = new List<Triangle>();
                list.Add(tri);
                vertexToTriangles.Add(tri.A, list);
            }
            else
            {
                vertexToTriangles[tri.A].Add(tri);
            }

            if (!vertexToTriangles.ContainsKey(tri.B))
            {
                var list = new List<Triangle>();
                list.Add(tri);
                vertexToTriangles.Add(tri.B, list);
            }
            else
            {
                vertexToTriangles[tri.B].Add(tri);
            }

            if (!vertexToTriangles.ContainsKey(tri.C))
            {
                var list = new List<Triangle>();
                list.Add(tri);
                vertexToTriangles.Add(tri.C, list);
            }
            else
            {
                vertexToTriangles[tri.C].Add(tri);
            }

            triangles[i / 3] = tri;
        }
        
        // Compute per vertex normals.
        for(int i = 0; i < triangles.Length; ++i)
        {
            foreach (var triangle in vertexToTriangles[triangles[i].A])
            {
                triangles[i].A.Normal += triangle.Normal;
            }
            triangles[i].A.Normal.Normalize();

            foreach (var triangle in vertexToTriangles[triangles[i].B])
            {
                triangles[i].B.Normal += triangle.Normal;
            }
            triangles[i].B.Normal.Normalize();

            foreach (var triangle in vertexToTriangles[triangles[i].C])
            {
                triangles[i].C.Normal += triangle.Normal;
            }
            triangles[i].C.Normal.Normalize();
        }

        // Merge vertices
        List<Vector3> newPositions = new List<Vector3>();
        List<Vector3> newNormals = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();
        List<int> newIndices = new List<int>();
        Dictionary<Vertex, int> vertexToIndex = new Dictionary<Vertex, int>();

        for (int i = 0; i < triangles.Length; ++i)
        {
            if(vertexToIndex.ContainsKey(triangles[i].A))
            {
                newIndices.Add(vertexToIndex[triangles[i].A]);
            }
            else
            {
                int index = newPositions.Count;
                vertexToIndex.Add(triangles[i].A, index);
                newPositions.Add(triangles[i].A.Position);
                newNormals.Add(triangles[i].A.Normal);
                newUVs.Add(triangles[i].A.UV);
                newIndices.Add(index);
            }

            if (vertexToIndex.ContainsKey(triangles[i].B))
            {
                newIndices.Add(vertexToIndex[triangles[i].B]);
            }
            else
            {
                int index = newPositions.Count;
                vertexToIndex.Add(triangles[i].B, index);
                newPositions.Add(triangles[i].B.Position);
                newNormals.Add(triangles[i].B.Normal);
                newUVs.Add(triangles[i].B.UV);
                newIndices.Add(index);
            }

            if (vertexToIndex.ContainsKey(triangles[i].C))
            {
                newIndices.Add(vertexToIndex[triangles[i].C]);
            }
            else
            {
                int index = newPositions.Count;
                vertexToIndex.Add(triangles[i].C, index);
                newPositions.Add(triangles[i].C.Position);
                newNormals.Add(triangles[i].C.Normal);
                newUVs.Add(triangles[i].C.UV);
                newIndices.Add(index);
            }
        }

        // Set mesh
        mesh.Clear();
        mesh.SetVertices(newPositions);
        mesh.SetNormals(newNormals);
        mesh.SetUVs(0, newUVs);
        mesh.SetIndices(newIndices.ToArray(), MeshTopology.Triangles, 0);
    }

    private struct VertexKey
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;

        // Change this if you require a different precision.
        private const int Tolerance = 100000;

        // Magic FNV values. Do not change these.
        private const long FNV32Init = 0x811c9dc5;
        private const long FNV32Prime = 0x01000193;

        public VertexKey(Vector3 position)
        {
            _x = (long)(Mathf.Round(position.x * Tolerance));
            _y = (long)(Mathf.Round(position.y * Tolerance));
            _z = (long)(Mathf.Round(position.z * Tolerance));
        }

        public override bool Equals(object obj)
        {
            var key = (VertexKey)obj;
            return _x == key._x && _y == key._y && _z == key._z;
        }

        public override int GetHashCode()
        {
            long rv = FNV32Init;
            rv ^= _x;
            rv *= FNV32Prime;
            rv ^= _y;
            rv *= FNV32Prime;
            rv ^= _z;
            rv *= FNV32Prime;

            return rv.GetHashCode();
        }
    }

    private struct VertexEntry
    {
        public int MeshIndex;
        public int TriangleIndex;
        public int VertexIndex;

        public VertexEntry(int meshIndex, int triIndex, int vertIndex)
        {
            MeshIndex = meshIndex;
            TriangleIndex = triIndex;
            VertexIndex = vertIndex;
        }
    }
}