using UnityEngine.Events;

public class PowerManager : PhyLabSceneManager
{
    public override void InitializeSpecificScene()
    {
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.Speak("Willkommen im Stromkreislabor! Hier kannst du mit verschiedenen Stromkreisen experimentieren.", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
        return;
    }

    public void WhatToDo(){
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "PowerCircuit");
        Manager.Assistant.Speak("Mit den Schaltern kannst du die Stromkreise schließen, so dass Strom durchfließt und die Lampen erleuchtet.", 3, true);
        Manager.Assistant.Speak("Aber Achtung: Manche Lampen sind kaputt, wechsele sie aus, um den Stromkreis zu schließen.", 3, false);
        Manager.Assistant.Speak("Beim letzten Stromkreis kannst du die Widerstände wechseln, achte darauf wann sich welches Licht wie verändert.", 3, false);
        Manager.Assistant.Speak("Versuche auf die Helligkeit der Lampen zu achten und die Unterschiede zwischen den Stromkreisen herauszufinden.", -1, false);
        //Patrck: let the instructions be repeatable
        //Manager.Assistant.ClearTalkingOptions();
        //Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
    }   
}