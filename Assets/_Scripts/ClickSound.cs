using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickSound : MonoBehaviour
{
    public AudioClip sound;

    private Button button { get { return GetComponent<Button>(); } }
    private AudioSource source { get { return GetComponent<AudioSource>(); } }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<AudioSource>();
        source.clip = sound;
        source.playOnAwake = false;
        source.volume = 0.5f;

        button.onClick.AddListener(() => PlaySound());
    }

    void PlaySound()
    {
        source.PlayOneShot(sound);
    }
}
