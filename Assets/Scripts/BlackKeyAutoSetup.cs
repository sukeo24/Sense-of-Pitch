using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlackKeyAutoSetup : MonoBehaviour
{
    [Header("白鍵を並べている親")]
    [SerializeField]
    private RectTransform whiteKeysRoot;

    [Header("黒鍵を生成する親")]
    [SerializeField]
    private RectTransform blackKeysRoot;

    [Header("黒鍵のPrefab")]
    [SerializeField]
    private PianoKey blackKeyPrefab;

    [Header("黒鍵の上端からの位置")]
    [SerializeField]
    private float blackKeyY = 0f;

#if UNITY_EDITOR
    [ContextMenu("黒鍵を自動生成")]
    private void GenerateBlackKeys()
    {
        if (whiteKeysRoot == null ||
            blackKeysRoot == null ||
            blackKeyPrefab == null)
        {
            Debug.LogError(
                "White Keys Root、Black Keys Root、Black Key Prefabを設定してください。",
                this
            );
            return;
        }

        // Horizontal Layout Groupによる白鍵配置を確定
        LayoutRebuilder.ForceRebuildLayoutImmediate(whiteKeysRoot);

        // 以前自動生成した黒鍵を削除
        for (int i = blackKeysRoot.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(
                blackKeysRoot.GetChild(i).gameObject
            );
        }

        Dictionary<string, RectTransform> whiteKeys =
            new Dictionary<string, RectTransform>();

        foreach (Transform child in whiteKeysRoot)
        {
            RectTransform rect = child as RectTransform;

            if (rect != null)
            {
                whiteKeys[child.name] = rect;
            }
        }

        int generatedCount = 0;

        // C0～B7の各オクターブに5本ずつ
        for (int octave = 1; octave <= 7; octave++)
        {
            generatedCount += CreateBlackKey(
                whiteKeys,
                $"C{octave}",
                $"D{octave}",
                $"Csharp{octave}"
            );

            generatedCount += CreateBlackKey(
                whiteKeys,
                $"D{octave}",
                $"E{octave}",
                $"Dsharp{octave}"
            );

            generatedCount += CreateBlackKey(
                whiteKeys,
                $"F{octave}",
                $"G{octave}",
                $"Fsharp{octave}"
            );

            generatedCount += CreateBlackKey(
                whiteKeys,
                $"G{octave}",
                $"A{octave}",
                $"Gsharp{octave}"
            );

            generatedCount += CreateBlackKey(
                whiteKeys,
                $"A{octave}",
                $"B{octave}",
                $"Asharp{octave}"
            );
        }

        Debug.Log(
            $"黒鍵を{generatedCount}個生成しました。",
            this
        );
    }

    private int CreateBlackKey(
        Dictionary<string, RectTransform> whiteKeys,
        string leftWhiteName,
        string rightWhiteName,
        string blackKeyName)
    {
        if (!whiteKeys.TryGetValue(
                leftWhiteName,
                out RectTransform leftWhite) ||
            !whiteKeys.TryGetValue(
                rightWhiteName,
                out RectTransform rightWhite))
        {
            Debug.LogWarning(
                $"{leftWhiteName} または {rightWhiteName} が見つかりません。",
                this
            );
            return 0;
        }

        PianoKey newKey =
            PrefabUtility.InstantiatePrefab(
                blackKeyPrefab,
                blackKeysRoot
            ) as PianoKey;

        if (newKey == null)
        {
            Debug.LogError("黒鍵Prefabを生成できませんでした。", this);
            return 0;
        }

        Undo.RegisterCreatedObjectUndo(
            newKey.gameObject,
            "黒鍵を自動生成"
        );

        newKey.gameObject.name = blackKeyName;

        if (TryConvertNameToNoteNumber(
                blackKeyName,
                out int noteNumber))
        {
            newKey.SetNoteNumber(
                noteNumber
            );
        }

        RectTransform blackRect =
            newKey.GetComponent<RectTransform>();

        // 左右の白鍵中心の中間点をワールド座標で求める
        Vector3 middleWorld =
            (leftWhite.TransformPoint(leftWhite.rect.center) +
             rightWhite.TransformPoint(rightWhite.rect.center)) * 0.5f;

        Vector3 localPosition =
            blackKeysRoot.InverseTransformPoint(middleWorld);

        blackRect.anchorMin = new Vector2(0f, 1f);
        blackRect.anchorMax = new Vector2(0f, 1f);
        blackRect.pivot = new Vector2(0.5f, 1f);

        blackRect.anchoredPosition =
            new Vector2(localPosition.x, blackKeyY);


        return 1;
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
            if (!normalized.StartsWith(
                    note.name
                ))
            {
                continue;
            }

            string octaveText =
                normalized.Substring(
                    note.name.Length
                );

            if (!int.TryParse(
                    octaveText,
                    out int octave
                ))
            {
                return false;
            }

            noteNumber =
                12 * (octave + 1) +
                note.pitchClass;

            return true;
        }

        return false;
    }

#endif
}
