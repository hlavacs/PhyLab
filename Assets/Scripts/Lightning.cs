using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    public GameObject LightningEffect;
    public float MinDuration;
    public float MaxDuration;
    public float MinPause;
    public float MaxPause;
    public float MinDurationFlickerDividerLowerBound = 5;
    public float MinDurationFlickerDividerUpperBound = 2;

    float TimeUntilNextEvent = 0.0f;
    float TimeUntilNextFlicker = 0.0f;
    bool IsActive = false;

    private void Start() {
        StopLightning();
    }

    private void Update() {
        TimeUntilNextEvent -= Time.deltaTime;
        if(IsActive){
            TimeUntilNextFlicker -= Time.deltaTime;
            if(TimeUntilNextFlicker <= 0.0f){
                LightningEffect.SetActive(!LightningEffect.activeSelf);
                TimeUntilNextFlicker = Random.Range(MinDuration / MinDurationFlickerDividerLowerBound, MinDuration / MinDurationFlickerDividerUpperBound);
            }
        }
        if(TimeUntilNextEvent <= 0.0f){
            if(IsActive){
                StopLightning();
            } else {
                StartLightning();
            }
        }
    }

    public void StartLightning(){
        IsActive = true;
        LightningEffect.SetActive(true);
        TimeUntilNextEvent = Random.Range(MinDuration, MaxDuration);
        TimeUntilNextFlicker = Random.Range(MinDuration / MinDurationFlickerDividerLowerBound, MinDuration / MinDurationFlickerDividerUpperBound);
    }

    public void StopLightning(){
        IsActive = false;
        LightningEffect.SetActive(false);
        TimeUntilNextEvent = Random.Range(MinPause, MaxPause);
    }
}
