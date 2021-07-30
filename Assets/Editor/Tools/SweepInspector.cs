using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Sweep))]
public class SweepInspector : Editor {
    struct SegmentFrame
    {
        public Vector3 Position;
        public Vector3 Forward;
        public Vector3 Up;
        public Vector3 Right;
        public Vector3 UpScaled;
        public Vector3 RightScaled;
        public float Length;
    }

    public override void OnInspectorGUI() {
        Sweep sweep = target as Sweep;


        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (sweep.Path && EditorGUI.EndChangeCheck()) {
            GenerateMesh(sweep);
        }

        if (sweep.Path && GUILayout.Button("Update")) {
            GenerateMesh(sweep);
        }
    }


    public static void GenerateMesh(Sweep t) {
        if(!t.Path) {
            return;
        }

        List<Vector3> Vertices = new List<Vector3>();
        List<Vector3> Normals = new List<Vector3>();
        List<Vector2> UV = new List<Vector2>();
        List<int> Indices = new List<int>();

        float minAngle = Mathf.Max(t.MinAngle, 1.0f);
        float absLength = 0;
        Vector3 lastPos = t.Path.GetPointLocal(0.0f);
        Vector3 lastDir = t.Path.GetDirectionLocal(0.0f);
        Vector3 lastUsedPos = lastPos;
        SegmentFrame capStartFrame = new SegmentFrame();
        SegmentFrame capEndFrame = new SegmentFrame();

        int crossPointCount = 0;
        int pointCount = 10000;
        for (int i = 0; i <= pointCount; ++i) {
            float alpha = (float)i / (float)pointCount;
            Vector3 pos = t.Path.GetPointLocal(alpha);
            Vector3 dir = t.Path.GetDirectionLocal(alpha);
            float angle = Vector3.Angle(dir, lastDir);

            float dist = (pos - lastPos).magnitude;
            absLength += dist;

            if(i == 0 || i == pointCount || angle >= minAngle)
            {
                Vector3 centerDir = (lastDir + dir).normalized;
                Vector3 right;
                Vector3 up;
                if (Mathf.Abs(centerDir.z) > 0.99f)
                {
                    //right = -Vector3.Cross(centerDir, Vector3.up).normalized;
                    //up = -Vector3.Cross(right, centerDir).normalized;
                    up = Vector3.up; //Vector3.Cross(-Vector3.right, centerDir).normalized;
                    right = Vector3.Cross(up, centerDir).normalized;
                }
                else
                {
                    up = Vector3.Cross(centerDir, Vector3.forward).normalized;
                    right = Vector3.Cross(up, centerDir).normalized;
                }

                Vector3 centerDirUp = (centerDir - Vector3.Dot(centerDir, up) * up).normalized;
                Vector3 centerDirRight = (centerDir - Vector3.Dot(centerDir, right) * right).normalized;

                Vector3 dirUp = (dir - Vector3.Dot(dir, up) * up).normalized;
                Vector3 dirRight = (dir - Vector3.Dot(dir, right) * right).normalized;

                float upAngle = Vector3.Dot(centerDirUp, dirUp);
                float rightAngle =  Vector3.Dot(centerDirRight, dirRight);
                SegmentFrame frame;
                frame.Position = pos;
                frame.Forward = centerDir;
                frame.Right = right;
                frame.Up = up;
                frame.RightScaled = right * Mathf.Lerp(1.414213f * 2.0f, 1.0f, Mathf.Abs(upAngle)); 
                frame.UpScaled = up * Mathf.Lerp(1.414213f * 2.0f, 1.0f, Mathf.Abs(rightAngle)); 
                frame.Length = absLength;

                if(i == 0)
                {
                    capStartFrame = frame;
                }
                if (i == pointCount)
                {
                    capEndFrame = frame;
                }

                if (t.CrossSectionType == Sweep.CrossSectionTypes.Circle) {
                    crossPointCount = AddSegmentCircle(frame, t, Vertices, UV, Normals, Indices);
                } else if (t.CrossSectionType == Sweep.CrossSectionTypes.Rectangle) {
                    crossPointCount = AddSegmentRectangle(frame, t, Vertices, UV, Normals, Indices);
                } else if (t.CrossSectionType == Sweep.CrossSectionTypes.Custom) {
                    crossPointCount = AddSegmentPath(frame, t, Vertices, UV, Normals, Indices);
                }
                if(i != 0) {
                    lastDir = dir; // (pos - lastUsedPos).normalized;
                    lastUsedPos = pos;
                }
            }
            lastPos = pos;
        }
        Vector3 capEndDir = lastDir;

        if (t.CapStart)
        {
            AddCap(0, crossPointCount, true, capStartFrame, t, Vertices, UV, Normals, Indices);
        }

        if (t.CapEnd)
        {
            int offset = crossPointCount;
            if(t.CapStart)
            {
                if(t.CrossSectionType == Sweep.CrossSectionTypes.Rectangle)
                {
                    offset += 4;
                } else
                {
                    offset += crossPointCount;
                }
            }
            AddCap(Vertices.Count - offset, crossPointCount, false, capEndFrame, t, Vertices, UV, Normals, Indices);
        }

        MeshFilter filter = t.GetComponent<MeshFilter>();
        MeshRenderer renderer = t.GetComponent<MeshRenderer>();

        if (!filter || !renderer) return;

        Mesh mesh;
        string goID = t.gameObject.GetInstanceID().ToString();
        string meshAssetPath = "Assets/Generated/" + goID + "_sweep.asset";
        bool alreadyExists = false;

        if (filter.sharedMesh) {
            string existingPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
            if (existingPath == meshAssetPath) {
                alreadyExists = true;
                mesh = filter.sharedMesh;
            } else {
                mesh = new Mesh();
            }
        } else {
            mesh = new Mesh();
        }

        mesh.triangles = null;
        mesh.SetVertices(Vertices);
        mesh.SetUVs(0, UV);
        if (Normals.Count == Vertices.Count) {
            mesh.SetNormals(Normals);
        }

        mesh.SetIndices(Indices.ToArray(), MeshTopology.Triangles, 0, true, 0);
        if(Normals.Count != Vertices.Count) {
            //mesh.RecalculateNormals();
            mesh.RecalculateNormals(60.0f);
        }
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        
        MeshUtility.Optimize(mesh);
        Unwrapping.GenerateSecondaryUVSet(mesh);

        filter.sharedMesh = mesh;

        if (alreadyExists) {
            AssetDatabase.SaveAssets();
        } else {
            AssetDatabase.CreateAsset(mesh, meshAssetPath);
        }
    }

