using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePanel : BasePanel<GamePanel>
{
    public UIButton btnClose;
    public UILabel labTime;
    
    public List<GameObject> hpObj;

    //当前游戏运行的时间
    public float nowTime = 0;
    
    public override void Init()
    {
        btnClose.onClick.Add(new EventDelegate(() =>
        {
            //点击 退出按钮后
            //暂停游戏
            Time.timeScale = 0;
            //显示 确定退出面板
            TipPanel.Instance.ShowMe();
        }));
    }

    /// <summary>
    /// 提供给外部 改变血量的方法
    /// </summary>
    /// <param name="hp"></param>
    public void ChangeHp(int hp)
    {
        for (int i = 0; i < hpObj.Count; i++)
        {
            hpObj[i].SetActive(i < hp);
        }
    }
    
    private void Update()
    {
        nowTime += Time.deltaTime;
        //更新时间显示
        labTime.text = "";
        
        //时
        if((int)nowTime / 3600 > 0)
            labTime.text += (int)nowTime / 3600 + "h";
        //分
        if((int)nowTime % 3600 /60 > 0 || labTime.text!= "")
            labTime.text += (int)nowTime % 3600 / 60 + "m";
        //秒
        labTime.text += (int)nowTime % 60 + "s";
    }
}
