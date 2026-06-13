using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float enemyHealth;

    // Start is called before the first frame update
    void Start()
    {
        enemyHealth = 100;
    }

    public void Update() 
    {
        if (enemyHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
        GameManager.Instance.SpendMoney(-20);

        Debug.Log(enemyHealth);
    }

    public void TakeHeadDamage(int damage)
    {
        enemyHealth -= damage * 2;
        GameManager.Instance.SpendMoney(-30);

        Debug.Log(enemyHealth);
    }
}