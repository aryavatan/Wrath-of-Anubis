using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float health = 50f;
    public int pointsOnHit = 10;
    public int pointsOnDeath = 50;
    public float despawnTime = 10f;
    public AudioSource damageAudioSource;

    // Taken off in 1.0 update 
    //public AudioClip hurtSound;

    public bool isDead = false;

    private void Start()
    {
        setRigidbodyState(true);
        setColliderState(true);
    }

    public bool TakeDamage(float damage)
    {
        damageAudioSource.Play();
        health -= damage;
        if (!isDead)
        {
            // Taken off in 1.0 update 
            //damageAudioSource.PlayOneShot(hurtSound);

            if (health <= 0)
            {
                isDead = true;
                Die();
                return true;
            }
            else
            {
                FindObjectOfType<GameUI>().AddMoney(pointsOnHit);
                return false;
            }
        }
        return true;
    }

    void Die()
    {
        // Disable other behaviors
        GetComponent<EnemyMovement>().Die();
        GetComponent<EnemyAttack>().Die();

        GetComponent<Animator>().enabled = false;

        setRigidbodyState(false);
        setColliderState(true);
        GetComponent<Collider>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        
        Destroy(gameObject, despawnTime);

        FindObjectOfType<GameUI>().AddMoney(pointsOnDeath);
        FindObjectOfType<WaveManager>().AddPlayerKill();

        this.enabled = false;
    }

    void setRigidbodyState(bool state)
    {
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = state;
            if (!state)
            {
                rb.gameObject.layer = 11;
                rb.transform.parent.gameObject.layer = 11;
            }
        }
    }

    public void setColliderState(bool state)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = state;
        }
    }
}
