using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPacket : MProjectile
{
    [Header("🌀 Curved Attack Settings")]
    [SerializeField] private bool useCurvedMovement = true;
    [SerializeField] private CurveType curveType = CurveType.Random;
    [SerializeField] private float curveIntensity = 2f;        // 곡선 강도
    [SerializeField] private float curveFrequency = 3f;        // 곡선 빈도
    [SerializeField] private float spiralRadius = 1f;          // 나선형 반지름
    [SerializeField] private bool visualizePath = true;        // 경로 시각화
    [SerializeField] private float lifeTime = 10.0f;
    
    [Header("🎯 Targeting")]
    [SerializeField] private float homingStrength = 1f;        // 플레이어 추적 강도 (0=추적안함, 1=완전추적)
    
    // 곡선 타입 열거형
    public enum CurveType
    {
        SineWave,     // 사인파 (구불구불)
        Bezier,       // 베지어 곡선
        Spiral,       // 나선형
        Zigzag,       // 지그재그
        Wave3D,       // 3D 파동
        Random        // 랜덤 선택
    }
    
    // 곡선 이동 변수들
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 initialDirection;
    private float timeAlive = 0f;
    private CurveType selectedCurveType;
    private PostProcessingManager postProcessing;
    
    // 베지어 곡선용 제어점들
    private Vector3 controlPoint1;
    private Vector3 controlPoint2;
    
    // 경로 시각화용
    private LineRenderer pathRenderer;
    private List<Vector3> pathPoints = new List<Vector3>();

    void Start()
    {
        postProcessing = FindObjectOfType<PostProcessingManager>();
        Debug.Log("PostProcessing reference: " + (postProcessing != null ? "Found" : "Not Found"));
        
        if (useCurvedMovement)
        {
            InitializeCurvedMovement();
        }
    }
    
    private void InitializeCurvedMovement()
    {
        startPosition = transform.position;
        
        // 플레이어 위치를 타겟으로 설정 (약간의 예측 추가)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 playerVelocity = Vector3.zero;
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerVelocity = playerRb.velocity;
            }
            
            // 플레이어 위치 예측 (선행 조준)
            float predictTime = Vector3.Distance(startPosition, player.transform.position) / speed;
            targetPosition = player.transform.position + playerVelocity * predictTime;
        }
        else
        {
            targetPosition = startPosition + transform.forward * 20f;
        }
        
        initialDirection = dir.normalized;
        
        // 곡선 타입 선택
        selectedCurveType = curveType == CurveType.Random ? 
            (CurveType)Random.Range(0, System.Enum.GetValues(typeof(CurveType)).Length - 1) : 
            curveType;
        
        Debug.Log($"🌀 DataPacket fired with {selectedCurveType} curve!");
        
        // 곡선 타입별 초기화
        switch (selectedCurveType)
        {
            case CurveType.Bezier:
                InitializeBezierCurve();
                break;
            case CurveType.Spiral:
                // 나선형은 별도 초기화 필요 없음
                break;
        }
        
        // 경로 시각화 설정
        if (visualizePath)
        {
            SetupPathVisualization();
        }
    }
    
    private void InitializeBezierCurve()
    {
        Vector3 toTarget = targetPosition - startPosition;
        Vector3 perpendicular = Vector3.Cross(toTarget, Vector3.up).normalized;
        
        // 제어점들을 랜덤하게 배치
        float distance = toTarget.magnitude;
        controlPoint1 = startPosition + toTarget * 0.33f + perpendicular * Random.Range(-curveIntensity, curveIntensity) * distance * 0.3f;
        controlPoint2 = startPosition + toTarget * 0.66f + perpendicular * Random.Range(-curveIntensity, curveIntensity) * distance * 0.3f;
        
        // Y축 변화도 추가 (3D 베지어)
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
            MoveCurved();
        }
        else
        {
            // 기본 직선 이동
            base.Update();
        }
        
        timeAlive += Time.deltaTime;
        
        // 경로 시각화 업데이트
        if (visualizePath && pathRenderer != null)
        {
            UpdatePathVisualization();
        }
    }
    
    private void MoveCurved()
    {
        Vector3 newPosition = CalculateCurvedPosition();
        Vector3 movement = newPosition - transform.position;
        
        // 이동
        transform.position = newPosition;
        
        // 이동 방향으로 회전
        if (movement.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(movement.normalized);
        }
    }
    
    private Vector3 CalculateCurvedPosition()
    {
        Vector3 basePosition = startPosition + initialDirection * speed * timeAlive;
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
        
        // 플레이어 추적 요소 추가
        if (homingStrength > 0f)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 toPlayer = (player.transform.position - transform.position).normalized;
                curveOffset += toPlayer * homingStrength * Time.deltaTime * speed * 0.1f;
            }
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
        float t = Mathf.Clamp01(timeAlive * speed / Vector3.Distance(startPosition, targetPosition));
        
        // 3차 베지어 곡선: B(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃
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
        pathPoints.Add(transform.position);
        
        // 최대 50개 점으로 제한
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
            Debug.Log($"🎯 Player hit by {selectedCurveType} curve attack!");
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
        // 경로 시각화 정리
        if (pathRenderer != null)
        {
            Destroy(pathRenderer);
        }
    }
    
    // 곡선 설정을 위한 공개 메서드들
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
