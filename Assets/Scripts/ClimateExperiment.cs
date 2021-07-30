using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimateExperiment : MonoBehaviour
{
    public List<GameObject> Trees;
    public List<GameObject> WindTurbines;
    public List<GameObject> PowerPlants;
    public List<GameObject> Ground;

    public GameObject TemperatureIndicator;
    public GameObject EnergyIndicator;
    public GameObject CO2Indicator;

    public GameObject IceBlock;
    public GameObject Water;

    public Color healthyGroundColor;
    public Color dryGroundColor;

    public GameObject WoodPlus;
    public GameObject WoodMinus;
    public GameObject WindTurbinePlus;
    public GameObject WindTurbineMinus;
    public GameObject PowerPlantPlus;
    public GameObject PowerPlantMinus;

    public float MinIceBlockPosition;
    public float MaxWaterScale;

    public int MaxIndicator;
    public int MinIndicator;

    List<GameObject> ActiveTrees = new List<GameObject>();
    List<GameObject> ActiveWindTurbines = new List<GameObject>();
    List<GameObject> ActivePowerPlants = new List<GameObject>();

    List<GameObject> HiddenTrees = new List<GameObject>();
    List<GameObject> HiddenWindTurbines = new List<GameObject>();
    List<GameObject> HiddenPowerPlants = new List<GameObject>();

    public int CO2PerPowerPlant;
    public int CO2PerWindTurbine;
    public int CO2PerWood;

    public int EnergyPerPowerPlant;
    public int EnergyPerWindTurbine;
    public int EnergyPerWood;

    public int MaxTemperature;
    public int MaxCO2;
    public int MinCO2;
    int MaxEnergy;

    public float CO2IncreasePerSecondPerCO2ProductionLevel;
    public float TemperatureIncreasePerSecondPerCO2Level;

    public float IceMeltThreshold;
    public float IceMeltPerTempOverThresholdAsFractionPerSecond;

    int CurrentPowerPlantsLevel = 0;
    int CurrentWindTurbineLevel = 0;
    int CurrentWoodLevel = 0;

    int MaxPowerPlantsLevel = 3;
    int MaxWindTurbineLevel = 3;
    int MaxWoodLevel = 3;

    int PowerPlantsPerLevel;
    int WindTurbinesPerLevel;
    int TreesPerLevel;

    int CO2Score = 0;
    int EnergyScore = 0;

    float CO2 = 0;
    float Temperature = 0;

    float NatureDegredation = 0;

    IEnumerator ClimateCoroutineInstance = null;

    public void Initialize()
    {
        foreach (GameObject powerPlant in PowerPlants)
        {
            HiddenPowerPlants.Add(powerPlant);
        }
        PowerPlantsPerLevel = HiddenPowerPlants.Count / MaxPowerPlantsLevel;

        foreach (GameObject windTurbine in WindTurbines)
        {
            HiddenWindTurbines.Add(windTurbine);
        }
        WindTurbinesPerLevel = HiddenWindTurbines.Count / MaxWindTurbineLevel;

        foreach (GameObject tree in Trees)
        {
            HiddenTrees.Add(tree);
        }

        foreach(GameObject ground in Ground)
        {
            ground.GetComponent<Renderer>().material.SetColor("_Color", healthyGroundColor);
        }

        TreesPerLevel = HiddenTrees.Count / MaxWoodLevel;
        MaxEnergy = MaxPowerPlantsLevel * EnergyPerPowerPlant + MaxWindTurbineLevel * EnergyPerWindTurbine + MaxWoodLevel * EnergyPerWood;

        RecalculateClimate();
        ClimateCoroutineInstance = ClimateCoroutine();
        StartCoroutine(ClimateCoroutineInstance);
    }

    public void ResetExperiment()
    {
        foreach(GameObject powerPlant in ActivePowerPlants)
        {
            powerPlant.SetActive(false);
            HiddenPowerPlants.Add(powerPlant);
        }
        foreach (GameObject windTurbine in ActiveWindTurbines)
        {
            windTurbine.SetActive(false);
            HiddenWindTurbines.Add(windTurbine);
        }
        foreach (GameObject tree in ActiveTrees)
        {
            tree.SetActive(false);
            HiddenTrees.Add(tree);
        }
        foreach (GameObject ground in Ground)
        {
            ground.GetComponent<Renderer>().material.SetColor("_Color", healthyGroundColor);
        }

        CurrentPowerPlantsLevel = 0;
        CurrentWindTurbineLevel = 0;
        CurrentWoodLevel = 0;

        CO2Score = 0;
        EnergyScore = 0;
        CO2 = 0;
        Temperature = 0;
        NatureDegredation = 0;

        UpdateIndicator(EnergyIndicator, EnergyScore, MaxEnergy);
        UpdateIndicator(CO2Indicator, CO2, MaxCO2);
        UpdateIndicator(TemperatureIndicator, Temperature, MaxTemperature);

        PowerPlantPlus.SetActive(true);
        WindTurbinePlus.SetActive(true);
        WoodPlus.SetActive(true);

        PowerPlantMinus.SetActive(false);
        WindTurbineMinus.SetActive(false);
        WoodMinus.SetActive(false);

        IceBlock.transform.position = new Vector3(0, 0, 0);
        Water.transform.localScale = new Vector3(1, 1, 1);
    }

    void RecalculateClimate()
    {
        CO2Score = CurrentPowerPlantsLevel * CO2PerPowerPlant + CurrentWindTurbineLevel * CO2PerWindTurbine + CurrentWoodLevel * CO2PerWood;
        EnergyScore = CurrentPowerPlantsLevel * EnergyPerPowerPlant + CurrentWindTurbineLevel * EnergyPerWindTurbine + CurrentWoodLevel * EnergyPerWood;
        UpdateIndicator(EnergyIndicator, EnergyScore, MaxEnergy);
    }

    IEnumerator ClimateCoroutine()
    {
        while (true)
        {
            float cO2IncreaseBasedOnScore = CO2Score * CO2IncreasePerSecondPerCO2ProductionLevel * Time.deltaTime;
            if (cO2IncreaseBasedOnScore > 0 && CO2 < 0) {
                CO2 = 0;
            }
            CO2 += cO2IncreaseBasedOnScore;
            CO2 = Mathf.Clamp(CO2, MinCO2, MaxCO2);
            UpdateIndicator(CO2Indicator, CO2, MaxCO2);

            float temperatureIncreaseBasedOnCO2 = TemperatureIncreasePerSecondPerCO2Level * CO2 * Time.deltaTime;
            float temperatureIncreaseBasedOnGreenhouse = Temperature * 0.03f * Time.deltaTime;
            Temperature += (temperatureIncreaseBasedOnCO2 + temperatureIncreaseBasedOnGreenhouse);
            Temperature = Mathf.Clamp(Temperature, 0, MaxTemperature);
            UpdateIndicator(TemperatureIndicator, Temperature, MaxTemperature);

            UpdateWorldBasedOnTemperature();

            yield return null;
        }
    }

    void UpdateIndicator(GameObject indicator, float score, float maxScore)
    {
        score = Mathf.Clamp(score, 0, maxScore);
        float targetY = MinIndicator + ((MaxIndicator - MinIndicator) * (score / maxScore));
        indicator.transform.localEulerAngles = new Vector3(indicator.transform.localEulerAngles.x, targetY, indicator.transform.localEulerAngles.z);
    }

    void UpdateWorldBasedOnTemperature()
    {
        if(Temperature > IceMeltThreshold)
        {
            float toDegrade = (Temperature - IceMeltThreshold) * IceMeltPerTempOverThresholdAsFractionPerSecond * Time.deltaTime;
            NatureDegredation += toDegrade;

            float newIceY = Mathf.Clamp(NatureDegredation * MinIceBlockPosition, MinIceBlockPosition, 0);
            IceBlock.transform.localPosition = new Vector3(IceBlock.transform.localPosition.x, newIceY, IceBlock.transform.localPosition.z);

            float newWaterY = Mathf.Clamp(1 + (NatureDegredation * MaxWaterScale), 1, MaxWaterScale);
            Water.transform.localScale = new Vector3(Water.transform.localScale.x, newWaterY, Water.transform.localScale.z);

            foreach (GameObject ground in Ground)
            {
                ground.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(healthyGroundColor, dryGroundColor, NatureDegredation));
            }
        }
    }

    public void IncreasePowerPlants()
    {
        if (CurrentPowerPlantsLevel < MaxPowerPlantsLevel)
        {
            for(int i = 0; i < PowerPlantsPerLevel; i++)
            {
                AddPowerPlant();
            }
            CurrentPowerPlantsLevel++;
            RecalculateClimate();
        }

        PowerPlantMinus.SetActive(true);
        if(CurrentPowerPlantsLevel == MaxPowerPlantsLevel)
        {
            PowerPlantPlus.SetActive(false);
        }
    }

    void AddPowerPlant()
    {
        GameObject powerPlantToAdd = GetRandomGameObjectFromList(HiddenPowerPlants);
        ActivePowerPlants.Add(powerPlantToAdd);
        powerPlantToAdd.SetActive(true);
    }

    public void DecreasePowerPlants()
    {
        if (CurrentPowerPlantsLevel > 0)
        {
            for(int i = 0; i < PowerPlantsPerLevel; i++)
            {
                RemovePowerPlant();
            }
            CurrentPowerPlantsLevel--;
            RecalculateClimate();
        }

        PowerPlantPlus.SetActive(true);
        if (CurrentPowerPlantsLevel == 0)
        {
            PowerPlantMinus.SetActive(false);
        }
    }

    void RemovePowerPlant()
    {
        GameObject powerPlantToRemove = GetRandomGameObjectFromList(ActivePowerPlants);
        HiddenPowerPlants.Add(powerPlantToRemove);
        powerPlantToRemove.SetActive(false);
    }

    public void IncreaseWindTurbines()
    {
        if (CurrentWindTurbineLevel < MaxWindTurbineLevel)
        {
            for (int i = 0; i < WindTurbinesPerLevel; i++)
            {
                AddWindTurbine();
            }
            CurrentWindTurbineLevel++;
            RecalculateClimate();
        }

        WindTurbineMinus.SetActive(true);
        if (CurrentWindTurbineLevel == MaxWindTurbineLevel)
        {
            WindTurbinePlus.SetActive(false);
        }
    }

    void AddWindTurbine()
    {
        GameObject windTurbineToAdd = GetRandomGameObjectFromList(HiddenWindTurbines);
        ActiveWindTurbines.Add(windTurbineToAdd);
        windTurbineToAdd.SetActive(true);
    }

    public void DecreaseWindTurbines()
    {
        if (CurrentWindTurbineLevel > 0)
        {
            for (int i = 0; i < WindTurbinesPerLevel; i++)
            {
                RemoveWindTurbine();
            }
            CurrentWindTurbineLevel--;
            RecalculateClimate();
        }

        WindTurbinePlus.SetActive(true);
        if (CurrentWindTurbineLevel == 0)
        {
            WindTurbineMinus.SetActive(false);
        }
    }

    void RemoveWindTurbine()
    {
        GameObject windTurbineToRemove = GetRandomGameObjectFromList(ActiveWindTurbines);
        HiddenWindTurbines.Add(windTurbineToRemove);
        windTurbineToRemove.SetActive(false);
    }

    public void IncreaseWood()
    {
        if (CurrentWoodLevel < MaxWoodLevel)
        {
            for (int i = 0; i < TreesPerLevel; i++)
            {
                AddTree();
            }
            CurrentWoodLevel++;
            RecalculateClimate();
        }

        WoodMinus.SetActive(true);
        if (CurrentWoodLevel == MaxWoodLevel)
        {
            WoodPlus.SetActive(false);
        }
    }

    void AddTree()
    {
        GameObject treeToAdd = GetRandomGameObjectFromList(HiddenTrees);
        ActiveTrees.Add(treeToAdd);
        treeToAdd.SetActive(true);
    }

    public void DecreaseWood()
    {
        if (CurrentWoodLevel > 0)
        {
            for (int i = 0; i < TreesPerLevel; i++)
            {
                RemoveTree();
            }
            CurrentWoodLevel--;
            RecalculateClimate();
        }

        WoodPlus.SetActive(true);
        if (CurrentWoodLevel == 0)
        {
            WoodMinus.SetActive(false);
        }
    }

    void RemoveTree()
    {
        GameObject treeToRemove = GetRandomGameObjectFromList(ActiveTrees);
        HiddenTrees.Add(treeToRemove);
        treeToRemove.SetActive(false);
    }

    GameObject GetRandomGameObjectFromList(List<GameObject> gameObjectList)
    {
        int indexToRemove = Random.Range(0, gameObjectList.Count - 1);
        GameObject gameObjectToReturn = gameObjectList[indexToRemove];
        gameObjectList.RemoveAt(indexToRemove);
        return gameObjectToReturn;
    }
}