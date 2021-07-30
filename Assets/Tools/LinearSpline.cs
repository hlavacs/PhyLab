using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearSpline : Spline
{
    public bool Loop
    {
        get
        {
            return loop;
        }
        set
        {
            loop = value;
        }
    }

    public override Vector3 GetControlPoint(int index)
    {
        if(points.Length == 0)
        {
            return Vector3.zero;
        }

        if(index >= points.Length)
        {
            if (loop)
            {
                return points[0];
            } else
            {
                return points[points.Length - 1];
            }
        }
        return points[index];
    }

    public override Vector3 GetDirection(float t)
    {
        return transform.TransformDirection(GetDirectionLocal(t));
    }

    public override Vector3 GetDirectionLocal(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 2;
        }
        else
        {
            t = Mathf.Clamp01(t) * points.Length;
            i = (int)t;
            t -= i;
        }
        return (GetControlPoint(i + 1) - GetControlPoint(i)).normalized;
    }

    public int GetPointCount()
    {
        if(loop)
        {
            return points.Length + 1;
        }
        return points.Length;
    }

    public override Vector3 GetPoint(float t)
    {
        return transform.TransformPoint(GetPointLocal(t));
    }

    public override Vector3 GetPointLocal(float t)
    {
        int i;
        t = Mathf.Clamp01(t) * points.Length;
        i = (int)t;
        t -= i;
        return Vector3.Lerp(GetControlPoint(i), GetControlPoint(i + 1), t);
    }

    public override Vector3 GetVelocity(float t)
    {
        return transform.TransformPoint(GetVelocityLocal(t));
    }

    public override Vector3 GetVelocityLocal(float t)
    {
        return Vector3.zero;
    }

    public override void InsertVertexAt(float insertAlpha)
    {
        float t = Mathf.Clamp01(insertAlpha) * (points.Length);
        int insertIndex = (int)t;

        Vector3[] newPoints = new Vector3[points.Length + 1];
        int k = 0;
        for (int i = 0; i < points.Length; ++i)
        {
            newPoints[k++] = points[i];
            if (i == insertIndex)
            {
                newPoints[k++] = GetPointLocal(insertAlpha);
            }
        }
        points = newPoints;
    }

    public override void RemovePoint(int point)
    {
        if(points.Length == 0)
        {
            return;
        }
        Vector3[] newPoints = new Vector3[points.Length - 1];
        int k = 0;
        for(int i = 0; i < points.Length; ++i)
        {
            if(i != point) { 
                newPoints[k++] = points[i];
            }
        }
        points = newPoints;
    }

    public override void RemoveVertex(int deleteIndex)
    {
        RemovePoint(deleteIndex);
    }

    public override void SetControlPoint(int index, Vector3 point)
    {
        if (index < 0 || index >= points.Length)
        {
            return;
        }

        points[index] = point;
    }
}
