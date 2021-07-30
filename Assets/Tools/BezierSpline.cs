using UnityEngine;
using System;
using System.Collections.Generic;

public enum BezierControlPointMode
{
    Free,
    Aligned,
    Mirrored
}

public class BezierSpline : Spline
{
    [SerializeField]
    private BezierControlPointMode[] modes;

	public bool Loop {
		get {
			return loop;
		}
		set {
			loop = value;
			if (value == true) {
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}

	public int ControlPointCount {
		get {
			return points.Length;
		}
	}

	public override Vector3 GetControlPoint (int index) {
		return points[index];
	}

	public override void SetControlPoint (int index, Vector3 point) {
		if (index % 3 == 0) {
			Vector3 delta = point - points[index];
			if (loop) {
				if (index == 0) {
					points[1] += delta;
					points[points.Length - 2] += delta;
					points[points.Length - 1] = point;
				}
				else if (index == points.Length - 1) {
					points[0] = point;
					points[1] += delta;
					points[index - 1] += delta;
				}
				else {
					points[index - 1] += delta;
					points[index + 1] += delta;
				}
			}
			else {
				if (index > 0) {
					points[index - 1] += delta;
				}
				if (index + 1 < points.Length) {
					points[index + 1] += delta;
				}
			}
		}
		points[index] = point;
		EnforceMode(index);
	}

	public BezierControlPointMode GetControlPointMode (int index) {
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop) {
			if (modeIndex == 0) {
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1) {
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}

	private void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1)) {
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) {
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0) {
				enforcedIndex = points.Length - 2;
			}
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
	}

	public int CurveCount {
		get {
			return (points.Length - 1) / 3;
		}
	}

	public override Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
	}

	public override Vector3 GetPointLocal(float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
	}

	public override Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}

	public override Vector3 GetVelocityLocal (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t);
	}

	public override Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

	public override Vector3 GetDirectionLocal (float t) {
		return GetVelocityLocal(t).normalized;
	}

    public void InsertCurveAfter(int point, float alpha)
    {
        int index = point * 3 + 1;
        index = Mathf.Clamp(index, 0, points.Length - 3);

        Vector3[] newPoints = new Vector3[points.Length + 3];
        BezierControlPointMode[] newModes = new BezierControlPointMode[modes.Length + 1];
        int target = 0;
        for (int i = 0; i < newPoints.Length; i++) {
            if(i < index) {
                newPoints[target] = points[i];
            } else if (i >= index + 3) {
                newPoints[target] = points[i - 3];
            }
            ++target;
        }

        target = 0;
        for (int i = 0; i < newModes.Length; i++) {
            if (i < point) {
                newModes[target] = modes[i];
            } else {
                newModes[target] = modes[i - 1];
            }
            ++target;
        }

        int indexBefore = Mathf.Clamp(index - 3, 0, points.Length - 3);
        Vector3 direction;
        if(index == indexBefore) {
            direction = Vector3.right;
        } else {
            direction = Vector3.Normalize(points[indexBefore] - points[index]);
        }
        Vector3 centerPoint = points[indexBefore] + direction * alpha;
        newPoints[index] = centerPoint - direction;
        newPoints[index + 1] = centerPoint;
        newPoints[index + 2] = centerPoint + direction;

        points = newPoints;
        modes = newModes;

        EnforceMode(index);
    }

    public override void InsertVertexAt(float insertAlpha)
    {
        float t = 0;
        int index = 0;
        int modeIndex = 0;
        if (t >= 1f) {
            t = 1f;
            index = points.Length - 4;
            modeIndex = modes.Length - 1;
        } else { 
            t = Mathf.Clamp01(insertAlpha) * CurveCount;
            index = (int)t;
            modeIndex = index;
            t -= index;
            index *= 3;
        }

        Debug.Log("Inserting vertex at point " + index.ToString() + " with alpha " + t.ToString());

        List<Vector3> newPoints = new List<Vector3>(points);
        List<BezierControlPointMode> newModes = new List<BezierControlPointMode>(modes);

        int cpBefore = index;
        //UNUSED int bpBefore = index + 1;
        int bpAfter = index + 2;
        int cpAfter = index + 3;

        Vector3 lastPos = GetControlPoint(cpBefore);
        Vector3 nextPos = GetControlPoint(cpAfter);
        Vector3 insertPos = GetPointLocal(insertAlpha);
        Vector3 insertDir = GetDirectionLocal(insertAlpha);

        newPoints.Insert(bpAfter, insertPos + insertDir * (nextPos - insertPos).magnitude * 0.3f);
        newPoints.Insert(bpAfter, insertPos);
        newPoints.Insert(bpAfter, insertPos - insertDir * (lastPos - insertPos).magnitude * 0.3f);

        newModes.Insert(modeIndex, BezierControlPointMode.Aligned);

        points = newPoints.ToArray();
        modes = newModes.ToArray();
    }

    public override void RemoveVertex(int deleteIndex)
    {
        List<Vector3> newPoints = new List<Vector3>(points);
        List<BezierControlPointMode> newModes = new List<BezierControlPointMode>(modes);

        int bpBefore = deleteIndex - 1;
        int modeIndex = deleteIndex / 3;

        if(deleteIndex == 0) {
            newPoints.RemoveAt(0);
            newPoints.RemoveAt(0);
            newPoints.RemoveAt(0);
        } else { 
            newPoints.RemoveAt(bpBefore);
            newPoints.RemoveAt(bpBefore);
            if(deleteIndex != points.Length - 1) { 
                newPoints.RemoveAt(bpBefore);
            } else {
                newPoints.RemoveAt(bpBefore - 1);
            }
        }

        newModes.RemoveAt(modeIndex);
        
        points = newPoints.ToArray();
        modes = newModes.ToArray();
    }

    public void AddCurve () {
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point.x += 1f;
		points[points.Length - 3] = point;
		point.x += 1f;
		points[points.Length - 2] = point;
		point.x += 1f;
		points[points.Length - 1] = point;

		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);

		if (loop) {
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}

    public override void RemovePoint(int point)
    {
        if (points.Length <= 4 || point == 0)
            return;

        int index = point * 3 + 1;
        index = Mathf.Clamp(index, 0, points.Length - 1);

        Vector3[] newPoints = new Vector3[points.Length - 3];
        for (int i = 0; i < newPoints.Length; i++) {
            if( i < index) {
                newPoints[i] = points[i];
            } else {
                newPoints[i] = points[i + 3];
            }
        }

        points = newPoints;

        BezierControlPointMode[] newModes = new BezierControlPointMode[modes.Length - 1];
        for (int i = 0; i < newModes.Length; i++) {
            if (i < index) {
                newModes[i] = modes[i];
            } else {
                newModes[i] = modes[i + 1];
            }
        }

        modes = newModes;
    }
	
	public void Reset () {
		points = new Vector3[] {
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		};
	}
}