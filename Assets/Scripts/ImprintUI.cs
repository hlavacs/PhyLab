using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using System;
using UnityEngine.Events;
public class ImprintUI : MonoBehaviour
{

    public Button StartGameButton;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadDevice("None"));
        StartGameButton.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        StartCoroutine(LoadDevice("Cardboard", () => { SceneManager.LoadScene("Base", LoadSceneMode.Single); }));
    }

    IEnumerator LoadDevice(string newDevice, UnityAction callback = null)
    {
        if (String.Compare(XRSettings.loadedDeviceName, newDevice, true) != 0)
        {
            XRSettings.LoadDeviceByName(newDevice);
            yield return null;
            XRSettings.enabled = true;
        }

        if (callback != null)
        {
            callback.Invoke();
        }
    }
}
