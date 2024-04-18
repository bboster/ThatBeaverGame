using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScalingStatsSO : ScriptableObject
{
    public List<ScalingStat> ScalingStats;
}

public enum ScalableStat
{
    SPEED,
    JUMP_HEIGHT,
    SIZE,
    FORCE
}

[System.Serializable]
public class ScalingStat
{
    [SerializeField]
    string name;

    [Space]

    [SerializeField]
    ScalableStat stat;

    [SerializeField]
    float scalingValue;

    public ScalableStat GetStat()
    {
        return stat;
    }

    public float GetScalingValue()
    {
        return scalingValue;
    }
}
