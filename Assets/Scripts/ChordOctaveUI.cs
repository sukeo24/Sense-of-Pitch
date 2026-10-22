using TMPro;
using UnityEngine;

public class ChordOctaveUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private ChordRegisterManager registerManager;

    [SerializeField]
    private TMP_Text octaveText;

    private void Start()
    {
        RefreshText();
    }

    public void OctaveDown()
    {
        if (registerManager == null)
        {
            return;
        }

        registerManager.OctaveDown();
        RefreshText();
    }

    public void OctaveUp()
    {
        if (registerManager == null)
        {
            return;
        }

        registerManager.OctaveUp();
        RefreshText();
    }

    private void RefreshText()
    {
        if (registerManager == null ||
            octaveText == null)
        {
            return;
        }

        octaveText.text =
            $"Octave {registerManager.CurrentOctave}";
    }
}