using UnityEngine;

public class MainMenuSoundTrack : MonoBehaviour
{
    public AudioClip loop;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Invoke("BeginSecondLoop", 121);
    }

    void BeginSecondLoop()
    {
        audioSource.clip = loop;
        audioSource.loop = true;
        audioSource.Play();
    }
}
