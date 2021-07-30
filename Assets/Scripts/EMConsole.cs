using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EMConsole : MonoBehaviour
{
    public TextMesh Display;
    public bool TestMode = false;

    public Material SelectedMaterial;
    public Material UnselectedMaterial;

    public SpectralControl SelectedControl;

    [Range(1, 10)]
    public int SpectralOverride = 7;

    public GameObject[] HideForXRay;

    private void Start() {
        Shader.SetGlobalInt("_GlobalSpectralValue", 7);
    }

    public void SetSpectrum(SpectralControl control) {
        Display.text = control.Name;
        Shader.SetGlobalInt("_GlobalSpectralValue", control.Value);
        if(SelectedControl) {
            SelectedControl.GetComponent<MeshRenderer>().material = UnselectedMaterial;
        }

        var sun = RenderSettings.sun;
        if(sun) {
            if(control.Value == 7) {
                sun.enabled = true;
            } else {
                sun.enabled = false;
            }
        }

        foreach (var obj in HideForXRay) {
            obj.SetActive(true);
        }
        if (control.Value <= 2) {
            foreach (var obj in HideForXRay) {
                obj.SetActive(false);
            }
        }

        control.GetComponent<MeshRenderer>().material = SelectedMaterial;
        SelectedControl = control;
    }

    private void Update() {
        if(TestMode) {
            Shader.SetGlobalInt("_GlobalSpectralValue", SpectralOverride);
            var sun = RenderSettings.sun;
            if (sun) {
                if (SpectralOverride == 7) {
                    sun.enabled = true;
                } else {
                    sun.enabled = false;
                }
            }
        }
    }
}
