using UnityEngine;

public class StartButtonAlive : MonoBehaviour
{
    [Header("Breathing / Membesar")]
    [SerializeField] private float scaleAmount = 0.07f;
    [SerializeField] private float scaleSpeed = 2.8f;

    [Header("Floating")]
    [SerializeField] private float floatAmount = 6f;
    [SerializeField] private float floatSpeed = 1.7f;

    [Header("Sway")]
    [SerializeField] private float rotationAmount = 1.5f;
    [SerializeField] private float rotationSpeed = 1.4f;

    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
    }

    private void Update()
    {
        float t = Time.unscaledTime;

        // =========================
        // BREATHING
        // =========================

        // Nilai 0 sampai 1 secara halus
        float breathe =
            (Mathf.Sin(t * scaleSpeed) + 1f) * 0.5f;

        // Tambahan gelombang kecil agar tidak terasa robotik
        float micro =
            Mathf.Sin(t * scaleSpeed * 2.15f + 0.8f) * 0.008f;

        float scale =
            1f +
            (breathe * scaleAmount) +
            micro;

        rectTransform.localScale = new Vector3(
            originalScale.x * scale,
            originalScale.y * scale,
            originalScale.z
        );

        // =========================
        // FLOATING
        // =========================

        float y =
            Mathf.Sin(t * floatSpeed) *
            floatAmount;

        // Gerakan horizontal sangat kecil
        float x =
            Mathf.Sin(t * floatSpeed * 0.63f + 1.2f) *
            1.5f;

        rectTransform.anchoredPosition =
            originalPosition +
            new Vector2(x, y);

        // =========================
        // SWAY
        // =========================

        float rotation =
            Mathf.Sin(t * rotationSpeed + 0.4f) *
            rotationAmount;

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