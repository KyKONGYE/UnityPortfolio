using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 开火点位置的 类型枚举
/// </summary>
public enum E_Pos_Type
{
    TopLeft,
    Top,
    TopRight,
    
    Left,
    Right,
    
    BottomLeft,
    Bottom,
    BottomRight
}

public class FireObject : MonoBehaviour
{
    public E_Pos_Type type;
    
    //当前 开火点的数据信息
    private FireInfo fireInfo;
    private int nowNum;
    private float nowCD;
    private float nowDelay;
    //当前组开火点 使用的子弹信息
    private BulletInfo nowBulletInfo;
    //散弹时 每颗子弹的间隔角度
    private float changeAngle;
    
    //表示屏幕上的点
    private Vector3 screenPos;
    //初始发射子弹的方向 主要用于 作为散弹的初始方向 用于计算
    private Vector3 initDir;
    //这个是发射散弹时 记录上一次方向的
    private Vector3 nowDir;

    // Update is called once per frame
    void Update()
    {
        //测试 ：打印 飞机的z轴坐标 来确定 发射点的Z 276(也就是游戏平面离摄像机镜头的 横截面 距离 为了让玩家和发射器处于同一平面)
        //print(Camera.main.WorldToScreenPoint(PlayerObject.Instance.transform.position));
        //更新位置
        UpdatePos();
        //每一次 都检测是否需要重置 开火点 数据
        ResetFireInfo();
        //发射子弹
        UpdateFire();
    }

    /// <summary>
    /// 更新发射点位置 达到分辨率自适应
    /// </summary>
    private void UpdatePos()
    {
        //这里设置为 和玩家位置转屏幕坐标后的 z位置一样 目的是 让点和玩家 所在的 横截面是一致的
        screenPos.z = 276;
        switch (type)
        {
            case E_Pos_Type.TopLeft:
                screenPos.x = 0;
                screenPos.y = Screen.height;
                
                initDir = Vector3.right;
                break;
            case E_Pos_Type.Top:
                screenPos.x = Screen.width / 2;
                screenPos.y = Screen.height;
                
                initDir = Vector3.right;
                break;
            case E_Pos_Type.TopRight:
                screenPos.x = Screen.width;
                screenPos.y = Screen.height;
                
                initDir = Vector3.left;
                break;
            case E_Pos_Type.Left:
                screenPos.x = 0;
                screenPos.y = Screen.height / 2;
                
                initDir = Vector3.right;
                break;
            case E_Pos_Type.Right:
                screenPos.x = Screen.width;
                screenPos.y = Screen.height / 2;
                
                initDir = Vector3.left;
                break;
            case E_Pos_Type.BottomLeft:
                screenPos.x = 0;
                screenPos.y = 0;
                
                initDir = Vector3.right;
                break;
            case E_Pos_Type.Bottom:
                screenPos.x = Screen.width / 2;
                screenPos.y = 0;
                
                initDir = Vector3.right;
                break;
            case E_Pos_Type.BottomRight:
                screenPos.x = Screen.width;
                screenPos.y = 0;
                
                initDir = Vector3.left;
                break;
        }
        //再把屏幕点 转成 世界坐标点 这样得到的 就是 我们想要的坐标
        this.transform.position = Camera.main.ScreenToWorldPoint(screenPos);
    }
    //重置当前要发射的炮台数据
    private void ResetFireInfo()
    {
        //（二）自己定一个规则 只有当cd和数量都为0时 才认为需要重新获取我们的 发射点数据
        if (nowCD != 0 && nowNum != 0)
            return;
        //（三）组间休息 时间 判断(如果cd为0 并且 子弹都已经发射完毕 要判断是否在组间休息时间)
        if (fireInfo != null)
        {
            nowDelay -= Time.deltaTime;
            //还在组间休息
            if (nowDelay > 0)
                return;
        }
        
        //（一）从数据中随机取出一条 来按照规则 发射子弹
        List<FireInfo> list = GameDataMgr.Instance.fireData.fireInfoList;
        fireInfo = list[Random.Range(0, list.Count)];
        //我们不能直接改变数据中的内容 我们应该拿变量 临时存储下来 这样就不会影响我们数据本身了
        nowNum = fireInfo.num;
        nowCD = fireInfo.cd;
        nowDelay = fireInfo.delay;
        
        //通过 开火点数据 取出当前要使用的子弹数据信息
        string[] strs = fireInfo.ids.Split(',');
        //得到开始ID和结束ID 用于取出子弹的信息
        int btginID = int.Parse(strs[0]);
        int endID = int.Parse(strs[1]);
        int randomBulletID = Random.Range(btginID, endID);
        //需要减去1 因为ID是从1开始的 list索引是从0开始的
        nowBulletInfo = GameDataMgr.Instance.bulletData.bulletInfoList[randomBulletID - 1];
        //如果是散弹 就需要计算 我们的 间隔角度
        if (fireInfo.type == 2)
        {
            switch (type)
            {
                //角落和 中心 分类 直接使用贯穿
                case E_Pos_Type.TopLeft:
                case E_Pos_Type.TopRight:
                case E_Pos_Type.BottomLeft:
                case E_Pos_Type.BottomRight:
                    changeAngle = 90f / (nowNum + 1);
                    break; 
                case E_Pos_Type.Top:
                case E_Pos_Type.Left:
                case E_Pos_Type.Right:
                case E_Pos_Type.Bottom:
                    changeAngle = 180f / (nowNum + 1);
                    break;
            }
        }
    }
    
