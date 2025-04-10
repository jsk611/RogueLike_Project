using UnityEngine;
using System.Collections;
using UnityEngine.U2D; // 스프라이트 아틀라스 네임스페이스 추가

public class BinaryDeathEffect : MonoBehaviour
{
    [Header("Particle System")]
    public ParticleSystem particleSystem;
    public Material binaryParticleMaterial; // 바이너리 셰이더 머티리얼

    [Header("Binary Particle Settings")]
    public SpriteAtlas binaryAtlas; // 스프라이트 아틀라스
    public string zeroSpriteName = "zero"; // 아틀라스 내 0 스프라이트 이름
    public string oneSpriteName = "one"; // 아틀라스 내 1 스프라이트 이름
    [Range(0f, 1f)]
    public float zeroToOneRatio = 0.5f; // 0과 1의 비율

    [Header("Effect Settings")]
    public Color primaryColor = new Color(0f, 1f, 0.5f, 1f);
    public Color secondaryColor = new Color(0f, 0.6f, 1f, 1f);
    [Range(0.1f, 10f)]
    public float emissionRate = 50f;
    [Range(0.5f, 5f)]
    public float effectRadius = 1f;
    [Range(1f, 5f)]
    public float effectDuration = 2f;
    [Range(1f, 10f)]
    public float riseSpeed = 3f;
    [Range(0f, 3f)]
    public float glowIntensity = 1.5f;

    // 캐싱된 스프라이트
    private Sprite zeroSprite;
    private Sprite oneSprite;

    private void Awake()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        // 아틀라스에서 스프라이트 로드
        LoadSpritesFromAtlas();

        ConfigureParticleSystem();
    }

    void LoadSpritesFromAtlas()
    {
        if (binaryAtlas == null)
        {
            Debug.LogError("스프라이트 아틀라스가 할당되지 않았습니다.");
            return;
        }

        // 아틀라스에서 스프라이트 로드
        zeroSprite = binaryAtlas.GetSprite(zeroSpriteName);
        oneSprite = binaryAtlas.GetSprite(oneSpriteName);

        // 스프라이트 로드 확인
        if (zeroSprite == null)
            Debug.LogError($"'{zeroSpriteName}' 이름의 스프라이트를 아틀라스에서 찾을 수 없습니다.");
        if (oneSprite == null)
            Debug.LogError($"'{oneSpriteName}' 이름의 스프라이트를 아틀라스에서 찾을 수 없습니다.");
    }

    void ConfigureParticleSystem()
    {
        // 아틀라스에서 스프라이트를 로드하지 못했다면 설정하지 않음
        if (zeroSprite == null || oneSprite == null)
            return;

        // 메인 모듈 설정
        var main = particleSystem.main;
        main.startLifetime = effectDuration;
        main.startSpeed = riseSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        // 이미션 모듈 설정
        var emission = particleSystem.emission;
        emission.rateOverTime = emissionRate;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, (short)(emissionRate * 0.5f))
        });

        // 셰이프 모듈 설정
        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = effectRadius;
        shape.rotation = new Vector3(-90f, 0f, 0f); // 위쪽을 향하도록 회전


        // 텍스쳐 시트 애니메이션 설정
        var textureSheet = particleSystem.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.mode = ParticleSystemAnimationMode.Sprites;

        // 기존 스프라이트 제거 후 아틀라스에서 로드한 스프라이트 추가
        textureSheet.AddSprite(zeroSprite);
        textureSheet.AddSprite(oneSprite);

        // 크기 변화 설정
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.7f),
            new Keyframe(0.2f, 1f),
            new Keyframe(0.7f, 1f),
            new Keyframe(1f, 0f)
        ));

        // 랜더러 설정 (셰이더 머티리얼 적용)
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (binaryParticleMaterial != null)
        {
            renderer.material = binaryParticleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            // 셰이더 프로퍼티 업데이트
            binaryParticleMaterial.SetColor("_PrimaryColor", primaryColor);
            binaryParticleMaterial.SetColor("_SecondaryColor", secondaryColor);
            binaryParticleMaterial.SetFloat("_GlowIntensity", glowIntensity);
            binaryParticleMaterial.SetFloat("_RiseSpeed", riseSpeed);
        }
    }

    public void TriggerDeathEffect(Vector3 position)
    {
        // 파티클 시스템 위치 설정 및 재생
        transform.position = position;
        particleSystem.Clear();

        // 0과 1 비율에 따라 파티클 스프라이트 인덱스 설정
        SetParticleSprites();

        particleSystem.Play();

        // 효과 완료 후 정리 (선택적)
        StartCoroutine(CleanupAfterEffect());
    }

    private void SetParticleSprites()
    {
        // 생성된 파티클에 0 또는 1 스프라이트 인덱스 할당
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
        int count = particleSystem.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            // zeroToOneRatio에 따라 0 또는 1 선택 (0: 제로, 1: 원)
            bool useOne = Random.value < zeroToOneRatio;
            particles[i].randomSeed = (uint)(useOne ? 1 : 0);
        }

        if (count > 0)
        {
            particleSystem.SetParticles(particles, count);
        }
    }

    private IEnumerator CleanupAfterEffect()
    {
        yield return new WaitForSeconds(effectDuration + 1f);
        // 자동 파괴를 원하면 아래 주석 해제
        // Destroy(gameObject);
    }

    // Unity 인스펙터에서 값 변경 시 파티클 시스템 업데이트
    private void OnValidate()
    {
        if (Application.isPlaying && particleSystem != null)
        {
            // 아틀라스에서 스프라이트 다시 로드
            LoadSpritesFromAtlas();
            ConfigureParticleSystem();
        }
    }
}