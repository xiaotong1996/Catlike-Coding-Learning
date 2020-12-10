using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere3 : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    //? control the responsiveness of the sphere
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f, maxSnapSpeed = 100f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 1;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1;
    // current velocity
    Vector3 velocity, desiredVelocity;

    Rigidbody body;

    bool desiredJump;

    bool onGround;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct;

    Vector3 contactNormal, steepNormal;

    int groundContactCount, steepCountactCount;

    bool OnGround => groundContactCount > 0;

    bool OnSteep => steepCountactCount > 0;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
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
            "_BaseColor", OnGround ? Color.black : Color.white
        );
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    bool CheckSteepContacts()
    {
        if (steepCountactCount > 1)
        {
            steepNormal.Normalize();
            if (steepNormal.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;

        //? adjust velocity to align with the ground
        float dot = Vector3.Dot(velocity, hit.normal);
        //? the velocity might point down, which should be ignored
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    private void Update()
    {
        //AdjustColor();
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
        stepsSinceLastGrounded++;
        stepsSinceLastJump++;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
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
        groundContactCount = steepCountactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
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
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; ++i)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minDot)
            {
                // onGround = true;
                groundContactCount += 1;
                contactNormal += normal;
            }
            else if (normal.y > -0.01f)
            {
                steepCountactCount += 1;
                steepNormal += normal;
            }
        }
    }

    void Jump()
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }
        //if (OnGround || jumpPhase < maxAirJumps){
        stepsSinceLastJump = 0;
        ++jumpPhase;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;
        //}
    }
}
