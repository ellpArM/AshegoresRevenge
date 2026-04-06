using UnityEngine;
using UnityEngine.EventSystems;

public class AtlasClickHandler : MonoBehaviour, IPointerClickHandler
{
    public VoxelEditorUI editor;

    public void OnPointerClick(PointerEventData eventData)
    {
        RectTransform rect = GetComponent<RectTransform>();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        Vector2 uv = new Vector2(
            (localPoint.x + rect.rect.width / 2) / rect.rect.width,
            (localPoint.y + rect.rect.height / 2) / rect.rect.height
        );

        editor.OnAtlasClicked(uv);
    }
}
