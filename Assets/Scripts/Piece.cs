using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Piece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int correctRow;
    public int correctCol;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // If still under original parent (meaning NOT dropped into a slot), reset
        if (transform.parent == originalParent)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}
