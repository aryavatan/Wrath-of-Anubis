using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 15f;
    public float damageRate = 1;
    public float hitDelay;

    Animator anim;
    EnemyHealth health;
    float timer = 0f;
    bool playerInRange = false;
    bool dead = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        timer += Time.deltaTime;    
    }

    private void OnTriggerStay(Collider collision)
    {
        bool readyToAttack = timer > (1 / damageRate);
        
        if (collision.gameObject.tag == "Player")
        {
            playerInRange = true;
            if (playerInRange && readyToAttack)
            {
                Attack(collision.gameObject.GetComponent<PlayerHealth>());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerInRange = false;
        }
    }

    private void Attack(PlayerHealth player)
    {
        anim.SetTrigger("Attack");
        StartCoroutine(DamagePlayer(player, hitDelay));
        timer = 0f;
    }

    IEnumerator DamagePlayer(PlayerHealth player, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerInRange && !dead)
        {
            player.TakeDamage(damage);
        }
    }

    public void Die()
    {
        GetComponent<Collider>().enabled = false;
        playerInRange = false;
        dead = true;
        this.enabled = false;
    }
}
