using UnityEngine;

public class FixedValues : MonoBehaviour
{
    static string Username = "";
    static int EastereggCount = 0;
    static int EastereggsToFind = 0;

    public static string GetUsername(){
        if(Username == ""){
            return "USERNAME_NOT_SET";
        }
        return Username;
    }

    public static void SetUsername(string username){
        Username = username;
    }

    public static int GetEastereggsToFind(){
        return EastereggsToFind;
    }

    public static void SetEastereggsToFind(int eastereggsToFind){
        EastereggsToFind = eastereggsToFind;
    }

    public static string GetEastereggKeyName(Easteregg easteregg){
        return easteregg.name + "-" + GetUsername();
    }

    public static void FoundNewEasteregg(Easteregg easteregg){
        PlayerPrefs.SetInt("eastereggCount-" + GetUsername(), HowManyEastereggsFound() + 1);
        PlayerPrefs.SetInt(easteregg.name + "-" + GetUsername(), 1);
    }

    public static int HowManyEastereggsFound(){
        if(PlayerPrefs.HasKey("eastereggCount-" + GetUsername())){
            return PlayerPrefs.GetInt("eastereggCount-" + GetUsername());
        }
        return 0;
    }
}
