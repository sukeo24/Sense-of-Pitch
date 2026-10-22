using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class KeySelectButton : MonoBehaviour
{
    [Header("このボタンのキー")]
    [Range(0, 11)]
    [SerializeField]
    private int keyPitchClass;

    [Header("参照")]
    [SerializeField]
    private KeyManager keyManager;

    [Header("色")]
    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color selectedColor =
        new Color(0.35f, 0.75f, 1f, 1f);

    private Image buttonImage;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
    }

    private void Start()
    {
        RefreshVisual();
    }

    public void SelectKey()
    {
        if (keyManager == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: KeyManagerが未設定です。",
                this
            );

            return;
        }

        keyManager.SetKey(keyPitchClass);

        KeySelectButton[] keyButtons =
            FindObjectsByType<KeySelectButton>(
            );

        foreach (KeySelectButton keyButton in keyButtons)
        {
            keyButton.RefreshVisual();
        }
    }

    public void RefreshVisual()
    {
        if (buttonImage == null ||
            keyManager == null)
        {
            return;
        }

        bool isSelected =
            keyManager.CurrentKeyPitchClass ==
            keyPitchClass;

        buttonImage.color =
            isSelected
                ? selectedColor
                : normalColor;
    }
}