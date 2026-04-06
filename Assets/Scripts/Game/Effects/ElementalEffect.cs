using UnityEngine;

public class ElementalEffect : MonoBehaviour
{
    public void EndEffect(float delay = 1)
    {
        foreach (var item in GetComponentsInChildren<ParticleSystem>())
        {
            item.Stop();
        }
        Destroy(gameObject, delay);
    }
}
