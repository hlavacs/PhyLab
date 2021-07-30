using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public struct BezierPoint {
    public Vector3 Point { get; set; }
    public Vector3 InPoint { get; set; }
    public Vector3 OutPoint { get; set; }
}


[System.Serializable]
public struct BezierPointCoordinateSystem {
    public Vector3 Point { get; set; }
    public Vector3 Forward { get; set; }
    public Vector3 Right { get; set; }
    public Vector3 Up { get; set; }
}

[System.Serializable]
public struct LinearPoint {
    public Vector3 Point { get; set; }
    public float Distance { get; set; }
}


[System.Serializable]
public class SimpleBezier {
    public List<BezierPoint> Points = new List<BezierPoint>();
    public List<float> PointDistances = new List<float>();
    
    public List<LinearPoint> LinearPoints = new List<LinearPoint>();

    public float LengthInMeters;

    public void AddPoint(Vector3 center, Vector3 inPoint, Vector3 outPoint) {
        BezierPoint point = new BezierPoint();
        point.Point = center;
        point.InPoint = inPoint;
        point.OutPoint = outPoint;
        Points.Add(point);
    }

    int GetIndexAfter(float distance) {
        for(int i = 0; i < PointDistances.Count; ++i) {
            if(PointDistances[i] > distance) {
                return i;
            }
        }
        return 0;
    }

    public void Update() {
        Vector3 lastPoint = Vector3.zero;
        PointDistances.Clear();
        LinearPoints.Clear();
        float overallLength = 0;
        for(int i = 1; i <= Points.Count; ++i) {
            BezierPoint prev = Points[i - 1];
            BezierPoint next = Points[i % Points.Count];
            PointDistances.Add(overallLength);

            int interpolationCount = 100;
            for(int k = 0; k < interpolationCount; ++k) {
                float t = (float)k / (float)interpolationCount;
                Vector3 point = Bezier.GetPoint(prev.Point, prev.OutPoint, next.InPoint, next.Point, t);
                if(!(i == 1 && k == 0)) {
                    float length = (lastPoint - point).magnitude;
                    overallLength += length;
                }

                if(k % 10 == 0) {
                    LinearPoint linearPoint = new LinearPoint();
                    linearPoint.Distance = overallLength;
                    linearPoint.Point = point;
                    LinearPoints.Add(linearPoint);
                }

                lastPoint = point;
            }
        }
        LengthInMeters = overallLength;
    }

    public void Clear() {
        Points.Clear();
        PointDistances.Clear();
    }

    float GetLoopedDistance(float distance) {
        if(LengthInMeters <= 0) {
            Update();
            if(LengthInMeters <= 0) {
                Debug.LogWarning("Bezier spline position at distance " + distance + " couldn't be found, because length is 0!");
                return 0;
            }
        }
        while(distance < 0) {
            distance += LengthInMeters;
        }
        return distance % LengthInMeters;
    }

    public Vector3 GetAtDistance(float distanceMeters) {
        distanceMeters = GetLoopedDistance(distanceMeters);
        int pointIndexAfter = GetIndexAfter(distanceMeters);
        int pointIndexBefore = pointIndexAfter - 1;
        if(pointIndexBefore < 0) {
            pointIndexBefore = Points.Count - 1;
        }

        BezierPoint prev = Points[pointIndexBefore];
        BezierPoint next = Points[pointIndexAfter];
        Vector3 lastPoint = prev.Point;

        float overallLength = PointDistances[pointIndexBefore];
        int interpolationCount = 50;
        float delta = 1.0f / (float)interpolationCount;
        float t = delta;
        for(int k = 0; k < interpolationCount; ++k) {
            Vector3 point = Bezier.GetPoint(prev.Point, prev.OutPoint, next.InPoint, next.Point, t);
            Vector3 dir = (lastPoint - point);
            float length = dir.magnitude;
            overallLength += length;

            if(overallLength >= distanceMeters) {
                float alpha = (overallLength - distanceMeters) / length;
                return point + dir * alpha;
            }
            lastPoint = point;
            t += delta;
        }
        return lastPoint;
    }

