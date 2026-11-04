using System.Collections;
using UnityEngine;

public class TitleIntroAnimation : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delay = 0f;
    [SerializeField] private float duration = 0.75f;

    [Header("Slide")]
    [SerializeField] private float slideDistance = 140f;

    [Header("Scale")]
    [SerializeField] private float startScale = 0.92f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 finalPosition;
    private Vector3 finalScale;

    private Vector2 startPosition;
    private Vector3 initialScale;

    private Coroutine animationRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Simpan posisi asli SEBELUM diubah
        finalPosition = rectTransform.anchoredPosition;
        finalScale = rectTransform.localScale;

        startPosition =
            finalPosition + new Vector2(slideDistance, 0f);

        initialScale =
            finalScale * startScale;

        // PENTING:
        // sembunyikan sebelum frame pertama dirender
        ApplyStartState();
    }

    private void Start()
    {
        animationRoutine = StartCoroutine(PlayIntro());
    }

    private void ApplyStartState()
    {
        rectTransform.anchoredPosition = startPosition;
        rectTransform.localScale = initialScale;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator PlayIntro()
    {
        // Tetap tersembunyi selama delay
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);

            // Lebih lembut daripada SmoothStep biasa
            float moveEase = EaseOutQuart(t);

            // Fade dibuat sedikit lebih cepat,
            // supaya terasa muncul natural
            float fadeEase = EaseOutCubic(t);

            rectTransform.anchoredPosition =
                Vector2.LerpUnclamped(
                    startPosition,
                    finalPosition,
                    moveEase
                );

            rectTransform.localScale =
                Vector3.LerpUnclamped(
                    initialScale,
                    finalScale,
                    moveEase
                );

            canvasGroup.alpha = fadeEase;

            yield return null;
        }

        // Pastikan hasil akhirnya presisi
        rectTransform.anchoredPosition = finalPosition;
        rectTransform.localScale = finalScale;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        animationRoutine = null;
    }

    private float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    private float EaseOutQuart(float x)
    {
        return 1f - Mathf.Pow(1f - x, 4f);
    }
}