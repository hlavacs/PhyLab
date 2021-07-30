using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class SpectralManager : PhyLabSceneManager
{
    public override void InitializeSpecificScene()
    {
        Manager.Assistant.Speak("In dieser Welt kannst du das sichtbare Licht beeinflussen, Cool!", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
        return;
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Spectral");
        Manager.Assistant.Speak("Mit den Schaltern vor dir kannst du ändern was du siehst.", 3, true);
        Manager.Assistant.Speak("Die Gegenstände hier strahlen verschiedene elektromagnetische Wellen aus.", 3, false);
        Manager.Assistant.Speak("In manchen Einstellungen sieht man vielleicht ganz neue Dinge.", -1, false);
    }

    public void LeaveScene() {
        Shader.SetGlobalInt("_GlobalSpectralValue", 7);
        LoadScene("Lobby");
    }
}
