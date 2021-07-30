using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iceblock : MonoBehaviour
{
    public float TimeToFreeze = -2.0f;
    public float TimeToThaw = 2.0f;

    float CurrentTime;
    Vector3 StartScale;

    // Start is called before the first frame update
    void Start()
    {
        StartScale = transform.localScale;
    }

    public void SetTemperature(float temperature) {
        if(temperature > 273.15f) {
            if(CurrentTime > TimeToThaw) {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 0.2f);
                if(transform.localScale.magnitude < 0.02) {
                    gameObject.SetActive(false);
                }
            } else {
                CurrentTime += Time.deltaTime;
            }
        } else {
             if(CurrentTime < TimeToFreeze) {
                 if(!gameObject.activeSelf) {
                    gameObject.SetActive(true);
                }
                transform.localScale = Vector3.MoveTowards(transform.localScale, StartScale, Time.deltaTime * 0.2f);
            } else {
                CurrentTime -= Time.deltaTime;
            }
        }
    }
}
