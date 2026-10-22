using TMPro;
using UnityEngine;

public class ModeManager : MonoBehaviour
{
    public enum AppMode
    {
        ChordProgression,
        EarCopy,
        BassExplorer
    }

    [Header("現在のモード")]
    [SerializeField]
    private AppMode currentMode =
        AppMode.ChordProgression;

    [Header("TopBarのモード表示")]
    [SerializeField]
    private TMP_Text modeText;

    [Header("各モードのUI")]
    [SerializeField]
    private GameObject chordProgressionUI;

    [SerializeField]
    private GameObject earCopyUI;

    public AppMode CurrentMode =>
        currentMode;

    private BassExplorerUI bassExplorer;

    private void Start()
    {
        bassExplorer = BassExplorerUI.Create(modeText != null ? modeText.font : TMP_Settings.defaultFontAsset, ToggleMode);
        ApplyMode();
    }

    public void ToggleMode()
    {
        currentMode = (AppMode)(((int)currentMode + 1) % 3);

        ApplyMode();
    }

    private void OnDestroy()
    {
        if (bassExplorer != null) Destroy(bassExplorer.gameObject);
    }

    private void ApplyMode()
    {
        bool isChordProgression =
            currentMode ==
            AppMode.ChordProgression;

        if (chordProgressionUI != null)
        {
            chordProgressionUI.SetActive(
                isChordProgression
            );
        }

        if (earCopyUI != null)
        {
            earCopyUI.SetActive(
                currentMode == AppMode.EarCopy
            );
        }

        if (bassExplorer != null)
        {
            bool isBass = currentMode == AppMode.BassExplorer;
            if (isBass)
            {
                var player = FindFirstObjectByType<ProgressPlayer>();
                if (player != null) player.Stop();
            }
            bassExplorer.gameObject.SetActive(isBass);
        }

        if (modeText != null)
        {
            modeText.text =
                isChordProgression
                    ? "コード進行"
                    : currentMode == AppMode.EarCopy ? "耳コピ" : "ベース";
        }
    }
}