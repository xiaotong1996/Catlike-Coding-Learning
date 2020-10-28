using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Graph : MonoBehaviour
{
    public enum TransitionMode { Cycle, Random }
    [SerializeField]
    TransitionMode transitionMode = TransitionMode.Cycle;

    [SerializeField]
    Transform pointPrefab = default;

    [SerializeField, Range(10, 100)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function = default;

    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;

    [SerializeField]
    TMP_Dropdown functionSelecter = default;

    [SerializeField]
    TMP_Dropdown getNextFunctionMode = default;
    [SerializeField]
    bool autoSwitchFunction = false;
    Transform[] points;

    float duration;

    bool transitioning;

    FunctionLibrary.FunctionName transitionFunction;

    public bool AutoSwitchFunction
    {
        get
        {
            return autoSwitchFunction;
        }
        set
        {
            autoSwitchFunction = value;
        }
    }

    void Awake()
    {
        float step = 2f / resolution;
        var scale = Vector3.one * step;
        points = new Transform[resolution * resolution];
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = Instantiate(pointPrefab);
            point.localScale = scale;
            point.SetParent(transform, false);
            points[i] = point;
        }
    }

    void Update()
    {
        if (!autoSwitchFunction)
        {
            UpdateManually();
        }
        else
        {
            UpdateAutomatically();
        }
    }

    void UpdateManually()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration > transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else
        {
            transitioning = true;
            transitionFunction = function; ;
            function = (FunctionLibrary.FunctionName)functionSelecter.value;
        }

        if (transitioning)
        {
            UpdateFunctionTransition();
        }
        else
        {
            UpdateFunction();
        }

    }


    void UpdateAutomatically()
    {
        transitionMode = (TransitionMode)getNextFunctionMode.value;
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration > transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function;
            PickNextFunction();
            functionSelecter.value = (int)function;
        }

        if (transitioning)
        {
            UpdateFunctionTransition();
        }
        else
        {
            UpdateFunction();
        }
    }
    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionName(function);
    }

    void UpdateFunction()
    {
        FunctionLibrary.GraphFunction f = FunctionLibrary.GetGraphFunction(function);
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = f(u, v, time);
        }
    }

    void UpdateFunctionTransition()
    {
        FunctionLibrary.GraphFunction from = FunctionLibrary.GetGraphFunction(transitionFunction);
        FunctionLibrary.GraphFunction to = FunctionLibrary.GetGraphFunction(function);
        float progress = duration / transitionDuration;

        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
        }
    }
}
