using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TipPanel : BasePanel<TipPanel>
{
    public UIButton btnSure;
    public UIButton btnClose;
    
    public override void Init()
    {
        btnSure.onClick.Add(new EventDelegate(() =>
        {
            //返回开始场景
            SceneManager.LoadScene("BeginScene");
        }));

        btnClose.onClick.Add(new EventDelegate(() =>
        {
            //继续游戏
            Time.timeScale = 1;
            HideMe();
                
        }));
        //开始时隐藏自己
        HideMe();
    }
    
}
