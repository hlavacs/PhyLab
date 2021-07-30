using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LinearSpline))]
public class LinearSplineInspector : Editor {
    
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private static Color[] modeColors = {
		Color.white,
		Color.yellow,
		Color.cyan
	};

	private LinearSpline spline;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private int selectedIndex = -1;


    Vector2 scrollPos;

    public override void OnInspectorGUI () {
		spline = target as LinearSpline;
		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Toggle Loop");
			EditorUtility.SetDirty(spline);
			spline.Loop = loop;
		}
		if (selectedIndex >= 0 && selectedIndex < spline.GetPointCount()) {
            DrawSelectedPointInspector();
		}

        SerializedProperty tps = serializedObject.FindProperty("points");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps, true);
        if (EditorGUI.EndChangeCheck()) { 
            serializedObject.ApplyModifiedProperties();
        }

    }

	private void DrawSelectedPointInspector() {
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(selectedIndex, point);
		}
	}

    public static float DistanceToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    bool control = false;
    //bool shift = false; Unused
    float insertAlpha = 0;
    Vector3 insertPos = new Vector3();
    int deleteIndex = -1;

    private void OnSceneGUI () {
		spline = target as LinearSpline;
		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

        Event e = Event.current;
        int defaultControlID = GUIUtility.GetControlID(FocusType.Passive);
        switch (e.type) {
            case EventType.Layout:
                HandleUtility.AddDefaultControl(defaultControlID);
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.Escape) {
                    // Clear selection.
                    Selection.objects = new UnityEngine.Object[0];
                }
                break;
            case EventType.ValidateCommand:
                if (e.commandName == "SelectAll" || e.commandName == "SoftDelete") {
                    e.Use();
                }
                break;
            case EventType.ExecuteCommand:
                Debug.Log("Command: " + e.commandName);
                if (e.commandName == "SelectAll") {
                    // Select all.
                    e.Use();
                } else if (e.commandName == "SoftDelete") {
                    // Delete selected vertex.
                    e.Use();
                }
                break;
        }

        Rect viewSize = SceneView.currentDrawingSceneView.camera.pixelRect;
        if (e.mousePosition.x > 0 && e.mousePosition.y > 0 &&
            e.mousePosition.x < viewSize.width &&
            e.mousePosition.y < viewSize.height) {

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (e.shift) {
                // Shift stuff.
                float minDist = float.MaxValue;
                for (float alpha = 0.001f; alpha <= 1.0f; alpha += 0.001f) {
                    Vector3 next = spline.GetPoint(alpha);
                    float dist = DistanceToLine(mouseRay, next);
                    if (dist < minDist) {
                        insertPos = next;
                        insertAlpha = alpha;
                        minDist = dist;
                    }
                }

                Handles.Label(insertPos + Vector3.up * 0.4f, "Alpha: " + insertAlpha.ToString());
                Handles.color = Color.green;
                Handles.CubeCap(0, insertPos, Quaternion.identity, 0.1f);
                SceneView.RepaintAll();
            } else if(e.control) {
                float minDist = float.MaxValue;
                Vector3 deletePos = new Vector3();
                for (int i = 0; i < spline.GetPointCount(); i++) {
                    Vector3 pos = spline.transform.TransformPoint(spline.GetControlPoint(i));

                    float dist = DistanceToLine(mouseRay, pos);
                    if (dist < minDist) {
                        //insertIndex = 0;
                        deletePos = pos;
                        deleteIndex = i;
                        minDist = dist;
                    }
                }

                if(spline.GetPointCount() > 2) { 
                    Handles.color = Color.red;
                    Handles.CubeCap(0, deletePos, Quaternion.identity, 0.1f);
                }
                SceneView.RepaintAll();
            }

            switch (e.GetTypeForControl(defaultControlID)) {
                case EventType.MouseDown:
                    if (e.shift && e.button == 0) {
                        // Add new vertex.
                        selectedIndex = -1;
                        Undo.RecordObject(spline, "Added Point");
                        EditorUtility.SetDirty(spline);
                        spline.InsertVertexAt(insertAlpha);
                        UpdateDependentScripts();
                    } else if (e.control && e.button == 0) {
                        // Remove vertex.
                        if(spline.GetPointCount() > 2) { 
                            selectedIndex = -1;
                            Undo.RecordObject(spline, "Removed Point");
                            EditorUtility.SetDirty(spline);
                            spline.RemoveVertex(deleteIndex);
                            UpdateDependentScripts();
                        }
                    }
                    break;
            }
        }


        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < spline.GetPointCount(); i++) {
			Vector3 p1 = ShowPoint(i);
            Handles.color = Color.white;
            Handles.DrawLine(p0, p1);
			p0 = p1;
		}

        if(spline.Loop)
        {
            Handles.color = Color.white;
            Handles.DrawLine(handleTransform.TransformPoint(spline.GetControlPoint(0)), handleTransform.TransformPoint(spline.GetControlPoint(spline.GetPointCount() - 1)));
        }
    }


	private Vector3 ShowPoint (int index) {
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0) {
			size *= 2f;
		}
		Handles.color = Color.white;
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
            if(control) {
                spline.RemovePoint(index);
            } else {
                selectedIndex = index;
            }
			Repaint();
		}
		if (selectedIndex == index) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
                UpdateDependentScripts();
            }
		}
		return point;
	}

    private void UpdateDependentScripts()
    {
        Sweep[] sweeps = FindObjectsOfType<Sweep>();
        foreach (var sweep in sweeps)
        {
            if (sweep.Path == spline || sweep.CrossSection == spline)
            {
                SweepInspector.GenerateMesh(sweep);
            }
        }
    }
}