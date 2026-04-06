using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public event Action OnDayEnd;

    const float SecondsPerDay = 60f;

    float dayTimer;
    float timeScale = 1f;
    bool running;

    public void Initialize()
    {
        running = true;
    }

    public void SetTimeScale(float scale)
    {
        timeScale = scale;
    }

    void Update()
    {
        if (!running) return;

        dayTimer += Time.deltaTime * timeScale;
        if (dayTimer >= SecondsPerDay)
        {
            dayTimer -= SecondsPerDay;
            OnDayEnd?.Invoke();
        }
    }
}
