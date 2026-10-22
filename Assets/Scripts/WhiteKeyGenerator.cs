using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WhiteKeyGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("参照")]
    [SerializeField]
    private RectTransform whiteKeysRoot;

    [SerializeField]
    private PianoKey whiteKeyPrefab;

    [SerializeField]
    private RectTransform keyboardContent;

    [Header("生成範囲")]
    [SerializeField]
    private int lowestNoteNumber = 23;   // B0

    [SerializeField]
    private int highestNoteNumber = 108; // C8

    [ContextMenu("白鍵をB0〜C8まで生成")]
    private void GenerateWhiteKeys()
    {
        if (whiteKeysRoot == null ||
            whiteKeyPrefab == null)
        {
            Debug.LogWarning(
                "White Keys Root と White Key Prefab を設定してください。",
                this
            );

            return;
        }

        int createdCount = 0;

        for (int noteNumber = lowestNoteNumber;
             noteNumber <= highestNoteNumber;
             noteNumber++)
        {
            if (IsBlackKey(noteNumber))
            {
                continue;
            }

            PianoKey newKey =
                PrefabUtility.InstantiatePrefab(
                    whiteKeyPrefab,
                    whiteKeysRoot
                ) as PianoKey;

            if (newKey == null)
            {
                continue;
            }

            string keyName =
                NoteNumberToName(
                    noteNumber
                );

            newKey.gameObject.name =
                keyName;

            newKey.SetNoteNumber(
                noteNumber
            );

            TMPro.TMP_Text label =
                newKey.GetComponentInChildren<
                    TMPro.TMP_Text
                >(
                    true
                );

            if (label != null)
            {
                label.text =
                    keyName;
            }

            EditorUtility.SetDirty(
                newKey
            );

            createdCount++;
        }

        Canvas.ForceUpdateCanvases();

        if (keyboardContent != null &&
            whiteKeysRoot != null)
        {
            float width =
                whiteKeysRoot.rect.width;

            keyboardContent.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width
            );
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            whiteKeysRoot
        );

        Canvas.ForceUpdateCanvases();

        if (keyboardContent != null)
        {
            keyboardContent.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                whiteKeysRoot.rect.width
            );
        }

        Debug.Log(
            $"白鍵を{createdCount}個生成しました。",
            this
        );
    }

    private static bool IsBlackKey(
        int noteNumber)
    {
        int pitchClass =
            ((noteNumber % 12) + 12) % 12;

        return pitchClass == 1 ||
               pitchClass == 3 ||
               pitchClass == 6 ||
               pitchClass == 8 ||
               pitchClass == 10;
    }

    private static string NoteNumberToName(
        int noteNumber)
    {
        string[] noteNames =
        {
            "C",
            "Csharp",
            "D",
            "Dsharp",
            "E",
            "F",
            "Fsharp",
            "G",
            "Gsharp",
            "A",
            "Asharp",
            "B"
        };

        int pitchClass =
            ((noteNumber % 12) + 12) % 12;

        int octave =
            noteNumber / 12 - 1;

        return
            noteNames[pitchClass] +
            octave;
    }
#endif
}