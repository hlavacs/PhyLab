using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DirectionEnum {Forward, Backward}

public class NavigationArrow : MonoBehaviour
{
    public GameObject TargetCheckpoint;

    public void WalkToTarget(){
        FindObjectOfType<GameManager>().WalkToTarget(TargetCheckpoint);
    }
}
