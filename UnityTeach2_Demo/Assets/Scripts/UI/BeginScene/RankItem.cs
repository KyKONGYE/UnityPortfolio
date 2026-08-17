using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 个人理解 把排行榜 单条数据 的三个控件 组合成 1个控件，这样就能在 RankPanel里用一个List列表对其进行管理
/// </summary>
public class RankItem : MonoBehaviour
{
    public UILabel labRank;
    public UILabel labName;
    public UILabel labTime;

    /// <summary>
    /// 根据排行榜 单条数据 对组合控件 进行显示初始化
    /// </summary>
    /// <param name="rank">排名</param>
    /// <param name="name">名字</param>
    /// <param name="time">时间</param>
    public void InitInfo(int rank,string name,int time)
    {
        labRank.text = rank.ToString();
        labName.text = name;
        //时间要转换成 时分秒的形式
        string str = "";
        //时
        if(time / 3600 > 0)
            str += time / 3600 + "h";
        //分
        if(time % 3600 /60 > 0 || str!= "")
            str += time % 3600 / 60 + "m";
        //秒
        str += time % 60 + "s";
        labTime.text = str;
    }
}
