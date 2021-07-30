using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cart : MonoBehaviour
{
    public float Weight;
    float Velocity = 0;
    float MinHeight = 0;
    public Spline ActiveSpline;
    public float CurrentSplinePos;

    public void SetSpline(Spline spline) {
        ActiveSpline = spline;
        CurrentSplinePos = 0;
        transform.position = ActiveSpline.GetPoint(CurrentSplinePos);
        MinHeight = ActiveSpline.GetPoint(0.5f).y;
    }

    public float GetKineticEnergy() {
        return Weight * 0.5f * Velocity * Velocity; 
    }

    public float GetPotentialEnergy() {
        return Weight * 9.81f * (ActiveSpline.GetPoint(CurrentSplinePos).y - MinHeight);
    }

    // Update is called once per frame
    void Update()
    {
        if(ActiveSpline) {
            Vector3 movementDir = ActiveSpline.GetDirection(CurrentSplinePos);
            Velocity += movementDir.y * -9.81f * Time.deltaTime * 0.8f; // 0.8 is a fudge factor to account for something.
            CurrentSplinePos += Velocity * Time.deltaTime * 0.05f; // 0.05f is a fudge factor to get nice movement speed
            transform.position = ActiveSpline.GetPoint(CurrentSplinePos);
            transform.LookAt(transform.position + ActiveSpline.GetDirection(CurrentSplinePos));
        }
    }
}
