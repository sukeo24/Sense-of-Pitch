using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public class SoundPlayer : MonoBehaviour
{
    [Header("音源：低い音から半音順")]
    [SerializeField]
    private AudioClip[] toneClips;

    [Header("Tone Clipsの最低音番号")]
    [SerializeField]
    private int lowestNoteNumber = 23;

    [Header("鍵盤：Tone Clipsと同じ順番")]
    private PianoKey[] pianoKeys;

    [Header("鍵盤表示用のキー管理")]
    [SerializeField]
    private KeyManager keyManager;

    [Header("鍵盤用Voice数")]
    [Min(8)]
    [SerializeField]
    private int keyboardVoiceCount = 24;

    [SerializeField]
    private AudioMixerGroup pianoMixerGroup;

    private AudioSource[] keyboardSources;
    
    private Coroutine[] keyboardFadeCoroutines;

    private int nextKeyboardVoiceIndex;

    private readonly Dictionary<
        int,
        Stack<int>
    > activeKeyboardVoices =
        new Dictionary<
            int,
            Stack<int>
        >();

#if UNITY_EDITOR
    [ContextMenu("ピアノ音源を自動登録")]
    private void AutoRegisterPianoClips()
    {
        const string folderPath =
            "Assets/ピアノ音源";

        string[] guids =
            AssetDatabase.FindAssets(
                "t:AudioClip Piano_",
                new[] { folderPath }
            );

        toneClips = guids
            .Select(
                AssetDatabase.GUIDToAssetPath
            )
            .Select(
                path =>
                    AssetDatabase
                        .LoadAssetAtPath<AudioClip>(
                            path
                        )
            )
            .Where(clip => clip != null)
            .OrderBy(
                clip =>
                    ExtractNumber(clip.name)
            )
            .ToArray();

        EditorUtility.SetDirty(this);

        Debug.Log(
            $"ピアノ音源を" +
            $"{toneClips.Length}個登録しました。"
        );
    }

    private static int ExtractNumber(
        string clipName)
    {
        string numberPart =
            clipName.Split('_').Last();

        return int.TryParse(
            numberPart,
            out int number
        )
            ? number
            : int.MaxValue;
    }
#endif

    [Header("音を離した後のフェード時間")]
    [SerializeField]
    private float fadeDuration = 0.5f;

    [Header("コード進行のDSP予約用Voice数")]
    [Tooltip("×4コードを1つ先まで予約できるよう、32以上を推奨")]
    [Min(16)]
    [SerializeField]
    private int scheduledVoiceCount = 32;

    [Header("コード刻みのリリース時間")]
    [Tooltip("次の発音開始後も、前の音をこの秒数だけ減衰させる")]
    [Range(0.01f, 0.5f)]
    [SerializeField]
    private float scheduledReleaseTime = 0.15f;

    [Header("停止前の無音時間")]
    [Tooltip("フェード完了後、完全な無音を確認してからAudioSourceを止める")]
    [Range(0f, 0.1f)]
    [SerializeField]
    private float scheduledSilentMargin = 0.03f;

    [Header("停止ボタンのフェード時間")]
    [SerializeField]
    private float stopFadeTime = 0.2f;

    private AudioSource[] audioSources;
    private Coroutine[] fadeCoroutines;
    private int[] activeSoundCounts;

    private AudioSource[] scheduledSources;
    private Coroutine[] scheduledFadeCoroutines;
    private int nextScheduledSourceIndex;

    private readonly Dictionary<
        int,
        Stack<HighlightedKey>
    > highlightedKeysByNote =
        new Dictionary<
            int,
            Stack<HighlightedKey>
        >();

    private class HighlightedKey
    {
        public PianoKey key;
        public bool isRoot;
    }

    private void Awake()
    {
        pianoKeys =
            FindObjectsByType<PianoKey>(
                FindObjectsInactive.Exclude
            );

        int clipCount =
            toneClips != null
                ? toneClips.Length
                : 0;

        keyboardSources =
            new AudioSource[
                keyboardVoiceCount
            ];

        keyboardFadeCoroutines =
            new Coroutine[
                keyboardVoiceCount
            ];

        for (int i = 0;
            i < keyboardSources.Length;
            i++)
        {
            keyboardSources[i] =
                CreateAudioSource();
        }

        scheduledSources =
            new AudioSource[
                scheduledVoiceCount
            ];

        scheduledFadeCoroutines =
            new Coroutine[
                scheduledVoiceCount
            ];

        for (int i = 0;
             i < scheduledSources.Length;
             i++)
        {
            scheduledSources[i] =
                CreateAudioSource();
        }
    }

    private AudioSource CreateAudioSource()
    {
        AudioSource source =
            gameObject
                .AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
        source.outputAudioMixerGroup = pianoMixerGroup;

        return source;
    }

    public void PlaySound(int noteNumber)
    {
        int clipIndex =
            NoteNumberToIndex(
                noteNumber
            );

        if (!IsValidIndex(clipIndex))
        {
            return;
        }

        int voiceIndex =
            GetNextKeyboardVoice();

        AudioSource source =
            keyboardSources[
                voiceIndex
            ];

        if (keyboardFadeCoroutines[
                voiceIndex
            ] != null)
        {
            StopCoroutine(
                keyboardFadeCoroutines[
                    voiceIndex
                ]
            );

            keyboardFadeCoroutines[
                voiceIndex
            ] = null;
        }

        source.Stop();
        source.clip =
            toneClips[clipIndex];
        source.volume = 1f;
        source.Play();

        if (!activeKeyboardVoices
                .TryGetValue(
                    noteNumber,
                    out Stack<int> voices
                ))
        {
            voices =
                new Stack<int>();

            activeKeyboardVoices.Add(
                noteNumber,
                voices
            );
        }

        voices.Push(
            voiceIndex
        );
    }

    public void StopSound(int noteNumber)
    {
        if (!activeKeyboardVoices
                .TryGetValue(
                    noteNumber,
                    out Stack<int> voices
                ) ||
            voices.Count == 0)
        {
            return;
        }

        int voiceIndex =
            voices.Pop();

        if (voices.Count == 0)
        {
            activeKeyboardVoices.Remove(
                noteNumber
            );
        }

        AudioSource source =
            keyboardSources[
                voiceIndex
            ];

        if (source == null ||
            !source.isPlaying)
        {
            return;
        }

        if (keyboardFadeCoroutines[
                voiceIndex
            ] != null)
        {
            StopCoroutine(
                keyboardFadeCoroutines[
                    voiceIndex
                ]
            );
        }

        keyboardFadeCoroutines[
            voiceIndex
        ] =
            StartCoroutine(
                FadeKeyboardVoice(
                    voiceIndex,
                    fadeDuration
                )
            );
    }

    public void PlayChord(
        int[] notes,
        int rootNoteNumber)
    {
        if (notes == null ||
            notes.Length == 0)
        {
            return;
        }

        foreach (int noteNumber
                 in notes)
        {
            PlaySound(noteNumber);
        }

        ShowChordHighlight(
            notes,
            rootNoteNumber
        );
    }

    private int GetNextKeyboardVoice()
    {
        int voiceIndex =
            nextKeyboardVoiceIndex;

        nextKeyboardVoiceIndex =
            (nextKeyboardVoiceIndex + 1) %
            keyboardSources.Length;

        return voiceIndex;
    }

    private IEnumerator FadeKeyboardVoice(
        int voiceIndex,
        float duration)
    {
        AudioSource source =
            keyboardSources[
                voiceIndex
            ];

        if (source == null)
        {
            keyboardFadeCoroutines[
                voiceIndex
            ] = null;

            yield break;
        }

        float startVolume =
            source.volume;

        if (duration <= 0f)
        {
            source.Stop();
            source.volume = 1f;

            keyboardFadeCoroutines[
                voiceIndex
            ] = null;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.deltaTime;

            source.volume =
                Mathf.Lerp(
                    startVolume,
                    0f,
                    elapsed / duration
                );

            yield return null;
        }

        source.Stop();
        source.volume = 1f;

        keyboardFadeCoroutines[
            voiceIndex
        ] = null;
    }

    public void StopChord(
        int[] notes,
        int rootNoteNumber)
    {
        if (notes == null ||
            notes.Length == 0)
        {
            return;
        }

        foreach (int noteNumber
                 in notes)
        {
            StopSound(noteNumber);
        }

        HideChordHighlight(
            notes,
            rootNoteNumber
        );
    }

    public void ScheduleChord(
        int[] notes,
        double startDspTime,
        double releaseStartDspTime)
    {
        if (notes == null ||
            notes.Length == 0)
        {
            return;
        }

        foreach (int noteNumber
                 in notes)
        {
            int clipIndex =
                NoteNumberToIndex(
                    noteNumber
                );

            if (!IsValidIndex(clipIndex))
            {
                continue;
            }

            AudioSource source =
                GetNextScheduledSource(
                    out int sourceIndex
                );

            if (scheduledFadeCoroutines[
                    sourceIndex
                ] != null)
            {
                StopCoroutine(
                    scheduledFadeCoroutines[
                        sourceIndex
                    ]
                );

                scheduledFadeCoroutines[
                    sourceIndex
                ] = null;
            }

            source.Stop();
            source.clip =
                toneClips[clipIndex];
            source.volume = 1f;

            source.PlayScheduled(
                startDspTime
            );

            scheduledFadeCoroutines[
                sourceIndex
            ] =
                StartCoroutine(
                    FadeScheduledSource(
                        source,
                        sourceIndex,
                        releaseStartDspTime,
                        scheduledReleaseTime,
                        scheduledSilentMargin
                    )
                );
        }
    }

    public void StopAllScheduled()
    {
        if (scheduledSources == null)
        {
            return;
        }

        for (int i = 0;
             i < scheduledSources.Length;
             i++)
        {
            if (scheduledFadeCoroutines != null &&
                scheduledFadeCoroutines[i] != null)
            {
                StopCoroutine(
                    scheduledFadeCoroutines[i]
                );

                scheduledFadeCoroutines[i] =
                    null;
            }

            AudioSource source =
                scheduledSources[i];

            if (source != null)
            {
                StartCoroutine(
                    FadeStopScheduledSource(
                        source,
                        stopFadeTime
                    )
                );
            }
        }
    }

    private AudioSource
        GetNextScheduledSource(
            out int sourceIndex)
    {
        sourceIndex =
            nextScheduledSourceIndex;

        AudioSource source =
            scheduledSources[
                sourceIndex
            ];

        nextScheduledSourceIndex =
            (nextScheduledSourceIndex + 1) %
            scheduledSources.Length;

        return source;
    }

    public void ShowChordHighlight(
        int[] notes,
        int rootNoteNumber)
    {
        if (notes == null ||
            notes.Length == 0)
        {
            return;
        }

        foreach (int noteNumber
                 in notes)
        {
            PianoKey key =
                GetPianoKey(
                    noteNumber
                );

            if (key == null)
            {
                continue;
            }

            bool isRoot =
                noteNumber ==
                rootNoteNumber;

            key.AddChordHighlight(
                isRoot
            );

            if (!highlightedKeysByNote
                    .TryGetValue(
                        noteNumber,
                        out Stack<
                            HighlightedKey
                        > keyStack
                    ))
            {
                keyStack =
                    new Stack<
                        HighlightedKey
                    >();

                highlightedKeysByNote.Add(
                    noteNumber,
                    keyStack
                );
            }

            keyStack.Push(
                new HighlightedKey
                {
                    key = key,
                    isRoot = isRoot
                }
            );
        }
    }

    public void HideChordHighlight(
        int[] notes,
        int rootNoteNumber)
    {
        if (notes == null ||
            notes.Length == 0)
        {
            return;
        }

        foreach (int noteNumber
                 in notes)
        {
            if (!highlightedKeysByNote
                    .TryGetValue(
                        noteNumber,
                        out Stack<
                            HighlightedKey
                        > keyStack
                    ) ||
                keyStack.Count == 0)
            {
                continue;
            }

            HighlightedKey highlightedKey =
                keyStack.Pop();

            if (highlightedKey.key != null)
            {
                highlightedKey.key
                    .RemoveChordHighlight(
                        highlightedKey.isRoot
                    );
            }

            if (keyStack.Count == 0)
            {
                highlightedKeysByNote.Remove(
                    noteNumber
                );
            }
        }
    }

    private PianoKey GetPianoKey(
        int soundingNoteNumber)
    {
        if (pianoKeys == null)
        {
            return null;
        }

        int displayedNoteNumber =
            soundingNoteNumber;

        if (keyManager != null &&
            keyManager.TransposeKeyboard)
        {
            displayedNoteNumber -=
                keyManager
                    .TransposeSemitones;
        }

        foreach (PianoKey key
                 in pianoKeys)
        {
            if (key != null &&
                key.NoteNumber ==
                displayedNoteNumber)
            {
                return key;
            }
        }

        return null;
    }

    private IEnumerator FadeScheduledSource(
        AudioSource source,
        int sourceIndex,
        double releaseStartDspTime,
        float releaseTime,
        float silentMargin)
    {
        double fadeEndDspTime =
            releaseStartDspTime +
            releaseTime;

        double stopDspTime =
            fadeEndDspTime +
            silentMargin;

        while (
            AudioSettings.dspTime <
            releaseStartDspTime
        )
        {
            yield return null;
        }

        while (
            AudioSettings.dspTime <
            fadeEndDspTime
        )
        {
            double elapsed =
                AudioSettings.dspTime -
                releaseStartDspTime;

            float t =
                Mathf.Clamp01(
                    (float)(
                        elapsed /
                        releaseTime
                    )
                );

            source.volume =
                Mathf.Lerp(
                    1f,
                    0f,
                    t
                );

            yield return null;
        }

        source.volume = 0f;

        while (
            AudioSettings.dspTime <
            stopDspTime
        )
        {
            yield return null;
        }

        source.Stop();
        source.volume = 1f;

        scheduledFadeCoroutines[
            sourceIndex
        ] = null;
    }

    private IEnumerator FadeOutAndStop(
        int index,
        float duration)
    {
        AudioSource source =
            audioSources[index];

        if (source == null)
        {
            fadeCoroutines[index] =
                null;

            yield break;
        }

        float startVolume =
            source.volume;

        if (duration <= 0f)
        {
            source.Stop();
            source.volume = 1f;

            fadeCoroutines[index] =
                null;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (source == null)
            {
                fadeCoroutines[index] =
                    null;

                yield break;
            }

            if (activeSoundCounts[index] > 0)
            {
                source.volume = 1f;

                fadeCoroutines[index] =
                    null;

                yield break;
            }

            elapsed +=
                Time.deltaTime;

            source.volume =
                Mathf.Lerp(
                    startVolume,
                    0f,
                    elapsed / duration
                );

            yield return null;
        }

        source.Stop();
        source.volume = 1f;

        fadeCoroutines[index] =
            null;
    }

    private IEnumerator FadeStopScheduledSource(
        AudioSource source,
        float duration)
    {
        float startVolume =
            source.volume;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            source.volume =
                Mathf.Lerp(
                    startVolume,
                    0f,
                    elapsed / duration
                );

            yield return null;
        }

        source.Stop();
        source.volume = 1f;
    }

    public void StopAllScheduledImmediately()
    {
        if (scheduledSources == null)
        {
            return;
        }

        for (int i = 0;
            i < scheduledSources.Length;
            i++)
        {
            if (scheduledFadeCoroutines != null &&
                scheduledFadeCoroutines[i] != null)
            {
                StopCoroutine(
                    scheduledFadeCoroutines[i]
                );

                scheduledFadeCoroutines[i] = null;
            }

            AudioSource source =
                scheduledSources[i];

            if (source == null)
            {
                continue;
            }

            source.Stop();
            source.volume = 1f;
        }
    }

    private bool IsValidIndex(int index)
    {
        return toneClips != null &&
               index >= 0 &&
               index < toneClips.Length;
    }

    private int NoteNumberToIndex(
        int noteNumber)
    {
        return noteNumber -
               lowestNoteNumber;
    }
}
