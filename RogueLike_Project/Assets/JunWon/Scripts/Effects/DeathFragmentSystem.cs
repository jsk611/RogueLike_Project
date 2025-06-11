using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class DeathFragmentSystem : MonoBehaviour
{

    [Header("조각 설정")]
    [SerializeField] private List<Rigidbody> voxelChildren = new List<Rigidbody>();
    [SerializeField] private Transform[] fragmentPrefabs; // 미리 만든 조각 프리팹들
    [SerializeField] private bool useProceduralFragments = true; // 자동으로 조각 생성할지
    [SerializeField] private int fragmentCount = 15; // 생성할 조각 개수
    [SerializeField] private Vector3 fragmentSize = new Vector3(0.5f, 0.5f, 0.5f); // 조각 크기

    [Header("물리 효과")]
    [SerializeField] private float explosionForce = 300f; // 폭발 힘
    [SerializeField] private float explosionRadius = 5f; // 폭발 반경
    [SerializeField] private Vector3 explosionUpward = Vector3.up; // 위쪽 방향 힘
    [SerializeField] private float fragmentLifetime = 10f; // 조각이 사라지는 시간

    [Header("중력 및 물리")]
    [SerializeField] private float gravityMultiplier = 1f; // 중력 배수
    [SerializeField] private float bounceForce = 0.3f; // 바운스 힘
    [SerializeField] private PhysicMaterial fragmentPhysicMaterial; // 물리 재질

    [Header("시각 효과")]
    [SerializeField] private Material fragmentMaterial; // 조각 재질
    [SerializeField] private GameObject explosionEffect; // 폭발 이펙트
    [SerializeField] private AudioClip explosionSound; // 폭발 사운드
    [SerializeField] private bool fadeOutFragments = true; // 조각 페이드아웃
    [SerializeField] private float fadeStartTime = 5f; // 페이드 시작 시간

    [Header("파티클 효과")]
    [SerializeField] private ParticleSystem dustParticles; // 먼지 파티클
    [SerializeField] private ParticleSystem sparkParticles; // 스파크 파티클

    private AudioSource audioSource;
    private List<GameObject> createdFragments = new List<GameObject>();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                voxelChildren.Add(rb);
            }
        }
    }

    /// <summary>
    /// 보스 사망 시 조각 떨어뜨리기 시작
    /// </summary>
    public void TriggerDeathFragmentation()
    {
        StartCoroutine(DeathFragmentationSequence());
    }

    /// <summary>
    /// 사망 연출 시퀀스
    /// </summary>
    private IEnumerator DeathFragmentationSequence()
    {
        // 1. 폭발 이펙트 및 사운드
        PlayExplosionEffects();

        // 2. 잠시 대기 (긴장감 조성)
        yield return new WaitForSeconds(0.3f);

        // 3. 조각들 떨어트리기 
        StartCoroutine(FallFragments());

        // 5. 조각들 페이드아웃 시작
        //if (fadeOutFragments)
        //{
        //    yield return new WaitForSeconds(fadeStartTime);
        //    StartCoroutine(FadeOutFragments());
        //}

        // 6. 일정 시간 후 모든 조각 제거
        yield return new WaitForSeconds(fragmentLifetime);
        CleanupAllFragments();
    }


    

    /// <summary>
    /// 조각 물리 설정
    /// </summary>
    private void SetupFragmentPhysics(GameObject fragment)
    {
        // Rigidbody 추가 및 설정
        Rigidbody rb = fragment.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = fragment.AddComponent<Rigidbody>();
        }

        // 중력 설정
        rb.useGravity = true;
        rb.mass = Random.Range(0.5f, 2f);
        rb.drag = Random.Range(0.1f, 0.3f);
        rb.angularDrag = Random.Range(0.1f, 0.5f);

        // 물리 재질 적용
        Collider col = fragment.GetComponent<Collider>();
        if (col != null && fragmentPhysicMaterial != null)
        {
            col.material = fragmentPhysicMaterial;
        }

        // 폭발 힘 적용
        Vector3 explosionDirection = (fragment.transform.position - transform.position).normalized;
        Vector3 force = explosionDirection * explosionForce + explosionUpward * explosionForce * 0.5f;

        // 랜덤 요소 추가
        force += Random.insideUnitSphere * explosionForce * 0.3f;

        rb.AddForce(force);
        rb.AddTorque(Random.insideUnitSphere * explosionForce * 0.5f);

        // 조각별 컴포넌트 추가
        fragment.AddComponent<FragmentBehavior>().Initialize(fragmentLifetime, fadeStartTime, fadeOutFragments);
    }

    /// <summary>
    /// 폭발 이펙트 재생
    /// </summary>
    private void PlayExplosionEffects()
    {
        // 폭발 이펙트
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }

        // 폭발 사운드
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // 파티클 효과
        if (dustParticles != null)
        {
            dustParticles.Play();
        }

        if (sparkParticles != null)
        {
            sparkParticles.Play();
        }
    }

    private IEnumerator FallFragments()
    {
        foreach (Rigidbody fragment in voxelChildren)
        {
            fragment.useGravity = true;
            fragment.isKinematic = false;
        }

         yield return null;
    }

    /// <summary>
    /// 조각들 페이드아웃
    /// </summary>
    private IEnumerator FadeOutFragments()
    {
        float fadeDuration = fragmentLifetime - fadeStartTime;

        foreach (Rigidbody fragment in voxelChildren)
        {
            if (fragment != null)
            {
                FragmentBehavior fragmentBehavior = fragment.GetComponent<FragmentBehavior>();
                if (fragmentBehavior != null)
                {
                    fragmentBehavior.StartFadeOut(fadeDuration);
                }
            }
        }

        yield return null;
    }

    /// <summary>
    /// 모든 조각 정리
    /// </summary>
    private void CleanupAllFragments()
    {
        foreach (GameObject fragment in createdFragments)
        {
            if (fragment != null)
            {
                Destroy(fragment);
            }
        }

        createdFragments.Clear();

        // 보스 오브젝트도 제거 (필요한 경우)
        Destroy(gameObject, 1f);
    }
}
