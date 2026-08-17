using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPanel : BasePanel<EndPanel>
{
    public UIButton btnSure;
    public UIInput inputName;

    public UILabel labTime;

    private int endTime;
    public override void Init()
    {
        btnSure.onClick.Add(new EventDelegate(() =>
        {
            //恢复时间
            Time.timeScale = 1;
            //保存玩家 用户名与通关时间
            GameDataMgr.Instance.AddRankData(inputName.text,endTime);
            //结束游戏 返回开始面板
            SceneManager.LoadScene("BeginScene");
        }));
        HideMe();;
    }

    public override void ShowMe()
    {
        base.ShowMe();
        Time.timeScale = 0;
        //显示该面板时 就应该去记录 当前的时间
        //因为我们在游戏界面声明的这两个 对象都是public的，所以我们直接得
        //存储的是一个秒数
        endTime = (int)GamePanel.Instance.nowTime;
        //从游戏界面得到 显示的 当前时间
        labTime.text = GamePanel.Instance.labTime.text;
    }
}
