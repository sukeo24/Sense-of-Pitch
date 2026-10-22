using System.Collections;
using UnityEngine;

public class ProgressPlayer : MonoBehaviour
{
    public enum RhythmPattern
    {
        OncePerBar,
        TwicePerBar,
        FourTimesPerBar,
        Arpeggio
    }

    [Header("参照")]
    [SerializeField]
    private SoundPlayer soundPlayer;

    [SerializeField]
    private ChordGenerator chordGenerator;

    [Header("テンポ設定")]
    [SerializeField]
    private float bpm = 120f;

    [Tooltip("4/4拍子なら4拍で1小節")]
    [SerializeField]
    private int beatsPerChord = 4;

    [Header("リズム設定")]
    [SerializeField]
    private RhythmPattern rhythmPattern =
        RhythmPattern.OncePerBar;

    [Header("DSP予約")]
    [Tooltip("次の発音を、この秒数だけ先に予約する")]
    [Range(0.03f, 0.3f)]
    [SerializeField]
    private double scheduleLeadTime = 0.1d;

    private PlaybackItem[] pendingPlaybackItems;

    [Header("ループ再生")]
    [SerializeField]
    private bool loopEnabled = false;

    public bool LoopEnabled => loopEnabled;

    public RhythmPattern CurrentRhythmPattern =>
        rhythmPattern;

    private Coroutine playingCoroutine;

    private int[] highlightedNotes;
    private int highlightedRootNoteNumber = -1;

    private ChordButton currentChordButton;

    private EarCopySlot currentEarCopySlot;

    private class ScheduledHit
    {
        public int itemIndex;
        public int hitIndex;

        public PlaybackItem item;

        public int[] notes;
        public int rootNoteNumber;

        public int playCount;

        public double startDspTime;
        public double slotDuration;
    }

    public void Play(
        PlaybackItem[] playbackItems)
    {
        if (playbackItems == null ||
            playbackItems.Length == 0)
        {
            Debug.LogWarning(
                "再生するコード列が空です。",
                this
            );

            return;
        }

        StopPlaybackImmediately();

        pendingPlaybackItems = null;

        playingCoroutine =
            StartCoroutine(
                PlayRoutine(playbackItems)
            );
    }

    public void Stop()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        if (soundPlayer != null)
        {
            soundPlayer.StopAllScheduled();
        }

        HideCurrentHighlight();

        if (currentChordButton != null)
        {
            currentChordButton.SetPlaying(false);
            currentChordButton = null;
        }

        if (currentEarCopySlot != null)
        {
            currentEarCopySlot.SetPlaying(false);
            currentEarCopySlot = null;
        }

