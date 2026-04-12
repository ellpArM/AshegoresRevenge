using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float time;
    void Start()
    {
        Destroy(gameObject, time);
    }
}
