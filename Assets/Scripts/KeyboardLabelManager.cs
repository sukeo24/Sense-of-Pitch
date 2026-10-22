using TMPro;
using UnityEngine;

public class KeyboardLabelManager : MonoBehaviour
{
    public enum LabelMode
    {
        NoteName,
        Solfege
    }

    [Header("キー管理")]
    [SerializeField]
    private KeyManager keyManager;

    [Header("表示方式")]
    [SerializeField]
    private LabelMode currentMode =
        LabelMode.Solfege;

    private PianoKey[] pianoKeys;

    public LabelMode CurrentMode => currentMode;

    private static readonly string[] NoteNames =
    {
        "C",
        "C#/Db",
        "D",
        "D#/Eb",
        "E",
        "F",
        "F#/Gb",
        "G",
        "G#/Ab",
        "A",
        "A#/Bb",
        "B"
    };

    private static readonly string[] SolfegeNames =
    {
        "ド",
        "デ",
        "レ",
        "リ",
        "ミ",
        "ファ",
        "フィ",
        "ソ",
        "サ",
        "ラ",
        "チ",
        "シ"
    };

    private void Awake()
    {
        pianoKeys =
            FindObjectsByType<PianoKey>(
                FindObjectsInactive.Include
            );
    }

    private void OnEnable()
    {
        if (keyManager == null)
        {
            return;
        }

        keyManager.KeyChanged += RefreshAllLabels;

        keyManager.KeyboardTransposeChanged +=
            RefreshAllLabels;
    }

    private void OnDisable()
    {
        if (keyManager == null)
        {
            return;
        }

        keyManager.KeyChanged -= RefreshAllLabels;

        keyManager.KeyboardTransposeChanged -=
            RefreshAllLabels;
    }

    private void Start()
    {
        RefreshAllLabels();
    }

    public void ShowNoteNames()
    {
        currentMode =
            LabelMode.NoteName;

        RefreshAllLabels();
    }

    public void ShowSolfege()
    {
        currentMode =
            LabelMode.Solfege;

        RefreshAllLabels();
    }

    public void ToggleLabelMode()
    {
        currentMode =
            currentMode == LabelMode.NoteName
                ? LabelMode.Solfege
                : LabelMode.NoteName;

        RefreshAllLabels();
    }

    public void RefreshAllLabels()
    {
        if (pianoKeys == null ||
            pianoKeys.Length == 0)
        {
            pianoKeys =
                FindObjectsByType<PianoKey>(
                    FindObjectsInactive.Include
                );
        }

            int transposeSemitones =
                keyManager != null &&
                keyManager.TransposeKeyboard
                    ? keyManager.TransposeSemitones
                    : 0;

        foreach (PianoKey pianoKey in pianoKeys)
        {
            if (pianoKey == null)
            {
                continue;
            }

            TMP_Text label =
                pianoKey.GetComponentInChildren<TMP_Text>(
                    true
                );

            if (label == null)
            {
                continue;
            }

            int basePitchClass =
                PositiveModulo(
                    pianoKey.NoteNumber,
                    12
                );

            bool isBlackKey =
                basePitchClass == 1 ||
                basePitchClass == 3 ||
                basePitchClass == 6 ||
                basePitchClass == 8 ||
                basePitchClass == 10;

            if (currentMode == LabelMode.Solfege)
            {
                label.text =
                    SolfegeNames[basePitchClass];
            }
            else
            {
                if (isBlackKey)
                {
                    label.text = "";
                    continue;
                }

                int soundingNoteNumber =
                    pianoKey.NoteNumber +
                    transposeSemitones;

                int soundingPitchClass =
                    PositiveModulo(
                        soundingNoteNumber,
                        12
                    );

                int octave =
                    (soundingNoteNumber / 12) - 1;

                label.text =
                    NoteNames[soundingPitchClass] +
                    octave;
            }
        }
    }

    private static int PositiveModulo(
        int value,
        int modulo)
    {
        return
            ((value % modulo) + modulo)
            % modulo;
    }
}