using TMPro;
using UnityEngine;

public class KeyChangeUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private KeyManager keyManager;

    [SerializeField]
    private TMP_Text keyText;

    [Header("表示")]
    [SerializeField]
    private string prefix = "Key : ";

    private static readonly string[] KeyNames =
    {
        "C",
        "C♯ / D♭",
        "D",
        "D♯ / E♭",
        "E",
        "F",
        "F♯ / G♭",
        "G",
        "G♯ / A♭",
        "A",
        "A♯ / B♭",
        "B"
    };

    private void OnEnable()
    {
        if (keyManager != null)
        {
            keyManager.KeyChanged +=
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
            keyManager.KeyChanged -=
                RefreshVisual;
        }
    }

    public void PreviousKey()
    {
        if (keyManager == null)
        {
            return;
        }

        keyManager.PreviousKey();
    }

    public void NextKey()
    {
        if (keyManager == null)
        {
            return;
        }

        keyManager.NextKey();
    }

    private void RefreshVisual()
    {
        if (keyManager == null ||
            keyText == null)
        {
            return;
        }

        int keyIndex =
            keyManager.CurrentKeyPitchClass;

        if (keyIndex < 0 ||
            keyIndex >= KeyNames.Length)
        {
            return;
        }

        keyText.text =
            prefix +
            KeyNames[keyIndex];
    }
}