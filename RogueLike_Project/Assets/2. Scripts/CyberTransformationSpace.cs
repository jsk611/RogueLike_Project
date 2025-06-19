using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 사이버 변신 공간 시스템 - 완전한 시스템 재구성 연출
/// </summary>
public class CyberTransformationSpace : MonoBehaviour
{
    [Header("Cyber Space")]
    [SerializeField] private GameObject cyberSpaceSphere;
    [SerializeField] private Material cyberSpaceMaterial;
    
    [Header("Settings")]
    [SerializeField] private float spaceRadius = 3f;
    [SerializeField] private float formationTime = 1.5f;
    [SerializeField] private float maintainTime = 2f;
    [SerializeField] private float dissipationTime = 1.5f;
    
    [Header("Colors")]
    [SerializeField] private Color cyberColor = new Color(0.2f, 1f, 0.8f, 0.8f);
    [SerializeField] private Color glitchColor = new Color(1f, 0.3f, 0.8f, 0.6f);
    
    [Header("Cyber Effects")]
    [SerializeField] private float glitchIntensity = 2f;
    [SerializeField] private float energyFlow = 1.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spaceFormSound;
    [SerializeField] private AudioClip spaceDissolveSound;
    [SerializeField] private AudioClip dataScanSound;
    [SerializeField] private AudioClip firewallSound;
    [SerializeField] private AudioClip kernelBootSound;
    [SerializeField] private AudioClip compileSound;
    [SerializeField] private AudioClip systemBootSound;
    
    [Header("System Recompilation Settings")]
    [SerializeField] private Material dataMaterial;
    [SerializeField] private Material firewallMaterial;
    [SerializeField] private Material kernelMaterial;
    [SerializeField] private ParticleSystem dataStreamFX;
    [SerializeField] private ParticleSystem compilationFX;
    [SerializeField] private GameObject hologramUIPrefab;
    
    [Header("Cube Integration")]
    [SerializeField] private float cubeSize = 6f;
    [SerializeField] private bool debugMode = false;
    
    // Private variables
    private bool isActive = false;
    private Material materialInstance;
    private Vector3 originalScale;
    
    // Cube transformation related
    private List<Transform> voxels = new List<Transform>();
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> formationPositions = new Dictionary<Transform, Vector3>();
    
    // UI management
    private List<GameObject> activeHolograms = new List<GameObject>();
    private GameObject currentProgressBar;
    
    void Start()
    {
        InitializeComponents();
        InitializeCubeSystem();
        DeactivateAllEffects();
    }
    
    private void InitializeComponents()
    {
        // Create cyber space sphere if not assigned
        if (cyberSpaceSphere == null)
        {
            cyberSpaceSphere = CreateCyberSpaceSphere();
        }
        
        // Setup material
        MeshRenderer renderer = cyberSpaceSphere.GetComponent<MeshRenderer>();
        if (cyberSpaceMaterial != null)
        {
            materialInstance = new Material(cyberSpaceMaterial);
        }
        else
        {
            materialInstance = CreateDefaultCyberMaterial();
        }
        
        renderer.material = materialInstance;
        originalScale = cyberSpaceSphere.transform.localScale;
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.8f; // 3D audio
        }
        
