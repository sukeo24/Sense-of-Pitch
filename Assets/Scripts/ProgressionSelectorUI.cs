using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProgressionSelectorUI : MonoBehaviour
{
    [System.Serializable]
    public class ProgressionEntry
    {
        [Header("表示")]
        public string progressionName;

        public string romanNumerals;

        [Header("コードボタン表示")]
        public string[] chordLabels;

        [Header("旧UIのChordButtons")]
        public Transform templateChordButtons;
    }

    [Header("選択パネル")]
    [SerializeField]
    private GameObject progressionSelectOverlay;

    [Header("現在の進行")]
    [SerializeField]
    private TMP_Text currentNameText;

    [SerializeField]
    private TMP_Text romanNumeralsText;

    [SerializeField]
    private Transform currentChordButtons;

    [Header("再生")]
    [SerializeField]
    private ProgressPlayer progressPlayer;

    [Header("進行一覧")]
    [SerializeField]
    private ProgressionEntry[] progressions;

    [Header("初期進行")]
    [SerializeField]
    private int initialProgressionIndex = 0;


    private void Start()
    {
        if (progressionSelectOverlay != null)
        {
            progressionSelectOverlay.SetActive(false);
        }

        SelectProgression(
            initialProgressionIndex
        );
    }

    public void Open()
    {
        if (progressionSelectOverlay != null)
        {
            progressionSelectOverlay.SetActive(true);
        }
    }

    public void Close()
    {
        if (progressionSelectOverlay != null)
        {
            progressionSelectOverlay.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (progressionSelectOverlay == null)
        {
            return;
        }

        progressionSelectOverlay.SetActive(
            !progressionSelectOverlay.activeSelf
        );
    }

    public void SelectProgression(int index)
    {
        if (progressions == null ||
            index < 0 ||
            index >= progressions.Length)
        {
            return;
        }

        ProgressionEntry entry =
            progressions[index];

        if (entry == null ||
            entry.templateChordButtons == null)
        {
            return;
        }


        // 名前変更
        if (currentNameText != null)
        {
            currentNameText.text =
                entry.progressionName;
        }

        // ローマ数字変更
        if (romanNumeralsText != null)
        {
            romanNumeralsText.text =
                entry.romanNumerals;
        }

        ClearCurrentChordButtons();

        PlaybackItem[] newPlaybackItems =
            CreateChordButtons(entry);

        if (progressPlayer != null)
        {
            progressPlayer.ChangeProgression(
                newPlaybackItems
            );
        }

    }

    private void ClearCurrentChordButtons()
    {
        if (currentChordButtons == null)
        {
            return;
        }

        for (int i =
                 currentChordButtons.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                currentChordButtons.GetChild(i);

            // Playボタンなどは残して、
            // ChordButtonだけ消す
            if (child.GetComponent<ChordButton>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private PlaybackItem[] CreateChordButtons(
    ProgressionEntry entry)
    {
        if (currentChordButtons == null ||
            entry.templateChordButtons == null)
        {
            return System.Array.Empty<PlaybackItem>();
        }

        List<PlaybackItem> playbackItems =
            new List<PlaybackItem>();

        int chordIndex = 0;

        for (int i = 0;
             i < entry.templateChordButtons.childCount;
             i++)
        {
            Transform template =
                entry.templateChordButtons.GetChild(i);

            ChordButton templateChordButton =
                template.GetComponent<ChordButton>();

            if (templateChordButton == null)
            {
                continue;
            }

            GameObject clone =
                Instantiate(
                    template.gameObject,
                    currentChordButtons
                );

            clone.SetActive(true);

            ChordButton newChordButton =
                clone.GetComponent<ChordButton>();

            if (entry.chordLabels != null &&
                chordIndex < entry.chordLabels.Length)
            {
                TMP_Text label =
                    clone.GetComponentInChildren<TMP_Text>(
                        true
                    );

                if (label != null)
                {
                    label.text =
                        entry.chordLabels[chordIndex];
                }
            }

            if (newChordButton != null)
            {
                playbackItems.Add(
                    new PlaybackItem
                    {
                        chordData =
                            newChordButton.GetChordData(),

                        chordButton =
                            newChordButton
                    }
                );
            }

            chordIndex++;
        }

        return playbackItems.ToArray();
    }
}