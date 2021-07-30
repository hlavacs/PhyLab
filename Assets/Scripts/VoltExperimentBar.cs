using UnityEngine;

public class VoltExperimentBar : MonoBehaviour
{
    public float Potential;

    Material Material;    
    Color OriginalColor;
    bool Selected = false;
    Color LerpStartColor;
    Color LerpTargetColor = Color.green;
    float CurrentLerpFactor = 0.0f;

    private void Start() {
        Material = transform.GetComponentInChildren<MeshRenderer>().material;
        OriginalColor = Material.GetColor("_Color");
    }

    private void Update() {
        if(Selected){
            CurrentLerpFactor += Time.deltaTime;
            if(CurrentLerpFactor > 1.0f){
                CurrentLerpFactor = 0.0f;
                Material.color = LerpTargetColor;
                LerpTargetColor = LerpStartColor;
                LerpStartColor = Material.color;
            } else {
                Material.color = Color.Lerp(LerpStartColor, LerpTargetColor, CurrentLerpFactor);
            }
        }
    }

    public void Select(){
        LerpStartColor = OriginalColor;
        Selected = true;
    }

    public void Unselect(){
        Material.color = OriginalColor;
        LerpTargetColor = Color.green;
        CurrentLerpFactor = 0.0f;
        Selected = false;
    }
}
