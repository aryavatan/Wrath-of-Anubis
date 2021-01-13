using UnityEngine;

public class DeveloperCheats : MonoBehaviour
{
    public AudioClip cheaterAudio;
    
    GameUI ui;
    AudioSource audioSource;

    private void Awake()
    {
        // TODO: Check if cheats are enabled
        int enabled = PlayerPrefs.GetInt("DeveloperCheats", 0);
        if (enabled == 0)
        {
            this.enabled = false;
            return;
        }

        ui = FindObjectOfType<GameUI>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        //audioSource.volume = 0.7f;
        audioSource.clip = cheaterAudio;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            AddMoney();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            AddAllPerks();
        }
    }

    void AddMoney()
    {
        ui.AddMoney(1000);
        audioSource.Play();
    }

    void AddAllPerks()
    {
        Perk[] perks = FindObjectsOfType<Perk>();
        bool powerUpSoundPlayed = false;

        for (int i = 0; i < perks.Length; i++)
        {

            if (!perks[i].perkEnabled)
            {
                if (!powerUpSoundPlayed)
                    audioSource.Play();

                perks[i].EnablePerkEffect(!powerUpSoundPlayed);
                powerUpSoundPlayed = true;
            }
        }
    }
}
