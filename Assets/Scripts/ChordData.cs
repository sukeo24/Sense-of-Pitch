using System;

[Serializable]
public class ChordData
{
    public int degree;

    public ChordType chordType;

    public Inversion inversion;

    public ChordModifier modifiers =
    ChordModifier.None;
}