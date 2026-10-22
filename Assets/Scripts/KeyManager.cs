using System;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    [Header("現在のキー：C=0〜B=11")]
    [Range(0, 11)]
    [SerializeField]
    private int tonicPitchClass;

    public event Action KeyChanged;

    public event Action KeyboardTransposeChanged;

    [Header("鍵盤トランスポーズ")]
    [SerializeField]
    private bool transposeKeyboard = true;

    public bool TransposeKeyboard =>
        transposeKeyboard;

    public void SetTransposeKeyboard(bool enabled)
    {
        if (transposeKeyboard == enabled)
        {
            return;
        }

        transposeKeyboard = enabled;

        KeyboardTransposeChanged?.Invoke();
    }

    public void ToggleKeyboardTranspose()
    {
        SetTransposeKeyboard(
            !transposeKeyboard
        );
    }

    public void EnableKeyboardTranspose()
    {
        SetTransposeKeyboard(true);
    }

    public void DisableKeyboardTranspose()
    {
        SetTransposeKeyboard(false);
    }

    private static readonly int[] MajorScaleSemitoneOffsets =
    {
        0, 2, 4, 5, 7, 9, 11
    };

    // Cから最も近い方向へ移調
    private static readonly int[] TransposeOffsets =
    {
         0,  // C
         1,  // C#
         2,  // D
         3,  // Eb
         4,  // E
         5,  // F
         6,  // F#
         7,  // G
         8,  // Ab
         9,  // A
         10,  // Bb
         11   // B
    };

    public int TransposeSemitones =>
        TransposeOffsets[tonicPitchClass];

    public int CurrentKeyPitchClass
    {
        get
        {
            return tonicPitchClass;
        }
    }

    public int GetDegreeRootPitchClass(int degree)
    {
        if (degree < 1 || degree > 7)
        {
            Debug.LogError($"Degreeは1〜7です: {degree}");
            return tonicPitchClass;
        }

        int offset = MajorScaleSemitoneOffsets[degree - 1];
        return (tonicPitchClass + offset) % 12;
    }

    public void SetKey(
        int pitchClass)
    {
        tonicPitchClass =
            Mathf.Clamp(
                pitchClass,
                0,
                11
            );

        KeySelectButton[] buttons =
            FindObjectsByType<KeySelectButton>();

        foreach (
            KeySelectButton button in buttons
        )
        {
            button.RefreshVisual();
        }

        KeyChanged?.Invoke();
    }

    public void NextKey()
    {
        SetKey(
            (tonicPitchClass + 1) % 12
        );
    }

    public void PreviousKey()
    {
        SetKey(
            (tonicPitchClass + 11) % 12
        );
    }

    public void SetKeyC() => SetKey(0);
    public void SetKeyCs() => SetKey(1);
    public void SetKeyD() => SetKey(2);
    public void SetKeyEb() => SetKey(3);
    public void SetKeyE() => SetKey(4);
    public void SetKeyF() => SetKey(5);
    public void SetKeyFs() => SetKey(6);
    public void SetKeyG() => SetKey(7);
    public void SetKeyAb() => SetKey(8);
    public void SetKeyA() => SetKey(9);
    public void SetKeyBb() => SetKey(10);
    public void SetKeyB() => SetKey(11);
    
}