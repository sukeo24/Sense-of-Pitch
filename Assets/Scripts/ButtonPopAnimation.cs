using System.Collections;
using UnityEngine;

public class ButtonPopAnimation : MonoBehaviour
{
    [Header("拡大対象")]
    [SerializeField]
    private RectTransform target;

    [Header("アニメーション設定")]
    [SerializeField]
    private float pressedScale = 1.15f;

    [SerializeField]
    private float duration = 0.08f;

    private Vector3 defaultScale;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (target == null)
        {
            target = transform as RectTransform;
        }

        defaultScale = target.localScale;
    }

    public void Play()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine =
            StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        Vector3 enlargedScale =
            defaultScale * pressedScale;

        float halfDuration =
            duration * 0.5f;

        yield return AnimateScale(
            defaultScale,
            enlargedScale,
            halfDuration
        );

        yield return AnimateScale(
            enlargedScale,
            defaultScale,
            halfDuration
        );

        target.localScale = defaultScale;

        animationCoroutine = null;
    }

    private IEnumerator AnimateScale(
        Vector3 startScale,
        Vector3 endScale,
        float animationDuration)
    {
        if (animationDuration <= 0f)
        {
            target.localScale = endScale;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / animationDuration
                );

            target.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    t
                );

            yield return null;
        }

        target.localScale = endScale;
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (target != null)
        {
            target.localScale = defaultScale;
        }
    }
}