using UnityEngine;

public class ElectricResistor : MonoBehaviour
{
    public ElectricBoardManager BoardManager;
    public TextMesh OhmText;
    public int Resistance = 10;
    public bool InUse = true;

    public void ToggleInUse(){
        InUse = !InUse;
        RefreshText();
        BoardManager.Recalculate();
    }
    
    public void RefreshText(){
        if(!InUse){
            OhmText.text = "0 Ω";
        } else {
            OhmText.text = Resistance + " Ω";
        }
    }
}
