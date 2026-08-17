using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPanel : BasePanel<SettingPanel>
{
    //音乐开关
    public UIToggle togMusic;
    //音效开关
    public UIToggle togSound;
    //音乐大小
    public UISlider sldMusic;
    //音量大小
    public UISlider sldSound;
    
    //关闭按钮
    public UIButton btnClose;

    public override void Init()
    {
        //监听事件添加
        togMusic.onChange.Add(new EventDelegate(() =>
        {
            //开关 背景音乐 还要改变数据
            GameDataMgr.Instance.SetMusicIsOpen(togMusic.value);
        }));
        
        togSound.onChange.Add(new EventDelegate(() =>
        {
            //开关 游戏音效 还要改变数据
            GameDataMgr.Instance.SetSoundIsOpen(togSound.value);
        }));
        
        sldMusic.onChange.Add(new EventDelegate(() =>
        {
            //背景音乐 音量大小调节 还要改变数据
            GameDataMgr.Instance.SetMusicVolume(sldMusic.value);
        }));
        
        sldSound.onChange.Add(new EventDelegate(() =>
        {
            //背景音乐 音效大小调节 还要改变数据
            GameDataMgr.Instance.SetSoundVolume(sldSound.value);
        }));
        
        btnClose.onClick.Add(new EventDelegate(() =>
        {
            //隐藏自己
            HideMe();
        }));
        
        //游戏开始时 初始化完成后 隐藏自己
        HideMe();
    }

    public override void ShowMe()
    {
        base.ShowMe();
        //显示自己时 更新上面的内容
        MusicData musicData = GameDataMgr.Instance.musicData;
        togMusic.value = musicData.musicIsOpen;
        togSound.value = musicData.SoundIsOpen;
        sldMusic.value = musicData.musicvalue;
        sldSound.value = musicData.soundValue;
    }

    public override void HideMe()
    {
        base.HideMe();
        //隐藏自己时 需要保存该次设置的数据
        GameDataMgr.Instance.SaveMusicData();
    }
}