    static void AddCap(int startPoint, int pointCount, bool start, SegmentFrame frame, Sweep sweep, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> indices)
    {
        Vector3 normalDir = start ? -frame.Forward : frame.Forward;

        int indexOffset = vertices.Count;
        Vector2[] projectedPoints = new Vector2[pointCount];
        if (sweep.CrossSectionType == Sweep.CrossSectionTypes.Rectangle)
        {
            // Rectangle is special because it has 2 vertices per cross section point because of hard normal edges.
            vertices.Add(vertices[startPoint + 0]);
            vertices.Add(vertices[startPoint + 2]);
            vertices.Add(vertices[startPoint + 4]);
            vertices.Add(vertices[startPoint + 6]);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));

            normals.Add(normalDir);
            normals.Add(normalDir);
            normals.Add(normalDir);
            normals.Add(normalDir);

            if (start)
            {
                indices.Add(indexOffset + 0);
                indices.Add(indexOffset + 1);
                indices.Add(indexOffset + 2);

                indices.Add(indexOffset + 0);
                indices.Add(indexOffset + 2);
                indices.Add(indexOffset + 3);
            }
            else
            {
                indices.Add(indexOffset + 0);
                indices.Add(indexOffset + 2);
                indices.Add(indexOffset + 1);

                indices.Add(indexOffset + 0);
                indices.Add(indexOffset + 3);
                indices.Add(indexOffset + 2);
            }
        }
        else
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            int k = 0;
            for (int i = startPoint; i < startPoint + pointCount; ++i)
            {
                projectedPoints[k].x = Vector3.Dot(vertices[i], frame.Right);
                projectedPoints[k].y = Vector3.Dot(vertices[i], frame.Up);

                min.x = Mathf.Min(min.x, projectedPoints[k].x);
                min.y = Mathf.Min(min.y, projectedPoints[k].y);
                max.x = Mathf.Max(max.x, projectedPoints[k].x);
                max.y = Mathf.Max(max.y, projectedPoints[k].y);

                ++k;
            }

            Vector2 uvBounds = new Vector2(1.0f, 1.0f) / (max - min);

            k = 0;
            for (int i = startPoint; i < startPoint + pointCount; ++i)
            {
                vertices.Add(vertices[i]);
                uvs.Add((projectedPoints[k] - min) * uvBounds);
                ++k;
            }

            Triangulator tri = new Triangulator(projectedPoints);
            int[] newIndices = tri.Triangulate();

            //Note(daniel): Check winding order of generated triangles, sometimes they seem to be flipped so we unflip them.
            if(newIndices.Length >= 3) {
                if (start && sweep.FlipCapStart || !start && sweep.FlipCapEnd)
                {
                    for (int i = 0; i < newIndices.Length; ++i)
                    {
                        indices.Add(newIndices[newIndices.Length - i - 1] + indexOffset);
                    }
                }
                else
                {
                    for (int i = 0; i < newIndices.Length; ++i)
                    {
                        indices.Add(newIndices[i] + indexOffset);
                    }
                }
            }
        }

    }

    static int AddSegmentCircle(SegmentFrame frame, Sweep sweep, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> indices) {
        bool isStart = (vertices.Count == 0);
        int points = (int)(360.0f / sweep.MinAngleCrossSection);
       
        float twoPi = Mathf.PI * 2.0f;
        float angle = 0;
        float angleDelta = twoPi / (float)(points - 1);
        for (int i = 0; i < points; i++) {
            Vector3 vertP = frame.Position + (Mathf.Cos(angle) * sweep.Radius) * frame.UpScaled + (Mathf.Sin(angle) * sweep.Radius) * frame.RightScaled;
            vertices.Add(vertP);
            Vector2 uv = new Vector2(angle / twoPi, frame.Length / (sweep.Radius * twoPi));
            if(sweep.SwapUV)
            {
                uv = new Vector2(uv.y, uv.x);
            }
            uvs.Add(uv);
            angle += angleDelta;
        }

        if (!isStart) {
            int indexStart = vertices.Count - points * 2;
            for (int i = 0; i < points; i++) {
                int p0 = indexStart + i;
                int p1 = indexStart + ((i + 1) % points);
                int p2 = p0 + points;
                int p3 = p1 + points;

                indices.Add(p1);
                indices.Add(p0);
                indices.Add(p2);

                indices.Add(p1);
                indices.Add(p2);
                indices.Add(p3);
            }
        }
        return points;
    }


    static int AddSegmentRectangle(SegmentFrame frame, Sweep sweep, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> indices) {
        bool isStart = (vertices.Count == 0);

        vertices.Add(frame.Position - frame.UpScaled * sweep.Radius - frame.RightScaled * sweep.Radius2);
        vertices.Add(frame.Position + frame.UpScaled * sweep.Radius - frame.RightScaled * sweep.Radius2);
        vertices.Add(frame.Position + frame.UpScaled * sweep.Radius - frame.RightScaled * sweep.Radius2);
        vertices.Add(frame.Position + frame.UpScaled * sweep.Radius + frame.RightScaled * sweep.Radius2);
        vertices.Add(frame.Position + frame.UpScaled * sweep.Radius + frame.RightScaled * sweep.Radius2);
        vertices.Add(frame.Position - frame.UpScaled * sweep.Radius + frame.RightScaled * sweep.Radius2);
        vertices.Add(frame.Position - frame.UpScaled * sweep.Radius + frame.RightScaled * sweep.Radius2);
        vertices.Add(frame.Position - frame.UpScaled * sweep.Radius - frame.RightScaled * sweep.Radius2);

        normals.Add(-frame.Right);
        normals.Add(-frame.Right);
        normals.Add(frame.Up);
        normals.Add(frame.Up);
        normals.Add(frame.Right);
        normals.Add(frame.Right);
        normals.Add(-frame.Up);
        normals.Add(-frame.Up);

        float circumference = sweep.Radius * 2 + sweep.Radius2 * 2;

        float uvY = frame.Length / circumference;
        
        if (sweep.SwapUV)
        {
            uvs.Add(new Vector2(uvY, 0));
            uvs.Add(new Vector2(uvY, sweep.Radius / circumference));
            uvs.Add(new Vector2(uvY, sweep.Radius / circumference));
            uvs.Add(new Vector2(uvY, (sweep.Radius + sweep.Radius2) / circumference));
            uvs.Add(new Vector2(uvY, (sweep.Radius + sweep.Radius2) / circumference));
            uvs.Add(new Vector2(uvY, (sweep.Radius + sweep.Radius2 + sweep.Radius) / circumference));
            uvs.Add(new Vector2(uvY, (sweep.Radius + sweep.Radius2 + sweep.Radius) / circumference));
            uvs.Add(new Vector2(uvY, 0));
        }
        else
        {
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(sweep.Radius / circumference, uvY));
            uvs.Add(new Vector2(sweep.Radius / circumference, uvY));
            uvs.Add(new Vector2((sweep.Radius + sweep.Radius2) / circumference, uvY));
            uvs.Add(new Vector2((sweep.Radius + sweep.Radius2) / circumference, uvY));
            uvs.Add(new Vector2((sweep.Radius + sweep.Radius2 + sweep.Radius) / circumference, uvY));
            uvs.Add(new Vector2((sweep.Radius + sweep.Radius2 + sweep.Radius) / circumference, uvY));
            uvs.Add(new Vector2(0, uvY));
        }

        int points = 8;
        if (!isStart) {
            int indexStart = vertices.Count - points * 2;
            for (int i = 0; i < points; i++) {
                int p0 = indexStart + i;
                int p1 = indexStart + ((i + 1) % points);
                int p2 = p0 + points;
                int p3 = p1 + points;

                indices.Add(p1);
                indices.Add(p0);
                indices.Add(p2);

                indices.Add(p1);
                indices.Add(p2);
                indices.Add(p3);
            }
        }

        return points;
    }

    static int AddSegmentPath(SegmentFrame frame, Sweep sweep, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> indices)
    {
        bool isStart = (vertices.Count == 0);
        Spline path = sweep.CrossSection;


        int pCount = 0;
        float minAngle = Mathf.Max(sweep.MinAngleCrossSection, 1.0f);
        Vector3 lastDir = path.GetDirectionLocal(0.0f);
        Vector3 lastUsedPos = path.GetPointLocal(0.0f);
        int pointCount = 1000;
        for (int i = 0; i <= pointCount; ++i)
        {
            float alpha = (float)i / (float)pointCount;
            Vector3 p = path.GetPointLocal(alpha);
            Vector3 dir = path.GetDirectionLocal(alpha);
            float angle = Vector3.Angle(dir, lastDir);

            if (i == 0 || i == pointCount || angle >= minAngle)
            {
                Vector3 vertP = frame.Position + frame.UpScaled * p.x * sweep.Radius + frame.RightScaled * p.z * sweep.Radius2;
                vertices.Add(vertP);
#if true
                Vector3 vertN = Vector3.Cross(frame.Forward, -dir).normalized;
                vertN = (frame.UpScaled * vertN.x * sweep.Radius + frame.RightScaled * vertN.z * sweep.Radius2).normalized;
#else
                Vector3 vertN = (frame.UpScaled * p.x * sweep.Radius + frame.RightScaled * p.z * sweep.Radius2).normalized;
#endif
                if (sweep.SwapNormals)
                {
                    //normals.Add(-vertN);
                }
                else
                {
                   // normals.Add(vertN);
                }

                if (sweep.SwapUV)
                {
                    uvs.Add(new Vector2(sweep.UVOffsetX + frame.Length * sweep.UVScaleX, sweep.UVOffsetY + alpha * sweep.UVScaleY));
                }
                else
                {
                    uvs.Add(new Vector2(sweep.UVOffsetY + alpha * sweep.UVScaleY, sweep.UVOffsetX + frame.Length * sweep.UVScaleX));
                }

                ++pCount;
                if (i != 0)
                {
                    lastDir = (p - lastUsedPos).normalized;
                    lastUsedPos = p;
                }
            }
        }


        if (!isStart)
        {
            int indexStart = vertices.Count - pCount * 2;
            for (int i = 0; i < pCount; i++)
            {
                int p0 = indexStart + i;
                int p1 = indexStart + ((i + 1) % pCount);
                int p2 = p0 + pCount;
                int p3 = p1 + pCount;

                if(sweep.SwapNormals)
                {
                    indices.Add(p0);
                    indices.Add(p1);
                    indices.Add(p2);

                    indices.Add(p2);
                    indices.Add(p1);
                    indices.Add(p3);
                }
                else
                {
                    indices.Add(p1);
                    indices.Add(p0);
                    indices.Add(p2);

                    indices.Add(p1);
                    indices.Add(p2);
                    indices.Add(p3);
                }
            }
        }
        return pCount;
    }

}
