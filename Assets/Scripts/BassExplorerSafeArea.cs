using UnityEngine;

public sealed class BassExplorerSafeArea : MonoBehaviour
{
    private void Update()
    {
        if (Screen.width <= 0 || Screen.height <= 0) return;
        Rect safe = Screen.safeArea;
        var rect = (RectTransform)transform;
        rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
    }
}
