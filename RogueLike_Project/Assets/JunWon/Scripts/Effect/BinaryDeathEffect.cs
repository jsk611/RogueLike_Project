using UnityEngine;

public class BinaryDeathEffect : MonoBehaviour
{
    [Header("Particle System")]
    public ParticleSystem particleSystem;

    [Header("Binary Particle Settings")]
    public Sprite zeroSprite;
    public Sprite oneSprite;
    public Color zeroColor = Color.gray;
    public Color oneColor = Color.white;

    void Start()
    {
        ConfigureParticleSystem();
        TriggerDeathEffect(this.transform.position);
    }

    void ConfigureParticleSystem()
    {
        var main = particleSystem.main;
        main.startLifetime = 2f;
        main.startSpeed = 3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particleSystem.emission;
        emission.rateOverTime = 20;

        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var textureSheet = particleSystem.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.mode = ParticleSystemAnimationMode.Sprites;

        textureSheet.numTilesX = 2;
        textureSheet.numTilesY = 1;
        textureSheet.animation = ParticleSystemAnimationType.WholeSheet;

        textureSheet.SetSprite(0, zeroSprite);
        textureSheet.SetSprite(1, oneSprite);

        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;

        // 투명도와 색상 그라데이션 설정
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(zeroColor, 0f),
                new GradientColorKey(oneColor, 0.5f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.5f, 1.2f),
            new Keyframe(1f, 0f)
        ));
    }

    public void TriggerDeathEffect(Vector3 position)
    {
        // 파티클 시스템 위치 설정 및 재생
        transform.position = position;
        particleSystem.Play();
    }
}