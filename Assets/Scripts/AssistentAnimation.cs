using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssistentAnimation : MonoBehaviour
{
    public Transform[] JetControls;
    public float AngleMin = 0f;
    public float AngleMax = 30f;

    Vector3 currentOffsetTarget;
    Vector3 currentOffsetVelocity;
    public float MovementRadius = 0.2f;
    public float MovementSpeed = 0.1f;

    float currentTiltAngle = 0;
    float currentTiltTarget;
    float currentTiltVelocity;
    public float TiltAngle = 0.2f;
    public float TiltSpeed = 0.1f;


    public Transform Propeller;
    public float PropellerSpeed = 2000.0f;

    public ParticleSystem Particles;
    public float MinParticleSize;
    public float MaxParticleSize;

    void Update()
    {
        float floatValue = Mathf.Sin(Time.realtimeSinceStartup);
        float angle = Mathf.Lerp(AngleMin, AngleMax, floatValue);
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        foreach (var jetControl in JetControls) {
            Vector3 jetEuler = jetControl.localEulerAngles;
            jetEuler.x = angle;
            jetControl.localEulerAngles = jetEuler;
        }

        var main = Particles.main;
        main.startSize = Mathf.Lerp(MinParticleSize, MaxParticleSize, floatValue);

        float offsetDist = Vector3.Distance(currentOffsetTarget, transform.localPosition);
        if(offsetDist < 0.01f) {
            currentOffsetTarget = Random.insideUnitSphere * MovementRadius;
        }

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, currentOffsetTarget, ref currentOffsetVelocity, Time.deltaTime, MovementSpeed);


        float tiltDist = Mathf.Abs(currentTiltTarget - currentTiltAngle);
        if (tiltDist < 0.01f) {
            currentTiltTarget = (Random.value * 2.0f - 1.0f) * TiltAngle;
        }

        currentTiltAngle = Mathf.SmoothDamp(currentTiltAngle, currentTiltTarget, ref currentTiltVelocity, Time.deltaTime, TiltSpeed);
        transform.localRotation = Quaternion.Euler(currentTiltAngle, 90, 0);

        Propeller.Rotate(0, PropellerSpeed * Time.deltaTime, 0, Space.Self);
    }
}
