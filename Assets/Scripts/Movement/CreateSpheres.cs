using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSpheres : MonoBehaviour
{
    [SerializeField]
    Transform sphere = default;
    private void Awake()
    {
        Vector3 newPosition = sphere.position;
        for (int i = 0; i < 20; ++i)
        {
            Transform ball = Instantiate(sphere);
            newPosition.z += 3;
            ball.position = newPosition;
            ball.SetParent(transform, false);
        }
    }
}
