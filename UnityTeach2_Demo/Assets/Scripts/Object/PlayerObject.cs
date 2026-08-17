using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    private static PlayerObject instance;
    public static PlayerObject Instance => instance;
    
    //血量
    public int nowHp;
    public int maxHp;
    
    //速度
    public int speed;
    //旋转速度
    public int roundSpeed;
    //目标四元数角度(按住 左右键 飞机有向一侧倾斜的动向 )
    private Quaternion targetQ;
    
    //是否死亡
    public bool isDead = false;
    
    //当前世界坐标转屏幕上的点
    private Vector3 nowPos;
    //上一次玩家的位置 就是在位移前 玩家的位置(万一玩家的飞机飞出屏幕 需要把它拉回来)
    private Vector3 frontPos;

    private void Awake()
    {
        instance = this;
    }

    public void Dead()
    {
        //修改死亡标识
        isDead = true;
        //显示结束面板
        EndPanel.Instance.ShowMe();
    }

    public void Wound()
    {
        //死了就不会受伤了
        if (isDead)
            return;
        //减血
        this.nowHp -= 1;
        print(nowHp);
        //更新游戏面板上的血量显示
        GamePanel.Instance.ChangeHp(this.nowHp);
        //受伤后判断是否死亡
        if(nowHp <= 0)
            this.Dead();
    }

    private float hValue;
    private float vValue;
    // Update is called once per frame
    void Update()
    {
        //如果死亡 就没必要再移动了
        if (isDead)
            return;
        
        //移动 旋转逻辑
        
        //(一)旋转(这里注意 GetAxis和GetAxisRaw 两个方法的区别 前者是慢慢变化的(-1,1)有延迟，后者根据按键只会是-1,0,1三个值)
        hValue = Input.GetAxisRaw("Horizontal");
        vValue = Input.GetAxisRaw("Vertical");
        //如果我们没有按 AD键 那么目标角度 就是 0 0 0度 单位四元数模长为1 代表没有旋转角度
        if (hValue == 0)
            targetQ = Quaternion.identity;
        else
        //如果按 AD 键 就是0020 或者是 00~20 根据你按得左右决定
            targetQ = hValue < 0
                ? Quaternion.AngleAxis(20, Vector3.forward)
                : Quaternion.AngleAxis(-20, Vector3.forward);
        //让飞机 朝着 这个目标四元数 去旋转
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetQ, Time.deltaTime * roundSpeed);
        
        //(三)在位移之前记录自己的位置
        frontPos = this.transform.position;
        
        //(二)移动
        //vValue的正负会控制前进后退
        this.transform.Translate(Vector3.forward* vValue * speed * Time.deltaTime);
        //这里左右移动需要注意 第二个参数不填的话 飞机会越动越往 y轴下面，因为是旋转后 会往自身坐标系的 right移动
        this.transform.Translate(Vector3.right * hValue * speed * Time.deltaTime, Space.World);
        
        //(四)进行极限判断(nowPos现在是Vector2了,因为屏幕坐标是二维)
        nowPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //左右 溢出判断
        if (nowPos.x <= 0 || nowPos.x >= Screen.width)
        {
            //this.transform.position = frontPos;
            //这样写有问题 我们在边界后如果 依然按住 越界的方向键 我们的WS就没用了，因为我们一直把它强行拉回那个位置
            
            //所以我们x越界 就 只拉回x 这样的话我们 左右不合法 不影响我们上下移动
            this.transform.position = new Vector3(frontPos.x,this.transform.position.y,this.transform.position.z);
            
        }
        //上下 溢出判断
        if (nowPos.y <= 0 || nowPos.y >= Screen.height)
        {
            //问题同上,z溢出就只改z 混乱点：判断的 y是二维坐标 我们要改的是飞机的世界坐标是z
            //this.transform.position = frontPos;
            this.transform.position = new Vector3(this.transform.position.x,this.transform.position.y,frontPos.z);
        }
        
        //射线检测 用于销毁子弹
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            //这里只检测子弹层
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000,
                    1 << LayerMask.NameToLayer("Bullet")))
            {
                BulletObject bulletObject = hitInfo.collider.GetComponent<BulletObject>();
                //直接让被点中的子弹销毁
                bulletObject.Dead();
            }
        }
    }
}
