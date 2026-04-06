using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void Play(AudioClip clip)
    {
        if (clip == null)
        {
            Destroy(gameObject);
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();

        Destroy(gameObject, clip.length);
    }
}
