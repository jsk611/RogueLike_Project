using UnityEngine;

public class ScoreRules : MonoBehaviour
{
    [Header("Kill Score")]
    [SerializeField] private int baseKillScore = 100;

    [Header("Wave Bonus")]
    [SerializeField] private int timeBonusPerSecond = 10;

    public int CalculateKillScore()
    {
        return baseKillScore;
    }

    public int CalculateTimeBonus(float survivalTime)
    {
        return Mathf.FloorToInt(survivalTime * timeBonusPerSecond);
    }

}