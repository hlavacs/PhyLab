using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum CoilCircuitry
{
    parallel, serial
}

public class CoilManager : PhyLabSceneManager
{
    public MagnetCoil LeftMagnetCoil;
    public MagnetCoil RightMagnetCoil;
    public ParticleSystem ConnectedParticleSystem;

    public float PositionChangePerCurrentOverThreshold;
    public float SizeChangePerCurrent;
    public int CurrentThreshold;
    public CoilCircuitry Circuitry;
    public GameObject parallelCircuitryText;
    public GameObject serialCircuitryText;

    public InductionCoil InductionCoil;
    public GameObject InductionOffLight;
    public GameObject InductionOnLight;
    public GameObject InductionSwitch;
    public GameObject InductionIndicator;
    public GameObject CurrentPlus;
    public GameObject CurrentMinus;

    public int MaxCurrentLevel = 3;
    public int MinCurrentLevel = 1;
    public float SecondsPerChange = 1;
    public float IndicatorMinY = -90;
    public float IndicatorMaxY = 90;
    public Vector3 SwitchOffRotation;
    public Vector3 SwitchOnRotation;

    bool InductionEnabled = false;
    int CurrentLevel = 1;
    float CurrentChange = 0;
    float TargetChange = 0;
    float LastTargetChange = 0;

    Vector3 DefaultParticleSystemShapeScale;
    Vector3 DefaultMiddleParticleSystemScale;
    Vector3 DefaultLeftParticleSystemPosition;
    Vector3 DefaultRightParticleSystemPosition;
    Vector3 DefaultConnectedParticleSystemPosition;
    float DefaultParticleSystemStartSize;

    IEnumerator InductionCalculatorInstance = null;

    public override void InitializeSpecificScene()
    {
        ReadDefaultValues();
        ResetMagnetFields();
        LeftMagnetCoil.Initialize(this);
        RightMagnetCoil.Initialize(this);
        InductionCoil.Initialize();

        Manager.Assistant.Speak("Das hier ist das Induktions-Labor! Links siehst du das Magnetfeld zweier einfacher Leiter, rechts die Induktionsspannung einer Spule.", -1, true);
        Manager.Assistant.ClearTalkingOptions();
        UnityEvent WhatToDoEvent = new UnityEvent();
        WhatToDoEvent.AddListener(WhatToDo);
        Manager.Assistant.AddTalkingOption("Was kann ich hier machen?", WhatToDoEvent);
        Manager.Assistant.AddTalkingOption("Starte das Quiz!", QuizEvent);
    }

    public void WhatToDo()
    {
        Logger.LogEvent(Logger.LoggerEvent.ReadInstructions, "Coil");
        Manager.Assistant.Speak("Mit der Magnetfeld-Konsole kannst du die Stromstärke des Stroms der beiden Leiter, sowie die Schaltungsart ändern.", 3, true);
        Manager.Assistant.Speak("Achte darauf wie sich die Magnetfelder ändern, je nach Schaltung und Stromstärke", 3, false);
        Manager.Assistant.Speak("Mit der Induktions-Konsole kannst du die Stromstärke der linken Spule einstellen, sowie die Windungsanzahl der rechten Spule.", 3, false);
        Manager.Assistant.Speak("Achte darauf wie sich die Spannung der rechten Spule verändert", -1, false);
    }

    void ReadDefaultValues()
    {
        DefaultParticleSystemShapeScale = LeftMagnetCoil.ParticleSystem.shape.scale;
        DefaultMiddleParticleSystemScale = ConnectedParticleSystem.transform.localScale;
        DefaultLeftParticleSystemPosition = LeftMagnetCoil.ParticleSystem.transform.position;
        DefaultRightParticleSystemPosition = RightMagnetCoil.ParticleSystem.transform.position;
        DefaultConnectedParticleSystemPosition = ConnectedParticleSystem.transform.position;
        DefaultParticleSystemStartSize = LeftMagnetCoil.ParticleSystem.main.startSize.constant;
    }

