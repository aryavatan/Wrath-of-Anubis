using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 15f;
    public float damageRate = 1;
    public float hitDelay;

    Animator anim;
    float timer = 0f;
    bool playerInRange = false;
    bool dead = false;
    EnemyMovement movementScript;

    [Header("Debuffs On Attack")]
    public bool slownessDebuff = false;
    public float slownessDuration = 1f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        movementScript = GetComponent<EnemyMovement>();
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
        movementScript.ResetStopTimer();
        StartCoroutine(DamagePlayer(player, hitDelay));
        timer = 0f;
    }

    IEnumerator DamagePlayer(PlayerHealth player, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerInRange && !dead)
        {
            player.TakeDamage(damage);
            if (slownessDebuff)
            {
                player.GetComponent<PlayerMovement>().AddSlownessDebuff(slownessDuration);
                FindObjectOfType<GameUI>().EnableSlownessDebuffUI(slownessDuration);
            }
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
