using UnityEngine;

[ExecuteAlways]
public class FitReferenceResolution : MonoBehaviour
{
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    private RectTransform rectTransform;
    private RectTransform parentRect;

    private void OnEnable()
    {
        ApplyScale();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyScale();
    }

    private void ApplyScale()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform.parent == null)
            return;

        parentRect = rectTransform.parent as RectTransform;

        if (parentRect == null)
            return;

        float scaleX = parentRect.rect.width / referenceResolution.x;
        float scaleY = parentRect.rect.height / referenceResolution.y;

        float scale = Mathf.Min(scaleX, scaleY);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rectTransform.sizeDelta = referenceResolution;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}