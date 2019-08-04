using UnityEngine;
using System.Collections;

public class LivingEntity : MonoBehaviour, IDamageable
{

    public float startingHealth;                                //初始血量
    public float health { get; protected set; }                 //生命
    protected bool dead;                                        //判斷死亡

    public event System.Action OnDeath;                         //死亡觸發

    protected virtual void Start()
    {
        health = startingHealth;                                //初始生命
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        // Do some stuff here with hit var
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    public virtual void Die()
    {
        dead = true;
        if (OnDeath != null)
        {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
}
