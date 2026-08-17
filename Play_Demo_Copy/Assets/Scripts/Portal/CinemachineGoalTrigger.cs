using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;

/// <summary>
/// 到达目标箱触发 Cinemachine 运镜（挂在目标箱上，由菜单 Tools/Portal/生成 Cinemachine 运镜 自动挂载）。
///
/// 玩家走到目标箱后：锁定玩家 → 播放 Timeline（Cinemachine 在多个机位间切换）→ 恢复。
/// 注意：Cinemachine 运镜时 Brain 保持启用（由 Cinemachine 自动控制相机和朝向），
///       这和之前的 Animation Track 运镜（手动控制相机）不同。
/// </summary>
public class CinemachineGoalTrigger : MonoBehaviour
{
    [Tooltip("运镜时长（秒）")]
    public float sequenceDuration = 4.5f;

    [Tooltip("到达提示")]
    public string winMessage = "你到达了目标！";

    PlayableDirector director;
    bool triggered = false;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Camera mainCam = Camera.main;
        if (mainCam != null)
            director = mainCam.GetComponent<PlayableDirector>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        Debug.Log(winMessage);
        StartCoroutine(PlaySequence(other));
    }

    System.Collections.IEnumerator PlaySequence(Collider player)
    {
        // 锁定玩家（运镜期间不能移动）
        ThirdPersonController controller = player.GetComponentInParent<ThirdPersonController>();
        if (controller != null) controller.enabled = false;

        // 播放 Cinemachine 运镜
        if (director != null && director.playableAsset != null)
            director.Play();

        yield return new WaitForSeconds(sequenceDuration);

        // 恢复
        if (director != null) director.Stop();
        if (controller != null) controller.enabled = true;
    }
}
