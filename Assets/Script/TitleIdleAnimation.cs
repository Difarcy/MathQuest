using UnityEngine;

public class TitleWaveAnimation : MonoBehaviour
{
    [Header("Wave / Melambai")]
    [SerializeField] private float waveAngle = 4f;
    [SerializeField] private float waveSpeed = 4.2f;

    [Header("Pulse / Membesar")]
    [SerializeField] private float pulseAmount = 0.04f;
    [SerializeField] private float pulseSpeed = 4.5f;

    private RectTransform rectTransform;

    private Quaternion originalRotation;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalRotation = rectTransform.localRotation;
        originalScale = rectTransform.localScale;
    }

    private void Update()
    {
        float t = Time.unscaledTime;

        // Gerakan melambai.
        // Gelombang kedua membuat ayunan tidak terlalu robotik.
        float wave =
            Mathf.Sin(t * waveSpeed) +
            Mathf.Sin(t * waveSpeed * 2.15f + 0.7f) * 0.18f;

        float rotationZ = wave * waveAngle;

        rectTransform.localRotation =
            originalRotation * Quaternion.Euler(0f, 0f, rotationZ);

        // Membesar sedikit lalu kembali normal.
        // Tidak pernah mengecil dari ukuran asli.
        float pulse =
            1f +
            ((Mathf.Sin(t * pulseSpeed) + 1f) * 0.5f)
            * pulseAmount;

        rectTransform.localScale = new Vector3(
            originalScale.x * pulse,
            originalScale.y * pulse,
            originalScale.z
        );
    }

    private void OnDisable()
    {
        if (rectTransform == null)
            return;

        rectTransform.localRotation = originalRotation;
        rectTransform.localScale = originalScale;
    }
}