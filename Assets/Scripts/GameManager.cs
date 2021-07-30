using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    public Player Player;
    public Assistant Assistant;
    public Camera PlayerCamera;
    public LayerMask EverythingMask;
    public LayerMask NothingMask;
    
    public float FadeSeconds = 1.0f;
    public float FadeWarmup = 1.0f;
    public GameObject FadeObject;

    public string LobbyScene = "Lobby";
    public string QuizDirectory = "Quizzes";

    public float ForegroundLoggingInterval = 30.0f;

    public float TimeToDisplayAnswerText = 3.0f;

    Coroutine StoredLoadSceneCoroutine;
    Coroutine StoredWalkingCoroutine;
    Coroutine StoredFadeCoroutine;
    Coroutine AppInForegroundLoggerCoroutine;
    string CurrentSceneName = "";
    PhyLabSceneManager CurrentSceneManager = null;
    Coroutine NetworkSyncCoroutine = null;

    public bool LoadLobby = true;
    public bool SendLogToServer = false;
    public string ServerAdress = "";
    public string ServerUsername = "";
    public string ServerPassword = "";
    public float NetworkFailureTimeout = 10.0f;

    private void Start() {
        Logger.InitializeLogger(SendLogToServer);
        AppInForegroundLoggerCoroutine = StartCoroutine(AppInForegroundLogger());
        if(LoadLobby) {
            LoadScene(LobbyScene, null, true);
        } else {
            CurrentSceneName = SceneManager.GetActiveScene().name;
            CurrentSceneManager = FindObjectOfType<PhyLabSceneManager>();
            CurrentSceneManager.InitializeScene(this);
            Fade(true);
        }
    }

    private void Update() {
        if(SendLogToServer && !Logger.IsAllSynced() && NetworkSyncCoroutine == null){
            LogEntry logEntryToSync;
            if(Logger.GetLogEntryToSync(out logEntryToSync)){
                NetworkSyncCoroutine = StartCoroutine(TryToSendLogEntry(logEntryToSync));
            } else {
                Logger.AllIsSynced();
            }
        }
    }

    public void LoadScene(string sceneName, Action callback, bool firstLoad = false){
        if(StoredLoadSceneCoroutine != null){
            StopCoroutine(StoredLoadSceneCoroutine);
            StoredLoadSceneCoroutine = null;
        }
        StoredLoadSceneCoroutine = StartCoroutine(LoadSceneCoroutine(sceneName, callback, firstLoad));
    }

    public void Fade(bool fadeIn){
        if(StoredFadeCoroutine != null){
            StopCoroutine(StoredFadeCoroutine);
            StoredFadeCoroutine = null;
        }
        StoredFadeCoroutine = StartCoroutine(FadeCoroutine(fadeIn));
    }

    IEnumerator LoadSceneCoroutine(string sceneName, Action callback, bool firstLoad = false){
        if(!firstLoad){
            Fade(false);
            yield return new WaitForSeconds(FadeSeconds);
            Logger.LogEvent(Logger.LoggerEvent.OpenScene, sceneName);
        }
        if(CurrentSceneName != ""){
            Destroy(CurrentSceneManager);
            CurrentSceneManager = null;
            SetCameraToBlack();
            SceneManager.UnloadSceneAsync(CurrentSceneName);
        }
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        CurrentSceneName = sceneName;
        while(CurrentSceneManager == null){
            CurrentSceneManager = FindObjectOfType<PhyLabSceneManager>();
            yield return new WaitForEndOfFrame();
        }
        CurrentSceneManager.InitializeScene(this);
        SetCameraToScene();
        Fade(true);
        if(callback != null){
            callback();
        }
        StoredLoadSceneCoroutine = null;
    }

    public void SetCameraToBlack(){
        PlayerCamera.clearFlags = CameraClearFlags.SolidColor;
        PlayerCamera.cullingMask = NothingMask;
    }

    public void SetCameraToScene(){
        PlayerCamera.clearFlags = CameraClearFlags.Skybox;
        PlayerCamera.cullingMask = EverythingMask;
    }

    public IEnumerator FadeCoroutine(bool fadeIn){
        FadeObject.SetActive(true);
        Material FadeMaterial = FadeObject.GetComponent<Renderer>().material;
        float timeSinceStart = 0;
        if(fadeIn){
            while(timeSinceStart < FadeWarmup){
                timeSinceStart += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            timeSinceStart = 0;
        }
        while(timeSinceStart < FadeSeconds){
            float alpha = timeSinceStart / FadeSeconds;
            if(fadeIn){
                alpha = 1 - alpha;
            }

            FadeMaterial.color = new Color(FadeMaterial.color.r, FadeMaterial.color.g, FadeMaterial.color.b, alpha);
            timeSinceStart += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        FadeObject.SetActive(false);
        yield return null;
    }

    public void WalkToTarget(GameObject target){
        if(StoredWalkingCoroutine != null){
            StopCoroutine(StoredWalkingCoroutine);
            StoredWalkingCoroutine = null;
        }
        StoredWalkingCoroutine = StartCoroutine(WalkCoroutine(target));
    }

    IEnumerator WalkCoroutine(GameObject target){
        Fade(false);
        yield return new WaitForSeconds(FadeSeconds);
        Player.transform.position = target.transform.position;
        Player.transform.rotation = target.transform.rotation;
        CurrentSceneManager.Walk(target);
        Fade(true);
    }

    private void OnApplicationFocus(bool focusStatus) {
        if(focusStatus){
            AppInForegroundLoggerCoroutine = StartCoroutine(AppInForegroundLogger());
        } else {
            if(AppInForegroundLoggerCoroutine != null) {
                StopCoroutine(AppInForegroundLoggerCoroutine);
            } 
        }
    }

    IEnumerator AppInForegroundLogger(){
        while(true){
            yield return new WaitForSeconds(ForegroundLoggingInterval);
            Logger.LogEvent(Logger.LoggerEvent.HasAppOpenAndInForeground, ForegroundLoggingInterval + " seconds");
        }
    }

    IEnumerator TryToSendLogEntry(LogEntry logEntry){
        while(true){
            WWWForm form = new WWWForm();
            form.AddField("usercode", logEntry.UserID);
            form.AddField("event_name", logEntry.Event);
            form.AddField("event_description", logEntry.EventDescription);
            form.AddField("timestamp", logEntry.Timestamp);

            UnityWebRequest unityWebRequest = UnityWebRequest.Post(ServerAdress, form);
            unityWebRequest.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(ServerUsername + ":" + ServerPassword)));
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return unityWebRequest.SendWebRequest();

            if(unityWebRequest.responseCode == 201){
                unityWebRequest.Dispose();
                break;
            }
            unityWebRequest.Dispose();
            yield return new WaitForSeconds(NetworkFailureTimeout);
        }

        Logger.LogEntryWasSynced();
        NetworkSyncCoroutine = null;
    }
}
