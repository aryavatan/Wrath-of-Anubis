using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public int round = 0;
    public float timeBetweenWaves = 8f;
    public bool endlessMode = false;
    public int maxEnemyCount = 96;

    int endlessSpawnWave = 1;

    GameUI ui;
    Spawner[] spawnerList;
    bool roundFinished = false;
    float timer = 0f;

    int playerKills = 0;

    private void Awake()
    {
        //spawnerList = FindObjectsOfType<Spawner>();
        spawnerList = GetComponentsInChildren<Spawner>();
        ui = FindObjectOfType<GameUI>();
    }

    // Start the initial round in the Start method
    private void Start()
    {
        if (endlessMode)
        {
            StartEndlessMode();
        }
        //else
        //{
        //    StartNewRound();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (endlessMode)
        {
            ui.SetEndlessModeTimer(timer);
            CheckEndlessModeSpawning();
            timer += Time.deltaTime;
        }
        else if (!roundFinished)
        {
            CheckIfRoundOver();
        }
        else if (timer > timeBetweenWaves)
        {
            round++;
            StartNewRound();
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    void StartNewRound()
    {
        ui.SetWaveCounter(round);
        roundFinished = false;

        int spawnCount = 4 + (4 * round);
        float healthMultiplier, damageMultiplier;
            
        if (round <= 5)
        {
            healthMultiplier = 1f;
            damageMultiplier = 1f;
        }
        else
        {
            healthMultiplier = 1f + (round / 100f);
            damageMultiplier = 1f + (round / 100f);
        }

        foreach (Spawner spawner in spawnerList)
        {
            spawner.StartWave(spawnCount / spawnerList.Length, healthMultiplier, damageMultiplier);
        }
    }

    void StartEndlessMode()
    {
        int spawnCount = 32;
        float healthMultiplier = 1f + ((endlessSpawnWave - 1) / 100f);
        float damageMultiplier = 1f + ((endlessSpawnWave - 1) / 100f);

        foreach (Spawner spawner in spawnerList)
        {
            spawner.StartWave(spawnCount / spawnerList.Length, healthMultiplier, damageMultiplier);
        }
    }

    void CheckEndlessModeSpawning()
    {
        // First check if all spawners have finished spawning
        foreach (Spawner spawner in spawnerList)
        {
            if (spawner.spawnCount > 0)
            {
                return;
            }
        }

        int alive = 0;

        EnemyHealth[] enemiesInScene = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in enemiesInScene)
        {
            if (enemy.health > 0)
            {
                alive++;
            }
        }

        if (alive <= maxEnemyCount)
        {
            endlessSpawnWave++;
            StartEndlessMode();
        }
    }

    void CheckIfRoundOver()
    {
        // First check if all spawners have finished spawning
        foreach (Spawner spawner in spawnerList)
        {
            if (spawner.spawnCount > 0)
            {
                return;
            }
        }

        // Then check if any enemies are still alive
        EnemyHealth[] enemiesInScene = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in enemiesInScene)
        {
            if (enemy.health > 0)
            {
                return;
            }
        }

        // If all enemies defeated, finish the round
        roundFinished = true;
        timer = 0f;
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
        if (stats.highestRound < round)
            stats.highestRound = round;
        if (stats.highestKillsPerGame < playerKills)
            stats.highestKillsPerGame = playerKills;
        if (endlessMode && stats.endlessTime < timer)
            stats.endlessTime = timer;
        stats.totalKills += playerKills;

        // Save the player statistics
        PlayerStatistics.Save(stats);
    }
}
