using UnityEngine;

public class AmbientMusic : MonoBehaviour
{
    [Header("Play Even If Game Paused")]
    public AudioSource[] audioSources;

    // Start is called before the first frame update
    void Start()
    {
        foreach (AudioSource source in audioSources)
        {
            source.ignoreListenerPause = true;
        }
    }
}
