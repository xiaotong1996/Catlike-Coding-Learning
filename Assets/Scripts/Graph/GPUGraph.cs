using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GPUGraph : MonoBehaviour
{
    public enum TransitionMode { Cycle, Random }
    [SerializeField]
    TransitionMode transitionMode = TransitionMode.Cycle;

    const int maxResolution = 1000;
    ComputeBuffer positionsBuffer;

    [SerializeField, Range(10, maxResolution)]
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

    [SerializeField]
    ComputeShader computeShader = default;

    [SerializeField]
    Material material = default;

    [SerializeField]
    Mesh mesh = default;

    float duration;

    bool transitioning;

    FunctionLibrary.FunctionName transitionFunction;

    static readonly int
    positionId = Shader.PropertyToID("_Positions"),
    resolutionId = Shader.PropertyToID("_Resolution"),
    stepId = Shader.PropertyToID("_Step"),
    timeId = Shader.PropertyToID("_Time"),
    transitionProgressId = Shader.PropertyToID("_TransitionProgress"),
    scaleID = Shader.PropertyToID("_Scale");


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

    void UpdateFuncitonOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);

        if (transitioning)
        {
            computeShader.SetFloat(
                transitionProgressId,
                Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
            );
        }

        var kernelIndex = (int)function +
            (int)(transitioning ? transitionFunction : function) * FunctionLibrary.FunctionCount;
        computeShader.SetBuffer(kernelIndex, positionId, positionsBuffer);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);


        material.SetBuffer(positionId, positionsBuffer);
        material.SetVector(scaleID, new Vector4(step, 1f / step));
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

    private void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
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

        UpdateFuncitonOnGPU();

        // if (transitioning)
        // {
        //     UpdateFunctionTransition();
        // }
        // else
        // {
        //     UpdateFunction();
        // }

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

        UpdateFuncitonOnGPU();
        // if (transitioning)
        // {
        //     UpdateFunctionTransition();
        // }
        // else
        // {
        //     UpdateFunction();
        // }
    }
    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionName(function);
    }

    // void UpdateFunction()
    // {
    //     FunctionLibrary.GraphFunction f = FunctionLibrary.GetGraphFunction(function);
    //     float time = Time.time;
    //     float step = 2f / resolution;
    //     float v = 0.5f * step - 1f;
    //     for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
    //     {
    //         if (x == resolution)
    //         {
    //             x = 0;
    //             z += 1;
    //             v = (z + 0.5f) * step - 1f;
    //         }
    //         float u = (x + 0.5f) * step - 1f;
    //         points[i].localPosition = f(u, v, time);
    //     }
    // }

    // void UpdateFunctionTransition()
    // {
    //     FunctionLibrary.GraphFunction from = FunctionLibrary.GetGraphFunction(transitionFunction);
    //     FunctionLibrary.GraphFunction to = FunctionLibrary.GetGraphFunction(function);
    //     float progress = duration / transitionDuration;

    //     float time = Time.time;
    //     float step = 2f / resolution;
    //     float v = 0.5f * step - 1f;
    //     for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
    //     {
    //         if (x == resolution)
    //         {
    //             x = 0;
    //             z += 1;
    //             v = (z + 0.5f) * step - 1f;
    //         }
    //         float u = (x + 0.5f) * step - 1f;
    //         points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
    //     }
    // }
}
