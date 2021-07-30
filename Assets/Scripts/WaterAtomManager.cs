using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Particle {Electron, Proton}

public class WaterAtomManager : MonoBehaviour
{
    public LineRenderer WaterJet;
    public PlasticRod Rod;
    public List<Transform> FreeParticleSpot;
    public GameObject ProtonPrefab;
    public GameObject ElectronPrefab;
    public int MaxElectrons = 3;
    public int MaxProtons = 3;
    public GameObject AddElectronElement;
    public GameObject RemoveElectronElement;
    public GameObject AddProtonElement;
    public GameObject RemoveProtonElement;
    public float WaterChangeTime = 0.5f;
    public float ChangePerParticle = 1.5f;
    public Vector3 DisableElementPosition;

    Vector3 StartPosition;
    Vector3 EndPosition;
    int ProtonCount = 0;
    int ElectronCount = 0;
    float WaterJetLength;
    Coroutine ChangeWaterFlowCoroutine = null;
    float CurrentOffset = 0;
    Vector3 OriginalAddElectronPosition;
    Vector3 OriginalRemoveElectronPosition;
    Vector3 OriginalAddProtonPosition;
    Vector3 OriginalRemoveProtonPosition;
    List<Tuple<Transform, Particle, GameObject>> ParticleSpotsInUse = new List<Tuple<Transform, Particle, GameObject>>();

    private void Start() {        
        Vector3[] points = {Vector3.zero, Vector3.zero};
        int result = WaterJet.GetPositions(points);
        StartPosition = points[0];
        EndPosition = points[1];
        WaterJetLength = (StartPosition - EndPosition).y;
        OriginalAddElectronPosition = AddElectronElement.transform.position;
        OriginalRemoveElectronPosition = RemoveElectronElement.transform.position;
        OriginalAddProtonPosition = AddProtonElement.transform.position;
        OriginalRemoveProtonPosition = RemoveProtonElement.transform.position;
        RemoveElectronElement.transform.position = DisableElementPosition;
        RemoveProtonElement.transform.position = DisableElementPosition;
        UpdateWaterJet();
    }

    public void UpdateWaterJet(){
        if(ChangeWaterFlowCoroutine != null){
            StopCoroutine(ChangeWaterFlowCoroutine);
        }
        Vector3 vectorToRod = WaterJet.transform.position - Rod.transform.position;
        float rodDistance = vectorToRod.y;
        float rodHeight = vectorToRod.z;

        float distancePower = Mathf.Abs(Rod.charge) - (Mathf.Abs(rodDistance) * 8);
        if(distancePower > 0){
            if(Rod.charge < 0){
                distancePower = -distancePower;
            } else {
                distancePower *= 0.5f;
            }
            ChangeWaterFlowCoroutine = StartCoroutine(ChangeWaterStream(rodHeight, distancePower * 0.1f));
        } else {
            ChangeWaterFlowCoroutine = StartCoroutine(ChangeWaterStream(rodHeight, 0.0f));
        }
    }