    public void UpdateMagnetField()
    {
        ResetMagnetFields();


        int leftCurrent = LeftMagnetCoil.GetCurrent();
        int rightCurrent = RightMagnetCoil.GetCurrent();

        int currentSum = leftCurrent + rightCurrent;

        if (currentSum > CurrentThreshold)
        {
            int windingThresholdDifference = currentSum - CurrentThreshold;

            if (Circuitry == CoilCircuitry.parallel)
            {
                LeftMagnetCoil.ParticleSystem.Stop();
                RightMagnetCoil.ParticleSystem.Stop();
                ConnectedParticleSystem.Play();

                float scaleModifier = 1 + ((SizeChangePerCurrent / 2) * windingThresholdDifference);
                ConnectedParticleSystem.transform.localScale = new Vector3(DefaultMiddleParticleSystemScale.x * scaleModifier, DefaultMiddleParticleSystemScale.y, DefaultMiddleParticleSystemScale.z * (1 + ((scaleModifier - 1) / 2)));

                ParticleSystem.MainModule middleMain = ConnectedParticleSystem.main;
                middleMain.startSizeXMultiplier = DefaultParticleSystemStartSize / ConnectedParticleSystem.transform.localScale.x;
                middleMain.startSizeYMultiplier = DefaultParticleSystemStartSize / ConnectedParticleSystem.transform.localScale.y;

                int leftRightWindingDifference = leftCurrent - rightCurrent;

                if(leftRightWindingDifference > 0)
                {
                    ConnectedParticleSystem.transform.position = new Vector3(ConnectedParticleSystem.transform.position.x - (leftRightWindingDifference * PositionChangePerCurrentOverThreshold), ConnectedParticleSystem.transform.position.y, ConnectedParticleSystem.transform.position.z);
                } else
                {
                    ConnectedParticleSystem.transform.position = new Vector3(ConnectedParticleSystem.transform.position.x + (Mathf.Abs(leftRightWindingDifference) * PositionChangePerCurrentOverThreshold), ConnectedParticleSystem.transform.position.y, ConnectedParticleSystem.transform.position.z);
                }
            }
            else
            {
                float offsetSum = PositionChangePerCurrentOverThreshold * windingThresholdDifference;

                UpdateCoilShape(LeftMagnetCoil, leftCurrent);
                UpdateCoilShape(RightMagnetCoil, rightCurrent);

                float leftWindingProportion = leftCurrent / (float)(currentSum);
                float rightWindingProportion = rightCurrent / (float)(currentSum);

                LeftMagnetCoil.ParticleSystem.transform.position = new Vector3(DefaultLeftParticleSystemPosition.x - (offsetSum * leftWindingProportion), DefaultLeftParticleSystemPosition.y, DefaultLeftParticleSystemPosition.z);
                RightMagnetCoil.ParticleSystem.transform.position = new Vector3(DefaultRightParticleSystemPosition.x + (offsetSum * rightWindingProportion), DefaultRightParticleSystemPosition.y, DefaultRightParticleSystemPosition.z);
            }
        } else
        {
            UpdateCoilShape(LeftMagnetCoil, leftCurrent);
            UpdateCoilShape(RightMagnetCoil, rightCurrent);
        }
    }

    void UpdateCoilShape(MagnetCoil coil, int current)
    {
        float particleSystemSize = 1 + (SizeChangePerCurrent * (current - 2));

        ParticleSystem.ShapeModule shape = coil.ParticleSystem.shape;
        Vector3 newShapeScale = DefaultParticleSystemShapeScale * particleSystemSize;
        shape.scale = new Vector3(newShapeScale.x, newShapeScale.y, DefaultParticleSystemShapeScale.z);
    }

    public void ResetMagnetFields()
    {
        LeftMagnetCoil.ParticleSystem.transform.position = DefaultLeftParticleSystemPosition;
        RightMagnetCoil.ParticleSystem.transform.position = DefaultRightParticleSystemPosition;
        ConnectedParticleSystem.transform.position = DefaultConnectedParticleSystemPosition;

        LeftMagnetCoil.ParticleSystem.Play();
        RightMagnetCoil.ParticleSystem.Play();
        ConnectedParticleSystem.Stop();

        ParticleSystem.ShapeModule leftShape = LeftMagnetCoil.ParticleSystem.shape;
        leftShape.scale = DefaultParticleSystemShapeScale;

        ParticleSystem.ShapeModule rightShape = RightMagnetCoil.ParticleSystem.shape;
        rightShape.scale = DefaultParticleSystemShapeScale;

        ConnectedParticleSystem.gameObject.transform.localScale = DefaultMiddleParticleSystemScale;

        ParticleSystem.MainModule middleMain = ConnectedParticleSystem.main;
        middleMain.startSizeXMultiplier = DefaultParticleSystemStartSize / DefaultMiddleParticleSystemScale.x;
        middleMain.startSizeYMultiplier = DefaultParticleSystemStartSize / DefaultMiddleParticleSystemScale.y;
    }

