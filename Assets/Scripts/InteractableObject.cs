using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{   
    public bool LogEventWhenInteracting = false;
    public string TextForLog;
    public UnityEvent CallbackWhenSelected;
    bool Looking = false;
    float LookingTime = 0.0f;
    Material pointerMaterial;

    private void Update() {
        if(Looking){
            LookingTime += Time.deltaTime;
            float factor = LookingTime / 3.0f;
            Color color = Color.Lerp(Color.white, Color.black, factor);
            pointerMaterial.SetColor("_Color", color);
            if(factor >= 1){
                SetLooking(false);
                LookingTime = 0;
                if(LogEventWhenInteracting){
                    Logger.LogEvent(Logger.LoggerEvent.Interaction, TextForLog);
                }
                if(CallbackWhenSelected != null){  
                    CallbackWhenSelected.Invoke();
                }
            }
        }
    }

    public void SetLooking(bool looking){
        Looking = looking;
        if(!Looking){
            LookingTime = 0.0f;
            if(pointerMaterial){
                pointerMaterial.SetColor("_Color", Color.white);
            }
        } else {
            pointerMaterial = FindObjectOfType<GvrReticlePointer>().GetComponent<Renderer>().material;
        }
    }

    public void SetCallback(UnityEvent callback){
        CallbackWhenSelected = callback;
    }
}
