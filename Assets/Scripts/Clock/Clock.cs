using UnityEngine;
using System;
public class Clock : MonoBehaviour
{
    const float degreePerHour = 30f;
    const float degreePerMinute = 6f;
    const float degreePerSecond = 6f;
    [SerializeField]
    Transform hoursTransform = default, minutesTransform = default, secondsTransform = default;

    private bool continuous;

    public bool Continous
    {
        get
        {
            return continuous;
        }
        set
        {
            continuous = value;
        }
    }

    void UpdateContinuous()
    {
        TimeSpan time = DateTime.Now.TimeOfDay;
        hoursTransform.localRotation = Quaternion.Euler(0, (float)time.TotalHours * degreePerHour, 0);
        minutesTransform.localRotation = Quaternion.Euler(0, (float)time.TotalMinutes * degreePerMinute, 0);
        secondsTransform.localRotation = Quaternion.Euler(0, (float)time.TotalSeconds * degreePerSecond, 0);
    }

    void UpdateDiscrete()
    {
        DateTime time = DateTime.Now;
        hoursTransform.localRotation = Quaternion.Euler(0, time.Hour * degreePerHour, 0);
        minutesTransform.localRotation = Quaternion.Euler(0, time.Minute * degreePerMinute, 0);
        secondsTransform.localRotation = Quaternion.Euler(0, time.Second * degreePerSecond, 0);
    }

    void Update()
    {
        if (continuous)
        {
            UpdateContinuous();
        }
        else
        {
            UpdateDiscrete();
        }
    }
}