using System.Collections;
using UnityEngine;

public class ProgressionDrawerUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private GameObject progressionOverlay;

    [SerializeField]
    private RectTransform overlayRect;

    [Header("アニメーション")]
    [SerializeField]
    private float animationDuration = 0.2f;

    [Tooltip("開いたときのY位置")]
    [SerializeField]
    private float openY = 0f;

    private Coroutine animationCoroutine;

    private float overlayHeight;

    private void Awake()
    {
        if (overlayRect != null)
        {
            overlayHeight =
                overlayRect.rect.height;
        }

        if (progressionOverlay != null)
        {
            progressionOverlay.SetActive(false);
        }
    }

    public void Open()
    {
        if (progressionOverlay == null ||
            overlayRect == null)
        {
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        progressionOverlay.SetActive(true);

        overlayHeight =
            overlayRect.rect.height;

        overlayRect.anchoredPosition =
            new Vector2(
                overlayRect.anchoredPosition.x,
                -overlayHeight
            );

        animationCoroutine =
            StartCoroutine(
                AnimateOverlay(
                    -overlayHeight,
                    openY,
                    false
                )
            );
    }

    public void Close()
    {
        if (progressionOverlay == null ||
            overlayRect == null ||
            !progressionOverlay.activeSelf)
        {
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        overlayHeight =
            overlayRect.rect.height;

        animationCoroutine =
            StartCoroutine(
                AnimateOverlay(
                    overlayRect.anchoredPosition.y,
                    -overlayHeight,
                    true
                )
            );
    }

    public void Toggle()
    {
        if (progressionOverlay != null &&
            progressionOverlay.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private IEnumerator AnimateOverlay(
        float startY,
        float endY,
        bool hideWhenFinished)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    animationDuration
                );

            t =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            Vector2 position =
                overlayRect.anchoredPosition;

            position.y =
                Mathf.Lerp(
                    startY,
                    endY,
                    t
                );

            overlayRect.anchoredPosition =
                position;

            yield return null;
        }

        Vector2 finalPosition =
            overlayRect.anchoredPosition;

        finalPosition.y = endY;

        overlayRect.anchoredPosition =
            finalPosition;

        animationCoroutine = null;

        if (hideWhenFinished &&
            progressionOverlay != null)
        {
            progressionOverlay.SetActive(false);
        }
    }
}