    public Vector3[] CalculateWaterTrajectory(float height, float offset){
        List<Vector3> result = new List<Vector3>();
        float endLength = WaterJetLength - height;

        result.Add(StartPosition);

        if(offset < 0){
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height, StartPosition.z + (offset * 0.1f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.05f), StartPosition.z + (offset * 0.15f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.1f), StartPosition.z + (offset * 0.3f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.15f), StartPosition.z + (offset * 0.6f)));  
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.2f), StartPosition.z + (offset * 1.0f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.25f), StartPosition.z + (offset * 1.3f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.3f), StartPosition.z + (offset * 1.4f)));     
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.35f), StartPosition.z + (offset * 1.45f)));     
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.4f), StartPosition.z + (offset * 1.5f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 1.0f), StartPosition.z + (offset * 1.55f)));
        } else {
            result.Add(new Vector3(StartPosition.x, StartPosition.y - (height * 0.4f), StartPosition.z + (offset * 0.1f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - (height * 0.6f), StartPosition.z + (offset * 0.15f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - (height * 0.8f), StartPosition.z + (offset * 0.3f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - (height), StartPosition.z + (offset * 0.6f)));  
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.05f), StartPosition.z + (offset * 1.0f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.1f), StartPosition.z + (offset * 1.3f)));
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height - (endLength * 0.15f), StartPosition.z + (offset * 1.4f)));     
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height  - (endLength * 0.20f), StartPosition.z + (offset * 1.45f)));     
            result.Add(new Vector3(StartPosition.x, StartPosition.y - height  - (endLength * 0.25f), StartPosition.z + (offset * 1.5f)));
            result.Add(new Vector3(StartPosition.x, (StartPosition.y - height) - (endLength * 1.0f), StartPosition.z + (offset * 1.55f)));
        }

        return result.ToArray();        
    }

    public IEnumerator ChangeWaterStream(float rodHeight, float targetOffset){
        float startOffset = CurrentOffset;
        float timeSinceStart = 0.0f;
        Vector3[] newPoints;

        while(timeSinceStart < WaterChangeTime){
            CurrentOffset = Mathf.Lerp(startOffset, targetOffset, timeSinceStart / WaterChangeTime);

            newPoints = CalculateWaterTrajectory(rodHeight, CurrentOffset);
            WaterJet.positionCount = newPoints.Length;
            WaterJet.SetPositions(newPoints);

            timeSinceStart += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        CurrentOffset = targetOffset;
        newPoints = CalculateWaterTrajectory(rodHeight, CurrentOffset);
        WaterJet.positionCount = newPoints.Length;
        WaterJet.SetPositions(newPoints);

        yield return null;
    }

    public void AddElectron(){
        AddParticle(Particle.Electron);
    }

    public void RemoveElectron(){
        RemoveParticle(Particle.Electron);
    }

    public void AddProton(){
        AddParticle(Particle.Proton);
    }

    public void RemoveProton(){
        RemoveParticle(Particle.Proton);
    }

    public void AddParticle(Particle particle){
        int targetSpotIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, FreeParticleSpot.Count));
        Transform targetSpot = FreeParticleSpot[targetSpotIndex];
        FreeParticleSpot.Remove(targetSpot);
        GameObject instantiatedParticle = null;
        if(particle == Particle.Electron){
            instantiatedParticle = GameObject.Instantiate(ElectronPrefab, targetSpot);
            Rod.charge -= ChangePerParticle;
            if(++ElectronCount == MaxElectrons){
                DisableParticles(Particle.Electron, true);
            } else if(ElectronCount == 1){
                EnableParticles(Particle.Electron, false);
            }
        } else if(particle == Particle.Proton){
            instantiatedParticle = GameObject.Instantiate(ProtonPrefab, targetSpot);
            Rod.charge += ChangePerParticle;
            if(++ProtonCount == MaxProtons){
                DisableParticles(Particle.Proton, true);
            } else if(ProtonCount == 1){
                EnableParticles(Particle.Proton, false);
            }
        }
        ParticleSpotsInUse.Add(new Tuple<Transform, Particle, GameObject>(targetSpot, particle, instantiatedParticle));
        UpdateWaterJet();
    }

    public void RemoveParticle(Particle particle){
        Tuple<Transform, Particle, GameObject> particleToRemove = null;
        foreach (Tuple<Transform, Particle, GameObject> particleSpotInUse in ParticleSpotsInUse)
        {
            if(particleSpotInUse.Item2 == particle){
                particleToRemove = particleSpotInUse;
            }
        }
        if(particleToRemove != null){
            ParticleSpotsInUse.Remove(particleToRemove);
            FreeParticleSpot.Add(particleToRemove.Item1);
            Destroy(particleToRemove.Item3);
        }
        if(particle == Particle.Electron){
            Rod.charge += ChangePerParticle;
            if(ElectronCount-- == MaxElectrons){
                EnableParticles(Particle.Electron, true);
            } else if(ElectronCount == 0){
                DisableParticles(Particle.Electron, false);
            }
        } else if(particle == Particle.Proton){
            Rod.charge -= ChangePerParticle;
            if(ProtonCount-- == MaxProtons){
                EnableParticles(Particle.Proton, true);
            } else if(ProtonCount == 0){
                DisableParticles(Particle.Proton, false);
            }
        }
        UpdateWaterJet();
    }

    public void EnableParticles(Particle particle, bool add){
        if(particle == Particle.Electron){
            if(add){
                AddElectronElement.transform.position = OriginalAddElectronPosition;
            } else {
                RemoveElectronElement.transform.position = OriginalRemoveElectronPosition;
            }
        } else if(particle == Particle.Proton){
            if(add){
                AddProtonElement.transform.position = OriginalAddProtonPosition;
            } else {
                RemoveProtonElement.transform.position = OriginalRemoveProtonPosition;
            }
        }
    }

    public void DisableParticles(Particle particle, bool add){
        if(particle == Particle.Electron){
            if(add){
                AddElectronElement.transform.position = DisableElementPosition;
            } else {
                RemoveElectronElement.transform.position = DisableElementPosition;
            }
        } else if(particle == Particle.Proton){
            if(add){
                AddProtonElement.transform.position = DisableElementPosition;
            } else {
                RemoveProtonElement.transform.position = DisableElementPosition;
            }
        }
    }
}