        pendingPlaybackItems = null;
    }

    private void StopPlaybackImmediately()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        if (soundPlayer != null)
        {
            soundPlayer.StopAllScheduledImmediately();
        }

        HideCurrentHighlight();

        if (currentChordButton != null)
        {
            currentChordButton.SetPlaying(false);
            currentChordButton = null;
        }

        if (currentEarCopySlot != null)
        {
            currentEarCopySlot.SetPlaying(false);
            currentEarCopySlot = null;
        }

        pendingPlaybackItems = null;
    }

    public void ChangeProgression(
    PlaybackItem[] newPlaybackItems)
    {
        if (newPlaybackItems == null ||
            newPlaybackItems.Length == 0)
        {
            return;
        }

        // 再生していないなら、
        // UI側だけ変われば十分。
        // 次回Play時に新しいChordButtonから取得される。
        if (playingCoroutine == null)
        {
            pendingPlaybackItems = null;
            return;
        }

        pendingPlaybackItems =
            newPlaybackItems;
    }

    private void SetCurrentEarCopySlot(
    EarCopySlot nextSlot)
    {
        if (currentEarCopySlot == nextSlot)
            return;

        if (currentEarCopySlot != null)
        {
            currentEarCopySlot.SetPlaying(false);
        }

        currentEarCopySlot = nextSlot;

        if (currentEarCopySlot != null)
        {
            currentEarCopySlot.SetPlaying(true);
        }
    }

    public void ToggleLoop()
    {
        loopEnabled = !loopEnabled;
    }

    public void NextRhythm()
    {
        int nextIndex =
            ((int)rhythmPattern + 1) %
            System.Enum.GetValues(
                typeof(RhythmPattern)
            ).Length;

        rhythmPattern =
            (RhythmPattern)nextIndex;
    }

    private IEnumerator PlayRoutine(
        PlaybackItem[] playbackItems)
    {
        double firstStartDspTime =
            AudioSettings.dspTime +
            scheduleLeadTime;

        ScheduledHit current =
            ScheduleHit(
                playbackItems,
                itemIndex: 0,
                hitIndex: 0,
                startDspTime:
                    firstStartDspTime,
                playCount:
                    GetPlayCountForItem(
                        playbackItems[0]
                    )
            );

        while (current != null)
        {
            while (
                AudioSettings.dspTime <
                current.startDspTime
            )
            {
                yield return null;
            }

            SetCurrentChordButton(
                current.item.chordButton
            );

            SetCurrentEarCopySlot(
                current.item.earCopySlot
            );

            ShowHighlight(
                current.notes,
                current.rootNoteNumber
            );

            double nextStartDspTime =
                current.startDspTime +
                current.slotDuration;

            double reservationDspTime =
                nextStartDspTime -
                scheduleLeadTime;

            while (
                AudioSettings.dspTime <
                reservationDspTime
            )
            {
                yield return null;
            }

            ScheduledHit next;

            // 現在のコードの最後の刻みが終わるタイミングで、
            // 新しい進行が選択されていたら乗り換える。
            bool currentChordFinished =
                current.hitIndex + 1 >=
                current.playCount;

            if (currentChordFinished &&
                pendingPlaybackItems != null &&
                pendingPlaybackItems.Length > 0)
            {
                // ここからPlayRoutineが使う進行自体を
                // 新しいものへ差し替える
                playbackItems =
                    pendingPlaybackItems;

                pendingPlaybackItems = null;

                // 新しい進行は必ず1コード目から
                next =
                    ScheduleHit(
                        playbackItems,
                        itemIndex: 0,
                        hitIndex: 0,
                        startDspTime:
                            nextStartDspTime,
                        playCount:
                        GetPlayCountForItem(
                            playbackItems[0]
                        )
                    );
            }
            else
            {
                next =
                    ScheduleNextHit(
                        playbackItems,
                        current,
                        nextStartDspTime
                    );
            }

            while (
                AudioSettings.dspTime <
                nextStartDspTime
            )
            {
                yield return null;
            }

            HideCurrentHighlight();

            if (next == null)
            {
                if (currentChordButton != null)
                {
                    currentChordButton.SetPlaying(
                        false
                    );

                    currentChordButton = null;
                }

                if (currentEarCopySlot != null)
                {
                    currentEarCopySlot.SetPlaying(false);
                    currentEarCopySlot = null;
                }

                break;
            }

            current = next;
        }

        playingCoroutine = null;
    }

    private ScheduledHit ScheduleNextHit(
        PlaybackItem[] playbackItems,
        ScheduledHit current,
        double nextStartDspTime)
    {
        int nextItemIndex =
            current.itemIndex;

        int nextHitIndex =
            current.hitIndex + 1;

        int nextPlayCount =
            current.playCount;

        // 同じコード内の次の刻み
        if (nextHitIndex <
            current.playCount)
        {
            return ScheduleHit(
                playbackItems,
                nextItemIndex,
                nextHitIndex,
                nextStartDspTime,
                nextPlayCount
            );
        }

        // 次のコードへ進む
        nextItemIndex++;

        if (nextItemIndex >=
            playbackItems.Length)
        {
            if (!loopEnabled)
            {
                return null;
            }

            nextItemIndex = 0;
        }

        // 新しいコードに入る時点で、
        // 現在選ばれているリズムを読み直す
        nextPlayCount =
        GetPlayCountForItem(
            playbackItems[
                nextItemIndex
            ]
        );

        return ScheduleHit(
            playbackItems,
            nextItemIndex,
            hitIndex: 0,
            startDspTime:
                nextStartDspTime,
            playCount:
                nextPlayCount
        );
    }

    private ScheduledHit ScheduleHit(
        PlaybackItem[] playbackItems,
        int itemIndex,
        int hitIndex,
        double startDspTime,
        int playCount)
    {
        PlaybackItem item =
    playbackItems[itemIndex];

        ChordData currentChordData =
            item.chordData;

        // 耳コピスロット由来なら、
        // 予約する直前に現在のスロット内容を読む
        if (item.earCopySlot != null &&
            item.earCopySlot.HasChord)
        {
            currentChordData =
                item.earCopySlot.ChordData;
        }

        // ベースはコードとは独立して、再生直前の入力値を読む。
        bool hasBass = item.bassNoteSlot != null &&
                       item.bassNoteSlot.HasNote;
        int bassNoteNumber = hasBass
            ? item.bassNoteSlot.NoteNumber
            : -1;

        int rootNoteNumber = -1;
        int[] notes = new int[0];

        if (currentChordData != null)
        {
            notes = chordGenerator.GenerateChord(
                currentChordData.degree,
                currentChordData.chordType,
                currentChordData.inversion,
                currentChordData.modifiers,
                out rootNoteNumber
            );

        }
        else if (!item.bassOnly)
        {
            return null;
        }

        double beatDuration =
    60d / bpm;

        double barDuration =
            beatDuration *
            beatsPerChord;

        double chordDuration =
            barDuration *
            item.barFraction;

        double slotDuration =
            chordDuration /
            playCount;

        double releaseStartDspTime =
            startDspTime +
            slotDuration;

        int[] scheduledNotes;

        if (rhythmPattern ==
            RhythmPattern.Arpeggio && notes.Length > 0)
        {
            int noteIndex;

            if (notes.Length == 3)
            {
                int[] triadPattern =
                {
            0, 1, 2, 1,
            0, 1, 2, 1
        };

                noteIndex =
                    triadPattern[
                        hitIndex %
                        triadPattern.Length
                    ];
            }
            else if (notes.Length == 4)
            {
                int[] seventhPattern =
                {
            0, 1, 2, 3,
            2, 1, 0, 1
        };

                noteIndex =
                    seventhPattern[
                        hitIndex %
                        seventhPattern.Length
                    ];
            }
            else
            {
                noteIndex =
                    hitIndex %
                    notes.Length;
            }

            scheduledNotes =
                new int[]
                {
            notes[noteIndex]
                };
        }
        else
        {
            scheduledNotes =
                notes;
        }

        if (scheduledNotes.Length > 0)
        {
            soundPlayer.ScheduleChord(
                scheduledNotes,
                startDspTime,
                releaseStartDspTime
            );
        }

        // ベースは1小節の頭で1回鳴らし、小節末まで伸ばす。
        // 小節が2コードに分割されても、ベースは途中で再発音しない。
        if (item.bassAtBarStart && hitIndex == 0 && hasBass)
        {
            soundPlayer.ScheduleChord(
                new int[] { bassNoteNumber },
                startDspTime,
                startDspTime + barDuration
            );
        }

        // 再生ハイライトには、持続中のベースも含める。
        notes = hasBass
            ? AppendNote(scheduledNotes, bassNoteNumber)
            : scheduledNotes;
        if (hasBass)
        {
            rootNoteNumber = bassNoteNumber;
        }

        return new ScheduledHit
        {
            itemIndex = itemIndex,
            hitIndex = hitIndex,
            item = item,
            notes = notes,
            rootNoteNumber =
                rootNoteNumber,
            playCount = playCount,
            startDspTime =
                startDspTime,
            slotDuration =
                slotDuration
        };
    }

    private static int[] AppendNote(int[] notes, int note)
    {
        for (int i = 0; i < notes.Length; i++)
        {
            if (notes[i] == note)
            {
                return notes;
            }
        }
        int[] result = new int[notes.Length + 1];
        System.Array.Copy(notes, result, notes.Length);
        result[notes.Length] = note;
        return result;
    }

    private int GetPlayCount()
    {
        switch (rhythmPattern)
        {
            case RhythmPattern.OncePerBar:
                return 1;

            case RhythmPattern.TwicePerBar:
                return 2;

            case RhythmPattern.FourTimesPerBar:
                return 4;

            case RhythmPattern.Arpeggio:
                return 4;

            default:
                return 1;
        }
    }

    private int GetPlayCountForItem(
    PlaybackItem item)
    {
        // ベース単独の小節はリズムパターンに関係なく1回のみ。
        if (item.bassOnly)
        {
            return 1;
        }

        int basePlayCount =
            GetPlayCount();

        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                basePlayCount *
                item.barFraction
            )
        );
    }

    private void SetCurrentChordButton(
        ChordButton nextButton)
    {
        if (currentChordButton ==
            nextButton)
        {
            return;
        }

        if (currentChordButton != null)
        {
            currentChordButton.SetPlaying(
                false
            );
        }

        currentChordButton =
            nextButton;

        if (currentChordButton != null)
        {
            currentChordButton.SetPlaying(
                true
            );
        }
    }

    private void ShowHighlight(
        int[] notes,
        int rootNoteNumber)
    {
        HideCurrentHighlight();

        highlightedNotes = notes;

        highlightedRootNoteNumber =
            rootNoteNumber;

        soundPlayer.ShowChordHighlight(
            highlightedNotes,
            highlightedRootNoteNumber
        );
    }

    private void HideCurrentHighlight()
    {
        if (highlightedNotes == null ||
            highlightedNotes.Length == 0)
        {
            return;
        }

        soundPlayer.HideChordHighlight(
            highlightedNotes,
            highlightedRootNoteNumber
        );

        highlightedNotes = null;
        highlightedRootNoteNumber = -1;
    }
}