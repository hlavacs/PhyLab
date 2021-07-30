using System.Diagnostics;
using UnityEngine.Events;
public class VoltageManager : PhyLabSceneManager
{
    public override void InitializeSpecificScene()
    {
        Manager.Assistant.Speak("Das hier ist das Volta-Labor! Tausche mal die Metallstäbe und schau was passiert!!", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
        return;
    }
    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Voltage");
        Manager.Assistant.Speak("Durch längeres anschauen der Metallstäbe und der Elektroden am Experiment fügst du sie dort ein.", -1, true);
    }

}