using System.IO;
using UnityEngine;

public class Helper{
    public static Quiz TryToParseQuiz(string file){
        TextAsset quizAsset = Resources.Load<TextAsset>(file);
        return JsonUtility.FromJson<Quiz>(quizAsset.text);
    }
}