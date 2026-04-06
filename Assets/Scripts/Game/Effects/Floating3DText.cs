using UnityEngine;
using TMPro;

public class Floating3DText : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro textMesh;

    [Header("Animation Settings")]
    public float floatSpeed = 1f;
    public float fadeDuration = 1f;

    private Color startColor;
    private float lifetime = 0f;

    public void Initialize(string message, Color color, float size = 1f, float floatSpeed = 1f, float fadeDuration = 1f)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        textMesh.text = message;
        textMesh.color = color;
        textMesh.fontSize = size * 5f; // adjust scaling to your scene

        this.floatSpeed = floatSpeed;
        this.fadeDuration = fadeDuration;

        startColor = color;
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        lifetime += Time.deltaTime;
        float t = Mathf.Clamp01(lifetime / fadeDuration);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);

        if (t >= 1f)
            Destroy(gameObject);
    }
}
