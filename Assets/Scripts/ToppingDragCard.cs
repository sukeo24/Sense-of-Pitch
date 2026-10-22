using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToppingDragCard : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("トッピング情報")]
    [SerializeField]
    private string toppingLabel = "7";

    [Header("ドラッグ表示")]
    [SerializeField]
    private GameObject dragGhostPrefab;

    public string ToppingLabel =>
        toppingLabel;

    private GameObject dragGhost;
    private RectTransform dragGhostRect;

    private Canvas rootCanvas;

    private void Awake()
    {
        rootCanvas =
            GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (rootCanvas == null ||
            dragGhostPrefab == null)
        {
            return;
        }

        dragGhost =
            Instantiate(
                dragGhostPrefab,
                rootCanvas.transform
            );

        dragGhostRect =
            dragGhost.GetComponent<RectTransform>();

        TMP_Text ghostText =
            dragGhost
                .GetComponentInChildren<TMP_Text>();

        if (ghostText != null)
        {
            ghostText.text =
                toppingLabel;
        }

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

        UpdateGhostPosition(
            eventData
        );
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        UpdateGhostPosition(
            eventData
        );
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