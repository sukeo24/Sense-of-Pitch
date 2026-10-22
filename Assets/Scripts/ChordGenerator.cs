using System;
using UnityEngine;

public class ChordGenerator : MonoBehaviour
{
    [Header("キー管理")]
    [SerializeField]
    private KeyManager keyManager;

    [Header("コード音域管理")]
    [SerializeField]
    private ChordRegisterManager registerManager;

    public int[] GenerateChord(
    int degree,
    ChordType chordType,
    Inversion inversion,
    ChordModifier modifiers,
    out int rootNoteNumber)
    {
        rootNoteNumber = -1;

        if (keyManager == null)
        {
            Debug.LogError(
                "ChordGeneratorにKeyManagerが設定されていません。",
                this
            );

            return Array.Empty<int>();
        }

        if (registerManager == null)
        {
            Debug.LogError(
                "ChordGeneratorにChordRegisterManagerが設定されていません。",
                this
            );

            return Array.Empty<int>();
        }

        int tonicPitchClass =
            keyManager.GetDegreeRootPitchClass(1);

        int anchorNoteNumber =
            registerManager.GetKeyTonicAnchor(
                tonicPitchClass
            );

        int rootPitchClass =
            keyManager.GetDegreeRootPitchClass(
                degree
            );

        int[] intervals =
            GetIntervalsWithModifiers(
                chordType,
                modifiers
            );

        if (intervals == null ||
            intervals.Length == 0)
        {
            Debug.LogWarning(
                $"{chordType}の構成音が取得できませんでした。",
                this
            );

            return Array.Empty<int>();
        }

        int[] invertedIntervals =
            ApplyInversion(
                intervals,
                inversion
            );

        int rootAnchorNoteNumber =
            FindFirstNoteAtOrAbove(
                anchorNoteNumber,
                rootPitchClass
            );

        int[] generatedNotes =
            new int[invertedIntervals.Length];

        for (int i = 0;
             i < invertedIntervals.Length;
             i++)
        {
            generatedNotes[i] =
                rootAnchorNoteNumber +
                invertedIntervals[i];
        }

        NormalizeToRegister(
            generatedNotes,
            anchorNoteNumber
        );

        Array.Sort(generatedNotes);

        rootNoteNumber =
            FindRootNoteNumber(
                generatedNotes,
                rootPitchClass
            );

        return generatedNotes;
    }

    private static int[] ApplyInversion(
        int[] intervals,
        Inversion inversion)
    {
        int[] result =
            (int[])intervals.Clone();

        if (result.Length == 0)
        {
            return result;
        }

        int inversionCount =
            Mathf.Clamp(
                (int)inversion,
                0,
                result.Length - 1
            );

        for (int i = 0;
             i < inversionCount;
             i++)
        {
            Array.Sort(result);
            result[0] += 12;
        }

        Array.Sort(result);

        return result;
    }

    private static void NormalizeToRegister(
        int[] notes,
        int anchorNoteNumber)
    {
        if (notes == null ||
            notes.Length == 0)
        {
            return;
        }

        int lowestNoteNumber =
            notes[0];

        for (int i = 1;
             i < notes.Length;
             i++)
        {
            if (notes[i] <
                lowestNoteNumber)
            {
                lowestNoteNumber =
                    notes[i];
            }
        }

        int upperLimit =
            anchorNoteNumber + 12;

        int octaveShift = 0;

        while (
            lowestNoteNumber +
            octaveShift <
            anchorNoteNumber
        )
        {
            octaveShift += 12;
        }

        while (
            lowestNoteNumber +
            octaveShift >=
            upperLimit
        )
        {
            octaveShift -= 12;
        }

        for (int i = 0;
             i < notes.Length;
             i++)
        {
            notes[i] += octaveShift;
        }
    }

    private static int FindFirstNoteAtOrAbove(
        int anchorNoteNumber,
        int pitchClass)
    {
        int anchorPitchClass =
            PositiveModulo(
                anchorNoteNumber,
                12
            );

        int semitoneDistance =
            PositiveModulo(
                pitchClass -
                anchorPitchClass,
                12
            );

        return
            anchorNoteNumber +
            semitoneDistance;
    }

    private static int FindRootNoteNumber(
        int[] notes,
        int rootPitchClass)
    {
        if (notes == null)
        {
            return -1;
        }

        int normalizedRootPitchClass =
            PositiveModulo(
                rootPitchClass,
                12
            );

        foreach (int noteNumber in notes)
        {
            int notePitchClass =
                PositiveModulo(
                    noteNumber,
                    12
                );

            if (notePitchClass ==
                normalizedRootPitchClass)
            {
                return noteNumber;
            }
        }

        return -1;
    }

    private static int[] GetIntervalsWithModifiers(
    ChordType chordType,
    ChordModifier modifiers)
    {
        int[] baseIntervals =
            ChordLibrary.GetIntervals(
                chordType
            );

        var intervals =
            new System.Collections.Generic.List<int>(
                baseIntervals
            );

        // sus4
        if ((modifiers &
             ChordModifier.Sus4) != 0)
        {
            // Major 3rd / Minor 3rd を消す
            intervals.Remove(4);
            intervals.Remove(3);

            // Perfect 4th を追加
            if (!intervals.Contains(5))
            {
                intervals.Add(5);
            }
        }

        // sus2
        if ((modifiers &
             ChordModifier.Sus2) != 0)
        {
            // 3度を消す
            intervals.Remove(4);
            intervals.Remove(3);

            // Major 2nd
            if (!intervals.Contains(2))
            {
                intervals.Add(2);
            }
        }

        // 短7度
        if ((modifiers &
             ChordModifier.Seventh) != 0)
        {
            // 既存の7thがあれば一旦除去
            intervals.Remove(11);
            intervals.Remove(10);
            intervals.Remove(9);

            intervals.Add(10);
        }

        // 長7度
        if ((modifiers &
             ChordModifier.MajorSeventh) != 0)
        {
            intervals.Remove(11);
            intervals.Remove(10);
            intervals.Remove(9);

            intervals.Add(11);
        }

        // add9
        if ((modifiers &
             ChordModifier.Add9) != 0)
        {
            // 9th = ルートより14半音上
            if (!intervals.Contains(14))
            {
                intervals.Add(14);
            }
        }

        intervals.Sort();

        return intervals.ToArray();
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