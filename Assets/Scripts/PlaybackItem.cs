using System;

[Serializable]
public class PlaybackItem
{
    public ChordData chordData;

    public ChordButton chordButton;

    public EarCopySlot earCopySlot;

    public float barFraction = 1f;

    // 耳コピモード用。未指定なら従来のコード再生のまま。
    public BassNoteSlot bassNoteSlot;
    public bool bassAtBarStart;
    public bool bassOnly;
}