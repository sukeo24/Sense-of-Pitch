using UnityEngine;

public class KeyboardTouchModeManager : MonoBehaviour
{
    public static KeyboardTouchModeManager Instance { get; private set; }

    [SerializeField]
    private KeyboardTouchMode currentMode = KeyboardTouchMode.Glissando;

    public KeyboardTouchMode CurrentMode => currentMode;

    private void Awake()
    {
        Instance = this;
    }

    public void SetGlissandoMode()
    {
        currentMode = KeyboardTouchMode.Glissando;
    }

    public void SetScrollMode()
    {
        currentMode = KeyboardTouchMode.Scroll;
    }
}