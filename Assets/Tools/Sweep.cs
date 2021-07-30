using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Sweep : MonoBehaviour
{
    public enum CrossSectionTypes {
        Circle,
        Rectangle,
        Custom
    }

    public bool SwapUV;
    public bool SwapNormals;
    public Spline Path;

    public CrossSectionTypes CrossSectionType;

    public float Radius = 0.1f;
    public float Radius2 = 0.1f;

    public float UVOffsetX = 0.0f;
    public float UVOffsetY = 0.0f;

    public float UVScaleX = 1.0f;
    public float UVScaleY = 1.0f;

    public bool CapStart;
    // Note(daniel): For the love of cthulhu I couldn't find a way to do this reliably without user input...
    public bool FlipCapStart = false;

    public bool CapEnd;
    public bool FlipCapEnd = false;

    [Min(1.0f)]
    public float MinAngle = 10.0f;

    public Spline CrossSection;
    [Min(1.0f)]
    public float MinAngleCrossSection = 10.0f;
}
