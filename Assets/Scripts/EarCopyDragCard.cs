using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EarCopyDragCard : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("カード情報")]
    [SerializeField]
    private string cardLabel = "I";

    public string CardLabel => cardLabel;

    private GameObject dragGhost;
    private RectTransform dragGhostRect;

    private Canvas rootCanvas;
    private RectTransform canvasRect;

    [SerializeField]
    private GameObject dragGhostPrefab;

    private ChordButton chordButton;

    private void Awake()
    {
        rootCanvas =
            GetComponentInParent<Canvas>();

        chordButton =
            GetComponent<ChordButton>();

        if (rootCanvas != null)
        {
            canvasRect =
                rootCanvas.transform
                    as RectTransform;
        }
    }

    public ChordData GetChordData()
    {
        if (chordButton == null)
        {
            return null;
        }

        return chordButton.GetChordData();
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (rootCanvas == null)
        {
            return;
        }

        // 元カードは残したまま、
        // ドラッグ用コピーだけ作る
        dragGhost =
    Instantiate(
        dragGhostPrefab,
        rootCanvas.transform
    );

        TMP_Text ghostText =
            dragGhost.GetComponentInChildren<TMP_Text>();

        if (ghostText != null)
        {
            ghostText.text = cardLabel;
        }

        dragGhost.name =
            $"{gameObject.name}_DragGhost";

        dragGhostRect =
            dragGhost.GetComponent<RectTransform>();

        dragGhostRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        dragGhostRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        dragGhostRect.pivot =
            new Vector2(0.5f, 0.5f);

        CanvasGroup canvasGroup =
            dragGhost.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                dragGhost.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        dragGhost.transform.SetAsLastSibling();

        UpdateGhostPosition(eventData);
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (dragGhost != null)
        {
            Destroy(dragGhost);
        }

        dragGhost = null;
        dragGhostRect = null;
    }

    private void UpdateGhostPosition(
    PointerEventData eventData)
    {
        if (dragGhostRect == null)
        {
            return;
        }

        dragGhostRect.position =
            eventData.position;
    }
}