using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{

    public Rigidbody2D bulletRB;
    public float speed;

    public float bulletLife;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(destroyAfterTime(bulletLife));
        bulletRB = GetComponent<Rigidbody2D>();
        bulletRB.velocity = new Vector2(speed, bulletRB.velocity.y);
    }
    
    IEnumerator destroyAfterTime(float time){
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }

/*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            HealthManager.healthSingleton.receiveDamage(damage);
            Destroy(gameObject);
        }
    }
*/
}