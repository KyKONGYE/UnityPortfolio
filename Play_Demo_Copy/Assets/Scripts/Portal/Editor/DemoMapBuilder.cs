#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 室内传送门 Demo 地图生成器（编辑器脚本）。
///
/// 用法：菜单 Tools → Portal → 生成室内 Demo 地图。
/// 点击后在当前场景里生成一个室内房间（地面 + 四周墙 + 内部隔断墙 + 障碍物 + 灯光 + 玩家出生点）。
/// 生成的是普通场景物体（不是运行时生成），停止运行后依然存在，可随意修改/删除。
/// 重复点击会先删除旧的 DemoMap 再重建。
/// </summary>
public static class DemoMapBuilder
{
    [MenuItem("Tools/Portal/生成室内 Demo 地图")]
    public static void BuildDemoMap()
    {
        // 先删除旧的（避免重复）
        GameObject old = GameObject.Find("DemoMap");
        if (old != null) Object.DestroyImmediate(old);

        // 房间尺寸
        float width  = 20f;   // X 方向
        float depth  = 20f;   // Z 方向
        float height = 4f;    // 墙高
        float thick  = 0.5f;  // 墙厚

        // 材质（不同颜色方便区分）
        Material floorMat    = CreateMaterial("Floor",    new Color(0.32f, 0.32f, 0.36f));  // 深灰地面
        Material wallMat     = CreateMaterial("Wall",     new Color(0.72f, 0.72f, 0.78f));  // 浅灰外墙
        Material dividerMat  = CreateMaterial("Divider",  new Color(0.45f, 0.65f, 0.90f));  // 蓝色隔断墙
        Material boxMat      = CreateMaterial("Box",      new Color(0.85f, 0.62f, 0.40f));  // 橙色箱子

        GameObject root = new GameObject("DemoMap");

        // 地面
        CreatePrimitive(root, "Floor", new Vector3(0, -thick * 0.5f, 0), new Vector3(width, thick, depth), floorMat);

        // 四周墙
        CreatePrimitive(root, "Wall_North", new Vector3(0, height * 0.5f, -depth * 0.5f), new Vector3(width, height, thick), wallMat);
        CreatePrimitive(root, "Wall_South", new Vector3(0, height * 0.5f,  depth * 0.5f), new Vector3(width, height, thick), wallMat);
        CreatePrimitive(root, "Wall_East",  new Vector3( width * 0.5f, height * 0.5f, 0), new Vector3(thick, height, depth), wallMat);
        CreatePrimitive(root, "Wall_West",  new Vector3(-width * 0.5f, height * 0.5f, 0), new Vector3(thick, height, depth), wallMat);

        // 内部隔断墙（传送门可穿越的墙）
        CreatePrimitive(root, "Divider_1", new Vector3(-4f, height * 0.5f, 0f), new Vector3(thick, height, 8f), dividerMat);
        CreatePrimitive(root, "Divider_2", new Vector3( 4f, height * 0.5f, 3f), new Vector3(8f, height, thick), dividerMat);
        CreatePrimitive(root, "Divider_3", new Vector3( 0f, height * 0.5f, -6f), new Vector3(6f, height, thick), dividerMat);

        // 障碍物箱子
        CreatePrimitive(root, "Box_1", new Vector3(-7f, 0.5f, 6f),  new Vector3(1f, 1f, 1f), boxMat);
        CreatePrimitive(root, "Box_2", new Vector3( 7f, 0.75f, -6f), new Vector3(1.5f, 1.5f, 1.5f), boxMat);
        CreatePrimitive(root, "Box_3", new Vector3( 8f, 1f, 8f),    new Vector3(1f, 2f, 1f), boxMat);

        // 玩家出生点（空物体，标记位置）
        GameObject spawn = new GameObject("PlayerSpawnPoint");
        spawn.transform.SetParent(root.transform);
        spawn.transform.position = new Vector3(0, 0.05f, 8f);

        // 灯光
        GameObject lightGO = new GameObject("Directional Light");
        lightGO.transform.SetParent(root.transform);
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;

        // 选中根物体，方便查看
        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);

        Debug.Log("室内 Demo 地图已生成。请按 Ctrl+S 保存场景。\n" +
                  "玩家出生点：PlayerSpawnPoint (0, 0.05, 8)。\n" +
                  "搭建传送门：给玩家挂 PlayerMovement + PortalGun，再用菜单 Tools/Portal/一键创建传送门预制体。");
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

    static Material CreateMaterial(string name, Color color)
    {
        string dir = "Assets/Materials/DemoMap";
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Materials", "DemoMap");

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
