using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere2 : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    //? control the responsiveness of the sphere
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 1;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 40f;
    // current velocity
    Vector3 velocity, desiredVelocity;

    Rigidbody body;

    bool desiredJump;

    bool onGround;

    int jumpPhase;

    float minGroundDotProduct;

    Vector3 contactNormal;

    int groundContactCount;

    bool OnGround => groundContactCount > 0;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);
        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void AdjustColor()
    {
        GetComponent<Renderer>().material.SetColor(
            "_BaseColor", Color.white * (groundContactCount * 0.25f)
        );
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    private void Update()
    {
        AdjustColor();
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
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        desiredJump |= Input.GetButtonDown("Jump");
    }

    private void FixedUpdate()
    {
        UpdateState();

        //* airborne sphere is harder to control
        /*float acceleration = onGround ? maxAcceleration : maxAirAcceleration;

        //? changed velocity per frame
        //? in FixedUpdate() deltatTime = fixedDeltaTime = 0.02(default) which means this method is called 50 times per second
        //! increasing the time step may cause depenetration problem change Collision Detection mode to solve it
        // Time.fixedDeltaTime = 0.2f;
        float maxSpeedChange = acceleration * Time.fixedDeltaTime;
        //? two method to apply changes to current velocity
       if(velocity.x<desiredVelocity.x){
            velocity.x=Mathf.Min(maxSpeedChange+velocity.x,desiredVelocity.x);
        }else if(velocity.x>desiredVelocity.x){
            velocity.x=Mathf.Max(velocity.x-maxSpeedChange,desiredVelocity.x);
        }
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);*/

        //* let sphere move aligned with ground
        AdjustVelocity();


        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;
        ClearState();
    }

    void UpdateState()
    {
        velocity = body.velocity;
        if (OnGround)
        {
            jumpPhase = 0;
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    void ClearState()
    {
        // onGround = false;
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    private void OnCollisionEnter(Collision other)
    {
        EvaluateCollision(other);
    }

    private void OnCollisionStay(Collision other)
    {
        EvaluateCollision(other);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; ++i)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                // onGround = true;
                groundContactCount += 1;
                contactNormal += normal;
            }
        }
    }

    void Jump()
    {
        if (OnGround || jumpPhase < maxAirJumps)
        {
            ++jumpPhase;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            velocity += contactNormal * jumpSpeed;
        }
    }
}
