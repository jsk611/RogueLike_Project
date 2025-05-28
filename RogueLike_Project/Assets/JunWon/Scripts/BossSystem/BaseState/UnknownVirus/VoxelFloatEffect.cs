using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelFloatEffect : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatAmplitude = 0.3f;    // 떠다니는 강도
    public float floatSpeed = 1f;          // 떠다니는 속도
    public float randomOffset = 0.2f;      // 각 복셀마다 다른 움직임

    // 복셀들의 원래 위치 저장
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> voxelOffsets = new Dictionary<Transform, float>();

    void Start()
    {
        // 자식 객체들(복셀들)의 원래 위치 저장
        InitializeVoxelPositions();
    }

    void Update()
    {
        FloatVoxels();
    }

    private void InitializeVoxelPositions()
    {
        // 모든 자식 복셀들의 원래 위치와 개별 오프셋 저장
        foreach (Transform child in transform)
        {
            originalPositions[child] = child.localPosition;
            voxelOffsets[child] = Random.Range(0f, 2f * Mathf.PI); // 각각 다른 시작점
        }
    }

    private void FloatVoxels()
    {
        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel == null) continue;

            // 각 복셀마다 다른 패턴으로 떠다니게
            float timeOffset = voxelOffsets[voxel];

            // Y축 상하 움직임
            float floatY = Mathf.Sin((Time.time * floatSpeed) + timeOffset) * floatAmplitude;

            // X, Z축도 살짝 움직임 (더 자연스럽게)
            float floatX = Mathf.Cos((Time.time * floatSpeed * 0.7f) + timeOffset) * (floatAmplitude * 0.3f);
            float floatZ = Mathf.Sin((Time.time * floatSpeed * 0.5f) + timeOffset) * (floatAmplitude * 0.3f);

            // 랜덤 오프셋 추가
            Vector3 randomFloat = new Vector3(floatX, floatY, floatZ) * randomOffset;

            // 원래 위치에서 떠다니는 효과 적용
            Vector3 targetPosition = originalPositions[voxel] + randomFloat;
            voxel.localPosition = targetPosition;
        }
    }

    // 떠다니는 강도 조절 (외부에서 호출 가능)
    public void SetFloatIntensity(float intensity)
    {
        floatAmplitude = 0.3f * intensity;
        floatSpeed = 1f * intensity;
        randomOffset = 0.2f * intensity;
    }
}