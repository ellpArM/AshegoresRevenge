using UnityEngine;

public class BeamController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject startEffect;
    public GameObject hitEffect;

    void Start()
    {
        
    }
    public void PositionBeam(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end );

        if (startEffect != null)
            startEffect.transform.position = start;
        if (hitEffect != null)
            hitEffect.transform.position = end;

        if (startEffect != null)
            startEffect.transform.LookAt(end);
        if (hitEffect != null)
            hitEffect.transform.LookAt(start);
    }
    public void EndEffect()
    {
        ParticleSystem[] particleObjects = GetComponentsInChildren<ParticleSystem>();
        foreach (var item in particleObjects)
        {
            item.Stop();
            item.transform.parent = null;
            Destroy(item.gameObject, 1);
        }
    }
    public void SetArrowPoperties(Color color, float len)
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr == null || lr.positionCount < 2)
            return;

        // --- IMPORTANT: use instance material ---
        Material mat = lr.material;

        if (mat.HasProperty("_color"))
            mat.SetColor("_color", color);

        if (mat.HasProperty("_tiling"))
        {
            mat.SetVector("_tiling", new Vector2(len, 1));
        }
    }
}
