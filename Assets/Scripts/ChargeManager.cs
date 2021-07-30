using UnityEngine.Events;

public class ChargeManager : PhyLabSceneManager
{
    public override void InitializeSpecificScene()
    {
        Manager.Assistant.Speak("Willkommen im Ladungslabor! Was es wohl mit diesem Wasserstrahl auf sich hat?", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
        return;
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Atom");
        Manager.Assistant.Speak("Du kannst die Ladung dieses Plastikstabs mit den Teilchen ändern - die blauen sind Elektronen, die roten Protonen.", -1, true);
    }
    
}

