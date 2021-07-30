using System;
using System.Collections.Generic;
using UnityEngine;

public class MagnetCoil : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public int MaxCurrent = 8;
    public int MinCurrent = 2;
    public GameObject CurrentPlus;
    public GameObject CurrentMinus;

    int Current = 2;
    CoilManager CoilManager;

    public void Initialize(CoilManager coilManager)
    {
        CoilManager = coilManager;
    }

    public void IncreaseCurrent(int increase)
    {
        for(int i = 0; i < increase; i++)
        {
            IncreaseCurrentByOne();
        }
        CoilManager.UpdateMagnetField();
    }

    void IncreaseCurrentByOne()
    {
        if(Current < MaxCurrent)
        {
            Current++;
        }
        if(Current == MaxCurrent)
        {
            CurrentPlus.SetActive(false);
            CurrentMinus.SetActive(true);
        }
        else
        {
            CurrentPlus.SetActive(true);
            CurrentMinus.SetActive(true);
        }
    }

    public void DecreaseCurrent(int decrease)
    {
        for (int i = 0; i < decrease; i++)
        {
            DecreaseCurrentByOne();
        }
        CoilManager.UpdateMagnetField();
    }

    void DecreaseCurrentByOne()
    {
        if (Current > MinCurrent)
        {
            Current--;
        }
        if (Current == MinCurrent)
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

    public int GetCurrent()
    {
        return Current + 2;
    }
}
