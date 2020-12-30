using UnityEngine;

public class MainMenuSoundTrack : MonoBehaviour
{
    public AudioClip loop;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = loop;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
