using UnityEngine;

public class HeavyGunSlowness : MonoBehaviour
{
    public float movementMultiplier = 0.75f;
    PlayerMovement player;

    private void Awake()
    {
        player = FindObjectOfType<PlayerMovement>();
    }

    private void OnEnable()
    {
        player.movementMultiplier = movementMultiplier;
    }

    private void OnDisable()
    {
        player.movementMultiplier = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.movementMultiplier != 0.4f)
        {
            player.movementMultiplier = movementMultiplier;
        }
    }
}
