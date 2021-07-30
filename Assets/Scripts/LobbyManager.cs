using UnityEngine;
using UnityEngine.Events;

public class LobbyManager : PhyLabSceneManager
{
    public bool EnableAllScenes = true;

    public LobbyScene[] Class3;
    public LobbyScene[] Class4; 

    public override void InitializeSpecificScene()
    {
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.Speak("└[ ∵ ]┘ Hallo Freund der Physik! Ich heiße Assistenti und bin hier um dich zu unterstützen!", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);

        System.DateTime now = System.DateTime.Now;
        if(!EnableAllScenes && FixedValues.GetUsername().ToLower() != "wolfgang") {
            if(FixedValues.GetUsername().StartsWith("3")) {
                foreach (var scene in Class3) {
                    bool enabled = scene.UnlockDate.dateTime <= now;
                    scene.SetUnlocked(enabled);
                }
                foreach (var scene in Class4) {
                    scene.SetUnlocked(false);
                }
            } else if (FixedValues.GetUsername().StartsWith("4")) {
                foreach (var scene in Class3) {
                    scene.SetUnlocked(false);
                }
                foreach (var scene in Class4) {
                    bool enabled = scene.UnlockDate.dateTime <= now;
                    scene.SetUnlocked(enabled);
                }
            } else {
                foreach (var scene in Class3) {
                    scene.SetUnlocked(false);
                }
                foreach (var scene in Class4) {
                    scene.SetUnlocked(false);
                }   
            }
        }
        return;
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Lobby");
        Manager.Assistant.Speak("Schaue einfach länger auf eine der Welten um loszulegen!", -1, true);
    }

    public override void InitializeEastereggCounter(){
        EastereggCounter = GameObject.FindGameObjectWithTag("EastereggCounter").GetComponent<TextMesh>();
        if(EnableAllScenes){
            EastereggCount = 23;
            FixedValues.SetEastereggsToFind(EastereggCount);
        } else if(FixedValues.GetUsername().StartsWith("3")){
            EastereggCount = 11;
            FixedValues.SetEastereggsToFind(EastereggCount);
        } else if(FixedValues.GetUsername().StartsWith("4")){
            EastereggCount = 11;
            FixedValues.SetEastereggsToFind(EastereggCount);
        }
        FoundEastereggCount = FixedValues.HowManyEastereggsFound();
        UpdateEastereggCounter();
    }
}
