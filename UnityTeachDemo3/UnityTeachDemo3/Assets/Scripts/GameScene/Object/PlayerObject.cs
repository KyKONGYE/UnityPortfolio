using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    private Animator animator;

    //1.玩家属性的初始化
    //玩家攻击力
    private int atk;
    //玩家拥有的钱
    public int money;
    //旋转的速度
    private float roundSpeed = 50;

    //持枪对象才有的开火点
    public Transform gunPoint;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    /// <summary>
    /// 初始化玩家基础属性
    /// </summary>
    /// <param name="atk"></param>
    /// <param name="money"></param>
    public void InitPlayerInfo(int atk, int money)
    {
        this.atk = atk;
        this.money = money;
        //更新界面上钱的数量
        UpdateMoney();
    }

    // Update is called once per frame
    void Update()
    {
        //2.移动变化 动作变化
        //移动动作的变换 由于动作有位移  我们也应用了动作的位移  所以只要改变这两个值 就会有动作的变化 和 速度的变化
        animator.SetFloat("VSpeed", Input.GetAxis("Vertical"));
        animator.SetFloat("HSpeed", Input.GetAxis("Horizontal"));
        //旋转
        this.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * roundSpeed * Time.deltaTime);

        if( Input.GetKeyDown(KeyCode.LeftShift) )
        {
            animator.SetLayerWeight(1, 1);
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            animator.SetLayerWeight(1, 0);
        }

        if (Input.GetKeyDown(KeyCode.R))
            animator.SetTrigger("Roll");

        if (Input.GetMouseButtonDown(0))
            animator.SetTrigger("Fire");
            
    }


    //3.攻击动作的不同处理
    /// <summary>
    /// 专门用于处理刀武器攻击动作的伤害检测事件
    /// </summary>
    public void KnifeEvent()
    {
        //进行伤害检测
        Collider[] colliders = Physics.OverlapSphere(this.transform.position + this.transform.forward + this.transform.up, 1, 1 << LayerMask.NameToLayer("Monster"));

        //播放音效
        GameDataMgr.Instance.PlaySound("Music/Knife");

        //暂时无法继续写逻辑了 因为 我们没有怪物对应的脚本
        for (int i = 0; i < colliders.Length; i++)
        {
            //得到碰撞到的对象上的怪物脚本 让其受伤
            MonsterObject monster = colliders[i].gameObject.GetComponent<MonsterObject>();
            if (monster != null && !monster.isDead)
            {
                monster.Wound(this.atk);
                break;
            }
        }
    }

    public void ShootEvent()
    {
        //进行摄像检测 
        //前提是需要有开火点
        RaycastHit[] hits = Physics.RaycastAll(new Ray(gunPoint.position, this.transform.forward), 1000, 1 << LayerMask.NameToLayer("Monster"));

        //播放开枪音效
        GameDataMgr.Instance.PlaySound("Music/Gun");

        for (int i = 0; i < hits.Length; i++)
        {
            //得到对象上的怪物脚本 让其受伤
            //得到碰撞到的对象上的怪物脚本 让其受伤
            MonsterObject monster = hits[i].collider.gameObject.GetComponent<MonsterObject>();
            if (monster != null && !monster.isDead)
            {
                //进行打击特效的创建
                GameObject effObj = Instantiate(Resources.Load<GameObject>(GameDataMgr.Instance.nowSelRole.hitEff));
                effObj.transform.position = hits[i].point;
                effObj.transform.rotation = Quaternion.LookRotation(hits[i].normal);
                Destroy(effObj, 1);

                monster.Wound(this.atk);
                break;
            }
        }
    }


    //4.钱变化的逻辑
    public void UpdateMoney()
    {
        //间接的更新界面上 钱的数量
        UIManager.Instance.GetPanel<GamePanel>().UpdateMoney(money);
    }

    /// <summary>
    /// 提供给外部加钱的方法
    /// </summary>
    /// <param name="money"></param>
    public void AddMoney(int money)
    {
        //加钱
        this.money += money;
        UpdateMoney();
    }
}
