using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChoosePanel : BasePanel<ChoosePanel>
{
    //控件
    public UIButton btnClose;
    public UIButton btnLeft;
    public UIButton btnRight;
    public UIButton btnStart;
    
    
    //模型父对象(需要得到它的位置)
    public Transform heroPos;
    
    //下方属性相关对象
    public List<GameObject> hpObj;
    public List<GameObject> speedObj;
    public List<GameObject> volumObj;
    
    //当前显示的飞机模型对象
    private GameObject airPlaneObj;
    
    public override void Init()
    {
        btnStart.onClick.Add(new EventDelegate(() =>
        {
            //切场景
            SceneManager.LoadScene("GameScene");
        }));
        
        btnClose.onClick.Add(new EventDelegate(() =>
        {
            //隐藏自己
            HideMe();
            //显示开始界面
            BeginPanel.Instance.ShowMe();
        }));
        
        btnLeft.onClick.Add(new EventDelegate(() =>
        {
            //左按钮 减我们的索引
            --GameDataMgr.Instance.nowSelHeroIndex;
            //如果 在第一个 按左 变为 最后一个(如果小于最小的索引了 直接等于 最后一个索引)
            if (GameDataMgr.Instance.nowSelHeroIndex < 0)
            {
                GameDataMgr.Instance.nowSelHeroIndex = GameDataMgr.Instance.roleData.roleList.Count - 1;
            }
            ChangeNowHero();
        }));
        
        btnRight.onClick.Add(new EventDelegate(() =>
        {
            //右按钮 加我们的索引
            ++GameDataMgr.Instance.nowSelHeroIndex;
            if (GameDataMgr.Instance.nowSelHeroIndex >= GameDataMgr.Instance.roleData.roleList.Count)
            {
                GameDataMgr.Instance.nowSelHeroIndex = 0;
            }
            ChangeNowHero();
        }));
        HideMe();
    }

    public override void ShowMe()
    {
        base.ShowMe();
        //每次显示的时候 都从第一个开始选择
        GameDataMgr.Instance.nowSelHeroIndex = 0;
        ChangeNowHero();
    }

    public override void HideMe()
    {
        base.HideMe();
        //删除当前的模型
        DestroyObj();
    }
    
    /// <summary>
    /// 切换当前选择
    /// </summary>
    private void ChangeNowHero()
    {
        //得到当前选择的 玩家英雄数据
        RoleInfo info = GameDataMgr.Instance.GetNowSelHeroInfo();
        
        //更新模型
        //先删除上一次的飞机模型
        DestroyObj();
        //再创建当前的飞机模型
        airPlaneObj = Instantiate(Resources.Load<GameObject>(info.resName));
        //设置父对象
        airPlaneObj.transform.SetParent(heroPos,false);
        //设置角度和位置 缩放
        airPlaneObj.transform.localPosition = Vector3.zero;
        airPlaneObj.transform.localRotation = Quaternion.identity;
        airPlaneObj.transform.localScale = Vector3.one * info.scale;
        //修改层级(改成UI层)
        airPlaneObj.layer = LayerMask.NameToLayer("UI");
        
        //更新属性
        for (int i = 0; i < 10; i++)
        {
            hpObj[i].SetActive(i < info.hp);
            speedObj[i].SetActive(i < info.speed);
            volumObj[i].SetActive(i < info.volume);
        }
    }

    /// <summary>
    /// 用于删除上一次显示的模型对象
    /// </summary>
    private void DestroyObj()
    {
        if (airPlaneObj != null)
        {
            Destroy(airPlaneObj);
            airPlaneObj = null;
        }
    }

    private float time = 0;
    //是否鼠标选中 模型
    private bool isSel;
    private void Update()
    {
        //让模型 上下浮动 有漂浮感 注意是世界坐标系 因为本地坐标系是斜方向的 
        //Sin范围是 (-1,1)所以不需要我们控制方向 给一个 微小的增量(变化率) 他自己会上下浮动
        time += Time.deltaTime;
        heroPos.Translate(Vector3.up * Mathf.Sin(time) * 0.0001f,Space.World);
        
        //射线检测 让飞机可以随着 鼠标拖动 转动
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
                                1000,
                                1 << LayerMask.NameToLayer("UI")))
            {
                isSel = true;
            }
        }
        //抬起 取消 旋转
        if (Input.GetMouseButtonUp(0))
            isSel = false;
        
        //旋转对象
        if (Input.GetMouseButton(0) && isSel)
        {
            heroPos.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * -20, Vector3.up);
            heroPos.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * -20, Vector3.right);
        }
    }
}
