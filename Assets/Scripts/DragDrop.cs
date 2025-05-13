using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public int slotRow;
    public int slotCol;

    private Image slotImage;

    private void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        Piece piece = eventData.pointerDrag.GetComponent<Piece>();

        if (piece != null)
        {
            if (transform.childCount > 0)
            {
                Debug.Log("Slot already occupied!");
                return;
            }

            RectTransform pieceRect = piece.GetComponent<RectTransform>();
            RectTransform slotRect = GetComponent<RectTransform>();

            piece.transform.SetParent(transform);

            // DEBUG: Show placement info
            Debug.Log($"Piece ({piece.correctRow},{piece.correctCol}) placed into Slot ({slotRow},{slotCol})");

            // Center piece in slot
            pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
            pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
            pieceRect.pivot = new Vector2(0.5f, 0.5f);
            pieceRect.anchoredPosition = Vector2.zero;
            pieceRect.sizeDelta = slotRect.sizeDelta;

            if (piece.correctRow == slotRow && piece.correctCol == slotCol)
            {
                Debug.Log("Correct placement!");
                StartCoroutine(Pulse(pieceRect, Color.green));
                piece.GetComponent<CanvasGroup>().blocksRaycasts = false;
                piece.enabled = false;
            }
            else
            {
                Debug.Log("Wrong placement!");
                StartCoroutine(Pulse(pieceRect, Color.red));
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null) gm.AddPenalty();
            }
        }
    }



    private IEnumerator Pulse(RectTransform pieceRect, Color flashColor)
    {
        Color originalColor = slotImage.color;
        Vector3 originalScale = pieceRect.localScale;

        // Change slot color
        slotImage.color = flashColor;

        // Shrink piece
        pieceRect.localScale = originalScale * 0.8f;

        // Wait
        yield return new WaitForSeconds(0.2f);

        // Restore piece size and slot color
        pieceRect.localScale = originalScale;
        slotImage.color = originalColor;
    }
}
