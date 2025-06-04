using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpreadPattern
{
    Uniform,        // 균등하게 퍼짐
    Explosive,      // 폭발적으로 퍼짐
    Wave,          // 파도처럼 퍼짐
    Spiral,        // 나선형으로 퍼짐
    Random         // 랜덤하게 퍼짐
}

public class VirusCubeSpread : MonoBehaviour
{
    [Header("Spread Pattern Settings")]
    [SerializeField] private SpreadPattern spreadPattern = SpreadPattern.Explosive;
    [SerializeField] private float spreadIntensity = 1.5f;
    [SerializeField] private bool addRandomOffset = true;
    [SerializeField] private float randomOffsetAmount = 0.3f;

    /// <summary>
    /// 패턴에 따른 퍼짐 위치 계산
    /// </summary>
    public Vector3 CalculateSpreadPosition(Vector3 cubePos, int index, int totalCount)
    {
        Vector3 baseDirection = cubePos.normalized;
        Vector3 spreadPos = Vector3.zero;

        switch (spreadPattern)
        {
            case SpreadPattern.Uniform:
                spreadPos = UniformSpread(baseDirection);
                break;

            case SpreadPattern.Explosive:
                spreadPos = ExplosiveSpread(baseDirection, index, totalCount);
                break;

            case SpreadPattern.Wave:
                spreadPos = WaveSpread(baseDirection, index, totalCount);
                break;

            case SpreadPattern.Spiral:
                spreadPos = SpiralSpread(baseDirection, index, totalCount);
                break;

            case SpreadPattern.Random:
                spreadPos = RandomSpread(baseDirection);
                break;
        }

        // 랜덤 오프셋 추가
        if (addRandomOffset)
        {
            Vector3 randomOffset = Random.insideUnitSphere * randomOffsetAmount;
            spreadPos += randomOffset;
        }

        return spreadPos;
    }

    private Vector3 UniformSpread(Vector3 direction)
    {
        return direction * spreadIntensity * 2.5f;
    }

    private Vector3 ExplosiveSpread(Vector3 direction, int index, int totalCount)
    {
        // 중심에서 멀수록 더 멀리 퍼짐
        float distanceFromCenter = direction.magnitude;
        float explosiveForce = 1f + (distanceFromCenter * 2f);
        return direction * spreadIntensity * explosiveForce * 2.5f;
    }

    private Vector3 WaveSpread(Vector3 direction, int index, int totalCount)
    {
        // 시간차를 두고 파도처럼 퍼짐
        float wave = Mathf.Sin((index / (float)totalCount) * Mathf.PI * 2f + Time.time * 4f);
        float waveForce = 1f + wave * 0.5f;
        return direction * spreadIntensity * waveForce * 2.5f;
    }

    private Vector3 SpiralSpread(Vector3 direction, int index, int totalCount)
    {
        // 나선형으로 퍼짐
        float angle = (index / (float)totalCount) * Mathf.PI * 4f; // 2바퀴 나선
        Vector3 spiralOffset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.5f;
        return (direction + spiralOffset) * spreadIntensity * 2.5f;
    }

    private Vector3 RandomSpread(Vector3 direction)
    {
        // 완전 랜덤하게 퍼짐
        Vector3 randomDir = Random.insideUnitSphere.normalized;
        Vector3 blendedDirection = Vector3.Lerp(direction, randomDir, 0.3f);
        return blendedDirection * spreadIntensity * Random.Range(1.5f, 3.5f);
    }
}
