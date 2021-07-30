using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnergyManager : PhyLabSceneManager {
    public TextMesh DisplayText;

    public Cart[] Carts;
    Cart ActiveCart = null;

    public Spline MovementSpline;

    public override void InitializeSpecificScene() {
        Manager.Assistant.Speak("Willkommen im Energielabor. Ob ich in den Fahrzeugen da drüben wohl mitfahren darf?", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
        return;
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Energy");
        Manager.Assistant.Speak("Hier siehst du ein Fahrgeschäft in einem Vergnügungspark.", 3, true);
        Manager.Assistant.Speak("Unten kannst du zwischen drei verschiedenen Fahrzeugen wählen die alle unterschiedlich schwer sind.", 3, false);
        Manager.Assistant.Speak("Die Anzeige verrät dir wie viel potentielle und kinetische Energie aktuell im Fahrzeug stecken.", -1, false);
    }

    public void SelectCart(int cartIndex) {
        if(ActiveCart) {
            Destroy(ActiveCart.gameObject);
        }

        ActiveCart = Instantiate(Carts[cartIndex]);
        ActiveCart.SetSpline(MovementSpline);
        ActiveCart.transform.SetParent(transform);
    }

    public void Update() {
        if(ActiveCart) {
            DisplayText.text = "Masse: " + ActiveCart.Weight + "kg   Kinetische Energie: " + ActiveCart.GetKineticEnergy().ToString("00000") + "J   Potentielle Energie: " + ActiveCart.GetPotentialEnergy().ToString("00000") + "J"; 
        }
    }

    

}
