using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBoardManager : MonoBehaviour
{
    public ElectricLight[] Lights;

    public void Recalculate(){
        foreach (ElectricLight light in Lights)
        {
            light.Recalculate();
        }
    }
}
