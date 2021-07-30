using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Easteregg : MonoBehaviour
{
    [Multiline]
    public string Text;
    public Sprite Image;
    public Transform AssistantTarget;
    public bool UseAssistantTarget = false;
    public AudioClip Clip;

    public void Display() {
        CountEasteregg();
        var assistant = FindObjectOfType<Assistant>();
        if(assistant) {
            Vector3 target = UseAssistantTarget ? AssistantTarget.transform.position : Vector3.zero;
            assistant.DisplayEasteregg(this, UseAssistantTarget, target);
        }
    }

    public void CountEasteregg(){
        if(!PlayerPrefs.HasKey(FixedValues.GetEastereggKeyName(this))){
            PhyLabSceneManager sceneManager = FindObjectOfType<PhyLabSceneManager>();
            sceneManager.FoundEasteregg(this);
        }
    }
}
