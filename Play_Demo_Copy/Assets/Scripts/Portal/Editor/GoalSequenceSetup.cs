#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.Playables;

/// <summary>
/// 到达目标箱的运镜序列生成器（编辑器脚本）。
///
/// 用法：菜单 Tools → Portal → 生成到达运镜序列。
/// 作用：
///   1. 生成一段运镜 AnimationClip（相机绕目标箱一圈并拉近）
///   2. 生成 Timeline 资产（含 Animation Track）
///   3. 给主相机挂 PlayableDirector 并绑定
///   4. 给目标箱自动挂 GoalTrigger 触发脚本
///
/// 运行游戏后，走到金色目标箱，即自动播放这段运镜动画。
/// </summary>
public static class GoalSequenceSetup
{
    [MenuItem("Tools/Portal/生成到达运镜序列")]
    public static void CreateSequence()
    {
        string dir = "Assets/Timelines";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Timelines");

        GameObject goalBox = GameObject.Find("GoalBox");
        Camera mainCam = Camera.main;
        if (goalBox == null || mainCam == null)
        {
            Debug.LogError("找不到 GoalBox 或主相机(MainCamera)。请先生成传送门体验地图，并确保场景里有 MainCamera。");
            return;
        }

        // 1. 创建运镜 AnimationClip
        AnimationClip clip = CreateCameraClip(goalBox.transform.position);

        // 2. 创建 / 更新 Timeline 资产
        string path = dir + "/GoalSequence.playable";
        TimelineAsset timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);
        if (timeline == null)
        {
            timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, path);
        }
        foreach (var t in timeline.GetRootTracks())
            timeline.DeleteTrack(t);

        AnimationTrack track = timeline.CreateTrack<AnimationTrack>(null, "CameraMove");
        TimelineClip tclip = track.CreateClip(clip);
        tclip.displayName = "运镜";
        tclip.duration = clip.length;
        EditorUtility.SetDirty(timeline);

        // 3. PlayableDirector 挂主相机，绑定动画轨道到主相机
        PlayableDirector director = mainCam.GetComponent<PlayableDirector>();
        if (director == null) director = mainCam.gameObject.AddComponent<PlayableDirector>();

        // 关键：AnimationTrack 必须绑定 Animator 组件才能动画 Transform（直接绑 GameObject 不生效）
        Animator animator = mainCam.GetComponent<Animator>();
        if (animator == null) animator = mainCam.gameObject.AddComponent<Animator>();

        director.playableAsset = timeline;
        director.SetGenericBinding(track, animator);
        director.RebuildGraph();

        // 标记场景为脏，提醒保存（director 的绑定需要保存到场景）
        EditorSceneManager.MarkSceneDirty(mainCam.gameObject.scene);

        // 4. 给目标箱挂触发脚本
        if (goalBox.GetComponent<GoalTrigger>() == null)
            goalBox.AddComponent<GoalTrigger>();

        AssetDatabase.SaveAssets();
        Debug.Log("运镜序列已生成：Timeline = " + path + "，GoalTrigger 已挂到 GoalBox。\n运行后走到金色箱子即可触发运镜。");
    }

    static AnimationClip CreateCameraClip(Vector3 goalPos)
    {
        // 相机在目标箱上方俯视环绕（高度 6~8 米，高于墙顶 4 米，半径 3 米，避免穿墙）
        Vector3[] pos = new Vector3[]
        {
            goalPos + new Vector3(0, 7, -3),
            goalPos + new Vector3(3, 7, 0),
            goalPos + new Vector3(0, 8, 3),
            goalPos + new Vector3(-3, 7, 0),
            goalPos + new Vector3(0, 6, -2),
        };
        float[] times = { 0f, 1f, 2f, 3f, 4f };

        AnimationClip clip = new AnimationClip();
        clip.frameRate = 30f;

        AnimationCurve cx = new AnimationCurve();
        AnimationCurve cy = new AnimationCurve();
        AnimationCurve cz = new AnimationCurve();
        for (int i = 0; i < pos.Length; i++)
        {
            cx.AddKey(times[i], pos[i].x);
            cy.AddKey(times[i], pos[i].y);
            cz.AddKey(times[i], pos[i].z);
        }

        clip.SetCurve("", typeof(Transform), "localPosition.x", cx);
        clip.SetCurve("", typeof(Transform), "localPosition.y", cy);
        clip.SetCurve("", typeof(Transform), "localPosition.z", cz);

        string clipPath = "Assets/Timelines/GoalCameraClip.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }
}
#endif
