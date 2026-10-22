using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class EarCopySlot : MonoBehaviour,
    IDropHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField]
    private TMP_Text chordText;

    [Header("転回形リール")]
    [SerializeField]
    private TMP_Text previousText;

    [SerializeField]
    private TMP_Text currentText;

    [SerializeField]
    private TMP_Text nextText;

    [SerializeField]
    private TMP_Text farPreviousText;

    [SerializeField]
    private TMP_Text farNextText;


    [SerializeField]
    private float reelItemHeight = 60f;

    [SerializeField]
    private float reelSideScale = 0.65f;

    [SerializeField]
    [Range(0f, 1f)]
    private float reelSideAlpha = 0.3f;

    private float farPreviousBaseY;
    private float previousBaseY;
    private float currentBaseY;
    private float nextBaseY;
    private float farNextBaseY;

    [SerializeField]
    private float reelAnimationDuration = 0.15f;

    private Coroutine reelAnimationCoroutine;

    [Header("リール起動アニメーション")]
    [SerializeField]
    private float reelEnterDuration = 0.14f;

    [SerializeField]
    private float reelEnterSpread = 8f;

    [SerializeField]
    private float reelEnterPunchScale = 1.08f;

    private Coroutine reelModeAnimationCoroutine;
    private float reelModeSpread = 0f;

    [Header("リール時スロット")]
    [SerializeField]
    private float reelSlotScale = 1.03f;

    [SerializeField]
    private Color reelModeColor;

    private Vector3 slotBaseScale;

    [Header("コード試聴")]
    [SerializeField]
    private ChordGenerator chordGenerator;

    [SerializeField]
    private SoundPlayer soundPlayer;

    private int[] previewNotes;
    private int previewRootNoteNumber = -1;

    private ChordData chordData;

    public bool HasChord =>
        chordData != null;

    public ChordData ChordData =>
        chordData;

    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private Color emptyColor;

    [SerializeField]
    private Color filledColor;

    [SerializeField]
    private Color playingColor = Color.cyan;

    [SerializeField]
    private GameObject dragGhostPrefab;

    private GameObject dragGhost;

    [Header("ドラッグ感度")]
    [SerializeField]
    private float dragThreshold = 20f;

    private Vector2 dragStartPosition;
    private bool dragStarted;

    private bool reelDragging;

    [Header("長押しリール")]
    [SerializeField]
    private float longPressDuration = 0.4f;

    private Coroutine longPressCoroutine;

    private int[] reelHighlightNotes;
    private int reelHighlightRootNoteNumber = -1;

    private int reelPreviewOffset =
        int.MinValue;

    private bool pointerDown;
    private Vector2 pointerDownPosition;

    private bool hasChord;
    private bool isPlaying;

    private bool droppedToSlot;

    public string ChordLabel =>
    chordText != null
        ? chordText.text
        : "";

    public void InitializeReferences(
        ChordGenerator generator,
        SoundPlayer player)
    {
        chordGenerator = generator;
        soundPlayer = player;
    }

    public ChordGenerator ChordGenerator =>
        chordGenerator;

    public SoundPlayer SoundPlayer =>
        soundPlayer;

    private void Start()
    {
        if (previousText != null)
        {
            previousBaseY =
                previousText.rectTransform
                    .anchoredPosition.y;
        }

        if (currentText != null)
        {
            currentBaseY =
                currentText.rectTransform
                    .anchoredPosition.y;
        }

        if (nextText != null)
        {
            nextBaseY =
                nextText.rectTransform
                    .anchoredPosition.y;
        }

        if (farPreviousText != null)
        {
            farPreviousBaseY =
                farPreviousText.rectTransform
                    .anchoredPosition.y;
        }

        if (farNextText != null)
        {
            farNextBaseY =
                farNextText.rectTransform
                    .anchoredPosition.y;
        }

        slotBaseScale =
    transform.localScale;

        UpdateReelAppearance(0f);
    }


    private void UpdateReelAppearance(
    float offset)
    {
        UpdateReelItem(
            farPreviousText,
            farPreviousBaseY +
            offset +
            reelModeSpread * 2f
        );

        UpdateReelItem(
            previousText,
            previousBaseY +
            offset +
            reelModeSpread
        );

        UpdateReelItem(
            currentText,
            currentBaseY + offset
        );

        UpdateReelItem(
            nextText,
            nextBaseY +
            offset -
            reelModeSpread
        );

        UpdateReelItem(
            farNextText,
            farNextBaseY +
            offset -
            reelModeSpread * 2f
        );
    }

    private void PlayReelEnterAnimation()
    {
        if (reelModeAnimationCoroutine != null)
        {
            StopCoroutine(
                reelModeAnimationCoroutine
            );
        }

        reelModeAnimationCoroutine =
            StartCoroutine(
                ReelEnterRoutine()
            );
    }

    private IEnumerator ReelEnterRoutine()
    {
        float elapsed = 0f;

        while (elapsed < reelEnterDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / reelEnterDuration
                );

            // なめらかに開く
            float easedT =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            reelModeSpread =
                Mathf.Lerp(
                    0f,
                    reelEnterSpread,
                    easedT
                );

            UpdateReelAppearance(0f);

            // 中央だけ
            // 途中で少し膨らんで戻る
            float punch =
                Mathf.Sin(t * Mathf.PI);

            float scale =
                Mathf.Lerp(
                    1f,
                    reelEnterPunchScale,
                    punch
                );

            if (currentText != null)
            {
                currentText.rectTransform
                    .localScale =
                    Vector3.one * scale;
            }

            transform.localScale =
                Vector3.Lerp(
                    slotBaseScale,
                    slotBaseScale * reelSlotScale,
                    easedT
                );

            if (backgroundImage != null)
            {
                backgroundImage.color =
                    Color.Lerp(
                        filledColor,
                        reelModeColor,
                        easedT
                    );
            }

            yield return null;
        }

        reelModeSpread =
            reelEnterSpread;

        UpdateReelAppearance(0f);

        reelModeAnimationCoroutine = null;

        transform.localScale =
            slotBaseScale * reelSlotScale;

        if (backgroundImage != null)
        {
            backgroundImage.color =
                reelModeColor;
        }
    }

    private void UpdateReelItem(
        TMP_Text text,
        float y)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rect =
            text.rectTransform;

        Vector2 position =
            rect.anchoredPosition;

        position.y = y;

        rect.anchoredPosition =
            position;

        float normalizedDistance =
            Mathf.Clamp01(
                Mathf.Abs(y) /
                reelItemHeight
            );

        float scale =
            Mathf.Lerp(
                1f,
                reelSideScale,
                normalizedDistance
            );

        rect.localScale =
            Vector3.one * scale;

        Color color =
            text.color;

        color.a =
            Mathf.Lerp(
                1f,
                reelSideAlpha,
                normalizedDistance
            );

        text.color = color;
    }

    private void ShowReelPreviewHighlight(
    int offset)
    {
        if (soundPlayer == null ||
            chordGenerator == null ||
            chordData == null)
        {
            return;
        }

        // 同じ候補なら更新不要
        if (reelPreviewOffset == offset)
        {
            return;
        }

        HideReelPreviewHighlight();

        int noteCount =
            chordGenerator.GenerateChord(
                chordData.degree,
                chordData.chordType,
                chordData.inversion,
                chordData.modifiers,
                out _
            ).Length;

        if (noteCount <= 0)
        {
            return;
        }

        int current =
            (int)chordData.inversion;

        int preview =
            (current + offset + noteCount)
            % noteCount;

        reelHighlightNotes =
            chordGenerator.GenerateChord(
                chordData.degree,
                chordData.chordType,
                (Inversion)preview,
                chordData.modifiers,
                out reelHighlightRootNoteNumber
            );

        soundPlayer.ShowChordHighlight(
            reelHighlightNotes,
            reelHighlightRootNoteNumber
        );

        reelPreviewOffset = offset;
    }

    private void HideReelPreviewHighlight()
    {
        if (soundPlayer != null &&
            reelHighlightNotes != null &&
            reelHighlightNotes.Length > 0)
        {
            soundPlayer.HideChordHighlight(
                reelHighlightNotes,
                reelHighlightRootNoteNumber
            );
        }

        reelHighlightNotes = null;
        reelHighlightRootNoteNumber = -1;

        reelPreviewOffset =
            int.MinValue;
    }

    private void CycleInversion(
    int direction)
    {
        int noteCount =
            chordGenerator.GenerateChord(
                chordData.degree,
                chordData.chordType,
                chordData.inversion,
                chordData.modifiers,
                out _
            ).Length;

        if (noteCount <= 0)
        {
            return;
        }

        int current =
            (int)chordData.inversion;

        int next =
            (current + direction + noteCount)
            % noteCount;

        chordData.inversion =
            (Inversion)next;

        RefreshChordLabel();
    }

    private void AnimateReelFromCurrentPosition(
    int direction)
    {
        if (reelAnimationCoroutine != null)
        {
            return;
        }

        reelAnimationCoroutine =
            StartCoroutine(
                AnimateReelFromCurrentRoutine(
                    direction
                )
            );
    }

    private IEnumerator AnimateReelFromCurrentRoutine(
    int direction)
    {
        float startOffset =
            Mathf.Clamp(
                (dragStartPosition.y == 0f)
                    ? 0f
                    : currentText.rectTransform
                        .anchoredPosition.y -
                      currentBaseY,
                -reelItemHeight,
                reelItemHeight
            );

        float targetOffset =
            direction > 0
                ? reelItemHeight
                : -reelItemHeight;

        float elapsed = 0f;

        while (elapsed <
               reelAnimationDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    reelAnimationDuration
                );

            float easedT =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            float offset =
                Mathf.Lerp(
                    startOffset,
                    targetOffset,
                    easedT
                );

            UpdateReelAppearance(offset);

            yield return null;
        }

        CycleInversion(direction);

        UpdateReelAppearance(0f);

        reelAnimationCoroutine = null;
    }

    private void AnimateReelBack()
    {
        if (reelAnimationCoroutine != null)
        {
            return;
        }

        reelAnimationCoroutine =
            StartCoroutine(
                AnimateReelBackRoutine()
            );
    }

    private IEnumerator AnimateReelBackRoutine()
    {
        float startOffset =
            currentText.rectTransform
                .anchoredPosition.y -
            currentBaseY;

        float elapsed = 0f;

        while (elapsed <
               reelAnimationDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    reelAnimationDuration
                );

            float easedT =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            float offset =
                Mathf.Lerp(
                    startOffset,
                    0f,
                    easedT
                );

            UpdateReelAppearance(offset);

            yield return null;
        }

        UpdateReelAppearance(0f);

        reelAnimationCoroutine = null;
    }

    private IEnumerator LongPressRoutine()
    {
        yield return
            new WaitForSecondsRealtime(
                longPressDuration
            );

        if (!pointerDown ||
            dragStarted ||
            !HasChord)
        {
            longPressCoroutine = null;
            yield break;
        }

        reelDragging = true;

        PlayReelEnterAnimation();

        // 長押し成立地点を
        // リール操作の基準位置にする
        dragStartPosition =
            pointerDownPosition;

        // 押した瞬間に出たハイライトだけ消す。
        // 音は止めない。
        if (previewNotes != null &&
            previewNotes.Length > 0 &&
            soundPlayer != null)
        {
            soundPlayer.HideChordHighlight(
                previewNotes,
                previewRootNoteNumber
            );
        }

        ShowReelPreviewHighlight(0);

        longPressCoroutine = null;
    }

    public void OnPointerDown(
    PointerEventData eventData)
    {
        if (!HasChord ||
            chordGenerator == null ||
            soundPlayer == null)
        {
            return;
        }

        previewNotes =
    chordGenerator.GenerateChord(
        chordData.degree,
        chordData.chordType,
        chordData.inversion,
        chordData.modifiers,
        out previewRootNoteNumber
    );

        soundPlayer.PlayChord(
            previewNotes,
            previewRootNoteNumber
        );

        pointerDown = true;
        pointerDownPosition =
            eventData.position;

        if (longPressCoroutine != null)
        {
            StopCoroutine(
                longPressCoroutine
            );
        }

        longPressCoroutine =
            StartCoroutine(
                LongPressRoutine()
            );
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {

        HideReelPreviewHighlight();

        pointerDown = false;

        if (reelModeAnimationCoroutine != null)
        {
            StopCoroutine(
                reelModeAnimationCoroutine
            );

            reelModeAnimationCoroutine = null;
        }

        reelModeSpread = 0f;

        transform.localScale =
            slotBaseScale;

        UpdateReelAppearance(0f);
        UpdateVisual();

        if (longPressCoroutine != null)
        {
            StopCoroutine(
                longPressCoroutine
            );

            longPressCoroutine = null;
        }

        if (previewNotes == null ||
            previewNotes.Length == 0 ||
            soundPlayer == null)
        {
            return;
        }

        soundPlayer.StopChord(
            previewNotes,
            previewRootNoteNumber
        );

        previewNotes = null;
        previewRootNoteNumber = -1;
    }

    public void SetPlaying(bool playing)
    {
        isPlaying = playing;
        UpdateVisual();
    }

    public void OnDrop(
    PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        // ⓪ トッピングカードから来た場合
        ToppingDragCard topping =
            eventData.pointerDrag
                .GetComponent<ToppingDragCard>();

        if (topping != null)
        {
            ApplyTopping(topping.ToppingLabel);
            return;
        }

        // ① 上のダイアトニックカードから来た場合
        EarCopyDragCard card =
            eventData.pointerDrag
                .GetComponent<EarCopyDragCard>();

        if (card != null)
        {
            // 空なら今まで通り
            if (!HasChord)
            {
                chordData =
                    card.GetChordData();

                hasChord = true;

                if (chordText != null)
                {
                    chordText.text =
                        card.CardLabel;
                }

                RefreshReelLabels();
                UpdateVisual();

                return;
            }

            // すでにコードがあるなら
            // 親MeasureSlotへ2個目追加を依頼
            MeasureSlot measureSlot =
                GetComponentInParent<MeasureSlot>();

            if (measureSlot != null)
            {
                measureSlot.AddSecondChord(
                    card
                );
            }

            return;
        }

        // ② 別スロットから来た場合
        EarCopySlot sourceSlot =
            eventData.pointerDrag
                .GetComponent<EarCopySlot>();

        if (sourceSlot == null ||
            !sourceSlot.HasChord)
        {
            return;
        }

        // 元のスロットに戻した
        if (sourceSlot == this)
        {
            sourceSlot.MarkDroppedToSlot();
            return;
        }

        sourceSlot.MarkDroppedToSlot();

        // ターゲットに既にコードがあるなら交換
        if (HasChord)
        {
            ChordData targetChordData =
                chordData;

            string targetLabel =
                ChordLabel;

            chordData =
                sourceSlot.ChordData;

            if (chordText != null)
            {
                chordText.text =
                    sourceSlot.ChordLabel;
            }

            RefreshReelLabels();

            sourceSlot.SetChord(
                targetChordData,
                targetLabel
            );

            hasChord = true;
            UpdateVisual();

            return;
        }

        // 空スロットなら普通に移動
        chordData =
            sourceSlot.ChordData;

        hasChord = true;

        if (chordText != null)
        {
            chordText.text =
                sourceSlot.ChordLabel;
        }

        RefreshReelLabels();
        UpdateVisual();

        sourceSlot.ClearSlot();
    }

    public void MarkDroppedToSlot()
    {
        droppedToSlot = true;
    }

    public void ClearSlot()
    {
        chordData = null;
        hasChord = false;
        isPlaying = false;

        if (chordText != null)
        {
            chordText.text = "";
        }

        RefreshReelLabels();
        UpdateVisual();
    }

    private void RemoveFromMeasure()
    {
        MeasureSlot measure =
            GetComponentInParent<MeasureSlot>();

        if (measure == null)
        {
            ClearSlot();
            return;
        }

        measure.RemoveSlot(this);
    }

    public void SetChord(
    ChordData newChordData,
    string newLabel)
    {
        chordData =
            newChordData;

        hasChord =
            chordData != null;

        if (chordText != null)
        {
            chordText.text =
                newLabel;
        }

        RefreshReelLabels();
        UpdateVisual();
    }

    public void OnBeginDrag(
    PointerEventData eventData)
    {
        droppedToSlot = false;
        dragStarted = false;
        

        dragStartPosition =
            eventData.position;

    }

    public void OnDrag(
    PointerEventData eventData)
    {
        if (!HasChord ||
            dragGhostPrefab == null)
        {
            return;
        }

        Vector2 delta =
            eventData.position -
            dragStartPosition;

        // 長押し成立済みなら
        // カードドラッグではなくリール操作
        if (reelDragging)
        {
            float y = delta.y;

            // 上へ1段到達
            if (y >= reelItemHeight)
            {
                CycleInversion(1);

                // 新しい現在位置を基準にする
                dragStartPosition =
                    eventData.position;

                UpdateReelAppearance(0f);

                // 新しい現在の転回形を
                // ハイライト
                ShowReelPreviewHighlight(0);

                return;
            }

            // 下へ1段到達
            if (y <= -reelItemHeight)
            {
                CycleInversion(-1);

                dragStartPosition =
                    eventData.position;

                UpdateReelAppearance(0f);

                ShowReelPreviewHighlight(0);

                return;
            }

            // まだ1段に届いていない間は
            // 普通に指へ追従
            UpdateReelAppearance(y);

            int previewOffset;

            if (y > 10f)
            {
                previewOffset = 1;
            }
            else if (y < -10f)
            {
                previewOffset = -1;
            }
            else
            {
                previewOffset = 0;
            }

            ShowReelPreviewHighlight(
                previewOffset
            );

            return;
        }

        // すでにカードドラッグ中
        if (dragStarted)
        {
            if (dragGhost != null)
            {
                dragGhost.transform.position =
                    eventData.position;
            }

            return;
        }

        if (delta.magnitude <
            dragThreshold)
        {
            return;
        }

        // ここからカードドラッグ
        dragStarted = true;

        Canvas canvas =
            GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            dragStarted = false;
            return;
        }

        dragGhost =
            Instantiate(
                dragGhostPrefab,
                canvas.transform
            );

        CanvasGroup canvasGroup =
            dragGhost.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                dragGhost.AddComponent<CanvasGroup>();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        TMP_Text ghostText =
            dragGhost.GetComponentInChildren<TMP_Text>();

        if (ghostText != null &&
            chordText != null)
        {
            ghostText.text =
                chordText.text;
        }

        dragGhost.transform.position =
            eventData.position;
    }

    public void OnEndDrag(
    PointerEventData eventData)
    {
        Vector2 dragDelta =
            eventData.position -
            dragStartPosition;

        if (dragGhost != null)
        {
            Destroy(dragGhost);
            dragGhost = null;
        }

        // リールを触っていた
        if (reelDragging &&
            HasChord)
        {
            if (Mathf.Abs(dragDelta.y) >= 50f)
            {
                int direction =
                    dragDelta.y > 0f
                        ? 1
                        : -1;

                AnimateReelFromCurrentPosition(
                    direction
                );
            }
            else
            {
                AnimateReelBack();
            }

            reelDragging = false;
            droppedToSlot = false;
            dragStarted = false;

            return;
        }

        // 元のスロット上で離したなら
        // 「元に戻した」扱いで保持
        bool returnedToOriginalSlot =
            RectTransformUtility
                .RectangleContainsScreenPoint(
                    transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera
                );

        // 別スロットにも落としておらず、
        // 元スロットにも戻していない
        // ＝外へ捨てた
        if (dragStarted &&
            !droppedToSlot &&
            !returnedToOriginalSlot &&
            HasChord)
        {
            RemoveFromMeasure();
        }

        droppedToSlot = false;
        dragStarted = false;
    }

    private void ApplyTopping(string topping)
    {
        if (!HasChord)
        {
            return;
        }

        if (topping == "7")
        {
            bool alreadyHas =
                (chordData.modifiers &
                 ChordModifier.Seventh) != 0;

            if (alreadyHas)
            {
                chordData.modifiers &=
                    ~ChordModifier.Seventh;
            }
            else
            {
                chordData.modifiers |=
                    ChordModifier.Seventh;

                chordData.modifiers &=
                    ~ChordModifier.MajorSeventh;
            }
        }
        else if (topping == "M7")
        {
            bool alreadyHas =
                (chordData.modifiers &
                 ChordModifier.MajorSeventh) != 0;

            if (alreadyHas)
            {
                chordData.modifiers &=
                    ~ChordModifier.MajorSeventh;
            }
            else
            {
                chordData.modifiers |=
                    ChordModifier.MajorSeventh;

                chordData.modifiers &=
                    ~ChordModifier.Seventh;
            }
        }
        else if (topping == "sus2")
        {
            bool alreadyHas =
                (chordData.modifiers &
                 ChordModifier.Sus2) != 0;

            if (alreadyHas)
            {
                chordData.modifiers &=
                    ~ChordModifier.Sus2;
            }
            else
            {
                chordData.modifiers |=
                    ChordModifier.Sus2;

                chordData.modifiers &=
                    ~ChordModifier.Sus4;
            }
        }
        else if (topping == "sus4")
        {
            bool alreadyHas =
                (chordData.modifiers &
                 ChordModifier.Sus4) != 0;

            if (alreadyHas)
            {
                chordData.modifiers &=
                    ~ChordModifier.Sus4;
            }
            else
            {
                chordData.modifiers |=
                    ChordModifier.Sus4;

                chordData.modifiers &=
                    ~ChordModifier.Sus2;
            }
        }
        else if (topping == "add9")
        {
            chordData.modifiers ^=
                ChordModifier.Add9;
        }

        RefreshChordLabel();
    }

    private void RefreshChordLabel()
    {
        if (chordText == null ||
            chordData == null)
        {
            return;
        }

        string degreeLabel =
            GetDegreeLabel(
                chordData.degree,
                chordData.chordType
            );

        string modifierLabel = "";

        if ((chordData.modifiers &
             ChordModifier.MajorSeventh) != 0)
        {
            modifierLabel += "M7";
        }
        else if ((chordData.modifiers &
                  ChordModifier.Seventh) != 0)
        {
            modifierLabel += "7";
        }

        if ((chordData.modifiers &
     ChordModifier.Sus2) != 0)
        {
            modifierLabel += "sus2";
        }
        else if ((chordData.modifiers &
                  ChordModifier.Sus4) != 0)
        {
            modifierLabel += "sus4";
        }

        if ((chordData.modifiers &
             ChordModifier.Add9) != 0)
        {
            modifierLabel += "add9";
        }

        string inversionLabel = "";

        int inversionIndex =
            (int)chordData.inversion;

        if (inversionIndex > 0)
        {
            inversionLabel =
                "-" + inversionIndex;
        }

        chordText.text =
            degreeLabel +
            modifierLabel +
            inversionLabel;

        RefreshReelLabels();
    }

    private void RefreshReelLabels()
    {
        if (chordData == null)
        {
            if (farPreviousText != null)
                farPreviousText.text = "";

            if (previousText != null)
                previousText.text = "";

            if (currentText != null)
                currentText.text = "";

            if (nextText != null)
                nextText.text = "";

            if (farNextText != null)
                farNextText.text = "";

            return;
        }

        int noteCount =
            chordGenerator.GenerateChord(
                chordData.degree,
                chordData.chordType,
                chordData.inversion,
                chordData.modifiers,
                out _
            ).Length;

        if (noteCount <= 0)
        {
            return;
        }

        int current =
            (int)chordData.inversion;

        int farPrevious =
            (current - 2 + noteCount * 2)
            % noteCount;

        int previous =
            (current - 1 + noteCount)
            % noteCount;

        int next =
            (current + 1)
            % noteCount;

        int farNext =
            (current + 2)
            % noteCount;

        if (farPreviousText != null)
        {
            farPreviousText.text =
                GetInversionLabel(
                    farPrevious
                );
        }

        if (previousText != null)
        {
            previousText.text =
                GetInversionLabel(
                    previous
                );
        }

        if (currentText != null)
        {
            currentText.text =
                GetInversionLabel(
                    current
                );
        }

        if (nextText != null)
        {
            nextText.text =
                GetInversionLabel(
                    next
                );
        }

        if (farNextText != null)
        {
            farNextText.text =
                GetInversionLabel(
                    farNext
                );
        }
    }

    private string GetInversionLabel(
    int inversionIndex)
    {
        string degreeLabel =
            GetDegreeLabel(
                chordData.degree,
                chordData.chordType
            );

        string modifierLabel = "";

        if ((chordData.modifiers &
             ChordModifier.MajorSeventh) != 0)
        {
            modifierLabel += "M7";
        }
        else if ((chordData.modifiers &
                  ChordModifier.Seventh) != 0)
        {
            modifierLabel += "7";
        }

        if ((chordData.modifiers &
             ChordModifier.Sus2) != 0)
        {
            modifierLabel += "sus2";
        }
        else if ((chordData.modifiers &
                  ChordModifier.Sus4) != 0)
        {
            modifierLabel += "sus4";
        }

        if ((chordData.modifiers &
             ChordModifier.Add9) != 0)
        {
            modifierLabel += "add9";
        }

        string inversionLabel =
            inversionIndex == 0
                ? ""
                : "-" + inversionIndex;

        return
            degreeLabel +
            modifierLabel +
            inversionLabel;
    }

    private string GetDegreeLabel(
    int degree,
    ChordType chordType)
    {
        string[] majorDegrees =
        {
        "",
        "I",
        "II",
        "III",
        "IV",
        "V",
        "VI",
        "VII"
    };

        if (degree < 1 ||
            degree > 7)
        {
            return degree.ToString();
        }

        string label =
            majorDegrees[degree];

        switch (chordType)
        {
            case ChordType.Minor:
                return label.ToLower();

            case ChordType.Diminished:
                return label.ToLower() + "°";

            default:
                return label;
        }
    }

    private void UpdateVisual()
    {
        if (backgroundImage == null)
        {
            return;
        }

        if (isPlaying)
        {
            backgroundImage.color =
                playingColor;
        }
        else if (hasChord)
        {
            backgroundImage.color =
                filledColor;
        }
        else
        {
            backgroundImage.color =
                emptyColor;
        }
    }
}