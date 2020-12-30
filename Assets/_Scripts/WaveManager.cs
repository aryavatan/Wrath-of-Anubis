using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Game Attributes")]
    public int roundNumber = 0;
    public float timeBetweenWaves = 8f;

    [Header("Spawn Settings")]
    public float timeBetweenSpawns = 1f;
    public float timeBetweenSpawnsMaximum = 2f;
    public int lowerEnemyLimit = 25;
    public int upperEnemyLimit = 40;

    [Header("Enemy Types")]
    public GameObject zombie;
    public GameObject mummy;
    public GameObject bigboy;

    // Private variables
    int playerKills = 0;
    bool startSpawning = false;
    bool lockSpawning = false;
    float spawnTimer = 0f;
    float _timeBetweenSpawns = 1f;
    int _lowerEnemyLimit = 25;
    int _upperEnemyLimit = 40;


    // Dependencies
    GameUI ui;
    Spawner[] spawnerList;
    Round currentRound;


    private void Awake()
    {
        spawnerList = GetComponentsInChildren<Spawner>();
        ui = FindObjectOfType<GameUI>();
    }

    // Start the initial round in the Start method
    private void Start()
    {
        _timeBetweenSpawns = timeBetweenSpawns;
        _lowerEnemyLimit = lowerEnemyLimit;
        _upperEnemyLimit = upperEnemyLimit;
        ui.SetWaveCounter(roundNumber);
        StartNewRound();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsRoundOver())
        {
            roundNumber++;
            StartNewRound();
        }
        else if (startSpawning && spawnTimer > _timeBetweenSpawns && !lockSpawning)
        {
            spawnTimer = 0f;
            Spawn();
            CheckEnemyLimits();
        }
        else
        {
            CheckSpawnLock();
        }

        spawnTimer += Time.deltaTime;
    }

    /// <summary>
    /// Updates the wave counter, generates a new round object, and begins to spawn from the the new Round object.
    /// </summary>
    void StartNewRound()
    {
        // Determine the upper and lower enemy limits
        if (roundNumber % 5 == 0)
        {
            _lowerEnemyLimit = lowerEnemyLimit / 5;
            _upperEnemyLimit = upperEnemyLimit / 2;
        }
        else
        {
            _lowerEnemyLimit = lowerEnemyLimit;
            _upperEnemyLimit = upperEnemyLimit;
        }

        currentRound = new Round(roundNumber);
        StartCoroutine(StartSpawning());
    }

    /// <summary>
    /// Returns true if all enemies in current round have been defeated, false otherwise
    /// </summary>
    bool IsRoundOver()
    {
        // First check if Round enemy count is 0
        if (currentRound.enemies.Count != 0)
            return false;

        // Then check if any enemies are still alive
        EnemyHealth[] enemiesInScene = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in enemiesInScene)
        {
            if (enemy.health > 0)
                return false;
        }

        // Round is finished if all enemies are defeated
        startSpawning = false;
        return true;
    }

    /// <summary>
    /// Enables enemy spawning from Round object after the delay between waves
    /// </summary>
    IEnumerator StartSpawning()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        ui.SetWaveCounter(roundNumber);
        startSpawning = true;
    }

    void Spawn()
    {
        if (currentRound.enemies.Count != 0)
        {
            // Find a random spawner that is available
            Spawner spawner;
            do
            {
                spawner = spawnerList[Random.Range(0, spawnerList.Length)];
            }
            while (!spawner.IsAvailable());

            // Spawn the next enemy at this spawner
            switch (currentRound.NextEnemy())
            {
                case EnemyType.ZOMBIE:
                    spawner.Spawn(zombie, currentRound.zombieHealth, currentRound.zombieDamage, currentRound.zombieSpeedIncrease);
                    break;
                case EnemyType.MUMMY:
                    spawner.Spawn(mummy, currentRound.mummyHealth, currentRound.mummyDamage);
                    break;
                case EnemyType.BIGBOY:
                    spawner.Spawn(bigboy, currentRound.bigboyHealth, currentRound.bigboyDamage);
                    break;
            }
        }
        else
        {
            startSpawning = false;
        }
    }

    /// <summary>
    /// This method will update the time between spawns with respect to the lower and upper enemy limit thresholds.
    /// </summary>
    void CheckEnemyLimits()
    {
        EnemyHealth[] enemiesInScene = FindObjectsOfType<EnemyHealth>();

        if (enemiesInScene.Length < _lowerEnemyLimit)
        {
            _timeBetweenSpawns = timeBetweenSpawns;
            return;
        }
        else
        {
            int count = enemiesInScene.Length;
            for (int i = 0; i < enemiesInScene.Length; i++)
            {
                if (enemiesInScene[i].health <= 0f)
                {
                    if (--count < _lowerEnemyLimit)
                    {
                        _timeBetweenSpawns = timeBetweenSpawns;
                        return;
                    }
                }
            }

            if (count >= _upperEnemyLimit)
            {
                lockSpawning = true;
                return;
            }

            else if (count >= _lowerEnemyLimit)
            {
                _timeBetweenSpawns = timeBetweenSpawnsMaximum;
                return;
            }
        }

        _timeBetweenSpawns = timeBetweenSpawns;
    }

    /// <summary>
    /// This method will disable the spawn lock if the number of enemies in the scene goes below the upper enemy limit.
    /// </summary>
    void CheckSpawnLock()
    {
        EnemyHealth[] enemiesInScene = FindObjectsOfType<EnemyHealth>();
        int count = enemiesInScene.Length;
        for (int i = 0; i < enemiesInScene.Length; i++)
        {
            if (enemiesInScene[i].isDead)
            {
                if (--count < upperEnemyLimit)
                {
                    lockSpawning = false;
                    return;
                }
            }
        }
    }

    public void AddPlayerKill()
    {
        playerKills++;
    }

    public int GetPlayerKills()
    {
        return playerKills;
    }

    public void SaveGameStatistics()
    {
        // Load the player statistics
        PlayerStatistics stats = PlayerStatistics.Load();

        // Update the player statistics
        if (stats.highestRound < roundNumber)
            stats.highestRound = roundNumber;
        if (stats.highestKillsPerGame < playerKills)
            stats.highestKillsPerGame = playerKills;
        stats.totalKills += playerKills;

        // Save the player statistics
        PlayerStatistics.Save(stats);
    }

    /// <summary>
    /// Encapsulates all the information about the current round
    /// </summary>
    private class Round
    {
        public List<EnemyType> enemies;

        // Enemy attributes
        public float zombieHealth = 1f;
        public float zombieDamage = 1f;
        public float zombieSpeedIncrease = 0f;
        public float mummyHealth = 1f;
        public float mummyDamage = 1f;
        public float bigboyHealth = 1f;
        public float bigboyDamage = 1f;

        /// <summary>
        /// The constructor will determine which and how many enemies will spawn in this round number
        /// </summary>
        public Round(int roundNumber)
        {
            int totalEnemies = 2 + (roundNumber * 6);

            // Special "5" Rounds
            if (roundNumber % 5 == 0)
            {
                totalEnemies /= 5;
                enemies = (new EnemyType[totalEnemies]).OfType<EnemyType>().ToList();
                
                int totalBigboys = roundNumber / 5;
                for (int i = 0; i < totalBigboys; i++)
                {
                    enemies[i] = EnemyType.BIGBOY;
                }

                // If at least round 10, spawn mummies as well
                if (roundNumber >= 10)
                {
                    totalEnemies -= totalBigboys;
                    int totalMummies = (int)Mathf.Round(totalEnemies * getMummyPercentage(roundNumber));
                    for (int i = enemies.Count-1; i > enemies.Count - 1 - totalMummies; i--)
                    {
                        enemies[i] = EnemyType.MUMMY;
                    }
                }
            }
            else if (roundNumber > 2)
            {
                // Regular upper rounds
                enemies = (new EnemyType[totalEnemies]).OfType<EnemyType>().ToList();
                int totalMummies = (int)Mathf.Round(totalEnemies * getMummyPercentage(roundNumber));
                
                for (int i = 0; i < totalMummies; i++)
                {
                    enemies[i] = EnemyType.MUMMY;
                }
            }
            else
            {
                // If round less than 2, only zombies
                enemies = (new EnemyType[totalEnemies]).OfType<EnemyType>().ToList();
            }

            SetEnemyAttributes(roundNumber);
        }
        
        /// <summary>
        /// Returns the next enemy type to be spawned in the round, chosen randomly
        /// </summary>
        public EnemyType NextEnemy()
        {
            if (enemies.Count == 0)
                return EnemyType.NULL;

            int rand = Random.Range(0, enemies.Count - 1);
            
            EnemyType returnType = enemies[rand];
            enemies.RemoveAt(rand);

            return returnType;
        }

        /// <summary>
        /// Returns the percentage of total enemies to be mummies in this round
        /// </summary>
        private float getMummyPercentage(int roundNumber)
        {
            if (roundNumber < 6)
            {
                return Random.Range(0.1f, 0.25f);
            }
            else
            {
                return Random.Range(0.2f, 0.35f);
            }
        }
    
        /// <summary>
        /// This method will set the health and damage multipliers for all the enemies with respect to the round number.
        /// </summary>
        private void SetEnemyAttributes(int roundNumber)
        {
            if (roundNumber % 5 == 0)
            {
                bigboyHealth = 0.9f + (roundNumber / 50f);
                bigboyDamage = 0.8f + (roundNumber / 25f);
            }
            if (roundNumber > 2)
            {
                zombieHealth = 1f + ((roundNumber - 2) / 10f);
                zombieDamage = 1f + ((roundNumber - 2) / 15f);
                zombieSpeedIncrease = roundNumber < 6 ? ((roundNumber - 2) / 4f) : 1f;

                mummyHealth = 1f + (roundNumber / 25f);
                mummyDamage = 1f + (roundNumber / 30f);
            }
        }
    }

    /// <summary>
    /// An enum containing the different enemy types
    /// </summary>
    private enum EnemyType
    {
        ZOMBIE,
        MUMMY,
        BIGBOY,
        NULL
    }
}

