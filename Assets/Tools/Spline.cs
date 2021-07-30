using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spline : MonoBehaviour
{
    public int ControlPointCount { get; private set; }

    [SerializeField]
    protected Vector3[] points = new Vector3[0];

    [SerializeField]
    protected bool loop;

    public void Clear(){
        points = new Vector3[0];
    }

    public float GetApproximateLength()
    {
        float length = 0.0f;
        for(int i = 0; i < points.Length - 2; ++i)
        {
            length += (points[i + 1] - points[i]).magnitude;
        }
        return length;
    }
    public abstract Vector3 GetControlPoint(int index);
    public abstract Vector3 GetDirection(float t);
    public abstract Vector3 GetDirectionLocal(float t);
    public abstract Vector3 GetPoint(float t);
    public abstract Vector3 GetPointLocal(float t);
    public abstract Vector3 GetVelocity(float t);
    public abstract Vector3 GetVelocityLocal(float t);
    public abstract void InsertVertexAt(float insertAlpha);
    public abstract void RemovePoint(int point);
    public abstract void RemoveVertex(int deleteIndex);
    public abstract void SetControlPoint(int index, Vector3 point);
}
