using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    Transform groundRing;
    [SerializeField]
    Transform bullets;

    Animator anim;
    AudioSource audio;

    float rotateSpeed = 50f;
    float defaultYPosition;
    bool bounceDown = false;
    float bounceRadius = 0.15f;
    Vector3 movement = new Vector3(0f, 0.3f, 0f);

    static int killsNeeded = 60;
    float despawnTime = 10f;


    private void Awake()
    {
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        defaultYPosition = bullets.transform.position.y;
    }

    float timer = 0f;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > despawnTime)
        {
            DespawnPowerup();
        }
    }

    private void FixedUpdate()
    {
        RotateRing();
        BounceBullets();
    }

    void RotateRing()
    {
        groundRing.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        bullets.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    void BounceBullets()
    {
        if (!bounceDown)
        {
            bullets.position += movement * Time.deltaTime;
            if (bullets.position.y >= defaultYPosition + bounceRadius)
            {
                bounceDown = true;
            }
        }
        else if (bounceDown)
        {
            bullets.position -= movement * Time.deltaTime;
            if (bullets.position.y <= defaultYPosition - bounceRadius)
            {
                bounceDown = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnablePowerup();
            Invoke("DestroyPowerup", 2f);
            //DestroyPowerup();
        }
    }

    void EnablePowerup()
    {
        GetComponent<BoxCollider>().enabled = false;

        audio.Play();
        anim.SetTrigger("End");

        FindObjectOfType<Gun>().RefillAmmo();   
    }

    void DestroyPowerup()
    {
        Destroy(gameObject);
    }

    void DespawnPowerup()
    {
        GetComponent<BoxCollider>().enabled = false;
        anim.SetTrigger("End");
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// This method will determine if it's time to spawn another powerup
    /// </summary>
    public static bool SpawnPowerup()
    {
        killsNeeded--;
        
        if (killsNeeded > 0)
        {
            return false;
        }

        killsNeeded = Random.Range(40, 100);
        return true;
    }

    /// <summary>
    /// Reset the static variable responsible for determining when the next powerup spawn will be
    /// </summary>
    public static void ResetPowerupSpawns()
    {
        killsNeeded = Random.Range(40, 100);
    }
}
