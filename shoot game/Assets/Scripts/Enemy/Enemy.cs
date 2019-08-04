using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class Enemy : LivingEntity
{
    //狀態
    public enum State { Idle, Chasing, Attacking };               
    State currentState;

    //死亡特效
    public ParticleSystem deathEffect;
    //死亡觸發(例:分數.....)
    public static event System.Action OnDeathStatic;

    UnityEngine.AI.NavMeshAgent pathfinder;         //路徑搜尋
    Transform target;                               //玩家座標
    LivingEntity targetEntity;                      //使用玩家父類別方法 用
    Material skinMaterial;                          //皮膚材質

    Color originalColour;                           //原色

    float attackDistanceThreshold = .5f;            //攻擊距離門檻
    float timeBetweenAttacks = 1;                   //攻擊時間間距
    float damage = 1;                               //攻擊傷害

    float nextAttackTime;                           //下次攻擊時間
    float myCollisionRadius;                        //自身剛體半徑
    float targetCollisionRadius;                    //玩家剛體半徑

    bool hasTarget;                                 //是否有玩家

    void Awake()
    {
        pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent>();                   //初始路徑搜尋

        if (GameObject.FindGameObjectWithTag("Player") != null)                     //是否玩家存在   初始設定
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (hasTarget)                                  //是否有玩家 
        {
            currentState = State.Chasing;               //追逐狀態
            targetEntity.OnDeath += OnTargetDeath;      //玩家死亡  觸發

            StartCoroutine(UpdatePath());               //開始更新路徑
        }
    }

    void Update()
    {
        if (hasTarget)                                  //是否有玩家
        {
            if (Time.time > nextAttackTime)             //現在時間 大於 下次可攻擊時間
            {
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;                                 //sqrMagnitude 長度平方
                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))     //與玩家距離 小於 攻擊距離
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;                                                        //下次可攻擊時間 = 現在時間 + 攻擊時間間隔
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);                                    //播放攻擊音效
                    StartCoroutine(Attack());                                                                               //攻擊動畫
                }

            }
        }

    }

    //設定怪物特色(各種屬性)
    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColour)
    {
        pathfinder.speed = moveSpeed;                                                           //移動速度

        if (hasTarget)
        {
            //damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);              //多少下玩家死亡
            damage = hitsToKillPlayer;                                                          //一般傷害
        }
        startingHealth = enemyHealth;                                                           //生命值

        deathEffect.startColor = new Color(skinColour.r, skinColour.g, skinColour.b, 1);        //死亡特效顏色
        skinMaterial = GetComponent<Renderer>().material;                                       //本身材質
        skinMaterial.color = skinColour;                                                        //外型顏色
        originalColour = skinMaterial.color;                                                    //設定原色
    }

    //受傷
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);      //播放  受傷音效

        if (damage >= health)                   //死亡
        {
            if (OnDeathStatic != null)          //觸發死亡方法  對應的
            {
                OnDeathStatic();
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);     //播放  死亡音效
            //物件消除時  創造死亡特效  依方向噴出
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    //玩家死亡觸發
    void OnTargetDeath()
    {
        hasTarget = false;              //玩家消失
        currentState = State.Idle;      //靜止狀態
    }

    //攻擊
    IEnumerator Attack()
    {

        currentState = State.Attacking;                                                         //攻擊狀態
        pathfinder.enabled = false;                                                             //停止路徑搜尋

        Vector3 originalPosition = transform.position;                                          //自身座標
        Vector3 dirToTarget = (target.position - transform.position).normalized;                //向量距離
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);           //攻擊座標

        float attackSpeed = 3;                                                                  //攻擊速度
        float percent = 0;                                                                      //動畫百分比

        skinMaterial.color = Color.red;                                                         //攻擊變色用
        bool hasAppliedDamage = false;                                                          //攻擊判定栓鎖

        while (percent <= 1)
        {

            if (percent >= .5f && !hasAppliedDamage)                                            //攻擊動畫   一半達成判定
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;                       //動畫曲線 y = -4x^2 + 4x
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColour;                                                    //攻擊完 變回原色
        currentState = State.Chasing;                                                           //狀態改回追逐
        pathfinder.enabled = true;                                                              //開啟路徑搜尋
    }

    //更新路徑    目前只有追逐
    IEnumerator UpdatePath()                
    {
        float refreshRate = .25f;                       //每次追逐時間間隔

        while (hasTarget)                               //玩家是否存在
        {
            if (currentState == State.Chasing)          //確認是否為追逐狀態
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;    // 比例 (單位向量)
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold - 0.1F); // 追逐座標   0.1為了比攻擊距離短
                if (!dead)      //父類別
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}