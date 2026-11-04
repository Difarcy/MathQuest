using UnityEngine;

public class ButtonIdleAnimation : MonoBehaviour
{
    [Header("Breathing")]
    [SerializeField] private float scaleAmount = 0.035f;
    [SerializeField] private float scaleSpeed = 2.3f;

    [Header("Floating")]
    [SerializeField] private float floatAmount = 3.5f;
    [SerializeField] private float floatSpeed = 1.6f;

    [Header("Sway")]
    [SerializeField] private float rotationAmount = 1.2f;
    [SerializeField] private float rotationSpeed = 1.3f;

    [Header("Organic Variation")]
    [SerializeField] private bool differentPhase = true;

    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Quaternion originalRotation;

    private float phase;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;

        if (differentPhase)
        {
            // Setiap tombol punya waktu gerak berbeda
            phase = transform.GetSiblingIndex() * 1.17f;
        }
    }

    private void Update()
    {
        float t = Time.unscaledTime;

        // =========================
        // BREATHING
        // =========================

        float breathe =
            (Mathf.Sin((t * scaleSpeed) + phase) + 1f) * 0.5f;

        // Gerakan kecil tambahan agar tidak terlalu sinus/robotik
        float microPulse =
            Mathf.Sin(
                (t * scaleSpeed * 2.21f)
                + phase
                + 0.7f
            ) * 0.005f;

        float scale =
            1f +
            (breathe * scaleAmount)
            + microPulse;

        rectTransform.localScale = new Vector3(
            originalScale.x * scale,
            originalScale.y * scale,
            originalScale.z
        );

        // =========================
        // FLOATING
        // =========================

        float y =
            Mathf.Sin(
                (t * floatSpeed)
                + phase
            ) * floatAmount;

        float x =
            Mathf.Sin(
                (t * floatSpeed * 0.63f)
                + phase
                + 1.1f
            ) * 1.2f;

        rectTransform.anchoredPosition =
            originalPosition +
            new Vector2(x, y);

        // =========================
        // SWAY
        // =========================

        float rotation =
            Mathf.Sin(
                (t * rotationSpeed)
                + phase
                + 0.4f
            ) * rotationAmount;

        rectTransform.localRotation =
            originalRotation *
            Quaternion.Euler(0f, 0f, rotation);
    }

    private void OnDisable()
    {
        if (rectTransform == null)
            return;

        rectTransform.localScale = originalScale;
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localRotation = originalRotation;
    }
}