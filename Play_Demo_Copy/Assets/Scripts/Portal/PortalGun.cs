using UnityEngine;

/// <summary>
/// 传送门放置器（挂到玩家或主相机上）。
///
/// 鼠标左键：从相机发射一条固定长度的射线
///   → 命中墙的"正面"
///   → 继续穿过墙，找到同一面墙的"背面"
///   → 在正、反面各生成一扇门并互相关联，构成穿墙传送门。
/// </summary>
public class PortalGun : MonoBehaviour
{
    [Header("预制体")]
    [Tooltip("传送门预制体（用菜单 Tools/Portal/一键创建传送门预制体 生成）")]
    public GameObject portalPrefab;

    [Header("射线")]
    [Tooltip("鼠标射线最大距离")]
    public float maxDistance = 100f;

    [Tooltip("只检测这些层（留空 = 全部）。建议只勾选墙所在的层")]
    public LayerMask wallLayerMask = ~0;

    [Tooltip("检测不到墙背面时，按这个默认墙厚生成背面的门")]
    public float defaultWallThickness = 1f;

    [Tooltip("门贴在墙表面外移的距离，防止与墙重叠闪烁")]
    public float surfaceOffset = 0.02f;

    [Header("门")]
    [Tooltip("门的存活时间（秒）")]
    public float portalLifetime = 15f;

    [Tooltip("门的大小（直径，米）")]
    public float portalDiameter = 2f;

    [Header("传送对象")]
    [Tooltip("只有带这个 Tag 的对象才能穿过门")]
    public string playerTag = "Player";

    Portal portalA;
    Portal portalB;
    float destroyTime;

    Collider[] selfColliders;   // 玩家自己的碰撞体，射线检测时跳过

    void Start()
    {
        // 收集整个角色（根物体）下的所有碰撞体，射线检测时跳过它们，避免打中自己
        selfColliders = transform.root.GetComponentsInChildren<Collider>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryPlacePortal();

        // 门到期自动销毁
        if ((portalA != null || portalB != null) && Time.time >= destroyTime)
            DestroyPortalPair();
    }

    void TryPlacePortal()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("找不到主相机 (MainCamera)，无法发射射线");
            return;
        }
        if (portalPrefab == null)
        {
            Debug.LogError("PortalGun 缺少 portalPrefab，请先用菜单 Tools/Portal 生成预制体并拖进来");
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // 1. 第一次射线：命中墙的正面（跳过玩家自己）
        RaycastHit frontHit;
        if (!TryRaycast(ray, out frontHit))
            return;

        // 2. 第二次射线：继续穿过墙，找墙的背面
        Vector3 backPos, backNormal;
        if (!FindBackFace(ray, frontHit, out backPos, out backNormal))
        {
            // 墙太薄 / 只有单面：按默认厚度在射线方向上补一个背面
            backPos = frontHit.point + ray.direction * defaultWallThickness;
            backNormal = -frontHit.normal;
        }

        // 先销毁上一对门
        DestroyPortalPair();

        // 3. 记录墙和玩家的渲染器，门内相机渲染时要临时隐藏它们（实现透视）
        Renderer[] wallRenderers = frontHit.collider.GetComponentsInChildren<Renderer>();

        Renderer[] playerRenderers = null;
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null) playerRenderers = player.GetComponentsInChildren<Renderer>();

        // 4. 生成两扇门并互相关联
        portalA = SpawnPortal(frontHit.point + frontHit.normal * surfaceOffset, frontHit.normal, wallRenderers, playerRenderers);
        portalB = SpawnPortal(backPos + backNormal * surfaceOffset, backNormal, wallRenderers, playerRenderers);
        if (portalA != null && portalB != null)
        {
            portalA.linkedPortal = portalB;
            portalB.linkedPortal = portalA;
        }

        destroyTime = Time.time + portalLifetime;
    }

    /// <summary>发射射线，跳过玩家自己，返回第一个命中的墙。</summary>
    bool TryRaycast(Ray ray, out RaycastHit hit)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, wallLayerMask, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit h in hits)
        {
            if (IsSelf(h.collider)) continue;
            hit = h;
            return true;
        }
        hit = default;
        return false;
    }

    bool IsSelf(Collider c)
    {
        if (selfColliders == null) return false;
        foreach (Collider sc in selfColliders)
            if (sc == c) return true;
        return false;
    }

    /// <summary>找墙的背面：从墙另一侧反向发射射线，命中的第一个点就是墙的背面。</summary>
    bool FindBackFace(Ray ray, RaycastHit frontHit, out Vector3 pos, out Vector3 normal)
    {
        // 从射线远端（墙后面）反向发射射线，命中的第一个点就是墙的背面。
        // （RaycastAll 对 BoxCollider 只返回一个"进入点"，拿不到背面，所以用反向射线）
        Vector3 farPoint = frontHit.point + ray.direction * maxDistance;
        Ray backRay = new Ray(farPoint, -ray.direction);
        RaycastHit[] hits = Physics.RaycastAll(backRay, maxDistance, wallLayerMask, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit h in hits)
        {
            if (IsSelf(h.collider)) continue;
            if (h.collider == frontHit.collider)
            {
                pos = h.point;
                normal = h.normal;
                return true;
            }
        }
        pos = Vector3.zero;
        normal = Vector3.zero;
        return false;
    }

    Portal SpawnPortal(Vector3 position, Vector3 normal, Renderer[] wallRenderers, Renderer[] playerRenderers)
    {
        // 让门的正面法线（局部 +Z）对齐墙面的法线方向
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, normal);
        GameObject go = Instantiate(portalPrefab, position, rotation);
        Portal portal = go.GetComponent<Portal>();
        if (portal == null) portal = go.AddComponent<Portal>();

        portal.diameter = portalDiameter;
        portal.playerTag = playerTag;
        portal.wallRenderers = wallRenderers;
        portal.playerRenderers = playerRenderers;
        portal.name = "Portal";
        return portal;
    }

    void DestroyPortalPair()
    {
        if (portalA != null) { Destroy(portalA.gameObject); portalA = null; }
        if (portalB != null) { Destroy(portalB.gameObject); portalB = null; }
    }
}
