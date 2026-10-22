using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ChordButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("音の管理")]
    [SerializeField]
    private SoundPlayer soundPlayer;

    [Header("コード生成")]
    [SerializeField]
    private ChordGenerator chordGenerator;

    [Header("度数")]
    [Tooltip("I=1, ii=2, iii=3, IV=4, V=5, vi=6, vii°=7")]
    [Range(1, 7)]
    [SerializeField]
    private int degree = 1;

    [Header("コードタイプ")]
    [SerializeField]
    private ChordType chordType = ChordType.Major;

    [Header("転回形")]
    [SerializeField]
    private Inversion inversion = Inversion.Root;

    [Header("通常色")]
    [SerializeField]
    private Color normalColor = Color.white;

    [Header("カーソルを置いた時")]
    [SerializeField]
    private Color highlightedColor =
        new Color(0.85f, 0.85f, 0.85f, 1f);

    [Header("押している時")]
    [SerializeField]
    private Color pressedColor =
        new Color(1f, 0.75f, 0.2f, 1f);

    [Header("自動再生中")]
    [SerializeField]
    private Color playingColor =
        new Color(0.3f, 0.9f, 0.4f, 1f);

    [SerializeField]
    private float playingScale = 1.08f;

    [SerializeField]
    private float popDuration = 0.08f;

    private int[] soundingNotes;
    private int soundingRootNoteNumber = -1;

    private Image buttonImage;
    private bool isPointerInside;
    private bool isPressed;

    private bool isAutoPlaying;

    private Vector3 originalScale;

    private Coroutine scaleCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;

        buttonImage = GetComponent<Image>();

        if (buttonImage != null)
        {
            buttonImage.raycastTarget = true;
            buttonImage.color = normalColor;
        }
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if (isPressed)
        {
            return;
        }

        isPressed = true;

        if (soundPlayer != null &&
            chordGenerator != null)
        {
            soundingNotes =
                chordGenerator.GenerateChord(
                    degree,
                    chordType,
                    inversion,
                    ChordModifier.None,
                    out soundingRootNoteNumber
                );

            soundPlayer.PlayChord(
                soundingNotes,
                soundingRootNoteNumber
            );
        }
        else
        {
            Debug.LogWarning(
                $"{gameObject.name}: " +
                "SoundPlayerまたはChordGeneratorが未設定です。",
                this
            );
        }

        UpdateColor();
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {
        ReleaseChord();
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        isPointerInside = true;
        UpdateColor();
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        isPointerInside = false;

        ReleaseChord();
        UpdateColor();
    }

    private void ReleaseChord()
    {
        if (!isPressed)
        {
            return;
        }

        isPressed = false;

        if (soundPlayer != null &&
            soundingNotes != null &&
            soundingNotes.Length > 0)
        {
            soundPlayer.StopChord(
                soundingNotes,
                soundingRootNoteNumber
            );
        }

        soundingNotes = null;
        soundingRootNoteNumber = -1;

        UpdateColor();
    }

    private void UpdateColor()
    {
        if (isAutoPlaying)
        {
            buttonImage.color =
                playingColor;
        }
        else if (isPressed)
        {
            buttonImage.color =
                pressedColor;
        }
        else if (isPointerInside)
        {
            buttonImage.color =
                highlightedColor;
        }
        else
        {
            buttonImage.color =
                normalColor;
        }
    }

    public ChordData GetChordData()
    {
        return new ChordData
        {
            degree = degree,
            chordType = chordType,
            inversion = inversion
        };
    }

    public void SetPlaying(bool playing)
    {
        isAutoPlaying = playing;

        UpdateColor();

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine =
            StartCoroutine(
                AnimatePlayingScale(playing)
            );
    }

    private System.Collections.IEnumerator
        AnimatePlayingScale(bool playing)
    {
        Vector3 startScale =
            transform.localScale;

        Vector3 targetScale =
            playing
                ? originalScale * playingScale
                : originalScale;

        float elapsed = 0f;

        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / popDuration
                );

            transform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        transform.localScale =
            targetScale;

        scaleCoroutine = null;
    }

}
