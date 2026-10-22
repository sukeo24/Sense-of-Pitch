using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class KeyboardNavigator : MonoBehaviour
{
    [Header("起動時に左端へ表示する音")]
    [SerializeField]
    private int defaultLeftNoteNumber = 48; // C3

    [Header("移動対象")]
    [SerializeField]
    private ScrollRect scrollRect;

    [Header("移動アニメーション")]
    [SerializeField]
    [Min(0f)]
    private float moveDuration = 0.2f;

    [Header("自動計算できない場合の1オクターブ幅")]
    [SerializeField]
    [Min(1f)]
    private float fallbackOctaveWidth = 900f;

    private float octaveWidth;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }

    private IEnumerator Start()
    {
        // Layout Groupなどによる鍵盤配置が終わるまで待つ
        yield return null;

        Canvas.ForceUpdateCanvases();

        octaveWidth = CalculateOctaveWidth();

        JumpToNote(defaultLeftNoteNumber);
    }

    private void JumpToNote(int noteNumber)
    {
        if (scrollRect == null ||
            scrollRect.content == null ||
            scrollRect.viewport == null)
        {
            Debug.LogWarning(
                "Scroll Rect、Content、Viewportを確認してください。",
                this
            );
            return;
        }

        PianoKey[] keys =
            scrollRect.content.GetComponentsInChildren<PianoKey>(false);

        PianoKey targetKey = null;

        foreach (PianoKey key in keys)
        {
            if (key != null && key.NoteNumber == noteNumber)
            {
                targetKey = key;
                break;
            }
        }

        if (targetKey == null)
        {
            Debug.LogWarning(
                $"Note Number {noteNumber} の鍵盤が見つかりません。",
                this
            );
            return;
        }

        RectTransform targetRect =
            targetKey.GetComponent<RectTransform>();

        if (targetRect == null)
        {
            return;
        }

        float scrollableWidth =
            scrollRect.content.rect.width -
            scrollRect.viewport.rect.width;

        if (scrollableWidth <= 0f)
        {
            return;
        }

        // 対象鍵盤の左端をContent内の座標へ変換
        Vector3 targetLeftWorld =
            targetRect.TransformPoint(
                new Vector3(
                    targetRect.rect.xMin,
                    targetRect.rect.center.y,
                    0f
                )
            );

        Vector3 targetLeftLocal =
            scrollRect.content.InverseTransformPoint(
                targetLeftWorld
            );

        // Content左端から対象鍵盤左端までの距離
        float distanceFromContentLeft =
            targetLeftLocal.x -
            scrollRect.content.rect.xMin;

        scrollRect.horizontalNormalizedPosition =
            Mathf.Clamp01(
                distanceFromContentLeft / scrollableWidth
            );

        scrollRect.velocity = Vector2.zero;
    }

    /// <summary>
    /// 1オクターブ左へ移動
    /// </summary>
    public void MoveLeftOctave()
    {
        MoveByOctaves(-1);
    }

    /// <summary>
    /// 1オクターブ右へ移動
    /// </summary>
    public void MoveRightOctave()
    {
        MoveByOctaves(1);
    }

    private void MoveByOctaves(int octaveCount)
    {
        if (scrollRect == null ||
            scrollRect.content == null ||
            scrollRect.viewport == null)
        {
            Debug.LogWarning(
                "Scroll Rect、Content、Viewportを確認してください。",
                this
            );
            return;
        }

        float scrollableWidth =
            scrollRect.content.rect.width -
            scrollRect.viewport.rect.width;

        if (scrollableWidth <= 0f)
        {
            Debug.LogWarning(
                "Contentの幅がViewport以下なので移動できません。",
                this
            );
            return;
        }

        float normalizedDistance =
            octaveWidth / scrollableWidth;

        float targetPosition = Mathf.Clamp01(
            scrollRect.horizontalNormalizedPosition +
            normalizedDistance * octaveCount
        );

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(
            MoveSmoothly(targetPosition)
        );
    }

    private IEnumerator MoveSmoothly(float targetPosition)
    {
        float startPosition =
            scrollRect.horizontalNormalizedPosition;

        if (moveDuration <= 0f)
        {
            scrollRect.horizontalNormalizedPosition =
                targetPosition;

            moveCoroutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / moveDuration
            );

            // 滑らかに加速・減速
            float easedProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            scrollRect.horizontalNormalizedPosition =
                Mathf.Lerp(
                    startPosition,
                    targetPosition,
                    easedProgress
                );

            yield return null;
        }

        scrollRect.horizontalNormalizedPosition =
            targetPosition;

        moveCoroutine = null;
    }

    /// <summary>
    /// 隣り合うC鍵盤の距離から1オクターブ幅を求める
    /// </summary>
    private float CalculateOctaveWidth()
    {
        if (scrollRect == null ||
            scrollRect.content == null)
        {
            return fallbackOctaveWidth;
        }

        PianoKey[] keys =
            scrollRect.content.GetComponentsInChildren<PianoKey>(
                false
            );

        List<PianoKey> cKeys = new List<PianoKey>();

        foreach (PianoKey key in keys)
        {
            if (key == null)
            {
                continue;
            }

            // MIDI番号を12で割った余りが0ならC
            if (PositiveModulo(key.NoteNumber, 12) == 0)
            {
                cKeys.Add(key);
            }
        }

        cKeys.Sort(
            (left, right) =>
                left.NoteNumber.CompareTo(right.NoteNumber)
        );

        if (cKeys.Count < 2)
        {
            Debug.LogWarning(
                "C鍵盤を2個以上取得できなかったため、" +
                $"仮の幅 {fallbackOctaveWidth} を使用します。",
                this
            );

            return fallbackOctaveWidth;
        }

        RectTransform firstRect =
            cKeys[0].GetComponent<RectTransform>();

        RectTransform secondRect =
            cKeys[1].GetComponent<RectTransform>();

        if (firstRect == null || secondRect == null)
        {
            return fallbackOctaveWidth;
        }

        Vector3 firstWorldPosition =
            firstRect.TransformPoint(firstRect.rect.center);

        Vector3 secondWorldPosition =
            secondRect.TransformPoint(secondRect.rect.center);

        Vector3 firstLocalPosition =
            scrollRect.content.InverseTransformPoint(
                firstWorldPosition
            );

        Vector3 secondLocalPosition =
            scrollRect.content.InverseTransformPoint(
                secondWorldPosition
            );

        float calculatedWidth = Mathf.Abs(
            secondLocalPosition.x -
            firstLocalPosition.x
        );

        return calculatedWidth > 1f
            ? calculatedWidth
            : fallbackOctaveWidth;
    }

    private static int PositiveModulo(int value, int modulo)
    {
        return ((value % modulo) + modulo) % modulo;
    }
}
