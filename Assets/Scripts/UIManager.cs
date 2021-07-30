using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using System;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public string UsernameKey = "username";

    public InputField UsernameInput;
    public Text UsernameOutput;
    public Text UsernameErrorText;

    public Button SetUsernameButton;
    public Button ChangeUsernameButton;
    public Button StartGameButton;

    public GameObject Imprint;

    private void Start() {
        Initialize();
        ReadUsername();
    }

    public void Initialize(){
        StartCoroutine(LoadDevice("None"));
        SetUsernameButton.onClick.AddListener(() => {SetUsername();});
        ChangeUsernameButton.onClick.AddListener(ChangeUsername);
        StartGameButton.onClick.AddListener(StartGame);
    }

    public void ReadUsername(){
        if(PlayerPrefs.HasKey("username")){
            SetUsername(PlayerPrefs.GetString("username"));
        }
    }

    public void StoreUsername(){
        PlayerPrefs.SetString("username", FixedValues.GetUsername());
        PlayerPrefs.Save();
    }

    public void SetUsername(string username = ""){
        if(username == ""){
            username = UsernameInput.text;
        }
        if(IsValidUsername(username)){
            UsernameErrorText.gameObject.SetActive(false);
            FixedValues.SetUsername(username);
            StoreUsername();
            UsernameOutput.text = FixedValues.GetUsername();

            UsernameInput.gameObject.SetActive(false);
            UsernameOutput.gameObject.SetActive(true);

            SetUsernameButton.gameObject.SetActive(false);
            //ChangeUsernameButton.gameObject.SetActive(true);
            StartGameButton.gameObject.SetActive(true);
        } else {
            UsernameErrorText.gameObject.SetActive(true);
            UsernameErrorText.text = "Entschuldige, aber dieser Name ist leider ungültig.";
        }
    }

    public void ChangeUsername(){
        UsernameInput.text = FixedValues.GetUsername();

        UsernameOutput.gameObject.SetActive(false);
        UsernameInput.gameObject.SetActive(true);

        ChangeUsernameButton.gameObject.SetActive(false);
        StartGameButton.gameObject.SetActive(false);
        SetUsernameButton.gameObject.SetActive(true);
    }

    public void StartGame(){
        StartCoroutine(LoadDevice("Cardboard", () => {SceneManager.LoadScene("Base", LoadSceneMode.Single);}));        
    }

    bool IsValidUsername(string username){
        if(username == "" || username.Contains(" ")){
            return false;
        }
        return true;
    }

    IEnumerator LoadDevice(string newDevice, UnityAction callback = null)
    {
        if (String.Compare(XRSettings.loadedDeviceName, newDevice, true) != 0)
        {
            XRSettings.LoadDeviceByName(newDevice);
            yield return null;
            XRSettings.enabled = true;
        }

        if(callback != null){
            callback.Invoke();
        }
    }
}
