#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 传送门体验地图生成器（编辑器脚本）。
///
/// 用法：菜单 Tools → Portal → 生成传送门体验地图。
/// 生成一个 40x30 的大型室内建筑，用多堵隔断墙分成多个区域。
/// 其中几堵墙是"整堵封死"的，玩家必须用传送门穿墙才能到达最深处的目标金箱。
///
/// 生成的是普通场景物体，停止运行后依然存在，可随意修改。
/// 重复点击会先删除旧的 PortalMap 再重建。
/// </summary>
public static class PortalMapBuilder
{
    [MenuItem("Tools/Portal/生成传送门体验地图")]
    public static void BuildMap()
    {
        GameObject old = GameObject.Find("PortalMap");
        if (old != null) Object.DestroyImmediate(old);

        // 材质
        Material floorMat    = CreateMaterial("Floor",     new Color(0.28f, 0.28f, 0.32f)); // 深灰地面
        Material wallMat     = CreateMaterial("Wall",      new Color(0.70f, 0.70f, 0.75f)); // 浅灰外墙
        Material dividerMat  = CreateMaterial("Divider",   new Color(0.45f, 0.65f, 0.90f)); // 蓝色隔断墙
        Material blockWall   = CreateMaterial("BlockWall", new Color(0.30f, 0.50f, 0.80f)); // 深蓝必穿墙
        Material pillarMat   = CreateMaterial("Pillar",    new Color(0.50f, 0.50f, 0.55f)); // 柱子
        Material boxMat      = CreateMaterial("Box",       new Color(0.85f, 0.62f, 0.40f)); // 橙色箱子
        Material goalMat     = CreateMaterial("Goal",      new Color(1.00f, 0.84f, 0.20f)); // 金色目标箱

        GameObject root = new GameObject("PortalMap");

        const float H  = 4f;    // 墙高
        const float TH = 0.5f;  // 墙厚

        // ---- 地面 40 x 30 ----
        CreatePrimitive(root, "Floor", new Vector3(0, -TH * 0.5f, 0), new Vector3(40, TH, 30), floorMat);

        // ---- 四周外墙 ----
        CreatePrimitive(root, "Wall_North", new Vector3(0, H * 0.5f, -15), new Vector3(40, H, TH), wallMat);
        CreatePrimitive(root, "Wall_South", new Vector3(0, H * 0.5f, 15), new Vector3(40, H, TH), wallMat);
        CreatePrimitive(root, "Wall_East",  new Vector3(20, H * 0.5f, 0), new Vector3(TH, H, 30), wallMat);
        CreatePrimitive(root, "Wall_West",  new Vector3(-20, H * 0.5f, 0), new Vector3(TH, H, 30), wallMat);

        // ---- 隔断墙 A（z=8，中间留 4 米缺口，引导进入中厅）----
        CreatePrimitive(root, "DividerA_West", new Vector3(-7, H * 0.5f, 8), new Vector3(10, H, TH), dividerMat);
        CreatePrimitive(root, "DividerA_East", new Vector3(7, H * 0.5f, 8), new Vector3(10, H, TH), dividerMat);

        // ---- 隔断墙 B（z=0，整堵横贯，必须用传送门穿）----
        CreatePrimitive(root, "DividerB_Block", new Vector3(0, H * 0.5f, 0), new Vector3(40, H, TH), blockWall);

        // ---- 隔断墙 C（x=0，北区再分东西两半，必须用传送门穿）----
        CreatePrimitive(root, "DividerC_Block", new Vector3(0, H * 0.5f, -7.5f), new Vector3(TH, H, 15), blockWall);

        // ---- 柱子（中厅）----
        CreatePrimitive(root, "Pillar_1", new Vector3(-12, H * 0.5f, 4), new Vector3(1, H, 1), pillarMat);
        CreatePrimitive(root, "Pillar_2", new Vector3(12, H * 0.5f, 4), new Vector3(1, H, 1), pillarMat);

        // ---- 障碍箱（散落）----
        CreatePrimitive(root, "Box_1", new Vector3(-5, 0.75f, 12), new Vector3(1.5f, 1.5f, 1.5f), boxMat);
        CreatePrimitive(root, "Box_2", new Vector3(5, 0.5f, 5), new Vector3(1, 1, 1), boxMat);
        CreatePrimitive(root, "Box_3", new Vector3(12, 1f, -5), new Vector3(1, 2, 1), boxMat);
        CreatePrimitive(root, "Box_4", new Vector3(3, 0.5f, -10), new Vector3(2, 1, 2), boxMat);

        // ---- 目标金箱（最深处的西北角，需要穿过多堵墙才能到达）----
        CreatePrimitive(root, "GoalBox", new Vector3(-15, 0.75f, -12), new Vector3(1.5f, 1.5f, 1.5f), goalMat);

        // ---- 玩家出生点 ----
        GameObject spawn = new GameObject("PlayerSpawnPoint");
        spawn.transform.SetParent(root.transform);
        spawn.transform.position = new Vector3(0, 0.05f, 12);

        // ---- 灯光 ----
        CreateDirectionalLight(root, new Vector3(50, -30, 0));
        CreatePointLight(root, "PointLight_Center", new Vector3(0, 3.5f, 4), 18f);
        CreatePointLight(root, "PointLight_North", new Vector3(0, 3.5f, -8), 18f);

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);

        Debug.Log("传送门体验地图已生成，请 Ctrl+S 保存场景。\n" +
                  "出生点：PlayerSpawnPoint (0, 0.05, 12)。\n" +
                  "目标：最深西北角的金色箱子（被几堵封死的墙隔开，必须用传送门穿墙过去）。");
    }

    static GameObject CreatePrimitive(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        if (mat != null)
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    static void CreateDirectionalLight(GameObject parent, Vector3 euler)
    {
        GameObject go = new GameObject("Directional Light");
        go.transform.SetParent(parent.transform);
        go.transform.rotation = Quaternion.Euler(euler);
        Light l = go.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.1f;
        l.shadows = LightShadows.Soft;
    }

    static void CreatePointLight(GameObject parent, string name, Vector3 pos, float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = range;
        l.intensity = 2f;
        l.shadows = LightShadows.Soft;
    }

    static Material CreateMaterial(string name, Color color)
    {
        string dir = "Assets/Materials/PortalMap";
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Materials", "PortalMap");

        string path = dir + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        EditorUtility.SetDirty(mat);
        return mat;
    }
}
#endif
