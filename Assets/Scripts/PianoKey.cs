using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Image))]
public class PianoKey : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IInitializePotentialDragHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("音の管理")]
    [SerializeField]
    private SoundPlayer soundPlayer;

    [Header("キー管理")]
    [SerializeField]
    private KeyManager keyManager;

    [Header("音高番号（C1=24〜C6=84）")]
    [SerializeField]
    private int noteNumber = 60;

    public int NoteNumber => noteNumber;

    private int soundingNoteNumber = -1;

    public void SetNoteNumber(int newNoteNumber)
    {
        noteNumber = newNoteNumber;
    }

    [Header("通常色")]
    [SerializeField]
    private Color normalColor = Color.white;

    [Header("カーソルを置いた時")]
    [SerializeField]
    private Color highlightedColor =
        new Color(0.8f, 0.8f, 0.8f, 1f);

    [Header("鍵盤を直接押した時")]
    [SerializeField]
    private Color pressedColor =
        new Color(1f, 0.75f, 0.2f, 1f);

    [Header("コードの構成音")]
    [SerializeField]
    private Color chordColor =
        new Color(1f, 0.75f, 0.2f, 1f);

    [Header("コードのルート音")]
    [SerializeField]
    private Color rootColor =
        new Color(0.2f, 0.7f, 1f, 1f);

    [Header("黒鍵用Sprite")]
    [SerializeField]
    private bool useBlackKeySprites = false;

    [SerializeField]
    private Sprite blackNormalSprite;

    [SerializeField]
    private Sprite blackChordSprite;

    [SerializeField]
    private Sprite blackRootSprite;

    [Header("鍵盤スクロール")]
    [SerializeField]
    private ScrollRect scrollRect;

    private bool isDraggingPointer;

    private PianoKey currentGlissandoKey;

    private readonly System.Collections.Generic.List<RaycastResult>
        raycastResults =
            new System.Collections.Generic.List<RaycastResult>();


    private Image keyImage;

    private bool isPointerInside;
    private bool isDirectlyPressed;

    // 複数のコードから同時に指定されても壊れないように数で管理
    private int chordHighlightCount;
    private int rootHighlightCount;

    private void Awake()
    {
        if (soundPlayer == null)
        {
            soundPlayer =
                FindAnyObjectByType<SoundPlayer>();
        }

        if (keyManager == null)
        {
            keyManager =
                FindAnyObjectByType<KeyManager>();
        }

        keyImage = GetComponent<Image>();

        if (keyImage != null)
        {
            keyImage.raycastTarget = true;
            keyImage.color = normalColor;
        }

        if (scrollRect == null)
        {
            scrollRect =
                GetComponentInParent<ScrollRect>();
        }
    }

    private void PressDirectly()
    {
        if (isDirectlyPressed)
        {
            return;
        }

        isDirectlyPressed = true;

        int transposeSemitones =
            keyManager != null &&
            keyManager.TransposeKeyboard
                ? keyManager.TransposeSemitones
                : 0;

        soundingNoteNumber =
            noteNumber + transposeSemitones;

        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(
                soundingNoteNumber
            );
        }

        UpdateColor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDraggingPointer = false;

        if (IsScrollMode())
        {
            currentGlissandoKey = null;
            return;
        }

        PressDirectly();

        // ベース音スロットが選択されている場合だけ、試聴した音高を記録。
        // グリッサンドによる移動時は記録しない。
        BassNoteSlot.TryAssignSelectedNote(soundingNoteNumber);

        currentGlissandoKey = this;
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {
        if (currentGlissandoKey != null)
        {
            currentGlissandoKey
                .ReleaseDirectPress();

            currentGlissandoKey = null;
        }
        else
        {
            ReleaseDirectPress();
        }

        isDraggingPointer = false;
    }
    public void OnPointerExit(
        PointerEventData eventData)
    {
        isPointerInside = false;

        if (!isDraggingPointer)
        {
            ReleaseDirectPress();
        }

        UpdateColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        UpdateColor();
    }

    private void ReleaseDirectPress()
    {
        if (!isDirectlyPressed)
        {
            return;
        }

        isDirectlyPressed = false;

        if (soundPlayer != null &&
            soundingNoteNumber >= 0)
        {
            soundPlayer.StopSound(
                soundingNoteNumber
            );
        }

        soundingNoteNumber = -1;

        UpdateColor();
    }

    // SoundPlayerから呼ばれる
    public void AddChordHighlight(bool isroot)
    {
        chordHighlightCount++;

        if (isroot)
        {
            rootHighlightCount++;
        }

        UpdateColor();
    }

    // SoundPlayerから呼ばれる
    public void RemoveChordHighlight(bool isroot)
    {
        chordHighlightCount =
            Mathf.Max(0, chordHighlightCount - 1);

        if (isroot)
        {
            rootHighlightCount =
                Mathf.Max(0, rootHighlightCount - 1);
        }

        UpdateColor();
    }

    private void UpdateColor()
    {
        if (keyImage == null)
        {
            return;
        }

        // 黒鍵だけSprite差し替え方式
        if (useBlackKeySprites)
        {
            if (rootHighlightCount > 0)
            {
                keyImage.sprite = blackRootSprite;
                keyImage.color = Color.white;
            }
            else if (chordHighlightCount > 0)
            {
                keyImage.sprite = blackChordSprite;
                keyImage.color = Color.white;
            }
            else
            {
                keyImage.sprite = blackNormalSprite;

                if (isDirectlyPressed)
                {
                    keyImage.color = pressedColor;
                }
                else if (isPointerInside)
                {
                    keyImage.color = highlightedColor;
                }
                else
                {
                    keyImage.color = normalColor;
                }
            }

            return;
        }

        // 白鍵は今まで通りTint
        if (isDirectlyPressed)
        {
            keyImage.color = pressedColor;
        }
        else if (rootHighlightCount > 0)
        {
            keyImage.color = rootColor;
        }
        else if (chordHighlightCount > 0)
        {
            keyImage.color = chordColor;
        }
        else if (isPointerInside)
        {
            keyImage.color = highlightedColor;
        }
        else
        {
            keyImage.color = normalColor;
        }
    }

    public void OnInitializePotentialDrag(
        PointerEventData eventData)
    {
        if (!IsScrollMode())
        {
            return;
        }

        if (scrollRect != null)
        {
            scrollRect
                .OnInitializePotentialDrag(
                    eventData
                );
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDraggingPointer = true;

        if (IsScrollMode())
        {
            if (scrollRect != null)
            {
                scrollRect.OnBeginDrag(eventData);
            }

            return;
        }

        currentGlissandoKey = this;
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (IsScrollMode())
        {
            if (scrollRect != null)
            {
                scrollRect.OnDrag(
                    eventData
                );
            }

            return;
        }

        UpdateGlissando(
            eventData
        );
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (IsScrollMode())
        {
            if (scrollRect != null)
            {
                scrollRect.OnEndDrag(
                    eventData
                );
            }
        }
        else
        {
            if (currentGlissandoKey != null)
            {
                currentGlissandoKey
                    .ReleaseDirectPress();

                currentGlissandoKey = null;
            }
        }

        isDraggingPointer = false;
    }

    private void UpdateGlissando(
        PointerEventData eventData)
    {
        if (EventSystem.current == null)
        {
            return;
        }

        raycastResults.Clear();

        EventSystem.current.RaycastAll(
            eventData,
            raycastResults
        );

        PianoKey targetKey = null;

        foreach (RaycastResult result
                in raycastResults)
        {
            targetKey =
                result.gameObject
                    .GetComponentInParent<PianoKey>();

            if (targetKey != null)
            {
                break;
            }
        }

        if (targetKey == null ||
            targetKey == currentGlissandoKey)
        {
            return;
        }

        if (currentGlissandoKey != null)
        {
            currentGlissandoKey
                .ReleaseDirectPress();
        }

        targetKey.PressDirectly();

        currentGlissandoKey =
            targetKey;
    }

    private bool IsScrollMode()
    {
        return
            KeyboardTouchModeManager.Instance != null &&
            KeyboardTouchModeManager.Instance.CurrentMode ==
            KeyboardTouchMode.Scroll;
    }
    
}