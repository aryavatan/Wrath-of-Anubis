using System.Collections;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public int cost = 2500;
    public AudioClip insufficientFundsAudio;

    GameUI ui;
    AudioSource audioSource;
    Animator anim;

    bool isOpen = false;
    bool playerInRange = false;
    float insufficientFundsAudioVolume = 0.4f;

    private void Awake()
    {
        ui = FindObjectOfType<GameUI>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Player in range and door is not already open
        if (playerInRange && !isOpen && Input.GetKeyDown(KeyCode.E))
        {
            OpenDoor();
        }
    }

    /// <summary>
    /// Returns true if the gate has been opened by the player, false otherwise
    /// </summary>
    public bool IsOpen()
    {
        return isOpen;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isOpen && other.CompareTag("Player"))
        {
            playerInRange = true;
            ui.SetGateTooltip(true, cost);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isOpen && other.CompareTag("Player"))
        {
            playerInRange = false;
            ui.SetGateTooltip(false);
        }
    }

    private void OpenDoor()
    {
        // First check if player can afford to open the door
        if (ui.SubtractMoney(cost))
        {
            // Play money spent audio
            audioSource.Play();

            isOpen = true;
            playerInRange = false;

            // Turn off tool tip
            ui.SetGateTooltip(false);

            // Trigger the gate opening animation
            anim.SetTrigger("Open");

            // Turn off the trigger box collider
            GetComponent<BoxCollider>().enabled = false;

            // Clean up other unneeded components
            StartCoroutine(CleanUp());
        }
        else
        {
            audioSource.PlayOneShot(insufficientFundsAudio, insufficientFundsAudioVolume);
        }
    }

    IEnumerator CleanUp()
    {
        yield return new WaitForSeconds(2f);
        
        audioSource.enabled = false;
        anim.enabled = false;
    }

}
