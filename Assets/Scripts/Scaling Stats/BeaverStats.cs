using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaverStats : MonoBehaviour
{
    [SerializeField]
    ScalingStatsSO defaultStats;

    Dictionary<ScalableStat, float> playerStats = new();

    private void Awake()
    {
        foreach (ScalingStat stat in defaultStats.ScalingStats)
            playerStats.Add(stat.GetStat(), stat.GetScalingValue());
    }

    public IEnumerator TemporaryAddStat(ScalingStatsSO statsToAdd, float duration)
    {
        //Debug.Log("Stat: " + stat + " amount: " + amt + " duration: " + duration);
        foreach(ScalingStat stat in statsToAdd.ScalingStats)
            AddStat(stat.GetStat(), stat.GetScalingValue());
        yield return new WaitForSeconds(duration);
        //Debug.Log(playerStats[stat]);
        foreach (ScalingStat stat in statsToAdd.ScalingStats)
            AddStat(stat.GetStat(), -stat.GetScalingValue());
    }

    public void AddStat(ScalableStat stat, float amount)
    {
        if (!playerStats.ContainsKey(stat))
            playerStats.Add(stat, amount);
        else
            playerStats[stat] += amount;
    }
}
