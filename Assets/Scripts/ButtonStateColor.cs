using UnityEngine;
using UnityEngine.UI;

public class ButtonStateColor : MonoBehaviour
{
    [Header("色を変える対象")]
    [SerializeField]
    private Graphic targetGraphic;

    [Header("Indexごとの色")]
    [SerializeField]
    private Color[] colors;

    [Header("初期Index")]
    [SerializeField]
    private int currentIndex = 0;

    public int CurrentIndex => currentIndex;

    private void Awake()
    {
        if (targetGraphic == null)
        {
            targetGraphic =
                GetComponent<Graphic>();
        }

        RefreshVisual();
    }

    /// <summary>
    /// 次の色へ進む。
    /// 最後まで行ったら0へ戻る。
    /// </summary>
    public void Next()
    {
        if (colors == null ||
            colors.Length == 0)
        {
            return;
        }

        currentIndex =
            (currentIndex + 1) %
            colors.Length;

        RefreshVisual();
    }

    /// <summary>
    /// 指定したIndexの色にする。
    /// </summary>
    public void SetIndex(int index)
    {
        if (colors == null ||
            colors.Length == 0)
        {
            return;
        }

        if (index < 0 ||
            index >= colors.Length)
        {
            Debug.LogWarning(
                $"{gameObject.name}: " +
                $"色Index {index} は範囲外です。",
                this
            );

            return;
        }

        currentIndex = index;

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (targetGraphic == null ||
            colors == null ||
            colors.Length == 0)
        {
            return;
        }

        currentIndex =
            Mathf.Clamp(
                currentIndex,
                0,
                colors.Length - 1
            );

        targetGraphic.color =
            colors[currentIndex];
    }
}