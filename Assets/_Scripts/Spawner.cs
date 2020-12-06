using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject zombie;
    public float spawnRate = 2f;
    float timer;

    bool spawnerOn = false;
    public int spawnCount;
    float enemyHealthMultiplier;
    float enemyDamageMultiplier;

    // Update is called once per frame
    void Update()
    {
        if (spawnerOn)
        {
            timer += Time.deltaTime;

            if (timer > spawnRate && spawnCount > 0)
            {
                // Spawn enemy
                GameObject spawn = Instantiate(zombie, transform.position, transform.rotation);

                // Adjust difficulty
                spawn.GetComponent<EnemyHealth>().health *= enemyHealthMultiplier;
                spawn.GetComponent<EnemyAttack>().damage *= enemyDamageMultiplier;

                // Turn off spawner when the quota of zombies spawned has been met
                if (--spawnCount == 0)
                {
                    spawnerOn = false;
                }
                
                timer = 0f;
            }
        }
    }

    public void StartWave(int spawnCount, float enemyHealthMultiplier, float enemyDamageMultiplier)
    {
        this.spawnCount = spawnCount;
        this.enemyHealthMultiplier = enemyHealthMultiplier;
        this.enemyDamageMultiplier = enemyDamageMultiplier;

        spawnerOn = true;
        timer = 0f;
    }
}
