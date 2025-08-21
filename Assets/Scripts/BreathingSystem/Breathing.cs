using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class Breathing : MonoBehaviour
{
    public static bool IsExhaling { get; private set; }
    public ParticleSystem bubbleEffect;
    public InputActionProperty BreathHolding;

    [Header("UI Components")]
    public Slider breathSlider;
    public GameObject breathUIRoot;

    [Header("Breath Settings")]
    [SerializeField] float breathInterval = 10f;
    [SerializeField] float exhaleDuration = 3f;
    [SerializeField] float maxHoldTime = 30f;

    private Coroutine breathingRoutine;
    private Coroutine currentExhaleRoutine;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool wasHolding = false;
    private bool isInForcedExhale = false;

    void Start()
    {
        breathingRoutine = StartCoroutine(BreathingLoop());
        if (breathSlider != null)
        {
            breathSlider.maxValue = maxHoldTime;
            breathSlider.value = maxHoldTime;
        }
        if (breathUIRoot != null)
        {
            breathUIRoot.SetActive(false);
        }
    }

    void Update()
    {
        isHolding = BreathHolding.action.IsPressed();

        if (breathUIRoot != null)
            breathUIRoot.SetActive(isHolding);

        // Handling the logic of holding down the breath
        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            float timeLeft = Mathf.Max(0, maxHoldTime - holdTimer);

            if (breathSlider != null)
                breathSlider.value = timeLeft;

            // Reach the maximum breath-holding time and exhale forcefully
            if (holdTimer >= maxHoldTime && !isInForcedExhale && !IsExhaling)
            {
                TriggerExhale(ExhaleType.Forced);
            }
        }
        else
        {
            // The button has just been released and the patient is not in the forced exhalation state.
            if (wasHolding && !isInForcedExhale && !IsExhaling)
            {
                TriggerExhale(ExhaleType.Release);
            }

            // Reset state
            if (wasHolding)
            {
                holdTimer = 0f;
                isInForcedExhale = false;
            }

            if (breathSlider != null)
                breathSlider.value = maxHoldTime;
        }

        wasHolding = isHolding;
    }

    // Exhalation type enumeration for easy management
    private enum ExhaleType
    {
        Normal,    // Timed exhalation
        Forced,    // Forced exhalation after holding breath for too long
        Release    // Release the button and exhale
    }

    // Uniform exhalation trigger method
    private void TriggerExhale(ExhaleType type)
    {
        // If you are already exhaling, stop the current exhalation first
        if (currentExhaleRoutine != null)
        {
            StopCoroutine(currentExhaleRoutine);
        }

        // Set state based on type
        if (type == ExhaleType.Forced)
        {
            isInForcedExhale = true;
        }

        currentExhaleRoutine = StartCoroutine(ExhaleRoutine(type));
    }

    IEnumerator BreathingLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(breathInterval);

            // Only perform timed exhalation when you are not holding your breath and not exhaling
            if (!isHolding && !IsExhaling)
            {
                TriggerExhale(ExhaleType.Normal);
            }
        }
    }

    // Unified exhalation coroutine
    IEnumerator ExhaleRoutine(ExhaleType type)
    {
        StartExhale();
        yield return new WaitForSeconds(exhaleDuration);
        StopExhale();

        // Cleaning status
        if (type == ExhaleType.Forced)
        {
            isInForcedExhale = false;
            holdTimer = 0f;
        }

        currentExhaleRoutine = null;
    }

    void StartExhale()
    {
        if (bubbleEffect != null && !bubbleEffect.isPlaying)
            bubbleEffect.Play();
        IsExhaling = true;
    }

    void StopExhale()
    {
        if (bubbleEffect != null && bubbleEffect.isPlaying)
            bubbleEffect.Stop();
        IsExhaling = false;
    }

    void OnDisable()
    {
        IsExhaling = false;
        if (currentExhaleRoutine != null)
        {
            StopCoroutine(currentExhaleRoutine);
            currentExhaleRoutine = null;
        }
    }
}