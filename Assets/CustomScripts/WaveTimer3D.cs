using UnityEngine;
using TMPro;

public class WaveTimer3D : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] public float timeRemaining = 30f;

    [SerializeField] private bool useMinutesSeconds = true;

    [Header("UI References")]
    [SerializeField] public TextMeshProUGUI[] timerTexts;

    [Header("Manager Reference")]
    public GameManager gameManager;   // <-- Added

    void Awake()
    {
        // Auto-find GameManager if not assigned
        if (gameManager == null)    
            gameManager = GameManager.Instance;
    }

    /// <summary>
    /// Called by GameManager to update the visible timer.
    /// </summary>
    public void SetTime(float time)
    {
        timeRemaining = Mathf.Max(0f, time);

        string display;

        if (useMinutesSeconds)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            display = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            display = Mathf.Ceil(timeRemaining).ToString();
        }

        UpdateTimerTexts(display);
    }

    private void UpdateTimerTexts(string text)
    {
        foreach (var t in timerTexts)
        {
            if (t != null)
                t.text = text;
        }
    }

    /// <summary>
    /// Optional: If you ever want the timer to pull data directly from GameManager each frame.
    /// Leave unused unless you want this behavior.
    /// </summary>
    void Update()
    {
        // Example: Uncomment if you want constant sync
        // if (gameManager != null)
        //     SetTime(gameManager.somePublicTimerValue);
    }
}