    public void ChangeCircuitry()
    {
        if (Circuitry == CoilCircuitry.parallel)
        {
            Circuitry = CoilCircuitry.serial;
            ParticleSystem.VelocityOverLifetimeModule rightVelocityOverLifetimeModule = LeftMagnetCoil.ParticleSystem.velocityOverLifetime;
            rightVelocityOverLifetimeModule.orbitalYMultiplier = -2;
            parallelCircuitryText.SetActive(false);
            serialCircuitryText.SetActive(true);
        }
        else
        {
            Circuitry = CoilCircuitry.parallel;
            ParticleSystem.VelocityOverLifetimeModule rightVelocityOverLifetimeModule = LeftMagnetCoil.ParticleSystem.velocityOverLifetime;
            rightVelocityOverLifetimeModule.orbitalYMultiplier = 2;
            parallelCircuitryText.SetActive(true);
            serialCircuitryText.SetActive(false);
        }

        UpdateMagnetField();
    }

    public void ToggleInduction()
    {
        InductionEnabled = !InductionEnabled;

        InductionCoil.WindingControls.SetActive(!InductionEnabled);
        InductionOffLight.SetActive(!InductionEnabled);
        InductionOnLight.SetActive(InductionEnabled);

        if (InductionEnabled)
        {
            InductionSwitch.transform.localEulerAngles = SwitchOnRotation;
        } else
        {
            InductionSwitch.transform.localEulerAngles = SwitchOffRotation;
        }

        TargetChange = CalculateInductionChange(0, CurrentLevel, InductionCoil.GetWindingLevel());
        StartInductionCalculation();
    }

    public void IncreaseCurrentLevel()
    {
        if(CurrentLevel < MaxCurrentLevel)
        {
            CurrentLevel++;
            if (InductionEnabled)
            {
                TargetChange = CalculateInductionChange(CurrentLevel - 1, CurrentLevel, InductionCoil.GetWindingLevel());
                StartInductionCalculation();
            }
        }
        if(CurrentLevel == MaxCurrentLevel)
        {
            CurrentPlus.SetActive(false);
            CurrentMinus.SetActive(true);
        } else
        {
            CurrentPlus.SetActive(true);
            CurrentMinus.SetActive(true);
        }
    }

    public void DecreaseCurrentLevel()
    {
        if (CurrentLevel > MinCurrentLevel)
        {
            CurrentLevel--;
            if (InductionEnabled)
            {
                TargetChange = CalculateInductionChange(CurrentLevel + 1, CurrentLevel, InductionCoil.GetWindingLevel());
                StartInductionCalculation();
            }
        }
        if (CurrentLevel == MinCurrentLevel)
        {
            CurrentPlus.SetActive(true);
            CurrentMinus.SetActive(false);
        }
        else
        {
            CurrentPlus.SetActive(true);
            CurrentMinus.SetActive(true);
        }
    }

    public float CalculateInductionChange(int priorCurrentLevel, int newCurrentLevel, int windingLevel)
    {
        int currentChange = Math.Abs(priorCurrentLevel - newCurrentLevel);
        int changeScore = currentChange * (windingLevel * 2); //winding level +1 = +2 windings
        int maxChangeScore = MaxCurrentLevel * (InductionCoil.MaxWindingLevel * 2);
        float inductionChange = changeScore / (float)(maxChangeScore);

        return inductionChange;
    }

    void StartInductionCalculation()
    {
        if(InductionCalculatorInstance != null)
        {
            StopCoroutine(InductionCalculatorInstance);
        }
        InductionCalculatorInstance = InductionCalculation();
        StartCoroutine(InductionCalculatorInstance);
    }

    IEnumerator InductionCalculation()
    {
        while (CurrentChange != TargetChange)
        {
            if(CurrentChange > TargetChange)
            {
                if(TargetChange > 0)
                {
                    LastTargetChange = TargetChange;
                    TargetChange = 0;
                }
                CurrentChange -= LastTargetChange * Time.deltaTime * (1 / SecondsPerChange);

                float indicatorTargetY = IndicatorMinY + (CurrentChange * (IndicatorMaxY - IndicatorMinY));
                InductionIndicator.transform.localEulerAngles = new Vector3(InductionIndicator.transform.localEulerAngles.x, indicatorTargetY, InductionIndicator.transform.localEulerAngles.z);
            }
            else if(CurrentChange < TargetChange && TargetChange > 0)
            {
                CurrentChange += TargetChange * Time.deltaTime * (1 / SecondsPerChange);

                float indicatorTargetY = IndicatorMinY + (CurrentChange * (IndicatorMaxY - IndicatorMinY));
                InductionIndicator.transform.localEulerAngles = new Vector3(InductionIndicator.transform.localEulerAngles.x, indicatorTargetY, InductionIndicator.transform.localEulerAngles.z);
            }
            else
            {
                InductionIndicator.transform.localEulerAngles = new Vector3(InductionIndicator.transform.localEulerAngles.x, IndicatorMinY, InductionIndicator.transform.localEulerAngles.z);
                CurrentChange = 0;
            }
            yield return null;
        }
    }
}