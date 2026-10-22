using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonIconSwitcher : MonoBehaviour
{
    [Header("画像を表示する対象")]
    [SerializeField]
    private Image targetImage;

    [Header("切り替える画像")]
    [SerializeField]
    private Sprite[] icons;

    [SerializeField]
    private int currentIndex;

    [Header("数字表示")]
    [SerializeField]
    private TMP_Text countText;

    [SerializeField]
    private string[] labels;

    private void Start()
    {
        RefreshVisual();
    }

    public void Next()
    {
        if (icons == null ||
            icons.Length == 0)
        {
            return;
        }

        currentIndex++;

        if (currentIndex >= icons.Length)
        {
            currentIndex = 0;
        }

        RefreshVisual();
    }

    public void SetIndex(int index)
    {
        if (icons == null ||
            icons.Length == 0)
        {
            return;
        }

        currentIndex =
            Mathf.Clamp(
                index,
                0,
                icons.Length - 1
            );

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (targetImage == null ||
            icons == null ||
            icons.Length == 0)
        {
            return;
        }

        targetImage.sprite =
            icons[currentIndex];

        if (
            countText != null &&
            labels != null &&
            currentIndex >= 0 &&
            currentIndex < labels.Length
        )
        {
            countText.text =
                labels[currentIndex];
        }
    }
}