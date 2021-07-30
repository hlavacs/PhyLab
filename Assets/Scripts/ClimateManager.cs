using UnityEngine.Events;

public class ClimateManager : PhyLabSceneManager
{
    public ClimateExperiment Experiment;

    public override void InitializeSpecificScene()
    {
        Experiment.Initialize();

        Manager.Assistant.Speak("Willkommen im Klimalabor! Vor dir siehst du ein kleines Biom, versuche mal die Gebäude zu bauen um Energie zu erzeugen oder Bäume zu pflanzen.", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Climate");
        Manager.Assistant.Speak("Mit der Konsole vor dir kannst du Windräder und Kohlekraftwerke bauen, sowie Bäume pflanzen.", 3, true);
        Manager.Assistant.Speak("Darunter siehst du die aktuelle CO2-Überproduktion, erzeugst du mehr CO2 als die Natur umwandeln kann steigt dieser Wert.", 3, false);
        Manager.Assistant.Speak("Zusätzlich siehst du die aktuelle Erwärmung, sowie die aktuell generierte Energie.", -1, false);

    }

}