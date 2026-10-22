using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A selectable bass-note slot. A keyboard can send MIDI note numbers via
/// BassNoteSlot.TryAssignSelectedNote(noteNumber).
/// </summary>
public class BassNoteSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("表示")]
    [SerializeField] private TMP_Text noteText;
    [SerializeField] private Outline selectionOutline;

    private static BassNoteSlot selectedSlot;

    private bool hasNote;
    private int noteNumber;

    public bool HasNote => hasNote;
    public int NoteNumber => noteNumber;
    public bool IsSelected => selectedSlot == this;
    public static BassNoteSlot SelectedSlot => selectedSlot;

    private void Awake()
    {
        // 子のTextMeshProを自動で取得する。Inspectorでのドラッグ不要。
        if (noteText == null)
        {
            noteText = GetComponentInChildren<TMP_Text>(true);
        }

        // 選択枠線も自動で用意する。
        if (selectionOutline == null)
        {
            selectionOutline = GetComponent<Outline>();
        }
        if (selectionOutline == null)
        {
            selectionOutline = gameObject.AddComponent<Outline>();
            selectionOutline.effectColor = new Color(0.20f, 0.85f, 1f, 1f);
            selectionOutline.effectDistance = new Vector2(2f, -2f);
            selectionOutline.useGraphicAlpha = false;
        }

        RefreshVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        Select();
    }

    public void Select()
    {
        if (selectedSlot == this)
        {
            return;
        }

        BassNoteSlot previous = selectedSlot;
        selectedSlot = this;

        if (previous != null)
        {
            previous.RefreshVisual();
        }

        RefreshVisual();
    }

    // Call this from the existing piano-key press handler AFTER the usual sound.
    // Returns false when no bass slot has been selected.
    public static bool TryAssignSelectedNote(int midiNoteNumber)
    {
        if (selectedSlot == null || !selectedSlot.isActiveAndEnabled)
        {
            return false;
        }

        return selectedSlot.SetNote(midiNoteNumber);
    }

    public bool SetNote(int midiNoteNumber)
    {
        if (midiNoteNumber < 0 || midiNoteNumber > 127)
        {
            Debug.LogWarning($"Invalid MIDI note number: {midiNoteNumber}", this);
            return false;
        }

        noteNumber = midiNoteNumber;
        hasNote = true;
        RefreshVisual();
        return true;
    }

    public void ClearNote()
    {
        hasNote = false;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (noteText != null)
        {
            noteText.text = hasNote ? GetNoteName(noteNumber) : "BASS";
        }

        if (selectionOutline != null)
        {
            selectionOutline.enabled = IsSelected;
        }
    }

    private static string GetNoteName(int midiNoteNumber)
    {
        string[] names =
        {
            "C", "C#", "D", "D#", "E", "F",
            "F#", "G", "G#", "A", "A#", "B"
        };

        int pitchClass = midiNoteNumber % 12;
        int octave = midiNoteNumber / 12 - 1;
        return names[pitchClass] + octave;
    }

    private void OnDisable()
    {
        if (selectedSlot == this)
        {
            selectedSlot = null;
        }
    }
}
