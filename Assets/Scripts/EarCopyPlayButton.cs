using System.Collections.Generic;
using UnityEngine;

public class EarCopyPlayButton : MonoBehaviour
{
    [Header("再生")]
    [SerializeField] private ProgressPlayer progressPlayer;

    [Header("小節")]
    [SerializeField] private MeasureSlot[] measures;

    [Header("ベース音（未設定なら BassSlots から自動取得）")]
    [SerializeField] private BassNoteSlot[] bassSlots;

    private BassNoteSlot[] FindBassSlots()
    {
        if (bassSlots != null && bassSlots.Length > 0)
        {
            return bassSlots;
        }

        // 既存の BassSlots → MeasureSlot → Slot1 構造をそのまま利用。
        GameObject container = GameObject.Find("BassSlots");
        if (container == null)
        {
            return new BassNoteSlot[0];
        }
        return container.GetComponentsInChildren<BassNoteSlot>(true);
    }

    public void Play()
    {
        if (progressPlayer == null)
        {
            return;
        }

        if (measures == null || measures.Length == 0)
        {
            Debug.LogWarning("耳コピ：小節が設定されていません。", this);
            return;
        }

        BassNoteSlot[] bass = FindBassSlots();
        List<PlaybackItem> items = new List<PlaybackItem>();

        for (int measureIndex = 0; measureIndex < measures.Length; measureIndex++)
        {
            MeasureSlot measure = measures[measureIndex];
            if (measure == null)
            {
                continue;
            }

            BassNoteSlot bassSlot = measureIndex < bass.Length
                ? bass[measureIndex]
                : null;

            List<EarCopySlot> filledSlots = new List<EarCopySlot>();
            EarCopySlot[] slots = measure.GetSlots();
            if (slots != null)
            {
                foreach (EarCopySlot slot in slots)
                {
                    if (slot != null && slot.HasChord)
                    {
                        filledSlots.Add(slot);
                    }
                }
            }

            if (filledSlots.Count == 0)
            {
                // コードなしでも、ベースだけの小節を鳴らせる。
                if (bassSlot != null && bassSlot.HasNote)
                {
                    items.Add(new PlaybackItem
                    {
                        bassNoteSlot = bassSlot,
                        bassAtBarStart = true,
                        bassOnly = true,
                        barFraction = 1f
                    });
                }
                continue;
            }

            float fraction = 1f / filledSlots.Count;
            for (int i = 0; i < filledSlots.Count; i++)
            {
                EarCopySlot slot = filledSlots[i];
                items.Add(new PlaybackItem
                {
                    chordData = slot.ChordData,
                    chordButton = null,
                    earCopySlot = slot,
                    barFraction = fraction,
                    bassNoteSlot = bassSlot,
                    bassAtBarStart = i == 0,
                    bassOnly = false
                });
            }
        }

        if (items.Count == 0)
        {
            Debug.Log("耳コピ：コードもベース音もありません。", this);
            return;
        }

        progressPlayer.Play(items.ToArray());
    }
}
