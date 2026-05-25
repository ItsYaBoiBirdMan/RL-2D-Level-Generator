using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GeneratorParameters
{
    public int BaseSeed;
    
    public int MinGroundHeight;
    public int MaxGroundHeight;

    public float GapChance;
    public int MinGap;
    public int MaxGap;
    public int MinDistanceBetweenGaps;

    public int PlatformAttempts;
    public int MinPlatformLength;
    public int MaxPlatformLength;

    public int MinPlatformHeightFromGround;
    public int MaxPlatformHeightFromGround;

    public int MinVerticalSeparation;

    public float EnemyDensity;
    public int MinDistanceBetweenEnemies;
    public float PlatformEnemyRatio; }
