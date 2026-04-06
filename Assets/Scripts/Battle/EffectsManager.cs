using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager instance;

    [Header("Prefabs")]
    public GameObject floatingTextPrefab;
    public GameObject soundEffectPrefab;

    private void Awake()
    {
        instance = this;
    }

    public void CreateFloatingText(
        Vector3 position,
        string message,
        Color color,
        float size = 2f,
        float floatSpeed = 1f,
        float fadeDuration = 2f)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("No Floating Text Prefab assigned in TextEffectsManager!");
            return;
        }

        GameObject textObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        var floatingText = textObj.GetComponent<Floating3DText>();
        if (floatingText != null)
        {
            floatingText.Initialize(message, color, size, floatSpeed, fadeDuration);
        }
    }
    public void CreateSoundEffect(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        SoundEffect sfx = Instantiate(soundEffectPrefab, position, Quaternion.identity).GetComponent<SoundEffect>();

        AudioSource src = sfx.GetComponent<AudioSource>();
        if (src != null) src.volume = volume;

        sfx.Play(clip);
    }
}