    public BezierPointCoordinateSystem GetCoordinateSystemAtDistance(float distanceMeters, Vector3 up) {
        BezierPointCoordinateSystem result = new BezierPointCoordinateSystem();
        result.Point = Vector3.zero;
        result.Forward = Vector3.forward;
        result.Up = up;
        result.Right = Vector3.right;

        distanceMeters = GetLoopedDistance(distanceMeters);

        Vector3 lastPoint = Vector3.zero;
        float overallLength = 0;
        float lastOverallLength = 0;
        for(int i = 1; i <= Points.Count; ++i) {
            BezierPoint prev = Points[i - 1];
            BezierPoint next = Points[i % Points.Count];

            int interpolationCount = 100;
            for(int k = 0; k < interpolationCount; ++k) {
                float t = (float)k / (float)interpolationCount;
                Vector3 point = Bezier.GetPoint(prev.Point, prev.OutPoint, next.InPoint, next.Point, t);
                if(!(i == 1 && k == 0)) {
                    float length = (lastPoint - point).magnitude;
                    overallLength += length;

                    if(overallLength >= distanceMeters) {
                        float alpha = (distanceMeters - lastOverallLength) / (overallLength - lastOverallLength);
                        result.Point = Vector3.Lerp(lastPoint, point, alpha);
                        result.Forward = (point - lastPoint).normalized;
                        result.Right = Vector3.Cross(result.Forward, up).normalized;
                        return result;
                    }
                }
                lastPoint = point;
                lastOverallLength = overallLength;
            }
        }
        Debug.LogWarning("Bezier spline position at distance " + distanceMeters + "(" + overallLength + ") couldn't be found!");
        return result;
    }

    public Vector3 GetNormalAtDistance(float distanceMeters, Vector3 up) {
        distanceMeters = GetLoopedDistance(distanceMeters);

        Vector3 lastPoint = Vector3.zero;
        float overallLength = 0;
        for(int i = 1; i <= Points.Count; ++i) {
            BezierPoint prev = Points[i - 1];
            BezierPoint next = Points[i % Points.Count];

            int interpolationCount = 100;
            for(int k = 0; k < interpolationCount; ++k) {
                float t = (float)k / (float)interpolationCount;
                Vector3 point = Bezier.GetPoint(prev.Point, prev.OutPoint, next.InPoint, next.Point, t);
                if(!(i == 1 && k == 0)) {
                    float length = (lastPoint - point).magnitude;
                    overallLength += length;
                }
                lastPoint = point;

                if(overallLength >= distanceMeters) {
                    return Vector3.Cross((point - lastPoint).normalized, up).normalized;
                }
            }
        }
        Debug.LogWarning("Bezier spline position at distance " + distanceMeters + " couldn't be found!");
        return Vector3.zero;
    }

    public Vector3 GetAtAlpha(float alpha) {
        return GetAtDistance(alpha * LengthInMeters);
    }

    public Vector3 GetNearestPoint(Vector3 position) {
        float minDistance = float.MaxValue;
        Vector3 minimalPoint = Vector3.zero;

        for(int i = 1; i <= Points.Count; ++i) {
            BezierPoint prev = Points[i - 1];
            BezierPoint next = Points[i % Points.Count];

            int interpolationCount = 100;
            for(int k = 0; k < interpolationCount; ++k) {
                float t = (float)k / (float)interpolationCount;
                Vector3 point = Bezier.GetPoint(prev.Point, prev.OutPoint, next.InPoint, next.Point, t);
                float dist = (point - position).sqrMagnitude;
                if(dist < minDistance) {
                    minDistance = dist;
                    minimalPoint = point;
                }
            }
        }
        return minimalPoint;
    }

    public float GetNearestDistance(Vector3 position) {
        float minDistance = float.MaxValue;
        float minimalDistance = 0;

        #if false
        Vector3 lastPoint = Vector3.zero;
        float overallLength = 0;
        for(int i = 1; i <= Points.Count; ++i) {
            BezierPoint prev = Points[i - 1];
            BezierPoint next = Points[i % Points.Count];

            int interpolationCount = 30;
            for(int k = 0; k < interpolationCount; ++k) {
                float t = (float)k / (float)interpolationCount;
                Vector3 point = Bezier.GetPoint(prev.Point, prev.OutPoint, next.InPoint, next.Point, t);
                if(!(i == 1 && k == 0)) {
                    float length = (lastPoint - point).magnitude;
                    overallLength += length;
                }
                lastPoint = point;
                
                float dist = (point - position).sqrMagnitude;
                if(dist < minDistance) {
                    minDistance = dist;
                    minimalDistance = overallLength;
                }
            }
        }
        #else
        int pointCount = LinearPoints.Count;
        if( pointCount < 1) {
            return 0.0f;
        }

        for(int i = 0; i < pointCount; ++i) {
            LinearPoint point = LinearPoints[i];
            float dist = (point.Point - position).sqrMagnitude;
            if(dist < minDistance) {
                minDistance = dist;
                minimalDistance = point.Distance;
            }
        }
        #endif

        return minimalDistance;
    }
}