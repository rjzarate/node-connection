using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : MaskableGraphic
{
    public List<Vector2> points;

    public float thickness = 10f;
    public bool center = true;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Count < 2)
            return;

        for (int i = 0; i < points.Count - 1; i++)
        {
            // Create a line segment between the next two points
            CreateLineSegment(points[i], points[i + 1], vh);

            int index = i * 5;

            // Add the line segment to the triangles array
            vh.AddTriangle(index, index + 1, index + 3);
            vh.AddTriangle(index + 3, index + 2, index);

            // These two triangles create the beveled edges
            // between line segments using the end point of
            // the last line segment and the start points of this one
            if (i != 0)
            {
                vh.AddTriangle(index, index - 1, index - 3);
                vh.AddTriangle(index + 1, index - 1, index - 2);
            }
        }
    }

    /// <summary>
    /// Creates a rect from two points that acts as a line segment
    /// </summary>
    /// <param name="point1">The starting point of the segment</param>
    /// <param name="point2">The endint point of the segment</param>
    /// <param name="vh">The vertex helper that the segment is added to</param>
    private void CreateLineSegment(Vector3 point1, Vector3 point2, VertexHelper vh)
    {
        Vector3 offset = center ? (rectTransform.sizeDelta / 2) : Vector2.zero;

        // Create vertex template
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        // Create the start of the segment
        Quaternion point1Rotation = Quaternion.Euler(0, 0, RotatePointTowards(point1, point2) + 90);
        vertex.position = point1Rotation * new Vector3(-thickness / 2, 0);
        vertex.position += point1 - offset;
        vh.AddVert(vertex);
        vertex.position = point1Rotation * new Vector3(thickness / 2, 0);
        vertex.position += point1 - offset;
        vh.AddVert(vertex);

        // Create the end of the segment
        Quaternion point2Rotation = Quaternion.Euler(0, 0, RotatePointTowards(point2, point1) - 90);
        vertex.position = point2Rotation * new Vector3(-thickness / 2, 0);
        vertex.position += point2 - offset;
        vh.AddVert(vertex);
        vertex.position = point2Rotation * new Vector3(thickness / 2, 0);
        vertex.position += point2 - offset;
        vh.AddVert(vertex);

        // Also add the end point
        vertex.position = point2 - offset;
        vh.AddVert(vertex);
    }

    /// <summary>
    /// Gets the angle that a vertex needs to rotate to face target vertex
    /// </summary>
    /// <param name="vertex">The vertex being rotated</param>
    /// <param name="target">The vertex to rotate towards</param>
    /// <returns>The angle required to rotate vertex towards target</returns>
    private float RotatePointTowards(Vector2 vertex, Vector2 target)
    {
        return (float)(Mathf.Atan2(target.y - vertex.y, target.x - vertex.x) * (180 / Mathf.PI));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UILineRenderer))]
public class UILineRendererEditor : Editor
{
    private UILineRenderer lineRenderer;

    private void OnEnable()
    {
        lineRenderer = (UILineRenderer)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw default inspector

        lineRenderer = (UILineRenderer)target;

        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(lineRenderer, "Add Point");
            if (lineRenderer.points == null)
            {
                lineRenderer.points = new List<Vector2> { Vector2.zero };
            }
            else
            {
                lineRenderer.points.Add(Vector2.zero);
            }
            EditorUtility.SetDirty(lineRenderer);
            if (lineRenderer != null)
                lineRenderer.SetVerticesDirty();
        }

        if (lineRenderer.points != null && lineRenderer.points.Count > 0)
        {
            if (GUILayout.Button("Remove Last Point"))
            {
                lineRenderer.points.RemoveAt(lineRenderer.points.Count - 1);
            }
        }
    }

    private void OnSceneGUI()
    {
        if (lineRenderer == null)
        {
            lineRenderer = (UILineRenderer)target;
        }

        if (lineRenderer.points == null || lineRenderer.points.Count == 0)
            return;

        RectTransform rt = lineRenderer.rectTransform;
        Vector3 offset = lineRenderer.center ? (Vector3)rt.sizeDelta / 2f : Vector3.zero;
        Transform tr = lineRenderer.transform;

        Handles.color = Color.yellow;
        for (int i = 0; i < lineRenderer.points.Count; i++)
        {
            Vector3 localPointWithoutOffset = lineRenderer.points[i];
            Vector3 worldPos = tr.TransformPoint(localPointWithoutOffset - offset);

            float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.1f;

            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.FreeMoveHandle(
                worldPos,
                handleSize,
                Vector3.zero,
                Handles.SphereHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(lineRenderer, "Move Line Point");
                Vector2 newLocalPos =
                    (Vector2)tr.InverseTransformPoint(newWorldPos) + (Vector2)offset;
                lineRenderer.points[i] = newLocalPos;
                EditorUtility.SetDirty(lineRenderer);
                if (lineRenderer != null)
                    lineRenderer.SetVerticesDirty();
            }
        }
    }
}
#endif
