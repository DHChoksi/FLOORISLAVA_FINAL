using UnityEngine;
using TMPro;

public class WaveTimer3D : MonoBehaviour
{
    [SerializeField] public float timeRemaining = 30f;
    [SerializeField] public TextMeshPro[] timerTexts;

    public void SetTime(float time)
    {
        string display = Mathf.Ceil(Mathf.Max(0, time)).ToString();
        foreach (var t in timerTexts)
        {
            if (t != null) t.text = display;
        }
    }
}
