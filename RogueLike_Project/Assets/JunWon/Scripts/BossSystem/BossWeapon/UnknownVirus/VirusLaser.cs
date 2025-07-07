using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusLaser : MonoBehaviour
{
    [Header("������ ����")]
    [SerializeField] bool canAttackPlayer = true;
    [SerializeField] private float damage = 10f;          // �������� ������ ������
    [SerializeField] private float lifeTime = 1.5f;       // ������ ���� �ð�
    [SerializeField] private bool applyKnockback = true;  // �˹� ���� ����
    [SerializeField] private float knockbackForce = 5f;   // �˹� ��

    [Header("�ð� ȿ��")]
    [SerializeField] private float startWidth = 0.1f;     // �ʱ� ������ �ʺ�
    [SerializeField] private float maxWidth = 0.5f;       // �ִ� ������ �ʺ�
    [SerializeField] private Color startColor = new Color(1f, 0.2f, 0.2f, 0.8f);  // ���� ����
    [SerializeField] private Color endColor = new Color(1f, 0f, 0f, 0f);          // ���� ����
    [SerializeField] private float heightOffset = 5f;     // ���� ���� ������

    [Header("Trigger ����")]
    [SerializeField] private Vector3 triggerSize = new Vector3(1f, 0.5f, 1f); // Ʈ���� �ݶ��̴� ũ��

    // ������Ʈ ����
    private LineRenderer lineRenderer;
    private BoxCollider triggerCollider;
    private bool hasDamaged = false;  // �̹� �������� �������� ����
    private bool isImpactReady = false; // ����Ʈ ȿ���� �غ�Ǿ����� ����

    // �ǰ� �̺�Ʈ ��������Ʈ
    public delegate void LaserHitEvent(GameObject target, float damage);
    public static event LaserHitEvent OnLaserHit;

    private void Awake()
    {
        // ���� ������ ������Ʈ ��������
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            SetupLineRenderer();
        }

        // Ʈ���� �ݶ��̴� ����
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }

        // �ݶ��̴� ����
        triggerCollider.isTrigger = true;
        triggerCollider.size = triggerSize;
        triggerCollider.center = Vector3.zero;

        // �ʱ⿡�� �ݶ��̴� ��Ȱ��ȭ (������ ����Ʈ ������ Ȱ��ȭ)
        triggerCollider.enabled = false;

        // �ڵ� ���� Ÿ�̸� ����
        Destroy(gameObject, lifeTime);
    }

    private void SetupLineRenderer()
    {
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = startWidth * 0.8f;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = startColor;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        // ��Ƽ���� ����
        Material laserMaterial = new Material(Shader.Find("Sprites/Default"));
        laserMaterial.color = startColor;
        lineRenderer.material = laserMaterial;

        // �߱� ȿ���� ���� ����
        laserMaterial.EnableKeyword("_EMISSION");
        laserMaterial.SetColor("_EmissionColor", startColor * 2f);
    }

    private void Start()
    {
        // �ʱ� ��ġ ����: �ϴÿ��� Ÿ������ �������� ����
        Vector3 targetPosition = transform.position;
        Vector3 startPosition = targetPosition + Vector3.up * heightOffset;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition);

        // ������ �ִϸ��̼� ����
        StartCoroutine(AnimateLaser(startPosition, targetPosition));
    }

    private IEnumerator AnimateLaser(Vector3 startPos, Vector3 endPos)
    {
        float elapsed = 0f;
        float impactTime = 0.3f; // Ÿ�ٿ� �����ϴ� �ð�

        // 1. �ϴÿ��� Ÿ������ ������ �������� �ܰ�
        while (elapsed < impactTime)
        {
            float t = elapsed / impactTime;
            Vector3 currentEndPos = Vector3.Lerp(startPos, endPos, t);

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, currentEndPos);

            // �ʺ� ���� ����
            float currentWidth = Mathf.Lerp(startWidth, maxWidth, t);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth * 0.8f;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. Ÿ�ٿ� ���� �� ȿ��
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // ����Ʈ ȿ�� (��� �÷���)
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        // ����Ʈ ��ġ ���� �� Ʈ���� Ȱ��ȭ
        transform.position = endPos;
        isImpactReady = true;
        triggerCollider.enabled = true;

        yield return new WaitForSeconds(0.1f);

        // 3. ���̵� �ƿ�
        elapsed = 0f;
        float fadeDuration = lifeTime - impactTime - 0.1f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;

            // ���� ���̵� �ƿ�
            lineRenderer.startColor = Color.Lerp(startColor, endColor, t);
            lineRenderer.endColor = Color.Lerp(startColor, endColor, t);

            // �ʺ� ����
            float currentWidth = Mathf.Lerp(maxWidth, startWidth * 0.5f, t);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth * 0.5f;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ���̵� �ƿ� �Ŀ��� �浹 ��Ȱ��ȭ
        triggerCollider.enabled = false;
    }

    // OnTriggerEnter�� �浹 ó��
    private void OnTriggerEnter(Collider other)
    {
        // ����Ʈ �غ� �ȵ� ���°ų� �̹� �������� ���� ��� ����
        if (!isImpactReady || hasDamaged) return;

        // �÷��̾� ���̾� Ȯ��
        if (other.CompareTag("Player"))
        {
            // �÷��̾� ������ ó��
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                // ������ ����
                if(canAttackPlayer)
                playerStatus.DecreaseHealth(damage);
                hasDamaged = true;

                // �̺�Ʈ �߻�
                OnLaserHit?.Invoke(other.gameObject, damage);

                // �˹� ȿ��
                if (applyKnockback)
                {
                    Rigidbody rb = other.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // �ణ �������� �˹� (���� ȿ��)
                        Vector3 knockbackDir = Vector3.up + (other.transform.position - transform.position).normalized;
                        knockbackDir.Normalize();
                        rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
                    }
                }

                // ��Ʈ ����Ʈ ����
                CreateHitEffect(transform.position);
            }
        }
    }

    // ��Ʈ ����Ʈ ����
    private void CreateHitEffect(Vector3 position)
    {
        // ���⿡ ��Ʈ ����Ʈ�� �߰��� �� �ֽ��ϴ� (��ƼŬ �ý��� ��)
        /*
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        */
    }

    // ����׿� ����� ǥ�� (Ʈ���� ����)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (triggerCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(triggerCollider.center, triggerCollider.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, triggerSize);
        }
    }

    // ������ ������ ���� (�ܺο��� ȣ�� ����)
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
}