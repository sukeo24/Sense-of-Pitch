using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MeasureSlot : MonoBehaviour
{
    [Header("スロット生成")]
    [SerializeField]
    private EarCopySlot slotPrefab;

    private HorizontalLayoutGroup layoutGroup;

    private void Awake()
    {
        layoutGroup =
            GetComponent<HorizontalLayoutGroup>();

        RefreshLayout();
    }

    public EarCopySlot[] GetSlots()
    {
        return GetComponentsInChildren<EarCopySlot>(
            false
        );
    }

    public void RemoveSlot(
    EarCopySlot slot)
    {
        if (slot == null)
        {
            return;
        }

        // 子が1個しかない場合は
        // Slot自体は残して中身だけ空にする
        if (transform.childCount <= 1)
        {
            slot.ClearSlot();
            RefreshLayout();
            return;
        }

        Destroy(slot.gameObject);

        StartCoroutine(
            RefreshLayoutNextFrame()
        );
    }

    private IEnumerator RefreshLayoutNextFrame()
    {
        yield return null;

        RefreshLayout();
    }

    public void RefreshLayout()
    {
        int childCount =
            transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child =
                transform.GetChild(i)
                    as RectTransform;

            if (child == null)
            {
                continue;
            }

            LayoutElement layoutElement =
                child.GetComponent<LayoutElement>();

            if (layoutElement == null)
            {
                layoutElement =
                    child.gameObject
                        .AddComponent<LayoutElement>();
            }

            layoutElement.flexibleWidth = 1f;
            layoutElement.preferredWidth = -1f;
        }

        LayoutRebuilder
            .ForceRebuildLayoutImmediate(
                transform as RectTransform
            );
    }

    public void AddSecondChord(
    EarCopyDragCard card)
    {
        if (card == null ||
            slotPrefab == null)
        {
            return;
        }

        if (transform.childCount >= 2)
        {
            return;
        }

        // 既存SlotからScene参照を受け取る
        EarCopySlot existingSlot =
            GetComponentInChildren<EarCopySlot>();

        EarCopySlot newSlot =
            Instantiate(
                slotPrefab,
                transform
            );

        if (existingSlot != null)
        {
            newSlot.InitializeReferences(
                existingSlot.ChordGenerator,
                existingSlot.SoundPlayer
            );
        }

        newSlot.SetChord(
            card.GetChordData(),
            card.CardLabel
        );

        RefreshLayout();
    }

}