    //检测开火
    private void UpdateFire()
    {
        //(四)当前状态 是不需要发射子弹的
        if (nowCD == 0 && nowNum == 0)
            return;
        //(二)cd更新
        nowCD -= Time.deltaTime;
        if(nowCD > 0)
            return;
        //(一)
        GameObject bullet;
        BulletObject bulletObj;
        switch (fireInfo.type)
        {
            //一个一个发射子弹 朝向玩家
            case 1:
                //动态创建子弹对象
                bullet = Instantiate(Resources.Load<GameObject>(nowBulletInfo.resName));
                //动态添加子弹脚本
                bulletObj = bullet.AddComponent<BulletObject>();
                //初始化子弹数据传入子弹脚本 进行初始化
                bulletObj.InitInfo(nowBulletInfo);
                
                //设置子弹的位置 和朝向
                bullet.transform.position = this.transform.position;
                //玩家的位置 减去 开火点的位置 得出一个方向向量
                bullet.transform.rotation = Quaternion.LookRotation(PlayerObject.Instance.transform.position - this.transform.position);

                //表示已经发射一颗子弹
                --nowNum;
                //重置CD
                nowCD = nowNum == 0 ? 0:fireInfo.cd;
                break;
            //发射散弹
            case 2:
                //(三)无CD 一瞬间 发射所有的散弹
                if (nowCD == 0)
                {
                    for (int i = 0; i < nowNum; i++)
                    {
                        //动态创建子弹对象
                        bullet = Instantiate(Resources.Load<GameObject>(nowBulletInfo.resName));
                        //动态添加子弹脚本
                        bulletObj = bullet.AddComponent<BulletObject>();
                        //初始化子弹数据传入子弹脚本 进行初始化
                        bulletObj.InitInfo(nowBulletInfo);
                        
                        //设置子弹的位置 和朝向
                        bullet.transform.position = this.transform.position;
                        //每次都会旋转一个角度 得到一个新的方向
                        nowDir = Quaternion.AngleAxis(changeAngle * i,Vector3.up) * initDir;
                        bullet.transform.rotation = Quaternion.LookRotation(nowDir);
                    }
                    //因为是瞬间创建完所有子弹 所以重置数据
                    nowCD = nowNum = 0;
                }
                else
                {
                    //和 上面瞬间发射散弹基本相同 i 变成(fireInfo.num - nowNum) 开火点总发射数量 - 已经发射的数量
                    //动态创建子弹对象
                    bullet = Instantiate(Resources.Load<GameObject>(nowBulletInfo.resName));
                    //动态添加子弹脚本
                    bulletObj = bullet.AddComponent<BulletObject>();
                    //初始化子弹数据传入子弹脚本 进行初始化
                    bulletObj.InitInfo(nowBulletInfo);
                        
                    //设置子弹的位置 和朝向
                    bullet.transform.position = this.transform.position;
                    //每次都会旋转一个角度 得到一个新的方向
                    nowDir = Quaternion.AngleAxis(changeAngle * (fireInfo.num - nowNum),Vector3.up) * initDir;
                    bullet.transform.rotation = Quaternion.LookRotation(nowDir);
                    
                    //下面就和 单颗逻辑差不多了
                    //表示已经发射一颗子弹
                    --nowNum;
                    //重置CD
                    nowCD = nowNum == 0 ? 0:fireInfo.cd;
                }
                break;
        }
    }
}
