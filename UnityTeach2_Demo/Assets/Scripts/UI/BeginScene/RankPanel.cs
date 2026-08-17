using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankPanel : BasePanel<RankPanel>
{
    public UIButton btnClose;
    public UIScrollView svList;
    
    //专门用于存储 下面的单条数据控件的
    private List<RankItem> itemList = new List<RankItem>();
    public override void Init()
    {
        btnClose.onClick.Add(new EventDelegate(() =>
        {
            //隐藏自己
            HideMe();
        }));
        //初始化完毕后 隐藏自己
        HideMe();
        //加入测试数据
        for (int i = 0; i < 20; i++)
        {
            GameDataMgr.Instance.AddRankData("KY"+i,Random.Range(40,4000));
        }
    }

    override public void ShowMe()
    {
        base.ShowMe();
        //更新排行榜面板上的信息
        
        //获取本地存储的排行榜数据
        List<RankInfo> list = GameDataMgr.Instance.rankData.rankList;
        //根据数据 更新面板上 组合控件的信息
        //组合控件数量 只会增加 不会减少 因为玩家只会玩游戏 增加数据 不会删除数据
        for (int i = 0; i < list.Count; i++)
        {
            //如果面板上 已经存在 组合控件 直接更新即可
            //(itemList.Count 代表排行榜上 有多少个控件 ，i<list.Count,意思是我们记录下来了排行榜有多少个数据)
            //(如果排行榜上有足够的控件，直接初始化 控件里的数据 不用再创建新的控件了，若是不够，创建新的，然后加入RankItem列表中记录下来
            if (itemList.Count > i)
            {
                itemList[i].InitInfo(i+1,list[i].name,list[i].time);
            }
            else
            {
                //创建预设体
                GameObject obj = Instantiate(Resources.Load<GameObject>("UI/rankItem"));
                //设置父对象
                obj.transform.SetParent(svList.transform,false);
                //设置位置
                obj.transform.localPosition = new Vector3(1, 128 - (i * 60), 0);
                
                //设置数据
                //得到脚本
                RankItem item = obj.GetComponent<RankItem>();
                //调用设置数据的方法
                item.InitInfo(i+1,list[i].name,list[i].time);
                //记录
                itemList.Add(item);
            }
        }
    }
}
