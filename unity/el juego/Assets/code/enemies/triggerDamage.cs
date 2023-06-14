using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerDamage : MonoBehaviour
{
    public int damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            HealthManager.healthSingleton.receiveDamage(damage);
        }
    }
}
