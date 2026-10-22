using System.Collections;
using UnityEngine;

public class MenuDrawerUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private GameObject menuOverlay;

    [SerializeField]
    private RectTransform sideMenuPanel;

    [Header("アニメーション")]
    [SerializeField]
    private float animationDuration = 0.2f;

    private Coroutine animationCoroutine;

    private float panelWidth;

    private void Awake()
    {
        if (sideMenuPanel != null)
        {
            panelWidth =
                sideMenuPanel.rect.width;
        }

        if (menuOverlay != null)
        {
            menuOverlay.SetActive(false);
        }
    }

    public void Open()
    {
        if (menuOverlay == null ||
            sideMenuPanel == null)
        {
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        menuOverlay.SetActive(true);

        panelWidth =
            sideMenuPanel.rect.width;

        sideMenuPanel.anchoredPosition =
            new Vector2(
                -panelWidth,
                sideMenuPanel.anchoredPosition.y
            );

        animationCoroutine =
            StartCoroutine(
                AnimatePanel(
                    -panelWidth,
                    0f,
                    false
                )
            );
    }

    public void Close()
    {
        if (menuOverlay == null ||
            sideMenuPanel == null ||
            !menuOverlay.activeSelf)
        {
            return;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        panelWidth =
            sideMenuPanel.rect.width;

        animationCoroutine =
            StartCoroutine(
                AnimatePanel(
                    sideMenuPanel.anchoredPosition.x,
                    -panelWidth,
                    true
                )
            );
    }

    public void Toggle()
    {
        if (menuOverlay != null &&
            menuOverlay.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private IEnumerator AnimatePanel(
        float startX,
        float endX,
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
                sideMenuPanel
                    .anchoredPosition;

            position.x =
                Mathf.Lerp(
                    startX,
                    endX,
                    t
                );

            sideMenuPanel
                .anchoredPosition =
                position;

            yield return null;
        }

        Vector2 finalPosition =
            sideMenuPanel
                .anchoredPosition;

        finalPosition.x = endX;

        sideMenuPanel
            .anchoredPosition =
            finalPosition;

        animationCoroutine = null;

        if (hideWhenFinished &&
            menuOverlay != null)
        {
            menuOverlay.SetActive(false);
        }
    }
}