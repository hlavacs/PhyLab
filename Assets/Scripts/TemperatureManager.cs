using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TemperatureManager : PhyLabSceneManager {
  
    public float CurrentTemperatureKelvin = 260;
    public TemperatureConsole Console;
    int TemperatureChange = 0;

    public Molecule[] Molecules;

    public ParticleSystem WaterVapor;
    public ParticleSystem WaterBubbles;

    public Iceblock[] Iceblocks;

    public override void InitializeSpecificScene() {
        Manager.Assistant.Speak("Willkommen in der Arktis! Brrrrrrr hier ist es kalt... Vielleicht kannst du mit dieser Konsole hier die Temperatur verändern?", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
        return;
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Temperature");
        Manager.Assistant.Speak("Mithilfe der beiden Schalter Plus und Minus kannst du die Temperatur des Wassers vor dir steuern.", 3, true);
        Manager.Assistant.Speak("Achte darauf was mit dem Wasser passiert wenn du die Temperatur änderst.", 3, false);
        Manager.Assistant.Speak("Links kannst du sehen wie sich die einzelnen Molekülen im Wasser bewegen.", -1, false);
    }

    public void IncreaseTemperature() {
        TemperatureChange = 1;
    }

    public void DecreaseTemperature() {
        TemperatureChange = -1;
    }

    public void KeepTemperature() {
        TemperatureChange = 0;
    }

    public void Update() {
        CurrentTemperatureKelvin = Mathf.Max(Mathf.Min(CurrentTemperatureKelvin + (float)TemperatureChange * Time.deltaTime * 20.0f, 10000000), 0);
        Console.SetTemperature(CurrentTemperatureKelvin);

        for(int i = 0; i < Molecules.Length; ++i) {
            Molecule molecule = Molecules[i];
            molecule.SetTemperature(CurrentTemperatureKelvin);
        }

        for(int i = 0; i < Iceblocks.Length; ++i) {
            Iceblock blocks = Iceblocks[i];
            blocks.SetTemperature(CurrentTemperatureKelvin);
        }
        var emBubbles = WaterBubbles.emission;
        emBubbles.enabled = CurrentTemperatureKelvin >= 373.15f;
        emBubbles.rateOverTime = Mathf.Lerp(0.0f, 200.0f, (CurrentTemperatureKelvin - 373.15f) / 100.0f);

        var emVapor = WaterVapor.emission;
        var mainVapor = WaterVapor.main;
        emVapor.enabled = CurrentTemperatureKelvin >= 273.15f;
        float vaporAlpha = Mathf.Clamp01((CurrentTemperatureKelvin - 273.15f) / 100.0f);
        emVapor.rateOverTime = Mathf.Lerp(0.0f, 6.0f, vaporAlpha);
        mainVapor.startSpeed =  Mathf.Lerp(0.0f, 2.0f, vaporAlpha);
        mainVapor.startColor = new Color(1.0f, 1.0f, 1.0f, vaporAlpha);
    }

}
