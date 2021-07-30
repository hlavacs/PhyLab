using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltExperiment : MonoBehaviour
{
    public VoltExperimentBar LeftBar;
    public VoltExperimentBar RightBar;
    public VoltExperimentBar LeftGhostBar;
    public VoltExperimentBar RightGhostBar;
    public Transform LeftBarTransform;
    public Transform RightbarTransform;
    public GameObject Needle;

    public float MaxNeedleXRotation = 180.0f;
    public float MaxPotentialDifference = 3.16f;

    bool LeftPlaceholderActive = true;
    bool RightPlaceholderActive = true;
    Vector3 OriginalLeftBarPosition;
    Vector3 OriginalRightBarPosition;
    Quaternion OriginalLeftBarRotation;
    Quaternion OriginalRightBarRotation;
    VoltExperimentBar SelectedBar;
    Quaternion OriginalNeedleRotation;
    Coroutine RotateNeedleCoroutine;

    private void Start() {
        OriginalNeedleRotation = Needle.transform.rotation;
    }

    public void LookAtBar(VoltExperimentBar bar){
        if(bar == SelectedBar || ((bar == LeftBar && SelectedBar == RightBar) || (bar == RightBar && SelectedBar == LeftBar))){
            // selected already selected or unnecessary change -> unselect
            SelectedBar.Unselect();
            SelectedBar = null;
        } else if(SelectedBar == null){
            // selected first bar -> only select
            SelectedBar = bar;
            SelectedBar.Select();
        } else {            
            // selected second bar
            if(bar == LeftBar || SelectedBar == LeftBar){
                // left bar should be changed
                if(bar == LeftBar){
                    // the now selected bar is the left bar
                    LeftGhostBar.transform.position = SelectedBar.transform.position;
                    LeftGhostBar.transform.rotation = SelectedBar.transform.rotation;
                    if(LeftBar != LeftGhostBar){
                        LeftBar.transform.position = OriginalLeftBarPosition;
                        LeftBar.transform.rotation = OriginalLeftBarRotation;
                    }
                    LeftBar = SelectedBar;
                } else {
                    // the first selected bar is the left bar
                    LeftGhostBar.transform.position = bar.transform.position;
                    LeftGhostBar.transform.rotation = bar.transform.rotation;
                    if(LeftBar != LeftGhostBar){
                        LeftBar.transform.position = OriginalLeftBarPosition;
                        LeftBar.transform.rotation = OriginalLeftBarRotation;
                    }
                    LeftBar = bar;
                }
                OriginalLeftBarPosition = LeftBar.transform.position;
                OriginalLeftBarRotation = LeftBar.transform.rotation;
                LeftBar.transform.position = LeftBarTransform.position;
                LeftBar.transform.rotation = LeftBarTransform.rotation;
            } else if(bar == RightBar || SelectedBar == RightBar){
                // right bar should be changed
                if(bar == RightBar){
                    // the now selected bar is the right bar
                    RightGhostBar.transform.position = SelectedBar.transform.position;
                    RightGhostBar.transform.rotation = SelectedBar.transform.rotation;
                    if(RightBar != RightGhostBar){
                        RightBar.transform.position = OriginalRightBarPosition;
                        RightBar.transform.rotation = OriginalRightBarRotation;
                    } 
                    RightBar = SelectedBar;
                } else {
                    // the first selected bar is the right bar
                    RightGhostBar.transform.position = bar.transform.position;
                    RightGhostBar.transform.rotation = bar.transform.rotation;
                    if(RightBar != RightGhostBar){
                        RightBar.transform.position = OriginalRightBarPosition;
                        RightBar.transform.rotation = OriginalRightBarRotation;
                    }
                    RightBar = bar;
                }
                OriginalRightBarPosition = RightBar.transform.position;
                OriginalRightBarRotation = RightBar.transform.rotation;
                RightBar.transform.position = RightbarTransform.position;
                RightBar.transform.rotation = RightbarTransform.rotation;
            }
            SelectedBar.Unselect();
            SelectedBar = null;
            EvaluateVolt();
        }        
    }

    public void EvaluateVolt(){
        if(RotateNeedleCoroutine != null){
            StopCoroutine(RotateNeedleCoroutine);
            RotateNeedleCoroutine = null;
        }

        Quaternion targetNeedleRotation = OriginalNeedleRotation;

        if(LeftBar != LeftGhostBar && RightBar != RightGhostBar){
            float voltDiff = Mathf.Abs(LeftBar.Potential - RightBar.Potential);
            float factor = voltDiff / MaxPotentialDifference;
            targetNeedleRotation *= Quaternion.Euler(factor * MaxNeedleXRotation, 0, 0);
        }

        RotateNeedleCoroutine = StartCoroutine(RotateNeedle(targetNeedleRotation));
    }

    IEnumerator RotateNeedle(Quaternion targetNeedleRotation){
        Quaternion startRotation = Needle.transform.rotation;
        float needleFactor = 0;
        while(needleFactor < 1.0f){
            needleFactor += Time.deltaTime;
            Needle.transform.rotation = Quaternion.Lerp(startRotation, targetNeedleRotation, needleFactor);
            yield return new WaitForEndOfFrame();
        }
        Needle.transform.rotation = targetNeedleRotation;
        RotateNeedleCoroutine = null;
    }
}
