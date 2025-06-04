using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentBehavior : MonoBehaviour
{
    private float lifetime;
    private float fadeStartTime;
    private bool shouldFade;
    private Renderer fragmentRenderer;
    private Material fragmentMaterial;
    private Color originalColor;
    private float spawnTime;

    [Header("바운스 효과")]
    [SerializeField] private float bounceReduction = 0.7f; // 바운스할 때마다 속도 감소
    [SerializeField] private int maxBounces = 3; // 최대 바운스 횟수
    private int bounceCount = 0;

    [Header("회전 감소")]
    [SerializeField] private float rotationDecay = 0.95f; // 회전 감쇠

    public void Initialize(float lifetime, float fadeStartTime, bool shouldFade)
    {
        this.lifetime = lifetime;
        this.fadeStartTime = fadeStartTime;
        this.shouldFade = shouldFade;
        this.spawnTime = Time.time;

        fragmentRenderer = GetComponent<Renderer>();
        if (fragmentRenderer != null)
        {
            fragmentMaterial = fragmentRenderer.material;
            originalColor = fragmentMaterial.color;
        }
    }

    private void Update()
    {
        // 회전 감쇠
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.angularVelocity *= rotationDecay;
        }

        // 수명 체크
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 바닥에 닿았을 때 바운스 효과
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                // 더 이상 바운스하지 않고 정지
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity *= 0.1f;
                    rb.angularVelocity *= 0.1f;
                }
            }
            else
            {
                // 바운스 효과 적용
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity *= bounceReduction;
                }
            }

            // 바운스 파티클 효과 (옵션)
            CreateBounceEffect(collision.contacts[0].point);
        }
    }

    private void CreateBounceEffect(Vector3 position)
    {
        // 간단한 먼지 효과
        GameObject dustEffect = new GameObject("DustEffect");
        dustEffect.transform.position = position;

        ParticleSystem particles = dustEffect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = Color.gray;
        main.maxParticles = 10;

        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 10)
        });

        Destroy(dustEffect, 2f);
    }

    public void StartFadeOut(float fadeDuration)
    {
        if (shouldFade && fragmentMaterial != null)
        {
            StartCoroutine(FadeOutCoroutine(fadeDuration));
        }
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / duration);

            Color newColor = originalColor;
            newColor.a = alpha;
            fragmentMaterial.color = newColor;

            yield return null;
        }
    }
}
