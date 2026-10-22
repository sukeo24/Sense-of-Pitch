using TMPro;
using UnityEngine;

public class KeyboardTouchModeToggleUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private KeyboardTouchModeManager modeManager;

    [SerializeField]
    private TMP_Text displayText;

    [Header("表示")]
    [SerializeField]
    private string glissandoText = "グリッサンド";

    [SerializeField]
    private string scrollText = "スクロール";

    private void Start()
    {
        RefreshVisual();
    }

    public void Toggle()
    {
        if (modeManager == null)
        {
            return;
        }

        if (modeManager.CurrentMode ==
            KeyboardTouchMode.Glissando)
        {
            modeManager.SetScrollMode();
        }
        else
        {
            modeManager.SetGlissandoMode();
        }

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (modeManager == null ||
            displayText == null)
        {
            return;
        }

        displayText.text =
            modeManager.CurrentMode ==
            KeyboardTouchMode.Glissando
                ? glissandoText
                : scrollText;
    }
}