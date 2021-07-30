using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TemperatureConsole : MonoBehaviour
{
    public TextMesh Display;

    public void SetTemperature(float tempKelvin) {
        Display.text = tempKelvin.ToString("0.00") + "° Kelvin\n" + ((float)tempKelvin - 273.15f).ToString("0.00") + "° Celsius";
    }
}
