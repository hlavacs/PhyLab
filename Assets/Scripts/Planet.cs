using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public Material Skybox;
    public Color FogColor;
    public float FogDensity;
    public float GForce = 9.81f;

    public ForceManager Manager;

    public void EnableWorld() {
        RenderSettings.skybox = Skybox;
        RenderSettings.fogDensity = FogDensity;
        RenderSettings.fogColor = FogColor;

        Manager.SetPlanet(this);
    }
}
