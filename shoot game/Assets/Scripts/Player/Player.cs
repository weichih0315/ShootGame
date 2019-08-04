using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{

    public float moveSpeed = 5;                     //移動速度

    public Crosshairs crosshairs;                   //準心   (滑鼠)

    Camera viewCamera;                              //攝影機
    PlayerController controller;                    //角色控制器
    GunController gunController;                    //槍控制器

    //初始
    void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        // Movement input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // Look input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Debug.DrawLine(ray.origin,point,Color.red);
            controller.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                gunController.Aim(point);
            }
        }

        // Weapon input
        if (Input.GetMouseButton(0))                    //滑鼠按下
        {
            gunController.OnTriggerHold();              //槍 觸發保持
        }
        if (Input.GetMouseButtonUp(0))                  //滑鼠放開
        {
            gunController.OnTriggerRelease();           //槍 觸發釋放
        }
        if (Input.GetKeyDown(KeyCode.R))                //補子彈
        {
            gunController.Reload();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gunController.SelectGun(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gunController.SelectGun(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            gunController.SelectGun(2);
        }

        if (transform.position.y < -10)                 //掉落地圖
        {
            TakeDamage(health);                         //直接死亡
        }
    }

    //新關卡  角色設定  暫時未用到
    void OnNewWave(int waveNumber)
    {
        //health = startingHealth;                      //回滿血
        //gunController.EquipGun(waveNumber - 1);       //換槍種
    }

    //角色死亡
    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);        //播放角色死亡音效
        base.Die();
    }

}
