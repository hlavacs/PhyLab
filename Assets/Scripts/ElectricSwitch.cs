using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSwitch : MonoBehaviour
{
    public ElectricBoardManager BoardManager;

    private bool On;

    public void Toggle(){
        On = !On;
        if(On){
            transform.rotation = Quaternion.Euler(0, 0, 0);
        } else {
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        Recalculate();
    }

    public bool IsOn(){
        return On;
    }

    public void Recalculate(){
        BoardManager.Recalculate();
    }
}
