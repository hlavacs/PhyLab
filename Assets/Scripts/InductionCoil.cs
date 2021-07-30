using System.Collections.Generic;
using UnityEngine;

public class InductionCoil : MonoBehaviour
{
    public int MaxWindingLevel = 3;
    public int MinWindingLevel = 1;

    public GameObject Head;
    public GameObject Tail;
    public GameObject WindingParent;
    public GameObject WindingPrefab;
    public GameObject WindingControls;

    public GameObject WindingPlus;
    public GameObject WindingMinus;

    int CurrentWindingLevel = 0;
    List<GameObject> SpawnedWindings = new List<GameObject>();

    public void Initialize()
    {
        IncreaseWindingLevel();
        WindingMinus.SetActive(false);
    }

    public void IncreaseWindingLevel()
    {
        if (CurrentWindingLevel < MaxWindingLevel)
        {
            CurrentWindingLevel++;

            List<Vector3> windingPositions = CalculatePositions();
            for (int i = 0; i < SpawnedWindings.Count; i++)
            {
                SpawnedWindings[i].transform.position = windingPositions[i];
            }

            GameObject windingOne = Instantiate(WindingPrefab, windingPositions[(CurrentWindingLevel * 2) - 2], Quaternion.identity, WindingParent.transform);
            windingOne.transform.localEulerAngles = new Vector3(0, 0, 0);
            GameObject windingTwo = Instantiate(WindingPrefab, windingPositions[(CurrentWindingLevel * 2) - 1], Quaternion.identity, WindingParent.transform);
            windingTwo.transform.localEulerAngles = new Vector3(0, 0, 0);

            SpawnedWindings.Add(windingOne);
            SpawnedWindings.Add(windingTwo);
        }
        WindingMinus.SetActive(true);
        if(CurrentWindingLevel == MaxWindingLevel)
        {
            WindingPlus.SetActive(false);
        }
    }

    public void DecreaseWindingLevel()
    {
        if (CurrentWindingLevel > MinWindingLevel)
        {
            CurrentWindingLevel--;
            List<Vector3> windingPositions = CalculatePositions();
            for (int i = 0; i < SpawnedWindings.Count - 2; i++)
            {
                SpawnedWindings[i].transform.position = windingPositions[i];
            }

            GameObject firstWindingToRemove = SpawnedWindings[SpawnedWindings.Count - 2];
            GameObject secondWindingToRemove = SpawnedWindings[SpawnedWindings.Count - 1];

            SpawnedWindings.Remove(firstWindingToRemove);
            SpawnedWindings.Remove(secondWindingToRemove);

            Destroy(firstWindingToRemove);
            Destroy(secondWindingToRemove);
        }

        WindingPlus.SetActive(true);
        if (CurrentWindingLevel == MinWindingLevel)
        {
            WindingMinus.SetActive(false);
        }
    }


    List<Vector3> CalculatePositions()
    {
        List<Vector3> positions = new List<Vector3>();

        Vector3 positionDifference = Head.transform.position - Tail.transform.position;
        Vector3 positionDifferencePerWinding = positionDifference / ((CurrentWindingLevel * 2) + 1);

        for (int i = 0; i < (CurrentWindingLevel * 2); i++)
        {
            positions.Add(Head.transform.position - (positionDifferencePerWinding * (i + 1)));
        }

        return positions;
    }

    public int GetWindingLevel()
    {
        return CurrentWindingLevel;
    }
}