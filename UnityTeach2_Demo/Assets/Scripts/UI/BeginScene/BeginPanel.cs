using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 开始界面
/// </summary>
public class BeginPanel : BasePanel<BeginPanel>
{
    public UIButton btnStart;
    public UIButton btnRank;
    public UIButton btnSetting;
    public UIButton btnQuit;
    
    public override void Init()
    {
        //添加监听事件
        btnStart.onClick.Add(new EventDelegate(() =>
        {
            //显示 选角面板
            ChoosePanel.Instance.ShowMe();
            //隐藏自己
            HideMe();
        }));
        
        btnRank.onClick.Add(new EventDelegate(() =>
        {
            //显示 排行榜面板
            RankPanel.Instance.ShowMe();
        }));
        
        btnSetting.onClick.Add(new EventDelegate(() =>
        {
            //显示 设置面板
            SettingPanel.Instance.ShowMe();
            
        }));
        
        btnQuit.onClick.Add(new EventDelegate(() =>
        {
            //退出游戏
            Application.Quit();
        }));
    }
}
