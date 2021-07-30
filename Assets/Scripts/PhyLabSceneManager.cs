using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class PhyLabSceneManager : MonoBehaviour
{
    [HideInInspector]
    public GameManager Manager;
    public Transform SpawnPoint;
    public GameObject[] Checkpoints;
    public Transform AssistantSpawnPoint;
    public string QuizName;
    public Material CustomSkybox = null;
    public float CustomAmbientIntensity = -1;
    public float CustomReflectionIntensity = -1;

    protected UnityEvent QuizEvent = new UnityEvent();
    protected UnityEvent NextQuestionEvent = new UnityEvent();
    protected TextMesh EastereggCounter;
    protected int EastereggCount = 0;
    protected int FoundEastereggCount = 0;

    List<Question> leftQuizQuestions;
    Question CurrentQuestion;
    Material OriginalSkybox;
    Easteregg[] SceneEastereggs;
    float OriginalAmbientIntensity;
    float OriginalReflectionIntensity;

    public void InitializeScene(GameManager manager){
        Manager = manager;

        OriginalSkybox = RenderSettings.skybox;
        OriginalAmbientIntensity = RenderSettings.ambientIntensity;
        OriginalReflectionIntensity = RenderSettings.reflectionIntensity;

        if(CustomSkybox != null){
            RenderSettings.skybox = CustomSkybox;
        }
        if(CustomAmbientIntensity >= 0){
            RenderSettings.ambientIntensity = CustomAmbientIntensity;
        }
        if(CustomReflectionIntensity >= 0){
            RenderSettings.reflectionIntensity = CustomReflectionIntensity;
        }

        Manager.Player.transform.position = SpawnPoint.position;
        Manager.Player.transform.rotation = SpawnPoint.rotation;
        Manager.Assistant.ContinueEasteregg(false);
        Manager.Assistant.transform.position = AssistantSpawnPoint.position;
        Manager.Assistant.FlyTo(AssistantSpawnPoint.position);

        Shader.SetGlobalInt("_GlobalSpectralValue", 7);
        QuizEvent.AddListener(StartQuiz);
        NextQuestionEvent.AddListener(AskNextQuestion);

        InitializeEastereggCounter();
        InitializeSpecificScene();
    }

    public void FoundEasteregg(Easteregg easteregg){
        FoundEastereggCount++;
        FixedValues.FoundNewEasteregg(easteregg);
        UpdateEastereggCounter();
    }

    public void UpdateEastereggCounter(){
        EastereggCounter.text = FoundEastereggCount + " / " + EastereggCount;
    }

    public virtual void InitializeEastereggCounter(){
        EastereggCounter = GameObject.FindGameObjectWithTag("EastereggCounter").GetComponent<TextMesh>();
        SceneEastereggs = Resources.FindObjectsOfTypeAll<Easteregg>();
        foreach(Easteregg sceneEasteregg in SceneEastereggs){
            EastereggCount++;
            if(PlayerPrefs.HasKey(FixedValues.GetEastereggKeyName(sceneEasteregg))){
                FoundEastereggCount++;
            }
        }
        UpdateEastereggCounter();
    }

    public void Walk(GameObject targetCheckpoint){
        for(int i = 0; i < Checkpoints.Length; i++){
            Checkpoints[i].SetActive(false);
        }
        targetCheckpoint.SetActive(true);
    }
    public void LoadScene(string scene){
        RenderSettings.skybox = OriginalSkybox;
        RenderSettings.ambientIntensity = OriginalAmbientIntensity;
        RenderSettings.reflectionIntensity = OriginalReflectionIntensity;
        Manager.LoadScene(scene, null, false);
    }
    public void StartQuiz(){
        DisableInteractions();
        leftQuizQuestions = (Helper.TryToParseQuiz(Manager.QuizDirectory + "/" + QuizName)).Questions.ToList();
        AskNextQuestion();
    }
    public void AskNextQuestion(){
        Manager.Assistant.ClearTalkingOptions();
        if(leftQuizQuestions.Count > 0){
            CurrentQuestion = leftQuizQuestions[0];
            leftQuizQuestions.RemoveAt(0);
            Manager.Assistant.Speak(CurrentQuestion.getQuestionText(), -1, true);
            List<Tuple<string, UnityEvent>> answers = new List<Tuple<string, UnityEvent>>();
            string[] wrongAnswers = CurrentQuestion.getWrongAnswers();
            for(int i = 0; i < wrongAnswers.Length; i++){
                UnityEvent wrongCallback = new UnityEvent();
                int answerIndex = i;
                wrongCallback.AddListener(() => WrongAnswer(wrongAnswers[answerIndex]));
                answers.Add(new Tuple<string, UnityEvent>(wrongAnswers[i], wrongCallback));
            }
            UnityEvent rightCallback = new UnityEvent();
            rightCallback.AddListener(() => RightAnswer(CurrentQuestion.getCorrectAnswer()));
            answers.Add(new Tuple<string, UnityEvent>(CurrentQuestion.getCorrectAnswer(), rightCallback));

            System.Random nextAnswerRandom = new System.Random();
            while(answers.Count > 0){
                int nextAnswer = nextAnswerRandom.Next(0, answers.Count);
                Manager.Assistant.AddTalkingOption(answers[nextAnswer].Item1, answers[nextAnswer].Item2);
                answers.RemoveAt(nextAnswer);
            }
        } else {
            EnableInteractions();
            Manager.Assistant.Speak("Das waren alle Fragen! Um zurück zur Levelauswahl zu kommen aktiviere die Tür!", -1, true);
            Manager.Assistant.AddTalkingOption("Wiederhole das Quiz!", QuizEvent);
        }
    }
    public void WrongAnswer(string answer){
        Manager.Assistant.ClearTalkingOptions();
        Manager.Assistant.Speak(CurrentQuestion.getWrongReaction(), -1, true);
        Logger.LogEvent(Logger.LoggerEvent.WrongAnswer, "Question: " + CurrentQuestion.getQuestionText() + ", Answer: " + answer);
        ContinueQuiz();
    }

    public void RightAnswer(string answer){
        Manager.Assistant.ClearTalkingOptions();
        Manager.Assistant.Speak(CurrentQuestion.getCorrectReaction(), -1, true);
        Logger.LogEvent(Logger.LoggerEvent.RightAnswer, "Question: " + CurrentQuestion.getQuestionText() + ", Answer: " + answer);
        ContinueQuiz();
    }

    public void ContinueQuiz()
    {
        //Patrick: Changed automatic progression to user input based
        //StartCoroutine(DisplayAnswer());
        Manager.Assistant.AddTalkingOption("Nächste Frage!", NextQuestionEvent);
    }

    public void EnableInteractions(){
        foreach(Easteregg sceneEasteregg in SceneEastereggs){
            Collider eastereggCollider = sceneEasteregg.GetComponent<Collider>();
            if(eastereggCollider != null){
                eastereggCollider.enabled = true;
            }
        }
    }

    public void DisableInteractions(){
        foreach(Easteregg sceneEasteregg in SceneEastereggs){
            sceneEasteregg.GetComponent<Collider>().enabled = false;
        }
    }

    IEnumerator DisplayAnswer(){
        yield return new WaitForSeconds(Manager.TimeToDisplayAnswerText);
        AskNextQuestion();
    }

    public abstract void InitializeSpecificScene();
}
