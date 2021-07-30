using System;
using UnityEngine;

[Serializable]
public class Question {
    [SerializeField]
    private string QuestionText;
    [SerializeField]
    private string CorrectAnwer;
    [SerializeField]
    private string[] WrongAnswers;
    [SerializeField]
    private string CorrectReaction;
    [SerializeField]
    private string WrongReaction;

    public string getQuestionText(){
        return QuestionText;
    }
    public string getCorrectAnswer(){
        return CorrectAnwer;
    }    
    public string[] getWrongAnswers(){
        return WrongAnswers;
    }
    public string getCorrectReaction(){
        return CorrectReaction;
    }    
    public string getWrongReaction(){
        return WrongReaction;
    }

    public Question(string questionText, string correctAnswer, string[] wrongAnswers, string correctReaction, string wrongReaction){
        QuestionText = questionText;
        CorrectAnwer = correctAnswer;
        WrongAnswers = wrongAnswers;
        CorrectReaction = correctReaction;
        WrongReaction = wrongReaction;
    }    
}