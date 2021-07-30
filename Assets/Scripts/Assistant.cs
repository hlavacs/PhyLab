using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public class Assistant : MonoBehaviour
{
    public GameManager GameManager;

    public float FlyingSpeed = 3.0f;

    public Text SpokenText;
    public Image TextBackground;

    public GameObject TalkingOptionsParent;
    public GameObject[] TalkingOptions;
    public GameObject SingleTalkingOption;

    public float TimePerWord = 0.05f;
    public int CharactersPerLine = 20;
    public float TextFadeOutTime = 1.0f;
    
    public Sprite[] talkingFaces;
    public Sprite smilingFace;
    public SpriteRenderer Face;

    Vector3 CurrentTarget;
    Coroutine CurrentSpeakCoroutine;
    Coroutine CurrentTextWaitCoroutine;
    Coroutine FadeOutCoroutine;
    List<Tuple<string, float>> NextText = new List<Tuple<string, float>>();
    int CurrentShownTalkingOptions = 0;


    Vector3 PreEastereggPosition;
    public GameObject Easteregg;
    public Image EastereggImage;
    public Text EastereggText;
    public GameObject EastereggTalkingOption;

    public AudioSource SpeechSource;
    Easteregg ActiveEasteregg = null;

    private void Update() {
        if((CurrentTarget - transform.position).magnitude > 0.2f){
            transform.position += (CurrentTarget - transform.position).normalized * FlyingSpeed * Time.deltaTime;
        }

        Vector3 lookAtPos = GameManager.Player.transform.position;
        //lookAtPos.y = transform.position.y;
        transform.LookAt(lookAtPos);
        transform.Rotate(Vector3.up, 20.0f, Space.World);
    }

    public void FlyTo(Vector3 position){
        CurrentTarget = position;
    }

    public void Speak(string text, float timeToDisplay, bool overwrite = false){
        if(overwrite){
            Mute(true);
        }
        if(CurrentSpeakCoroutine == null){
            CurrentSpeakCoroutine = StartCoroutine(SpeakCoroutine(text, timeToDisplay));
        } else {
            NextText.Add(new Tuple<string, float>(text, timeToDisplay));
        }
    }

    void ContinueTalking(){
        if(NextText.Count > 0){
            Speak(NextText[0].Item1, NextText[0].Item2);
            NextText.RemoveAt(0);
        } else {
            Mute();
        }
    }

    public void Mute(bool instant = false){
        Face.sprite = smilingFace;
        if(CurrentSpeakCoroutine != null){
            StopCoroutine(CurrentSpeakCoroutine);
            CurrentSpeakCoroutine = null;
        }
        if(CurrentTextWaitCoroutine != null){
            StopCoroutine(CurrentTextWaitCoroutine);
        }
        if(!instant){
            FadeOutCoroutine = StartCoroutine(FadeOut());
        } else{
            if(FadeOutCoroutine != null){
                StopCoroutine(FadeOutCoroutine);
            }
            TextBackground.enabled = false;
            SpokenText.text = "";
        }
    }

    public void AddTalkingOption(string text, UnityEvent callback){
        GameObject NewTalkingOption = TalkingOptions[CurrentShownTalkingOptions];
        if(CurrentShownTalkingOptions == 0){
            NewTalkingOption = SingleTalkingOption;
        } else if(CurrentShownTalkingOptions == 1){
            TalkingOptions[0].GetComponent<InteractableObject>().SetCallback(SingleTalkingOption.GetComponent<InteractableObject>().CallbackWhenSelected);
            TalkingOptions[0].GetComponentInChildren<Text>().text = SingleTalkingOption.GetComponentInChildren<Text>().text;
            SingleTalkingOption.SetActive(false);
            TalkingOptions[0].SetActive(true);
        }
        NewTalkingOption.GetComponent<InteractableObject>().SetCallback(callback);
        NewTalkingOption.GetComponentInChildren<Text>().text = text;
        NewTalkingOption.SetActive(true);
        CurrentShownTalkingOptions++;
    }

    public void ClearTalkingOptions(){
        for(int i = 0; i < TalkingOptions.Length; i++){
            TalkingOptions[i].GetComponent<InteractableObject>().SetLooking(false);
            TalkingOptions[i].SetActive(false);
        }
        CurrentShownTalkingOptions = 0;
    }

    IEnumerator SpeakCoroutine(string text, float timeToDisplay){
        string[] words = text.Split();
        int currentWord = 0;
        TextBackground.enabled = true;
        SpokenText.text = words[currentWord];
        int characterInLine = SpokenText.text.Length;

        while(++currentWord < words.Length){
            Face.sprite = talkingFaces[UnityEngine.Random.Range(0, talkingFaces.Length)];
            string wordToSpeak = words[currentWord];
            int wordLengthWithSpace = wordToSpeak.Length + 1;
            if(characterInLine + wordLengthWithSpace > CharactersPerLine){
                characterInLine = wordLengthWithSpace;
                SpokenText.text += "\n" + wordToSpeak;
            } else {
                characterInLine += wordLengthWithSpace;
                SpokenText.text += " " + wordToSpeak;
            }            
            yield return new WaitForSeconds(TimePerWord);
        }
        Face.sprite = smilingFace;

        if(timeToDisplay >= 0){
            CurrentTextWaitCoroutine = StartCoroutine(TextWait(timeToDisplay));
        }
        
    }

    IEnumerator TextWait(float timeToDisplay){
        yield return new WaitForSeconds(timeToDisplay);
        CurrentSpeakCoroutine = null;
        ContinueTalking();
    }

    IEnumerator FadeOut(){
        float timeSinceStart = 0;
        float backgroundStartAlpha = TextBackground.color.a;
        float textStartAlpha = SpokenText.color.a;

        while(timeSinceStart < TextFadeOutTime){
            float timeLeft = 1 - (timeSinceStart / TextFadeOutTime);
            TextBackground.color = new Color(TextBackground.color.r, TextBackground.color.g, TextBackground.color.b, backgroundStartAlpha * timeLeft);
            SpokenText.color = new Color(SpokenText.color.r, SpokenText.color.g, SpokenText.color.b, textStartAlpha * timeLeft);
            timeSinceStart += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        TextBackground.color = new Color(TextBackground.color.r, TextBackground.color.g, TextBackground.color.b, backgroundStartAlpha);
        SpokenText.color = new Color(SpokenText.color.r, SpokenText.color.g, SpokenText.color.b, textStartAlpha);
        TextBackground.enabled = false;
        SpokenText.text = "";
    }

    public void DisplayEasteregg(Easteregg egg, bool UseAssistantTarget, Vector3 AssistantTargetPosition) {
        if(ActiveEasteregg == egg) {
            return;
        }
        ActiveEasteregg = egg;
        PreEastereggPosition = transform.position;
        if(!UseAssistantTarget){
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraDirection = cameraTransform.forward;
            cameraDirection.y = 0;
            cameraDirection.Normalize();
            Vector3 cameraLeft = Vector3.Cross(cameraDirection, Vector3.up);

            Vector3 targetPosition = cameraTransform.position + cameraDirection * 1.5f + cameraLeft * 2.0f;
            FlyTo(targetPosition);
        } else {
            FlyTo(AssistantTargetPosition);
        }


        SpokenText.gameObject.SetActive(false);
        Easteregg.SetActive(true);

        EastereggText.text = egg.Text;
        EastereggImage.sprite = egg.Image;

        SpeechSource.Stop();
        if(egg.Clip) {
            SpeechSource.clip = egg.Clip;
            SpeechSource.Play();
        }

        EastereggTalkingOption.SetActive(true);
        TalkingOptionsParent.SetActive(false);
    }

    public void ContinueEasteregg(bool flyToPreEastereggPosition = true) {
        if(flyToPreEastereggPosition){
            FlyTo(PreEastereggPosition);
        }
        SpokenText.gameObject.SetActive(true);
        Easteregg.SetActive(false);
        EastereggTalkingOption.SetActive(false);
        TalkingOptionsParent.SetActive(true);
        ActiveEasteregg = null;
        SpeechSource.Stop();
    }
}