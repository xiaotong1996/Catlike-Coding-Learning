using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSpheres : MonoBehaviour
{
    [SerializeField]
    Transform sphere = default;


    [SerializeField, Range(1, 100)]
    int sphereNum = 1;
    private void Awake()
    {
        Vector3 newPosition = sphere.position;
        for (int i = 0; i < sphereNum; ++i)
        {
            Transform ball = Instantiate(sphere);
            newPosition.z += 3;
            ball.position = newPosition;
            ball.SetParent(transform, false);
        }
    }
}
