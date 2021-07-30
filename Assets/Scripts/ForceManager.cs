using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ForceManager : PhyLabSceneManager {
    float ActiveGForce = 9.81f;

    public Transform Spring;
    public Transform SpringAnchor;
    public Transform Weight;
    public float SpringOffsetBottom = 0.25f;

    public TextMesh ForceDescription;

    public void SetPlanet(Planet planet) {
        ActiveGForce = planet.GForce;
        ForceDescription.text = planet.GForce + "N";
    }

    public override void InitializeSpecificScene() {
        Manager.Assistant.Speak("Dieses Labor existiert auf mehreren Planeten gleichzeitig!", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
        return;
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Force");
        Manager.Assistant.Speak("Ich habe gehört man kann hier von Planet zu Planet reisen!", 3, true);
        Manager.Assistant.Speak("Du kannst beobachten wie sich das Gewicht von einem Kilogramm verändert.", -1, false);
    }

    public void Update() {

        Vector3 center = (Weight.position + SpringAnchor.position) * 0.5f;
        float size = (SpringAnchor.position.y - Weight.position.y);
        Spring.position = center;
        Spring.localScale = new Vector3(size, 0.1f, 1.0f);

    }

    public void FixedUpdate() {
        Weight.GetComponent<Rigidbody>().AddForce(new Vector3(0, -ActiveGForce, 0), ForceMode.Acceleration);
    }
}