        Debug.Log("[CyberSpace] 초기화 완료");
    }
    
    private void InitializeCubeSystem()
    {
        // Find and setup voxel references from child objects
        RefreshVoxelReferences();
        
        // Setup formation positions (target positions for transformation)
        SetupFormationPositions();
    }
    
    private void RefreshVoxelReferences()
    {
        voxels.Clear();
        originalPositions.Clear();
        
        // Find all potential voxel objects in children
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child != transform && (child.name.Contains("Voxel") || child.name.Contains("Cube") || child.name.Contains("Block")))
            {
                voxels.Add(child);
                originalPositions[child] = child.localPosition;
                
                if (debugMode)
                    Debug.Log($"[CyberSpace] Voxel found: {child.name}");
            }
        }
        
        Debug.Log($"[CyberSpace] {voxels.Count}개의 복셀 발견");
    }
    
    private void SetupFormationPositions()
    {
        formationPositions.Clear();
        
        // Create target positions for voxels (example: random sphere formation)
        for (int i = 0; i < voxels.Count; i++)
        {
            Vector3 targetPos = Random.insideUnitSphere * cubeSize * 0.5f;
            formationPositions[voxels[i]] = targetPos;
        }
    }
    
    private GameObject CreateCyberSpaceSphere()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "CyberSpace";
        sphere.transform.SetParent(transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.zero;
        
        // Remove collider
        Collider collider = sphere.GetComponent<Collider>();
        if (collider != null) DestroyImmediate(collider);
        
        return sphere;
    }
    
    private Material CreateDefaultCyberMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = cyberColor;
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = 3000;
        
        return material;
    }
    
    private void DeactivateAllEffects()
    {
        if (cyberSpaceSphere != null) 
            cyberSpaceSphere.SetActive(false);
        
        HideAllHolograms();
        isActive = false;
    }
    
    #region Public API
    
    public void ActivateCyberTransformation(GameObject targetObject = null, System.Action onTransformCallback = null)
    {
        if (isActive) return;
        
        if (targetObject != null) 
            CalculateTargetBounds(targetObject);
        
        RefreshVoxelReferences();
        isActive = true;
        StartCoroutine(CyberTransformationSequence(onTransformCallback));
    }
    
    public void DeactivateTransformation()
    {
        if (!isActive) return;
        
        StopAllCoroutines();
        StartCoroutine(EmergencyShutdown());
    }
    
    public bool IsTransforming => isActive;
    
    #endregion
    
    #region Bounds Calculation
    
    public void CalculateTargetBounds(GameObject targetObject)
    {
        Bounds totalBounds = new Bounds(targetObject.transform.position, Vector3.zero);
        
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            totalBounds.Encapsulate(renderer.bounds);
        }
        
        spaceRadius = Mathf.Max(totalBounds.size.x, totalBounds.size.y, totalBounds.size.z) * 0.8f;
        originalScale = Vector3.one * spaceRadius * 2f;
        
        Debug.Log($"[CyberSpace] 경계 계산 완료: {spaceRadius}");
    }
    
    #endregion
    
    #region Main Sequence
    
    private IEnumerator CyberTransformationSequence(System.Action onTransformCallback)
    {
        Debug.Log("[CyberSpace] 사이버 변신 시퀀스 시작");
        
        // Phase 1: System Recompilation
        yield return StartCoroutine(SystemRecompilationPhase());
        
        // Phase 2: Cyber Space Formation
        yield return StartCoroutine(FormCyberSpace());
        
        // Phase 3: Transformation Execution
        yield return StartCoroutine(MaintainSpaceAndTransform(onTransformCallback));
        
        // Phase 4: Space Dissolution
        yield return StartCoroutine(DissolveCyberSpace());
        
        isActive = false;
        Debug.Log("[CyberSpace] 사이버 변신 시퀀스 완료");
    }
    
    private IEnumerator EmergencyShutdown()
    {
        ShowHologramMessage("EMERGENCY SHUTDOWN");
        
        // Stop all particle effects
        if (dataStreamFX != null && dataStreamFX.isPlaying)
            dataStreamFX.Stop();
        if (compilationFX != null && compilationFX.isPlaying)
            compilationFX.Stop();
            
        HideAllHolograms();
        
        if (cyberSpaceSphere != null)
            cyberSpaceSphere.SetActive(false);
        
        yield return new WaitForSeconds(0.5f);
        isActive = false;
    }
    
    #endregion
    
    #region System Recompilation Phase
    
    private IEnumerator SystemRecompilationPhase()
    {
        Debug.Log("[Cyber] 시스템 재구성 단계");
        
        yield return StartCoroutine(DataCollectionPhase());
        yield return StartCoroutine(FirewallConstructionPhase());
        yield return StartCoroutine(KernelActivationPhase());
        yield return StartCoroutine(CodeCompilationPhase());
        yield return StartCoroutine(SystemBootPhase());
        
        Debug.Log("[Cyber] 시스템 재구성 완료");
    }
    
    private IEnumerator DataCollectionPhase()
    {
        Debug.Log("[Cyber] 데이터 수집 단계");
        ShowHologramMessage("SCANNING SYSTEM ARCHITECTURE...");
        PlayCyberSound("DataScan");
        
        // Activate data stream effect
        if (dataStreamFX != null)
        {
            dataStreamFX.transform.position = transform.position;
            dataStreamFX.Play();
        }
        
        // Convert voxels to data material
        foreach (var voxel in voxels)
        {
            StartCoroutine(ConvertToDataMaterial(voxel, 1.5f));
        }
        
        yield return new WaitForSeconds(1.5f);
        
        ShowHologramMessage("DATA COLLECTION: 100%");
        if (dataStreamFX != null) dataStreamFX.Stop();
    }
    
    private IEnumerator ConvertToDataMaterial(Transform voxel, float duration)
    {
        if (voxel == null) yield break;
        
        var renderer = voxel.GetComponent<Renderer>();
        if (renderer == null || dataMaterial == null) yield break;
        
        Material originalMat = renderer.material;
        Material dataMaterialInstance = new Material(dataMaterial);
        
        float timer = 0f;
        while (timer < duration && voxel != null)
        {
            float progress = timer / duration;
            
            // Transition to data material
            Color color = dataMaterialInstance.color;
            color.a = Mathf.Lerp(1f, 0.3f, progress);
            dataMaterialInstance.color = color;
            renderer.material = dataMaterialInstance;
            
            // Glitch effect
            if (Random.Range(0f, 1f) < 0.1f)
            {
                Vector3 originalPos = originalPositions.ContainsKey(voxel) ? originalPositions[voxel] : voxel.localPosition;
                voxel.localPosition += Random.insideUnitSphere * 0.05f;
                yield return new WaitForSeconds(0.02f);
                if (voxel != null) // Check again after wait
                    voxel.localPosition = originalPos;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator FirewallConstructionPhase()
    {
        Debug.Log("[Cyber] 방화벽 구축 단계");
        ShowHologramMessage("BUILDING SECURITY PERIMETER...");
        PlayCyberSound("FirewallActivate");
        
        yield return StartCoroutine(CreateFirewallGrid());
        
        ShowHologramMessage("FIREWALL STATUS: ACTIVE");
        yield return new WaitForSeconds(1.0f);
    }
    
    private IEnumerator CreateFirewallGrid()
    {
        GameObject firewallGrid = new GameObject("FirewallGrid");
        firewallGrid.transform.SetParent(transform);
        firewallGrid.transform.localPosition = Vector3.zero;
        
        List<LineRenderer> lines = new List<LineRenderer>();
        
        // Create grid lines around the cube
        for (int face = 0; face < 6; face++)
        {
            for (int line = 0; line < 8; line++)
            {
                GameObject lineObj = new GameObject($"GridLine_{face}_{line}");
                lineObj.transform.SetParent(firewallGrid.transform);
                
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                if (firewallMaterial != null)
                {
                    lr.material = new Material(firewallMaterial);
                }
                lr.startWidth = 0.02f;
                lr.endWidth = 0.02f;
                lr.positionCount = 2;
                
                Vector3[] positions = CalculateGridLinePositions(face, line);
                lr.SetPositions(positions);
                lines.Add(lr);
            }
        }
        
        // Animate grid appearance
        float duration = 0.8f;
        float timer = 0f;
        
        while (timer < duration)
        {
            float progress = timer / duration;
            
            foreach (var line in lines)
            {
                if (line.material != null)
                {
                    Color gridColor = line.material.color;
                    gridColor.a = progress;
                    line.material.color = gridColor;
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        Destroy(firewallGrid);
    }
    
    private Vector3[] CalculateGridLinePositions(int face, int line)
    {
        float halfSize = cubeSize * 0.5f;
        Vector3[] positions = new Vector3[2];
        
        switch (face)
        {
            case 0: // Front face
                positions[0] = new Vector3(-halfSize + line * (cubeSize / 8f), -halfSize, halfSize);
                positions[1] = new Vector3(-halfSize + line * (cubeSize / 8f), halfSize, halfSize);
                break;
            case 1: // Back face
                positions[0] = new Vector3(-halfSize + line * (cubeSize / 8f), -halfSize, -halfSize);
                positions[1] = new Vector3(-halfSize + line * (cubeSize / 8f), halfSize, -halfSize);
                break;
            case 2: // Left face
                positions[0] = new Vector3(-halfSize, -halfSize + line * (cubeSize / 8f), halfSize);
                positions[1] = new Vector3(-halfSize, -halfSize + line * (cubeSize / 8f), -halfSize);
                break;
            case 3: // Right face
                positions[0] = new Vector3(halfSize, -halfSize + line * (cubeSize / 8f), halfSize);
                positions[1] = new Vector3(halfSize, -halfSize + line * (cubeSize / 8f), -halfSize);
                break;
            case 4: // Top face
                positions[0] = new Vector3(-halfSize + line * (cubeSize / 8f), halfSize, -halfSize);
                positions[1] = new Vector3(-halfSize + line * (cubeSize / 8f), halfSize, halfSize);
                break;
            case 5: // Bottom face
                positions[0] = new Vector3(-halfSize + line * (cubeSize / 8f), -halfSize, -halfSize);
                positions[1] = new Vector3(-halfSize + line * (cubeSize / 8f), -halfSize, halfSize);
                break;
            default:
                positions[0] = Vector3.zero;
                positions[1] = Vector3.up;
                break;
        }
        
        return positions;
    }
    
    private IEnumerator KernelActivationPhase()
    {
        Debug.Log("[Cyber] 커널 활성화 단계");
        ShowHologramMessage("INITIALIZING SYSTEM KERNEL...");
        PlayCyberSound("KernelBoot");
        
        GameObject kernelCore = CreateKernelCore();
        yield return StartCoroutine(KernelScanEffect(kernelCore));
        
        ShowHologramMessage("KERNEL STATUS: ONLINE");
        
        yield return new WaitForSeconds(0.5f);
        Destroy(kernelCore);
    }
    
    private GameObject CreateKernelCore()
    {
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "KernelCore";
        core.transform.SetParent(transform);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * 0.3f;
        
        var renderer = core.GetComponent<Renderer>();
        if (kernelMaterial != null)
        {
            renderer.material = new Material(kernelMaterial);
        }
        
        core.AddComponent<KernelRotator>();
        
        return core;
    }
    
    private IEnumerator KernelScanEffect(GameObject kernel)
    {
        float scanDuration = 0.8f;
        float timer = 0f;
        
        while (timer < scanDuration)
        {
            float progress = timer / scanDuration;
            float scanRadius = Mathf.Lerp(0f, cubeSize, progress);
            
            foreach (var voxel in voxels)
            {
                float distance = Vector3.Distance(voxel.position, kernel.transform.position);
                
                if (distance <= scanRadius && distance > scanRadius - 0.5f)
                {
                    StartCoroutine(VoxelScanHighlight(voxel));
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator VoxelScanHighlight(Transform voxel)
    {
        if (voxel == null) yield break;
        
        var renderer = voxel.GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        Material originalMat = renderer.material;
        if (originalMat == null) yield break;
        
        Color originalColor = originalMat.color;
        
        float highlightDuration = 0.2f;
        float timer = 0f;
        
        while (timer < highlightDuration && voxel != null)
        {
            float intensity = Mathf.Sin(timer / highlightDuration * Mathf.PI);
            Color highlightColor = Color.Lerp(originalColor, Color.cyan, intensity * 0.8f);
            
            Material tempMat = new Material(originalMat);
            tempMat.color = highlightColor;
            renderer.material = tempMat;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // Restore original material safely
        if (voxel != null && renderer != null && originalMat != null)
            renderer.material = originalMat;
    }
    
    private IEnumerator CodeCompilationPhase()
    {
        Debug.Log("[Cyber] 코드 컴파일 단계");
        ShowHologramMessage("COMPILING NEW ARCHITECTURE...");
        PlayCyberSound("Compile");
        
        yield return StartCoroutine(ShowCompilationProgress());
        yield return StartCoroutine(AssembleNewStructure());
        
        ShowHologramMessage("COMPILATION: SUCCESS");
    }
    
    private IEnumerator ShowCompilationProgress()
    {
        currentProgressBar = CreateHologramProgressBar();
        
        float compileDuration = 1.5f;
        float timer = 0f;
        
        while (timer < compileDuration)
        {
            float progress = timer / compileDuration;
            
            UpdateProgressBar(currentProgressBar, progress);
            
            if (progress < 0.3f)
                ShowHologramMessage($"PARSING CODE... {(int)(progress * 100)}%");
            else if (progress < 0.7f)
                ShowHologramMessage($"OPTIMIZING... {(int)(progress * 100)}%");
            else
                ShowHologramMessage($"LINKING... {(int)(progress * 100)}%");
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (currentProgressBar != null) 
        {
            Destroy(currentProgressBar);
            currentProgressBar = null;
        }
    }
    
    private GameObject CreateHologramProgressBar()
    {
        GameObject progressBar = new GameObject("HologramProgressBar");
        progressBar.transform.SetParent(transform);
        progressBar.transform.localPosition = Vector3.up * 2f;
        
        // Create simple visual progress bar
        GameObject barObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barObject.transform.SetParent(progressBar.transform);
        barObject.transform.localPosition = Vector3.zero;
        barObject.transform.localScale = new Vector3(0f, 0.1f, 0.1f);
        
        var renderer = barObject.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.cyan;
        renderer.material.SetFloat("_Mode", 3);
        renderer.material.EnableKeyword("_ALPHABLEND_ON");
        
        return progressBar;
    }
    
    private void UpdateProgressBar(GameObject progressUI, float progress)
    {
        if (progressUI != null)
        {
            Transform barTransform = progressUI.transform.GetChild(0);
            if (barTransform != null)
            {
                barTransform.localScale = new Vector3(progress * 3f, 0.1f, 0.1f);
            }
        }
    }
    
    private IEnumerator AssembleNewStructure()
    {
        foreach (var voxel in voxels)
        {
            if (!formationPositions.ContainsKey(voxel)) continue;
            
            Vector3 targetPos = formationPositions[voxel];
            float delay = Random.Range(0f, 0.5f);
            
            StartCoroutine(CompileVoxelToPosition(voxel, targetPos, delay));
        }
        
        yield return new WaitForSeconds(2.0f);
    }
    
    private IEnumerator CompileVoxelToPosition(Transform voxel, Vector3 targetPos, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Vector3 startPos = voxel.localPosition;
        float moveDuration = 1.5f;
        float timer = 0f;
        
        while (timer < moveDuration)
        {
            float progress = timer / moveDuration;
            voxel.localPosition = Vector3.Lerp(startPos, targetPos, progress);
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        voxel.localPosition = targetPos;
    }
    
    private IEnumerator SystemBootPhase()
    {
        Debug.Log("[Cyber] 시스템 부팅 단계");
        ShowHologramMessage("BOOTING NEW SYSTEM...");
        PlayCyberSound("SystemBoot");
        
        string[] bootMessages = {
            "LOADING DRIVERS...",
            "MOUNTING FILESYSTEMS...",
            "STARTING SERVICES...",
            "SYSTEM READY"
        };
        
        foreach (string message in bootMessages)
        {
            ShowHologramMessage(message);
            yield return new WaitForSeconds(0.3f);
        }
        
        yield return StartCoroutine(FinalSystemActivation());
        HideAllHolograms();
    }
    
    private IEnumerator FinalSystemActivation()
    {
        foreach (var voxel in voxels)
        {
            StartCoroutine(VoxelSystemOnline(voxel));
            yield return new WaitForSeconds(0.02f);
        }
        
        if (compilationFX != null)
        {
            compilationFX.transform.position = transform.position;
            compilationFX.Emit(100);
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator VoxelSystemOnline(Transform voxel)
    {
        if (voxel == null) yield break;
        
        var renderer = voxel.GetComponent<Renderer>();
        if (renderer == null || renderer.material == null) yield break;
        
        Material originalMat = renderer.material;
        Color originalColor = originalMat.color;
        float pulseDuration = 0.3f;
        float timer = 0f;
        
        while (timer < pulseDuration && voxel != null)
        {
            float intensity = Mathf.Sin(timer / pulseDuration * Mathf.PI);
            Color pulseColor = Color.Lerp(originalColor, Color.green, intensity * 0.5f);
            
            Material tempMat = new Material(originalMat);
            tempMat.color = pulseColor;
            renderer.material = tempMat;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // Restore original material
        if (voxel != null && renderer != null && originalMat != null)
            renderer.material = originalMat;
    }
    
    #endregion
    
    #region Cyber Space Management
    
    private IEnumerator FormCyberSpace()
    {
        Debug.Log("[CyberSpace] 사이버 공간 형성");
        
        if (cyberSpaceSphere != null)
        {
            cyberSpaceSphere.SetActive(true);
            
            if (audioSource != null && spaceFormSound != null)
            {
                audioSource.PlayOneShot(spaceFormSound);
            }
            
            float timer = 0f;
            while (timer < formationTime)
            {
                float progress = timer / formationTime;
                float scale = Mathf.Lerp(0f, 1f, progress);
                
                cyberSpaceSphere.transform.localScale = originalScale * scale;
                UpdateMaterial(progress, timer);
                
                timer += Time.deltaTime;
                yield return null;
            }
            
            cyberSpaceSphere.transform.localScale = originalScale;
        }
    }
    
    private IEnumerator MaintainSpaceAndTransform(System.Action onTransformCallback)
    {
        Debug.Log("[CyberSpace] 변신 실행 및 공간 유지");
        
        onTransformCallback?.Invoke();
        
        float timer = 0f;
        while (timer < maintainTime)
        {
            UpdateMaterial(1f, timer);
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator DissolveCyberSpace()
    {
        Debug.Log("[CyberSpace] 사이버 공간 해제");
        
        if (audioSource != null && spaceDissolveSound != null)
        {
            audioSource.PlayOneShot(spaceDissolveSound);
        }
        
        float timer = 0f;
        while (timer < dissipationTime)
        {
            float progress = timer / dissipationTime;
            float scale = Mathf.Lerp(1f, 0f, progress);
            float alpha = Mathf.Lerp(1f, 0f, progress);
            
            if (cyberSpaceSphere != null)
            {
                cyberSpaceSphere.transform.localScale = originalScale * scale;
            }
            
            UpdateMaterial(alpha, timer);
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (cyberSpaceSphere != null)
        {
            cyberSpaceSphere.SetActive(false);
        }
    }
    
    private void UpdateMaterial(float intensity, float time)
    {
        if (materialInstance == null) return;
        
        if (materialInstance.HasProperty("_Alpha"))
            materialInstance.SetFloat("_Alpha", intensity * 0.8f);
        if (materialInstance.HasProperty("_GlitchSpeed"))
            materialInstance.SetFloat("_GlitchSpeed", 2f * intensity);
        if (materialInstance.HasProperty("_WaveSpeed"))
            materialInstance.SetFloat("_WaveSpeed", 1.5f * intensity);
        if (materialInstance.HasProperty("_EmissionPower"))
            materialInstance.SetFloat("_EmissionPower", 3f * intensity);
        
        // Update basic material properties
        Color color = materialInstance.color;
        color.a = intensity * cyberColor.a;
        materialInstance.color = color;
    }
    
    #endregion
    
    #region UI Management
    
    private void ShowHologramMessage(string message)
    {
        if (hologramUIPrefab != null)
        {
            var hologram = Instantiate(hologramUIPrefab, transform.position + Vector3.up * 3f, Quaternion.identity);
            var textComponent = hologram.GetComponentInChildren<Text>();
            if (textComponent != null) textComponent.text = message;
            
            activeHolograms.Add(hologram);
            StartCoroutine(DestroyHologramAfterDelay(hologram, 3f));
        }
        
        if (debugMode)
            Debug.Log($"[CyberUI] {message}");
    }
    
    private IEnumerator DestroyHologramAfterDelay(GameObject hologram, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (hologram != null)
        {
            activeHolograms.Remove(hologram);
            Destroy(hologram);
        }
    }
    
    private void HideAllHolograms()
    {
        // Clean up active holograms
        for (int i = activeHolograms.Count - 1; i >= 0; i--)
        {
            if (activeHolograms[i] != null)
            {
                Destroy(activeHolograms[i]);
            }
        }
        activeHolograms.Clear();
        
        // Clean up progress bar
        if (currentProgressBar != null)
        {
            Destroy(currentProgressBar);
            currentProgressBar = null;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up on component destruction
        StopAllCoroutines();
        HideAllHolograms();
        
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
            materialInstance = null;
        }
    }
    
    #endregion
    
    #region Audio Management
    
    private void PlayCyberSound(string soundType)
    {
        if (audioSource == null) return;
        
        AudioClip clip = null;
        switch (soundType)
        {
            case "DataScan": clip = dataScanSound; break;
            case "FirewallActivate": clip = firewallSound; break;
            case "KernelBoot": clip = kernelBootSound; break;
            case "Compile": clip = compileSound; break;
            case "SystemBoot": clip = systemBootSound; break;
        }
        
        if (clip != null) audioSource.PlayOneShot(clip);
    }
    
    #endregion
    
    #region Debug & Gizmos
    
    void OnDrawGizmosSelected()
    {
        // Draw cyber space area
        Gizmos.color = new Color(0.2f, 1f, 0.8f, 0.3f);
        Gizmos.DrawSphere(transform.position, spaceRadius);
        
        Gizmos.color = cyberColor;
        Gizmos.DrawWireSphere(transform.position, spaceRadius);
        
        // Draw cube bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * cubeSize);
        
        // Draw voxel positions
        if (Application.isPlaying && voxels != null)
        {
            Gizmos.color = Color.green;
            foreach (var voxel in voxels)
            {
                if (voxel != null)
                    Gizmos.DrawWireSphere(voxel.position, 0.1f);
            }
        }
    }
    
    #endregion
    
    #region Helper Components
    
    public class KernelRotator : MonoBehaviour
    {
        public Vector3 rotationSpeed = new Vector3(0, 90, 0);
        
        void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
    
    #endregion
} 