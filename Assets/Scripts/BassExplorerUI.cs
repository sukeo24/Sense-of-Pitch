using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class BassExplorerUI : MonoBehaviour
{
    private readonly bool[] upperG = { false, true, false, true };
    private readonly string[] bassNames = { "ミ  E", "ファ  F", "ソ  G", "ラ  A" };
    private readonly Image[] pads = new Image[4];
    private readonly Image[,] choices = new Image[4, 2];
    private readonly Color surface = new Color(0.14f, 0.20f, 0.27f);
    private readonly Color selected = new Color(0.16f, 0.48f, 0.47f);
    private TMP_FontAsset font;
    private TMP_Text status;
    private BassExplorerAudio sound;
    private int active = -1;

    public static BassExplorerUI Create(TMP_FontAsset font, UnityAction back)
    {
        var root = new GameObject("BassExplorerUI", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.SetActive(false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var ui = root.AddComponent<BassExplorerUI>();
        ui.font = font;
        ui.sound = root.AddComponent<BassExplorerAudio>();
        ui.Build(back);
        return ui;
    }

    private void Build(UnityAction back)
    {
        Panel("Background", transform, new Vector2(0, 0), new Vector2(1, 1), new Color(0.06f, 0.10f, 0.15f));
        var area = Panel("SafeArea", transform, Vector2.zero, Vector2.one, Color.clear).rectTransform;
        area.gameObject.AddComponent<BassExplorerSafeArea>();
        Label(area, "ベースで弾く", .05f, .85f, .65f, .96f, 48);
        Button(area, "戻る", .80f, .87f, .95f, .95f, back);
        Label(area, "上の和音を決めたら、下のベースを順にタップ", .05f, .75f, .95f, .84f, 30);
        Label(area, "上の和音  ·  C = ド ミ ソ   /   G = シ レ ソ", .05f, .66f, .95f, .74f, 26);
        for (int i = 0; i < 4; i++)
        {
            int index = i;
            float left = .05f + i * .23f;
            for (int c = 0; c < 2; c++)
            {
                int chord = c;
                var button = Button(area, c == 0 ? "C" : "G", left + c * .105f, .55f,
                    left + c * .105f + .095f, .65f, () => SetUpper(index, chord == 1));
                choices[i, c] = button.image;
            }
            var pad = Button(area, bassNames[i], left, .27f, left + .20f, .52f, () => Play(index));
            // Sound begins on touch-down, so consecutive taps feel like playing an instrument.
            pad.onClick.RemoveAllListeners();
            var trigger = pad.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entry.callback.AddListener(_ => Play(index));
            trigger.triggers.Add(entry);
            var submit = new EventTrigger.Entry { eventID = EventTriggerType.Submit };
            submit.callback.AddListener(_ => Play(index));
            trigger.triggers.Add(submit);
            pads[i] = pad.image;
        }
        status = Label(area, "ベースをタップすると音が続きます", .05f, .14f, .73f, .23f, 30);
        Button(area, "停止", .77f, .14f, .94f, .23f, Stop);
        Label(area, "低音 E2 / F2 / G2 / A2  ·  和音とベースは独立  ·  固定音高", .05f, .04f, .95f, .11f, 24);
        Refresh();
    }

    public void Play(int index)
    {
        if (index < 0 || index >= 4) return;
        active = index;
        sound.Play(index, upperG[index]);
        Refresh();
    }

    public void SetUpper(int index, bool useG)
    {
        if (index < 0 || index >= 4) return;
        upperG[index] = useG;
        if (active == index) sound.Play(index, useG);
        Refresh();
    }

    public void Stop()
    {
        active = -1;
        if (sound != null) sound.Stop();
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < 4; i++)
        {
            if (pads[i] != null) pads[i].color = active == i ? selected : surface;
            for (int c = 0; c < 2; c++)
                if (choices[i, c] != null) choices[i, c].color = upperG[i] == (c == 1) ? selected : surface;
        }
        if (status != null) status.text = active < 0 ? "停止中  ·  ベースをタップして演奏" :
            "ベース " + bassNames[active] + "  ＋  上の和音 " + (upperG[active] ? "G" : "C");
    }

    private void OnDisable() { Stop(); }
    private void OnApplicationPause(bool paused) { if (paused) Stop(); }
    private void OnApplicationFocus(bool focused) { if (!focused) Stop(); }

    private Image Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        var rect = image.rectTransform;
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return image;
    }

    private TMP_Text Label(Transform parent, string text, float x, float y, float right, float top, int size)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.enableAutoSizing = true;
        label.fontSizeMin = 14;
        label.fontSizeMax = size;
        label.color = new Color(.92f, .95f, .96f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        var rect = label.rectTransform;
        rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(right, top);
        rect.offsetMin = new Vector2(8, 4); rect.offsetMax = new Vector2(-8, -4);
        return label;
    }

    private Button Button(Transform parent, string text, float x, float y, float right, float top, UnityAction action)
    {
        var image = Panel(text, parent, new Vector2(x, y), new Vector2(right, top), surface);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        Label(image.transform, text, 0, 0, 1, 1, 42);
        return button;
    }
}
