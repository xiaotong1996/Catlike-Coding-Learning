using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    //? control the responsiveness of the sphere
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;
    // current velocity
    Vector3 velocity;

    [SerializeField]
    Rect allowedArea = new Rect(-4.5f, -4.5f, 9f, 9f);

    [SerializeField, Range(0f, 1f)]
    float bounciness = 0.5f;

    private void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        //* Input for position
        //? position constarined on circle edge
        //playerInput.Normalize();
        //? postition constarined in circle
        //playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        //transform.localPosition = new Vector3(playerInput.x, 0.5f, playerInput.y);

        //* Input for relative movement
        //? p1 = p0 + d
        // Vector3 displacement = new Vector3(playerInput.x, 0f, playerInput.y);
        // transform.localPosition += displacement;

        //* Input for velocity
        //? d = i * t
        // Vector3 velocity = new Vector3(playerInput.x, 0f, playerInput.y);
        // Vector3 displacement = velocity * Time.deltaTime;
        // transform.localPosition += displacement;
        //? change velocity scale
        // Vector3 velocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        //* Input for acceleration
        //? vn+1 = vn + at
        // Vector3 acceleration = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        // velocity += acceleration * Time.deltaTime;
        // Vector3 displacement = velocity * Time.deltaTime;

        //* Input for desired velocity
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        //? changed velocity per frame
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        //? two method to apply changes to current velocity
        /*if(velocity.x<desiredVelocity.x){
            velocity.x=Mathf.Min(maxSpeedChange+velocity.x,desiredVelocity.x);
        }else if(velocity.x>desiredVelocity.x){
            velocity.x=Mathf.Max(velocity.x-maxSpeedChange,desiredVelocity.x);
        }*/
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        Vector3 displacement = velocity * Time.deltaTime;
        Vector3 newPosition = transform.localPosition + displacement;
        //? clamp the position manually, to eliminate velocity towards the bounds
        // if (!allowedArea.Contains(new Vector2(newPosition.x, newPosition.z)))
        // {
        //     newPosition.x = Mathf.Clamp(newPosition.x, allowedArea.xMin, allowedArea.xMax);
        //     newPosition.z = Mathf.Clamp(newPosition.z, allowedArea.yMin, allowedArea.yMax);
        // }
        if (newPosition.x < allowedArea.xMin)
        {
            newPosition.x = allowedArea.xMin;
            // velocity.x = 0f;
            //? bouncing ball
            velocity.x = -velocity.x * bounciness;
        }
        else if (newPosition.x > allowedArea.xMax)
        {
            newPosition.x = allowedArea.xMax;
            // velocity.x = 0f;
            //? bouncing ball
            velocity.x = -velocity.x * bounciness;
        }
        if (newPosition.z < allowedArea.yMin)
        {
            newPosition.z = allowedArea.yMin;
            // velocity.z = 0f;
            //? bouncing ball
            velocity.z = -velocity.z * bounciness;
        }
        else if (newPosition.z > allowedArea.yMax)
        {
            newPosition.z = allowedArea.yMax;
            // velocity.z = 0f;
            //? bouncing ball
            velocity.z = -velocity.z * bounciness;
        }
        transform.localPosition = newPosition;

    }
}
