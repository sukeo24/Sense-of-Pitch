using TMPro;
using UnityEngine;

public class LabelModeToggleUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private KeyboardLabelManager keyboardLabelManager;

    [SerializeField]
    private TMP_Text displayText;

    [Header("表示文字")]
    [SerializeField]
    private string noteNameText = "音名 / C";

    [SerializeField]
    private string solfegeText = "階名 / ド";

    private void Start()
    {
        RefreshVisual();
    }

    public void Toggle()
    {
        if (keyboardLabelManager == null)
        {
            return;
        }

        keyboardLabelManager.ToggleLabelMode();

        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (keyboardLabelManager == null ||
            displayText == null)
        {
            return;
        }

        bool isSolfege =
            keyboardLabelManager.CurrentMode ==
            KeyboardLabelManager.LabelMode.Solfege;

        displayText.text =
            isSolfege
                ? solfegeText
                : noteNameText;
    }
}