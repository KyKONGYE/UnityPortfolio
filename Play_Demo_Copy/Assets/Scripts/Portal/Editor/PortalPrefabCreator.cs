#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器工具：一键生成传送门所需的 材质 + 预制体 + Portal 图层。
/// 菜单：Tools → Portal → 一键创建传送门预制体
///
/// 生成结果：
///   材质   Assets/Resources/meterail/Portal.mat
///   预制体 Assets/Prefabs/Portal/Portal.prefab
///   图层   Portal（自动写入 TagManager）
/// </summary>
public static class PortalPrefabCreator
{
    [MenuItem("Tools/Portal/一键创建传送门预制体")]
    public static void CreatePortalPrefab()
    {
        // 1. 找到 shader
        Shader shader = Shader.Find("Custom/Portal");
        if (shader == null)
            shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Scripts/Portal/Portal.shader");
        if (shader == null)
        {
            Debug.LogError("找不到 Portal shader，请确认 Assets/Scripts/Portal/Portal.shader 存在且没有编译错误");
            return;
        }

        // 2. 确保 Portal 图层存在
        EnsureLayer("Portal");

        // 3. 创建材质
        string matDir = "Assets/Resources/meterail";
        if (!AssetDatabase.IsValidFolder(matDir))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateFolder("Assets/Resources", "meterail");
        }
        string matPath = matDir + "/Portal.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, matPath);
        }
        else
        {
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
        }

        // 4. 创建预制体对象
        GameObject root = new GameObject("Portal");
        root.layer = LayerMask.NameToLayer("Portal");

        BoxCollider bc = root.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.center = Vector3.zero;
        bc.size = new Vector3(2f, 2f, 0.6f);

        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        root.AddComponent<Portal>();

        GameObject view = GameObject.CreatePrimitive(PrimitiveType.Quad);
        view.name = "View";
        view.transform.SetParent(root.transform, false);
        view.layer = LayerMask.NameToLayer("Portal");
        Object.DestroyImmediate(view.GetComponent<Collider>());   // 去掉 Quad 自带的碰撞体
        view.GetComponent<MeshRenderer>().sharedMaterial = mat;

        // 5. 保存为预制体
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Portal"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Portal");

        string prefabPath = "Assets/Prefabs/Portal/Portal.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = prefab;
        Debug.Log("传送门预制体已生成：" + prefabPath + "\n请把它拖到场景里 PortalGun 组件的 portalPrefab 上。");
    }

    static void EnsureLayer(string layerName)
    {
        string path = "ProjectSettings/TagManager.asset";
        var assets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets == null || assets.Length == 0)
        {
            Debug.LogWarning("无法读取 TagManager.asset，请手动在 Project Settings → Tags and Layers 添加 '" + layerName + "' 图层");
            return;
        }

        SerializedObject tagManager = new SerializedObject(assets[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        // 是否已存在
        for (int i = 8; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                return;
        }
        // 找空槽写入（用户自定义图层从第 8 个槽位开始）
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty sp = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
        Debug.LogWarning("图层槽位已满，无法自动创建 'Portal' 图层，请手动添加");
    }
}
#endif
