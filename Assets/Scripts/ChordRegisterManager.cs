using UnityEngine;

public class ChordRegisterManager : MonoBehaviour
{
    [Header("基準となる中心音域")]
    [Tooltip(
        "この音番号に最も近い、現在のキーの主音を" +
        "コード生成の基準音にします。"
    )]
    [SerializeField]
    private int referenceNoteNumber = 48;

    [SerializeField]
    private int currentOctave = 3;

public int CurrentOctave =>
    currentOctave;

    public int ReferenceNoteNumber =>
        referenceNoteNumber;

    /// <summary>
    /// referenceNoteNumberに最も近い、
    /// 指定された主音の実音番号を返す。
    ///
    /// 例:
    /// reference = 48
    /// C なら 48
    /// D なら 50
    /// B なら 47
    /// </summary>
    public int GetKeyTonicAnchor(
        int tonicPitchClass)
    {
        tonicPitchClass =
            PositiveModulo(tonicPitchClass, 12);

        int referencePitchClass =
            PositiveModulo(referenceNoteNumber, 12);

        int upwardDistance =
            PositiveModulo(
                tonicPitchClass - referencePitchClass,
                12
            );

        int upwardCandidate =
            referenceNoteNumber + upwardDistance;

        int downwardCandidate =
            upwardCandidate - 12;

        int upwardDistanceFromReference =
            Mathf.Abs(
                upwardCandidate - referenceNoteNumber
            );

        int downwardDistanceFromReference =
            Mathf.Abs(
                downwardCandidate - referenceNoteNumber
            );

        // 同距離の場合は上側を採用する。
        if (downwardDistanceFromReference <
            upwardDistanceFromReference)
        {
            return downwardCandidate;
        }

        return upwardCandidate;
    }

    public void SetReferenceNoteNumber(
        int noteNumber)
    {
        referenceNoteNumber = noteNumber;
    }

    public void SetOctave(int octave)
    {
        currentOctave =
            Mathf.Clamp(octave, 1, 6);

        referenceNoteNumber =
            12 * (currentOctave + 1);
    }

    public void OctaveDown()
    {
        SetOctave(
            currentOctave - 1
        );
    }

    public void OctaveUp()
    {
        SetOctave(
            currentOctave + 1
        );
    }

    private static int PositiveModulo(
        int value,
        int modulo)
    {
        return ((value % modulo) + modulo)
            % modulo;
    }
}
