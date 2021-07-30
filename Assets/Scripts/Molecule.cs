using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Molecule : MonoBehaviour
{
    float CurrentTemperature;
    Vector3 StartPosition;
    float CurrentTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartPosition = transform.position;
        CurrentTime = Random.Range(0, 1000);
    }

    public void SetTemperature(float temperature) {
        CurrentTemperature = temperature;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime * CurrentTemperature * 0.05f;
        while(CurrentTime > 1000000.0f) {
            CurrentTime -= 1000000.0f;
        }

        float maxMovement = Mathf.Lerp(0.0f, 0.25f, CurrentTemperature / 500.0f);
        float maxRotation = 3.3f * Mathf.Lerp(0.0f, 3.0f, CurrentTemperature / 100.0f);

        Vector3 offset = new Vector3(Mathf.Sin(CurrentTime) * maxMovement, Mathf.Sin(CurrentTime * 1.022f) * maxMovement, Mathf.Sin(CurrentTime * 1.0331f) * maxMovement);
        transform.position = StartPosition + offset;
        Vector3 randomRotation;
        randomRotation.x = Random.Range(-maxRotation, maxRotation);
        randomRotation.y = Random.Range(-maxRotation, maxRotation);
        randomRotation.z = Random.Range(-maxRotation, maxRotation);
        transform.Rotate(randomRotation);
    }
}
