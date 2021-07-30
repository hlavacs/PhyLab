using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    public float Speed = 100.0f;
    public Vector3 AxisToRotateAround;

    void Update()
    {
        transform.RotateAround(transform.position, AxisToRotateAround, -Time.deltaTime * Speed);
    }
}
