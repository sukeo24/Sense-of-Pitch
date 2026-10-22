using TMPro;
using UnityEngine;

public class TransposeToggleUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private KeyManager keyManager;

    [SerializeField]
    private TMP_Text displayText;

    [Header("表示")]
    [SerializeField]
    private string onColorCode = "#5CFF72";

    private void OnEnable()
    {
        if (keyManager != null)
        {
            keyManager.KeyboardTransposeChanged +=
                RefreshVisual;
        }
    }

    private void Start()
    {
        RefreshVisual();
    }

    private void OnDisable()
    {
        if (keyManager != null)
        {
            keyManager.KeyboardTransposeChanged -=
                RefreshVisual;
        }
    }

    public void Toggle()
    {
        if (keyManager == null)
        {
            return;
        }

        keyManager.SetTransposeKeyboard(
            !keyManager.TransposeKeyboard
        );
    }

    public void RefreshVisual()
    {
        if (keyManager == null ||
            displayText == null)
        {
            return;
        }

        displayText.richText = true;

        if (keyManager.TransposeKeyboard)
        {
            displayText.text =
                $"移調 <color={onColorCode}>ON</color>";
        }
        else
        {
            displayText.text =
                "移調 OFF";
        }
    }
}