using System.Collections.Generic;
using UnityEngine;

public class ProgressionPlayButton : MonoBehaviour
{
    [Header("再生担当")]
    [SerializeField]
    private ProgressPlayer progressPlayer;

    [Header("コードカードを並べている親")]
    [SerializeField]
    private Transform chordButtonsContainer;

    public PlaybackItem[] GetPlaybackItems()
    {
        if (chordButtonsContainer == null)
        {
            return System.Array.Empty<PlaybackItem>();
        }

        List<PlaybackItem> playbackItems =
            new List<PlaybackItem>();

        for (int i = 0;
             i < chordButtonsContainer.childCount;
             i++)
        {
            Transform child =
                chordButtonsContainer.GetChild(i);


            ChordButton chordButton =
                child.GetComponent<ChordButton>();

            if (chordButton == null)
            {
                continue;
            }

            PlaybackItem item =
                new PlaybackItem
                {
                    chordData =
                        chordButton.GetChordData(),

                    chordButton =
                        chordButton
                };

            playbackItems.Add(item);
        }

        return playbackItems.ToArray();
    }

    /// <summary>
    /// 現在のHierarchy上の並び順でコードを再生する。
    /// </summary>
    public void Play()
    {
        if (progressPlayer == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: ProgressPlayerが未設定です。",
                this
            );

            return;
        }

        if (chordButtonsContainer == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: " +
                "ChordButtonsContainerが未設定です。",
                this
            );

            return;
        }

        List<PlaybackItem> playbackItems =
            new List<PlaybackItem>();

        for (
            int i = 0;
            i < chordButtonsContainer.childCount;
            i++
        )
        {
            Transform child =
                chordButtonsContainer.GetChild(i);

            ChordButton chordButton =
                child.GetComponent<ChordButton>();

            if (chordButton == null)
            {
                continue;
            }

            PlaybackItem item =
                new PlaybackItem
                {
                    chordData =
                        chordButton.GetChordData(),

                    chordButton =
                        chordButton
                };

            playbackItems.Add(item);
        }

        if (playbackItems.Count == 0)
        {
            Debug.LogWarning(
                $"{gameObject.name}: " +
                "再生できるコードがありません。",
                this
            );

            return;
        }

        progressPlayer.Play(
            playbackItems.ToArray()
        );
    }
}