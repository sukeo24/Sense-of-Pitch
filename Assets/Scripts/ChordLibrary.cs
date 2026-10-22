using System;

public static class ChordLibrary
{
    public static int[] GetIntervals(ChordType chordType)
    {
        switch (chordType)
        {
            case ChordType.Major:
                return new[] { 0, 4, 7 };

            case ChordType.Minor:
                return new[] { 0, 3, 7 };

            case ChordType.Diminished:
                return new[] { 0, 3, 6 };

            case ChordType.Augmented:
                return new[] { 0, 4, 8 };

            case ChordType.Sus2:
                return new[] { 0, 2, 7 };

            case ChordType.Sus4:
                return new[] { 0, 5, 7 };

            case ChordType.Dominant7:
                return new[] { 0, 4, 7, 10 };

            case ChordType.Major7:
                return new[] { 0, 4, 7, 11 };

            case ChordType.Minor7:
                return new[] { 0, 3, 7, 10 };

            case ChordType.HalfDiminished7:
                return new[] { 0, 3, 6, 10 };

            case ChordType.Diminished7:
                return new[] { 0, 3, 6, 9 };

            case ChordType.Add9:
                return new[] { 0, 4, 7, 14 };

            case ChordType.MinorAdd9:
                return new[] { 0, 3, 7, 14 };

            case ChordType.Major9:
                return new[] { 0, 4, 7, 11, 14 };

            case ChordType.Dominant9:
                return new[] { 0, 4, 7, 10, 14 };

            case ChordType.Minor9:
                return new[] { 0, 3, 7, 10, 14 };

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(chordType),
                    chordType,
                    "未対応のコードタイプです。"
                );
        }
    }
}
