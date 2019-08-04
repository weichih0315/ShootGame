using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("UI")]
    public RectTransform newWaveBanner;                 //關卡旗幟
    public Text newWaveTitle;                           //關卡數顯示
    public Text newWaveEnemyCount;                      //敵人數量
    public Text scoreUI;                                //分數

    [Header("HP UI")]
    public RectTransform healthBar;                     //生命條
    public RectTransform fadeHealthBar;                 //漸退生命條

    [Header("Fade UI")]
    public Image fadePlane;                             //退色布幕

    [Header("GameOver UI")]
    public GameObject gameOverUI;                       //遊戲結束畫面
    public Text gameOverScoreUI;                        //遊戲結束分數


    private float timeBetweenFadeHealthChecks = 1;              //檢查漸退生命條時間間隔
    private float nextfadeHealthCheckTime;                      //下次檢查時間
    private Vector3 fadeHealthScaleOld;                         //HP判斷用
    private bool isFadeHealth;                                  //是否開始漸退

    private Spawner spawner;
    private Player player;

    void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void Start()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
    }

    void Update()
    {
        if (player != null)
        {
            scoreUI.text = ScoreKeeper.score.ToString("D6");            //分數顯示  6位10進制

            float healthPercent = 0;                                    //血條顯示控制
            healthPercent = player.health / player.startingHealth;
            healthBar.localScale = new Vector3(healthPercent, 1, 1);

            isFadeHealth = false;
            if (Time.time > nextfadeHealthCheckTime)              //定時確認是否漸退血條
            {
                nextfadeHealthCheckTime = Time.time + timeBetweenFadeHealthChecks;

                if (healthBar.localScale != fadeHealthBar.localScale && healthBar.localScale == fadeHealthScaleOld)
                    isFadeHealth = true;

                fadeHealthScaleOld = healthBar.localScale;
                if (isFadeHealth)
                    StartCoroutine(FadeHealth(1));
            }
        }
    }

    void OnNewWave(int waveNumber)
    {
        newWaveTitle.text = "- Rank " + waveNumber + " -";
        string enemyCountString = ((spawner.waves[waveNumber - 1].infinite) ? "Infinite" : spawner.waves[waveNumber - 1].enemyCount + "");
        newWaveEnemyCount.text = "Enemies: " + enemyCountString;

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    void OnGameOver()
    {
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, .95f), 1));
        gameOverScoreUI.text = scoreUI.text;
        scoreUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
    }

    IEnumerator AnimateNewWaveBanner()
    {

        float delayTime = 1.5f;
        float speed = 3f;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-170, 45, animatePercent);
            yield return null;
        }

    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    IEnumerator FadeHealth(float time)
    {
        float speed = 1 / time;
        float percent = 0;
        float temp = fadeHealthBar.localScale.x / healthBar.localScale.x;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            
            float fadeScale = Mathf.Lerp(temp, 1, percent);
            fadeHealthBar.localScale = new Vector3 (healthBar.localScale.x * fadeScale, 1, 1);
            yield return null;
        }
    }

    // UI Input
    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
