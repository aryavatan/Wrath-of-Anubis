using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyMovement : MonoBehaviour
{
    [Header("Zombie Attack Time")]
    public float attackTime = 2f;

    [Header("Zombie Footsteps")]
    public AudioSource footsteps;
    public float footstepsSpeed = 1f;

    [Header("Zombie Moaning")]
    public AudioSource ZombieSoundsSource;
    public AudioClip[] ZombieSounds;
    Transform player;
    NavMeshAgent nav;
    Animator anim;
    float stopTimer = 3f;
    float soundTimer = 10f;
    bool playerDead = false;
    bool playerInRange = false;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Disable footsteps in Prototype map
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            footsteps.clip = null;
        }
        else
        {
            footsteps.pitch = footstepsSpeed;
        }
    }

    void Update()
    {
        if (stopTimer >= attackTime && !playerDead && !playerInRange)
        {
            nav.isStopped = false;
            nav.SetDestination(player.position);
            anim.SetBool("Running", true);
        }

        if (soundTimer >= 3f)
        {
            ZombieMoan();
        }

        // Zombie footsteps
        if (!nav.isStopped)
            PlayFootsteps(true);
        else
            PlayFootsteps(false);

        soundTimer += Time.deltaTime;
        stopTimer += Time.deltaTime;
    }

    public void Die()
    {
        nav.isStopped = true;
        PlayFootsteps(false);
        this.enabled = false;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            nav.isStopped = true;
            playerInRange = true;
            stopTimer = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            playerInRange = false;
        }
    }

    private void ZombieMoan()
    {
        int index = Random.Range(0, ZombieSounds.Length);
        ZombieSoundsSource.PlayOneShot(ZombieSounds[index], 0.2f);
        soundTimer = 0f;
    }

    public void PlayerDied()
    {
        playerDead = true;
        if (nav.enabled)
            nav.isStopped = true;
        anim.SetBool("Running", false);
    }

    bool footstepPlaying = false;
    void PlayFootsteps(bool state)
    {
        if (footsteps.clip)
        {
            if (state == true && footstepPlaying == false)
            {
                footstepPlaying = true;
                footsteps.Play();
            }
            else if (state == false)
            {
                footsteps.Stop();
                footstepPlaying = false;
            }
        }
    }

    public void ResetStopTimer()
    {
        stopTimer = 0f;
    }
}
