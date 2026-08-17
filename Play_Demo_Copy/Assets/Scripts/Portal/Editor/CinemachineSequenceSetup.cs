#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Cinemachine;

/// <summary>
/// Cinemachine 运镜序列生成器（编辑器脚本）。
///
/// 用法：菜单 Tools → Portal → 生成 Cinemachine 运镜。
/// 作用：
///   1. 创建 3 个 Cinemachine 虚拟相机（不同机位，都看向目标箱）
///   2. 创建 Timeline 资产（Cinemachine Track + 3 个 shot）
///   3. 配置 PlayableDirector（绑定 CinemachineBrain + 各机位）
///   4. 给目标箱挂 CinemachineGoalTrigger
///
/// 运行后走到金色箱子，相机会在 3 个机位之间平滑切换（远景 → 中景 → 俯视）。
/// </summary>
public static class CinemachineSequenceSetup
{
    [MenuItem("Tools/Portal/生成 Cinemachine 运镜")]
    public static void CreateSequence()
    {
        GameObject goalBox = GameObject.Find("GoalBox");
        Camera mainCam = Camera.main;
        if (goalBox == null || mainCam == null)
        {
            Debug.LogError("找不到 GoalBox 或主相机(MainCamera)。请先生成传送门体验地图。");
            return;
        }
        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogError("主相机上找不到 CinemachineBrain。请确认用的是 StarterAssets 的 MainCamera。");
            return;
        }

        // 删除旧机位
        GameObject oldRig = GameObject.Find("GoalCameraRig");
        if (oldRig != null) Object.DestroyImmediate(oldRig);

        // 创建机位父物体（放在场景里，是普通场景物体）
        GameObject rig = new GameObject("GoalCameraRig");

        Vector3 goal = goalBox.transform.position;

        // 3 个机位（都在地图内、不穿墙，都看向目标箱）
        CinemachineVirtualCamera cam1 = CreateVcam(rig, "Shot1_远景", goal + new Vector3(3, 7, 3), goalBox.transform);
        CinemachineVirtualCamera cam2 = CreateVcam(rig, "Shot2_中景", goal + new Vector3(4, 5, 2), goalBox.transform);
        CinemachineVirtualCamera cam3 = CreateVcam(rig, "Shot3_俯视", goal + new Vector3(0, 8, 0), goalBox.transform);

        // 创建 Timeline 资产
        string dir = "Assets/Timelines";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Timelines");
        string path = dir + "/GoalCinemachineSequence.playable";

        TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
        if (timeline == null)
        {
            timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, path);
        }
        foreach (var t in timeline.GetRootTracks())
            timeline.DeleteTrack(t);

        CinemachineTrack track = timeline.CreateTrack<CinemachineTrack>(null, "Cinemachine Track");

        // 3 个 shot 依次排列（每个 1.5 秒，共 4.5 秒）
        AddShot(track, cam1, "Shot1", 0f, 1.5f);
        AddShot(track, cam2, "Shot2", 1.5f, 1.5f);
        AddShot(track, cam3, "Shot3", 3f, 1.5f);
        EditorUtility.SetDirty(timeline);

        // 配置 PlayableDirector
        PlayableDirector director = mainCam.GetComponent<PlayableDirector>();
        if (director == null) director = mainCam.gameObject.AddComponent<PlayableDirector>();
        director.playableAsset = timeline;
        director.SetGenericBinding(track, brain);           // CinemachineTrack 绑定 CinemachineBrain
        director.SetReferenceValue("Shot1", cam1);         // shot 的 exposedName 绑定到机位
        director.SetReferenceValue("Shot2", cam2);
        director.SetReferenceValue("Shot3", cam3);
        director.RebuildGraph();
        EditorSceneManager.MarkSceneDirty(mainCam.gameObject.scene);

        // 挂触发脚本
        if (goalBox.GetComponent<CinemachineGoalTrigger>() == null)
            goalBox.AddComponent<CinemachineGoalTrigger>();

        AssetDatabase.SaveAssets();
        Debug.Log("Cinemachine 运镜已生成：Timeline = " + path + "\n运行后走到金箱子，相机会在 3 个机位间平滑切换。");
    }

    static CinemachineVirtualCamera CreateVcam(GameObject parent, string name, Vector3 pos, Transform lookAt)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        CinemachineVirtualCamera vcam = go.AddComponent<CinemachineVirtualCamera>();
        vcam.LookAt = lookAt;
        return vcam;
    }

    static void AddShot(CinemachineTrack track, CinemachineVirtualCamera vcam, string exposedName, float start, float duration)
    {
        TimelineClip clip = track.CreateClip<CinemachineShot>();
        CinemachineShot shot = clip.asset as CinemachineShot;
        shot.VirtualCamera.exposedName = exposedName;
        clip.start = start;
        clip.duration = duration;
    }
}
#endif
