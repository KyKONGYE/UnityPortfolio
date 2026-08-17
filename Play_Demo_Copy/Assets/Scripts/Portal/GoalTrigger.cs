using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;
using StarterAssets;

/// <summary>
/// 到达目标箱触发运镜（挂在目标箱 GoalBox 上，由菜单 Tools/Portal/生成到达运镜序列 自动挂载）。
///
/// 玩家（Tag = Player）走进目标箱后：
///   1. 锁定玩家移动、接管相机
///   2. 播放 Timeline 运镜动画（相机绕目标箱一圈并拉近）
///   3. 播放结束恢复玩家控制和相机
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    [Tooltip("运镜时长（秒）")]
    public float sequenceDuration = 4f;

    [Tooltip("到达后的提示文字")]
    public string winMessage = "你到达了目标！";

    PlayableDirector director;
    CinemachineBrain brain;
    Camera mainCam;
    bool triggered = false;

    void Start()
    {
        // 目标箱改成触发器，并加运动学刚体（让 CharacterController 玩家能触发 OnTriggerEnter）
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        mainCam = Camera.main;
        if (mainCam != null)
        {
            director = mainCam.GetComponent<PlayableDirector>();
            brain = mainCam.GetComponent<CinemachineBrain>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        Debug.Log("[运镜] 触发！player=" + other.name
                  + ", director=" + (director != null)
                  + ", playableAsset=" + (director != null && director.playableAsset != null)
                  + ", brain=" + (brain != null));
        Debug.Log(winMessage);
        StartCoroutine(PlayCameraSequence(other));
    }

    System.Collections.IEnumerator PlayCameraSequence(Collider player)
    {
        // 1. 锁定玩家移动 + 接管相机（禁用 Cinemachine Brain，让运镜控制相机）
        ThirdPersonController controller = player.GetComponentInParent<ThirdPersonController>();
        if (controller != null) controller.enabled = false;
        if (brain != null) brain.enabled = false;

        // 2. 播放 Timeline 运镜（动画相机位置）
        if (director != null && director.playableAsset != null)
        {
            director.Play();
            Debug.Log("[运镜] Play 后 state=" + director.state);
        }
        else
        {
            Debug.LogWarning("[运镜] 无法播放！director=" + director
                             + ", playableAsset=" + (director != null ? director.playableAsset : null));
        }

        // 3. 运镜期间让相机始终看向目标箱（旋转由这里实时算，位置由 Timeline 动画）
        float t = 0f;
        while (t < sequenceDuration)
        {
            if (mainCam != null)
                mainCam.transform.LookAt(transform.position + Vector3.up * 0.5f);
            t += Time.deltaTime;
            yield return null;
        }

        // 4. 恢复
        if (director != null) director.Stop();
        if (brain != null) brain.enabled = true;
        if (controller != null) controller.enabled = true;
    }
}
