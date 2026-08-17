using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 传送门（单扇）。两扇 Portal 通过 linkedPortal 互相关联，构成"穿墙传送门"。
///
/// 渲染（URP）：门内相机 = 主相机视角，渲染时临时隐藏墙和玩家，URP 自动渲染到 RenderTexture。
/// 传送：玩家从正面进入触发器，传送到对面门。
/// </summary>
[DefaultExecutionOrder(100)]
public class Portal : MonoBehaviour
{
    [Header("关联")]
    [Tooltip("对面那扇门（由 PortalGun 自动赋值）")]
    public Portal linkedPortal;

    [Tooltip("显示门内画面的渲染器（Quad 的 MeshRenderer）")]
    public MeshRenderer viewRenderer;

    [Header("尺寸与传送")]
    [Tooltip("门的直径（米）")]
    public float diameter = 2f;

    [Tooltip("触发器沿门法线方向的厚度（米）")]
    public float triggerDepth = 0.6f;

    [Tooltip("传送后出现在出口门前方多远处（米）")]
    public float teleportOffset = 0.4f;

    [Tooltip("传送后的冷却时间（秒）")]
    public float teleportCooldown = 0.3f;

    [Header("传送对象")]
    [Tooltip("只有带这个 Tag 的对象才会被传送（玩家请标 Player）")]
    public string playerTag = "Player";

    [Header("渲染")]
    [Tooltip("门内画面分辨率缩放：1 = 与屏幕相同")]
    [Range(0.25f, 1f)]
    public float renderScale = 1f;

    Camera portalCamera;
    Camera mainCamera;
    RenderTexture renderTexture;
    float cooldownEndTime;

    // 门内相机渲染时要临时隐藏的物体（由 PortalGun 赋值）
    public Renderer[] wallRenderers;
    public Renderer[] playerRenderers;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    // 门内相机渲染前隐藏墙和玩家，渲染后立即恢复，实现"透视"到墙对面。
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam == portalCamera)
        {
            SetRenderersVisible(wallRenderers, false);
            SetRenderersVisible(playerRenderers, false);
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam == portalCamera)
        {
            SetRenderersVisible(wallRenderers, true);
            SetRenderersVisible(playerRenderers, true);
        }
    }

    void Awake()
    {
        SetLayerRecursively(transform, "Portal");

        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc == null) bc = gameObject.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.center = Vector3.zero;
        bc.size = new Vector3(diameter, diameter, triggerDepth);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (viewRenderer == null) viewRenderer = GetComponentInChildren<MeshRenderer>();
        if (viewRenderer != null)
            viewRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);

        CreateCameraAndTexture();
    }

    void CreateCameraAndTexture()
    {
        int w = Mathf.Max(1, Mathf.RoundToInt(Screen.width * renderScale));
        int h = Mathf.Max(1, Mathf.RoundToInt(Screen.height * renderScale));
        renderTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        renderTexture.name = "Portal RT " + name;

        GameObject camGO = new GameObject("Portal Camera");
        camGO.transform.SetParent(transform, false);
        portalCamera = camGO.AddComponent<Camera>();
        portalCamera.targetTexture = renderTexture;
        portalCamera.clearFlags = CameraClearFlags.Skybox;

        // URP 需要 UniversalAdditionalCameraData
        var urpData = portalCamera.GetUniversalAdditionalCameraData();
        urpData.renderType = CameraRenderType.Base;

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            portalCamera.fieldOfView = mainCamera.fieldOfView;
            portalCamera.nearClipPlane = 0.01f;
            portalCamera.farClipPlane = mainCamera.farClipPlane;

            // 让门内相机先于主相机渲染（先渲染 RT，主相机再采样）
            portalCamera.depth = mainCamera.depth - 1;

            int portalLayer = LayerMask.NameToLayer("Portal");
            portalCamera.cullingMask = portalLayer >= 0
                ? mainCamera.cullingMask & ~(1 << portalLayer)
                : mainCamera.cullingMask;
        }

        if (viewRenderer != null)
        {
            if (viewRenderer.sharedMaterial != null)
                viewRenderer.material = new Material(viewRenderer.sharedMaterial);
            viewRenderer.material.SetTexture("_MainTex", renderTexture);
        }
    }

    void LateUpdate()
    {
        UpdatePortalCamera();
    }

    void UpdatePortalCamera()
    {
        if (portalCamera == null || linkedPortal == null) return;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // 门内相机 = 主相机视角（同位置同朝向）。URP 在渲染阶段自动渲染它到 RT。
        portalCamera.transform.position = mainCamera.transform.position;
        portalCamera.transform.rotation = mainCamera.transform.rotation;
    }

    void SetRenderersVisible(Renderer[] renderers, bool visible)
    {
        if (renderers == null) return;
        foreach (Renderer r in renderers)
            if (r != null) r.enabled = visible;
    }

    // ---- 传送逻辑 ----
    void OnTriggerEnter(Collider other)
    {
        if (linkedPortal == null) return;
        if (Time.time < cooldownEndTime) return;
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;

        Vector3 vel = GetVelocity(other);
        if (vel.sqrMagnitude > 0.0001f && Vector3.Dot(vel, transform.forward) > 0f)
            return;

        Teleport(other.transform);
    }

    void Teleport(Transform obj)
    {
        Portal from = this;
        Portal to = linkedPortal;

        Vector3 relPos = from.transform.InverseTransformPoint(obj.position);
        Vector3 newPos = to.transform.TransformPoint(relPos) + to.transform.forward * teleportOffset;
        Quaternion newRot = obj.rotation;

        MoveTransform(obj, newPos, newRot);

        from.cooldownEndTime = Time.time + teleportCooldown;
        to.cooldownEndTime = Time.time + teleportCooldown;
    }

    Vector3 GetVelocity(Component c)
    {
        Rigidbody rb = c.GetComponent<Rigidbody>();
        if (rb != null) return rb.velocity;
        CharacterController cc = c.GetComponent<CharacterController>();
        if (cc != null) return cc.velocity;
        return Vector3.zero;
    }

    void MoveTransform(Transform t, Vector3 pos, Quaternion rot)
    {
        CharacterController cc = t.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            t.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
            return;
        }

        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.position = pos;
            rb.rotation = rot;
            return;
        }

        t.SetPositionAndRotation(pos, rot);
    }

    void SetLayerRecursively(Transform t, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0) return;
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerRecursively(child, layerName);
    }

    void OnDestroy()
    {
        if (renderTexture != null)
            renderTexture.Release();
    }
}
