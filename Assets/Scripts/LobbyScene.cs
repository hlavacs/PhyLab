using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    public UDateTime UnlockDate;

    public GameObject LockedObject;
    public GameObject UnlockedObject;


    public void SetUnlocked(bool unlocked) {
        LockedObject.SetActive(!unlocked);
        UnlockedObject.SetActive(unlocked);
    }
}
