using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class KeyboardAutoSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("子鍵盤を名前から一括採番")]
    private void AutoAssignAllKeys()
    {
        PianoKey[] keys = GetComponentsInChildren<PianoKey>(true);

        int successCount = 0;
        int failureCount = 0;

        foreach (PianoKey key in keys)
        {
            if (!TryConvertNameToNoteNumber(
                    key.gameObject.name,
                    out int noteNumber))
            {
                Debug.LogWarning(
                    $"変換できない鍵盤名: {key.gameObject.name}",
                    key
                );

                failureCount++;
                continue;
            }

            SerializedObject serializedKey =
                new SerializedObject(key);

            // 現在のPianoKey.csでは変数名がnoteNumber
            SerializedProperty noteProperty =
                serializedKey.FindProperty("noteNumber");

            if (noteProperty == null)
            {
                Debug.LogError(
                    "PianoKey.cs内にnoteNumberが見つかりません。",
                    key
                );

                failureCount++;
                continue;
            }

            noteProperty.intValue = noteNumber;
            serializedKey.ApplyModifiedProperties();

            EditorUtility.SetDirty(key);
            successCount++;
        }

        Debug.Log(
            $"鍵盤採番完了：成功 {successCount}個、失敗 {failureCount}個",
            this
        );
    }

    private static bool TryConvertNameToNoteNumber(
        string objectName,
        out int noteNumber)
    {
        noteNumber = -1;

        if (string.IsNullOrWhiteSpace(objectName))
        {
            return false;
        }

        string normalized = objectName
            .Trim()
            .Replace("#", "sharp")
            .Replace("♯", "sharp");

        (string name, int pitchClass)[] notes =
        {
            ("Csharp", 1),
            ("Dsharp", 3),
            ("Fsharp", 6),
            ("Gsharp", 8),
            ("Asharp", 10),
            ("C", 0),
            ("D", 2),
            ("E", 4),
            ("F", 5),
            ("G", 7),
            ("A", 9),
            ("B", 11)
        };

        foreach (var note in notes)
        {
            if (!normalized.StartsWith(note.name))
            {
                continue;
            }

            string octaveText =
                normalized.Substring(note.name.Length);

            if (!int.TryParse(octaveText, out int octave))
            {
                return false;
            }

            noteNumber =
                12 * (octave + 1) + note.pitchClass;

            return true;
        }

        return false;
    }
#endif
}
