using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [Header("Gate Restriction")]
    public bool hasGateRestriction;
    public Gate gate;

    bool playerNearby = false;

    /// <summary>
    /// Returns true if this spawner is available to start spawning from
    /// </summary>
    public bool IsAvailable()
    {
        if (hasGateRestriction)
        {
            return (gate.IsOpen() && !playerNearby);
        }

        return !playerNearby;
    }

    /// <summary>
    /// Will spawn an enemy at this spawn point
    /// </summary>
    /// <param name="enemy">Prefab of the enemy type to be spawned</param>
    /// <param name="healthMultiplier">Optional multiplier for the health of the enemy</param>
    /// <param name="damageMultiplier">Optional multiplier for the damage of the enemy</param>
    public void Spawn(GameObject enemy, float healthMultiplier = 1f, float damageMultiplier = 1f, float speedIncrease = 0f)
    {
        GameObject spawnedEnemy = Instantiate(enemy, transform.position, transform.rotation);
        spawnedEnemy.GetComponent<EnemyHealth>().health *= healthMultiplier;
        spawnedEnemy.GetComponent<EnemyAttack>().damage *= damageMultiplier;

        if (speedIncrease != 0f)
        {
            spawnedEnemy.GetComponent<NavMeshAgent>().speed += speedIncrease;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

}
