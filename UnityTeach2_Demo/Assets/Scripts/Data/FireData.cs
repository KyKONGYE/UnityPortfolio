using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;


public class FireData 
{
    public List<FireInfo> fireInfoList =  new List<FireInfo>();
}

public class FireInfo
{ 
    [XmlAttribute]
    public int id;//开火点Id
    [XmlAttribute]
    public int type; //开火点类型 是散弹 还是 顺序发射 1顺序 2散弹
    [XmlAttribute]
    public int num;//数量 该组子弹有多少颗
    [XmlAttribute]
    public float cd;//没颗子弹的间隔时间
    [XmlAttribute]
    public string ids;//关联的 子弹ID 1-10代表的 就是在 1~10ID的 子弹数据中去随机
    [XmlAttribute]
    public float delay;//组间 间隔时间
}
