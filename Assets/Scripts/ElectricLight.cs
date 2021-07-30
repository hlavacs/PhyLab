using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricLight : MonoBehaviour
{
    public ElectricBoardManager BoardManager;
    public Light ObjectLight;
    public ElectricSwitch[] Switches;
    public ElectricLight[] LedDependencies;
    public Material LedOff;
    public Material LedFull;
    public Material LedMedium;
    public Material LedDark;
    public Material LedVeryDark;
    public Material DefaultLedOn;
    public MeshRenderer LightRenderer;
    public bool Broken;
    public float DefaultLightIntentsity;
    public GameObject SparkParticleSystem;
    public ElectricResistor[] AddResistor;
    public ElectricResistor SubstractResistor;


    public void Recalculate(){
        foreach (ElectricSwitch electricSwitch in Switches)
        {
            if(!electricSwitch.IsOn()){
                SwitchOff();
                return;
            }
        }
        foreach (ElectricLight LedDependence in LedDependencies)
        {
            if(LedDependence.Broken){
                SwitchOff();
                return;
            }
        }
        SwitchOn(DefaultLightIntentsity);
    }

    private void SwitchOff(){
        SparkParticleSystem.SetActive(false);
        Material[] materials = GetComponent<Renderer>().materials;
        ObjectLight.intensity = 0.0f;
        materials[0] = LedOff;
        GetComponent<Renderer>().materials = materials;
    }

    private void SwitchOn(float intensity){
        Material[] materials = GetComponent<Renderer>().materials;
        if(Broken){
            SparkParticleSystem.SetActive(true);
            ObjectLight.intensity = 0.0f;
            materials[0] = LedOff;
            GetComponent<Renderer>().materials = materials;
            return;
        } else {
            SparkParticleSystem.SetActive(false);
        }
        if(AddResistor.Length > 0){
            int darknessLevel = 0;
            int darknessChange = 2;
            foreach (ElectricResistor resistor in AddResistor)
            {
                if(resistor.InUse){
                    darknessLevel += darknessChange;
                    darknessChange -= 1;
                }
            }
            if(darknessLevel > 0 && SubstractResistor != null){
                if(SubstractResistor.InUse){
                    darknessLevel -= 1;
                }
            }
            switch(darknessLevel){
                case 0:
                    materials[0] = LedFull;
                    ObjectLight.intensity = intensity;
                break;
                case 1:
                    materials[0] = LedMedium;
                    ObjectLight.intensity = intensity * 0.75f;
                break;
                case 2:
                    materials[0] = LedDark;
                    ObjectLight.intensity = intensity * 0.5f;
                break;
                case 3:
                    materials[0] = LedVeryDark;
                    ObjectLight.intensity = intensity * 0.25f;
                break;
            }
            GetComponent<Renderer>().materials = materials;
            return;
        }
        materials[0] = DefaultLedOn;
        GetComponent<Renderer>().materials = materials;
        ObjectLight.intensity = intensity;
    }

    public void Repair(){
        Broken = false;
        BoardManager.Recalculate();
    }

    public void Break(){
        Broken = true;
        BoardManager.Recalculate();
    }

    public void ToggleBroken(){
        if(Broken){
            Repair();
        } else {
            Break();
        }
    }

}
