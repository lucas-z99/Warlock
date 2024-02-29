using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator playerAnimator;
    public PlayerMovement movement;

    private float velocityZ = 0f;
    private float velocityX = 0f;
    public float acceleration = 2f;
    public float deceleration = 2f;
    public float maxWalkVelocity = 1f;
    float desiredAngle;
    float currentAngle;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (movement.isMoving)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(movement.movementDirection, Vector3.up);
            desiredAngle = desiredRotation.eulerAngles.y;
            currentAngle = transform.eulerAngles.y;

            print(currentAngle);
        }
        else
        {
            velocityX = 0f;
            velocityZ = 0f;
        }
        
        float verticalInput = 0f;
        float horizontalInput = 0f;

        if (desiredAngle > 180f)
        {
            desiredAngle -= 360f;
        }
        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }

        

        if ((desiredAngle > currentAngle - 1) && (desiredAngle < currentAngle + 1))
        {
            verticalInput = 1f;
            horizontalInput = 0f;
        }

        bool forward = verticalInput > 0 ? true : false;
        bool backward = verticalInput < 0 ? true : false;
        bool left = horizontalInput < 0 ? true : false;
        bool right = horizontalInput > 0 ? true : false;

        float currentMaxVelocity = maxWalkVelocity;

        if (forward && velocityZ <= currentMaxVelocity)
        {
            velocityZ = 1f;
        }
        if (backward && velocityZ > -currentMaxVelocity)
        {
            velocityZ -= Time.deltaTime * acceleration;
        }
        if (left && velocityX > -currentMaxVelocity)
        {
            velocityX -= Time.deltaTime * acceleration;
        }
        if (right && velocityX < currentMaxVelocity)
        {
            velocityX += Time.deltaTime * acceleration;
        }

        if (!forward && velocityZ > 0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }
        if (!backward && velocityZ < 0f)
        {
            velocityZ += Time.deltaTime * deceleration;
        }
        if (!forward && !backward && velocityZ != 0f && (velocityZ < -1f || velocityZ > 1f))
        {
            velocityZ = 0f;
        }

        if (!left && velocityX < 0f)
        {
            velocityZ += Time.deltaTime * deceleration;
        }
        if (!right && velocityX > 0f)
        {
            velocityZ -= Time.deltaTime * deceleration;
        }
        if (!left && !right && velocityX != 0f && (velocityX < -1f || velocityX > 1f))
        {
            velocityX = 0f;
        }

        playerAnimator.SetFloat("velocityZ", velocityZ);
        playerAnimator.SetFloat("velocityX", velocityX);
    }
}
