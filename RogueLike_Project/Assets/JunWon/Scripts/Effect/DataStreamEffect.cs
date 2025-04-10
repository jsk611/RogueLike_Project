using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataStreamEffect : MonoBehaviour
{
    [Header("Particle System")]
    public ParticleSystem particleSystem;
    public Material streamParticleMaterial;

    [Header("Stream Settings")]
    [Range(1, 20)]
    public int streamCount = 5;  // 데이터 스트림 개수
    [Range(5, 100)]
    public int particlesPerStream = 20;  // 하나의 스트림당 파티클 수
    [Range(0.01f, 1f)]
    public float streamWidth = 0.05f;  // 스트림 폭
    [Range(0.1f, 10f)]
    public float streamLength = 3f;  // 스트림 길이

    [Header("Target Points")]
    public Transform sourcePoint;  // 시작점
    public List<Transform> targetPoints = new List<Transform>();  // 목표점들

    [Header("Appearance")]
    public Color primaryColor = new Color(0f, 1f, 0.5f, 1f);
    public Color secondaryColor = new Color(0f, 0.6f, 1f, 1f);
    public Color glowColor = new Color(0f, 1f, 0f, 1f);
    [Range(0.5f, 10f)]
    public float streamSpeed = 4f;
    [Range(0f, 3f)]
    public float glowIntensity = 1f;
    [Range(0f, 0.3f)]
    public float noiseAmount = 0.05f;  // 스트림 요동 정도

    [Header("Timing")]
    [Range(0.5f, 10f)]
    public float effectDuration = 3f;  // 효과 지속 시간
    [Range(0.1f, 2f)]
    public float streamStartInterval = 0.2f;  // 스트림 시작 간격

    private List<Vector3> streamDirections = new List<Vector3>();
    private bool isEffectPlaying = false;

    private void Awake()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        if (sourcePoint == null)
            sourcePoint = transform;
    }

    void Start()
    {
        ConfigureParticleSystem();
    }

    void Update()
    {
        // 셰이더 파라미터 실시간 업데이트
        if (isEffectPlaying && streamParticleMaterial != null)
        {
            UpdateShaderParameters();
        }
    }

    void ConfigureParticleSystem()
    {
        if (particleSystem == null) return;

        // 기본 모듈 설정
        var main = particleSystem.main;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = streamCount * particlesPerStream;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = effectDuration;
        main.startSize = streamWidth;
        main.startSpeed = 0f;  // 속도는 셰이더에서 처리
        main.startColor = Color.white;  // 색상은 셰이더에서 처리

        // 이미션 모듈 비활성화 (수동으로 파티클 발생)
        var emission = particleSystem.emission;
        emission.enabled = false;

        // 셰이프 모듈 비활성화
        var shape = particleSystem.shape;
        shape.enabled = false;

        // 렌더러 설정
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (streamParticleMaterial != null)
        {
            renderer.material = streamParticleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.minParticleSize = 0.01f;
            renderer.maxParticleSize = 1f;
            renderer.sortMode = ParticleSystemSortMode.YoungestInFront;
        }

        // 텍스처 시트 애니메이션 모듈 설정 (데이터 타입 표현용)
        var textureSheet = particleSystem.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.numTilesX = 1;
        textureSheet.numTilesY = 1;
    }

    void UpdateShaderParameters()
    {
        if (streamParticleMaterial != null)
        {
            streamParticleMaterial.SetColor("_PrimaryColor", primaryColor);
            streamParticleMaterial.SetColor("_SecondaryColor", secondaryColor);
            streamParticleMaterial.SetColor("_GlowColor", glowColor);
            streamParticleMaterial.SetFloat("_StreamWidth", streamWidth);
            streamParticleMaterial.SetFloat("_StreamSpeed", streamSpeed);
            streamParticleMaterial.SetFloat("_GlowIntensity", glowIntensity);
            streamParticleMaterial.SetFloat("_NoiseAmount", noiseAmount);
        }
    }

    public void TriggerStreamEffect()
    {
        if (isEffectPlaying) return;

        if (targetPoints.Count == 0)
        {
            Debug.LogWarning("No target points specified for data stream effect.");
            return;
        }

        // 기존 파티클 초기화
        particleSystem.Clear();

        // 스트림 방향 계산
        CalculateStreamDirections();

        // 스트림 시작
        StartCoroutine(StartStreamEffect());
    }

    void CalculateStreamDirections()
    {
        streamDirections.Clear();

        // 소스 위치
        Vector3 source = sourcePoint.position;

        // 각 타겟 포인트에 대한 방향 계산
        foreach (Transform target in targetPoints)
        {
            if (target != null)
            {
                Vector3 direction = target.position - source;
                streamDirections.Add(direction.normalized);
            }
        }

        // 타겟이 부족한 경우, 랜덤 방향 추가
        while (streamDirections.Count < streamCount)
        {
            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f  // 2D 효과인 경우 Z축은 0으로 설정
            ).normalized;

            streamDirections.Add(randomDir);
        }
    }

    IEnumerator StartStreamEffect()
    {
        isEffectPlaying = true;

        // 각 스트림을 지정된 간격으로 시작
        for (int streamIndex = 0; streamIndex < streamCount; streamIndex++)
        {
            CreateStream(streamIndex);
            yield return new WaitForSeconds(streamStartInterval);
        }

        // 효과 종료 대기
        yield return new WaitForSeconds(effectDuration);

        isEffectPlaying = false;
    }

    void CreateStream(int streamIndex)
    {
        if (streamIndex >= streamDirections.Count) return;

        // 스트림의 방향 벡터
        Vector3 direction = streamDirections[streamIndex];

        // 방향 벡터를 0-1 사이 스칼라 값으로 변환 (각도 변환)
        float angle = Mathf.Atan2(direction.y, direction.x) / (2f * Mathf.PI);
        if (angle < 0) angle += 1f; // 0-1 범위로 조정

        // 각 스트림의 파티클 생성
        var particles = new ParticleSystem.Particle[particlesPerStream];

        for (int i = 0; i < particlesPerStream; i++)
        {
            // 스트림 내 위치에 따른 배치 (0~1 사이 진행도)
            float progress = (float)i / particlesPerStream;

            // 파티클 위치 계산 (시작점에서 방향 * 길이 * 진행도)
            Vector3 position = sourcePoint.position + direction * streamLength * progress;

            // 기본 파티클 설정
            particles[i].position = position;
            particles[i].startColor = new Color(angle, streamIndex / (float)streamCount,
                                               Random.Range(0.8f, 1.2f), 1.0f);
            particles[i].startSize = streamWidth;
            particles[i].remainingLifetime = effectDuration - (progress * streamStartInterval * streamCount);

            // 스트림 방향을 속도로 설정 (셰이더에서 위치 계산을 위한 정보)
            particles[i].velocity = direction * streamSpeed;

            // 랜덤 회전 설정 (시각적 다양성용)
            particles[i].rotation = Random.Range(0f, 360f);

            // 크기 변화 설정 (첫 파티클은 작게, 중간은 크게, 마지막은 다시 작게)
            float sizeMultiplier = 0.5f + Mathf.Sin(progress * Mathf.PI) * 0.5f;
            particles[i].startSize *= sizeMultiplier;
        }

        // 파티클 시스템에 추가
        particleSystem.SetParticles(particles, particles.Length);
    }

    // Unity 인스펙터에서 값 변경 시
    private void OnValidate()
    {
        if (Application.isPlaying && particleSystem != null)
        {
            ConfigureParticleSystem();
            UpdateShaderParameters();
        }
    }
}