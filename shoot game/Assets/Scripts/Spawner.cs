using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{

    public bool devMode;                                //測試用  直接過關

    public Wave[] waves;                                //關卡
    public Enemy enemy;                                 //敵人

    LivingEntity playerEntity;                          //玩家父類別用
    Transform playerT;                                  //玩家Transform

    Wave currentWave;                                   //目前關卡
    int currentWaveNumber;                              //關卡編號

    int enemiesRemainingToSpawn;                        //敵人 產生剩餘數量
    int enemiesRemainingAlive;                          //敵人剩餘數量
    float nextSpawnTime;                                //下次產生時間

    MapGenerator map;                                   //地圖

    float timeBetweenCampingChecks = 2;                 //玩家不動時間間隔
    float campThresholdDistance = 1.5f;                 //不動距離門檻
    float nextCampCheckTime;                            //下次不動時間
    Vector3 campPositionOld;                            //不動座標
    bool isCamping;                                     //是否不動

    bool isDisabled;                                    //產生致能

    public event System.Action<int> OnNewWave;          //新關卡觸發

    void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    void Update()
    {
        if (!isDisabled)            //致能
        {
            if (Time.time > nextCampCheckTime)              //定時確認是否不動
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);          //判斷是否不動
                campPositionOld = playerT.position;                                                                 //最後判斷角色座標
            }

            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)         //判斷產生敵人
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");
            }
        }

        //直接過關  測試用
        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }
    }

    //產生敵人動畫
    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();              //以可以行走地板 隨機產生敵人

        if (isCamping)                                              //如果不動  在原地產生敵人
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }

        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColour = tileMat.color;
        Color flashColour = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)                 //閃爍間隔
        {

            tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;       //敵人死亡觸發
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColour);
    }

    //玩家死亡
    void OnPlayerDeath()
    {
        isDisabled = true;          //產生器致能關掉
    }

    //敵人死亡
    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;    //敵人剩餘數量

        if (enemiesRemainingAlive == 0)
        {
            NextWave();             //下一關
        }
    }

    //角色座標回到中心
    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    //換下一關
    void NextWave()
    {
        if (currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound2D("Level Complete");            //播放過關音效
        }
        currentWaveNumber++;

        if (currentWaveNumber - 1 < waves.Length)                           //讀取關卡怪物數量等等
        {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);                               //載入關卡
            }
            ResetPlayerPosition();                                          //角色座標回到中心
        }
    }

    //關卡類別  方便設定
    [System.Serializable]
    public class Wave
    {
        public bool infinite;                       //無限
        public int enemyCount;                      //敵人數量
        public float timeBetweenSpawns;             //產生間隔時間

        public float moveSpeed;                     //移動速度
        public int hitsToKillPlayer;                //多少下殺掉玩家 (目前設定為一般傷害)
        public float enemyHealth;                   //敵人生命值
        public Color skinColour;                    //敵人顏色
    }

}
