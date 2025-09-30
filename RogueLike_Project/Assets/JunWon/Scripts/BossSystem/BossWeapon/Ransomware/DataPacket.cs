using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPacket : MProjectile
{
    [Header("?? Curved Attack Settings")]
    [SerializeField] private bool useCurvedMovement = true;
    [SerializeField] private CurveType curveType = CurveType.Random;
    [SerializeField] private float curveIntensity = 2f;        // 곡선 강도
    [SerializeField] private float curveFrequency = 3f;        // 곡선 빈도
    [SerializeField] private float spiralRadius = 1f;          // 회선 궤도 반경
    [SerializeField] private bool visualizePath = true;        // 경로 시각화 여부
    [SerializeField] private float lifeTime = 10.0f;

    [Header("Targeting")]
    [SerializeField] private float homingStrength = 1f;        // 플레이어 추적 강도 (0=추적 없음, 1=강한 추적)

    public enum CurveType
    {
        SineWave,
        Bezier,
        Spiral,
        Zigzag,
        Wave3D,
        Random
    }

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 initialDirection;
    private float timeAlive = 0f;
    private CurveType selectedCurveType;
    private PostProcessingManager postProcessing;

    private Vector3 controlPoint1;
    private Vector3 controlPoint2;

    private LineRenderer pathRenderer;
    private readonly List<Vector3> pathPoints = new List<Vector3>();

    private Transform playerTransform;
    private Rigidbody playerRigidbody;

    private Vector3 originalScale;
    private bool originalScaleCaptured = false;

    private bool pendingVisualOverride = false;
    private float pendingScaleMultiplier = 1f;
    private bool pendingShowPath = true;
    private float pendingPathWidthMultiplier = 1f;

    private float basePathStartWidth = 0.1f;
    private float basePathEndWidth = 0.05f;

    void Awake()
    {
        originalScale = transform.localScale;
        originalScaleCaptured = true;
        pendingShowPath = visualizePath;
    }

    void Start()
    {
        postProcessing = FindObjectOfType<PostProcessingManager>();
        Debug.Log("PostProcessing reference: " + (postProcessing != null ? "Found" : "Not Found"));

        RefreshPlayerCache();

        if (useCurvedMovement)
        {
            InitializeCurvedMovement();
        }

        ApplyVisualOverridesIfReady();
    }

    private void RefreshPlayerCache()
    {
        if (playerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRigidbody = player.GetComponent<Rigidbody>();
        }
    }

    private void InitializeCurvedMovement()
    {
        startPosition = transform.position;

        RefreshPlayerCache();

        if (playerTransform != null)
        {
            Vector3 playerVelocity = playerRigidbody != null ? playerRigidbody.velocity : Vector3.zero;
            float predictTime = Vector3.Distance(startPosition, playerTransform.position) / Mathf.Max(0.01f, GetSpeed());
            targetPosition = playerTransform.position + playerVelocity * predictTime;
        }
        else
        {
            targetPosition = startPosition + transform.forward * 20f;
        }

        initialDirection = dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward;

        selectedCurveType = curveType == CurveType.Random ?
            (CurveType)Random.Range(0, System.Enum.GetValues(typeof(CurveType)).Length - 1) :
            curveType;

        Debug.Log($"[DataPacket] Fired with {selectedCurveType} curve!");

        switch (selectedCurveType)
        {
            case CurveType.Bezier:
                InitializeBezierCurve();
                break;
        }

        if (visualizePath)
        {
            SetupPathVisualization();
        }
    }

    private void InitializeBezierCurve()
    {
        Vector3 toTarget = targetPosition - startPosition;
        Vector3 perpendicular = Vector3.Cross(toTarget, Vector3.up).normalized;

        float distance = toTarget.magnitude;
        controlPoint1 = startPosition + toTarget * 0.33f + perpendicular * Random.Range(-curveIntensity, curveIntensity) * distance * 0.3f;
        controlPoint2 = startPosition + toTarget * 0.66f + perpendicular * Random.Range(-curveIntensity, curveIntensity) * distance * 0.3f;

        controlPoint1.y += Random.Range(-curveIntensity, curveIntensity) * distance * 0.2f;
        controlPoint2.y += Random.Range(-curveIntensity, curveIntensity) * distance * 0.2f;
    }

    private void SetupPathVisualization()
    {
        pathRenderer = gameObject.AddComponent<LineRenderer>();
        pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathRenderer.startWidth = 0.1f;
        pathRenderer.endWidth = 0.05f;
        pathRenderer.positionCount = 0;
        pathRenderer.useWorldSpace = true;

        basePathStartWidth = pathRenderer.startWidth;
        basePathEndWidth = pathRenderer.endWidth;

        ApplyVisualOverridesIfReady();
    }

    void Update()
    {
        if (lifeTime < timeAlive)
        {
            Destroy(gameObject);
            return;
        }

        if (useCurvedMovement)
        {
            UpdateTargetPrediction();
            UpdateHomingDirection();
            MoveCurved();
        }
        else
        {
            base.Update();
        }

        timeAlive += Time.deltaTime;

        if (visualizePath && pathRenderer != null && pathRenderer.enabled)
        {
            UpdatePathVisualization();
        }
    }

    private void UpdateTargetPrediction()
    {
        if (playerTransform == null)
        {
            RefreshPlayerCache();
        }

        if (playerTransform == null)
            return;

        Vector3 velocity = playerRigidbody != null ? playerRigidbody.velocity : Vector3.zero;
        float predictTime = Mathf.Clamp(Vector3.Distance(transform.position, playerTransform.position) / Mathf.Max(0.01f, GetSpeed()), 0.05f, 1.5f);
        targetPosition = playerTransform.position + velocity * predictTime;
    }

    private void UpdateHomingDirection()
    {
        if (homingStrength <= 0f)
            return;

        Vector3 toTarget = targetPosition - transform.position;
        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        Vector3 desiredDirection = toTarget.normalized;
        initialDirection = Vector3.Slerp(initialDirection, desiredDirection, homingStrength * Time.deltaTime * 0.6f);
        dir = Vector3.Slerp(dir, desiredDirection, homingStrength * Time.deltaTime);
    }

    private void MoveCurved()
    {
        Vector3 newPosition = CalculateCurvedPosition();
        Vector3 movement = newPosition - transform.position;

        transform.position = newPosition;

        if (movement.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(movement.normalized);
        }
    }

    private Vector3 CalculateCurvedPosition()
    {
        Vector3 basePosition = startPosition + initialDirection * GetSpeed() * timeAlive;
        Vector3 curveOffset = Vector3.zero;

        switch (selectedCurveType)
        {
            case CurveType.SineWave:
                curveOffset = CalculateSineWave();
                break;
            case CurveType.Bezier:
                return CalculateBezierPosition();
            case CurveType.Spiral:
                curveOffset = CalculateSpiral();
                break;
            case CurveType.Zigzag:
                curveOffset = CalculateZigzag();
                break;
            case CurveType.Wave3D:
                curveOffset = CalculateWave3D();
                break;
        }

        if (homingStrength > 0f && playerTransform != null)
        {
            Vector3 toPlayer = targetPosition - (basePosition + curveOffset);
            curveOffset += toPlayer * homingStrength * Time.deltaTime * 0.5f;
        }

        return basePosition + curveOffset;
    }

    private Vector3 CalculateSineWave()
    {
        float time = timeAlive * curveFrequency;
        Vector3 right = Vector3.Cross(initialDirection, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, initialDirection).normalized;

        float sideOffset = Mathf.Sin(time) * curveIntensity;
        float verticalOffset = Mathf.Sin(time * 0.7f) * curveIntensity * 0.5f;

        return right * sideOffset + up * verticalOffset;
    }

    private Vector3 CalculateBezierPosition()
    {
        float totalDistance = Mathf.Max(0.01f, Vector3.Distance(startPosition, targetPosition));
        float t = Mathf.Clamp01(timeAlive * GetSpeed() / totalDistance);

        float oneMinusT = 1f - t;
        Vector3 position = oneMinusT * oneMinusT * oneMinusT * startPosition +
                          3f * oneMinusT * oneMinusT * t * controlPoint1 +
                          3f * oneMinusT * t * t * controlPoint2 +
                          t * t * t * targetPosition;

        return position;
    }

    private Vector3 CalculateSpiral()
    {
        float time = timeAlive * curveFrequency;
        Vector3 right = Vector3.Cross(initialDirection, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, initialDirection).normalized;

        float spiralX = Mathf.Cos(time) * spiralRadius * (1f + timeAlive * 0.5f);
        float spiralY = Mathf.Sin(time) * spiralRadius * (1f + timeAlive * 0.5f);

        return right * spiralX + up * spiralY;
    }

    private Vector3 CalculateZigzag()
    {
        float time = timeAlive * curveFrequency;
        Vector3 right = Vector3.Cross(initialDirection, Vector3.up).normalized;

        float zigzag = Mathf.Sign(Mathf.Sin(time)) * curveIntensity;
        float smooth = Mathf.Lerp(-curveIntensity, curveIntensity, (Mathf.Sin(time) + 1f) * 0.5f);

        return right * Mathf.Lerp(zigzag, smooth, 0.3f);
    }

    private Vector3 CalculateWave3D()
    {
        float time = timeAlive * curveFrequency;
        Vector3 right = Vector3.Cross(initialDirection, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, initialDirection).normalized;

        float waveX = Mathf.Sin(time) * curveIntensity;
        float waveY = Mathf.Cos(time * 1.3f) * curveIntensity * 0.7f;
        float waveZ = Mathf.Sin(time * 0.8f) * curveIntensity * 0.3f;

        return right * waveX + up * waveY + initialDirection * waveZ;
    }

    private void UpdatePathVisualization()
    {
        if (pathRenderer == null || !pathRenderer.enabled)
            return;

        pathPoints.Add(transform.position);

        if (pathPoints.Count > 50)
        {
            pathPoints.RemoveAt(0);
        }

        pathRenderer.positionCount = pathPoints.Count;
        pathRenderer.SetPositions(pathPoints.ToArray());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[DataPacket] Player hit by {selectedCurveType} curve attack!");
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.ConditionOverload(StatusBehaviour.Condition.Frozen, 0, 0.5f);
                playerStatus.DecreaseHealth(damage);
            }
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (pathRenderer != null)
        {
            Destroy(pathRenderer);
        }
    }

    private void ApplyVisualOverridesIfReady()
    {
        if (!pendingVisualOverride)
            return;

        if (!originalScaleCaptured)
        {
            originalScale = transform.localScale;
            originalScaleCaptured = true;
        }

        transform.localScale = originalScale * pendingScaleMultiplier;
        visualizePath = pendingShowPath;

        if (pathRenderer != null)
        {
            pathRenderer.enabled = pendingShowPath;
            pathRenderer.startWidth = basePathStartWidth * pendingPathWidthMultiplier;
            pathRenderer.endWidth = basePathEndWidth * pendingPathWidthMultiplier;
        }
    }

    public void ConfigureVisualProfile(float scaleMultiplier, bool showPath, float pathWidthMultiplier = 1f)
    {
        pendingVisualOverride = true;
        pendingScaleMultiplier = Mathf.Max(0.05f, scaleMultiplier);
        pendingShowPath = showPath;
        pendingPathWidthMultiplier = Mathf.Max(0.1f, pathWidthMultiplier);

        ApplyVisualOverridesIfReady();
    }

    public void SetCurveType(CurveType type)
    {
        curveType = type;
    }

    public void SetCurveIntensity(float intensity)
    {
        curveIntensity = intensity;
    }

    public void SetHomingStrength(float strength)
    {
        homingStrength = Mathf.Clamp01(strength);
    }

    public void EnableRandomCurve()
    {
        curveType = CurveType.Random;
    }
}
