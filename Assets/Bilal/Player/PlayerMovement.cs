using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera; //used to move player in the direction facing the camera
    private Rigidbody rigidBody;

    [Header("Mouse Cursor")]
    public bool showMouse = true; //used to switch explore/combat modes

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float currentSpeed;
    public float rotationSpeed = 10f;
    
    [Header("Physics")]
    public float fallRate = 1f;
    public float jumpHeight = 7f;
    private bool isGrounded; //is the player on the ground?
    private float upVelocity = -1f; //controls the up vector of player movement (positive to jump, negative to fall)

    //animation variables used in PlayerAnimation Script
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isWalking;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool isJumping;

    [HideInInspector] public Vector3 movementDirection = Vector3.zero; //vector that controls movement (also used in PlayerAnimation Script)
    private Quaternion desiredRotation; //controls which way the player rotates

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>(); //reference
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.visible = showMouse ? true : false; //control cursor visibility
        Movement();
    }

    void Movement()
    {
        //get movement input keys
        bool Run = Input.GetKey(KeyCode.LeftShift);
        bool Jump = Input.GetKey(KeyCode.Space);

        //get movement input axes
        float verticalInput = Input.GetAxisRaw("Vertical"); //"W" & "S" keys - already defined inside editor
        float horizontalInput = Input.GetAxisRaw("Horizontal"); //"A" & "D" keys - already defiined inside editor

        //calculate movement direction to always move forward facing the camera (isometric)
        Vector3 movementInput = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0) * new Vector3(horizontalInput, 0, verticalInput);
        movementDirection = movementInput.normalized; //normalizing ensures a magnitude of 1

        if (showMouse) //combat mode
        {
            Ray mouse = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(mouse, out var hit))
            {
                Vector3 mouseHit = hit.point;
                Vector3 offset = transform.position + 1.5f * Vector3.up;
                Vector3 direction = mouseHit - offset;
                desiredRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                //rotate player with mouse
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else //explore mode
        {
            desiredRotation = Quaternion.LookRotation(movementDirection, Vector3.up); //rotate to movement direction
        }

        isMoving = (movementDirection != Vector3.zero) ? true : false; //is the player moving?
        if (isMoving)
        {
            if (!Run)
            {
                currentSpeed = walkSpeed;
                isWalking = true;
                isRunning = false;
            }
            if (isWalking && Run && !isJumping)
            {
                currentSpeed = runSpeed;
                isRunning = true;
                isWalking = false;
            }

            //rotate player in explore mode only when moving to avoid resetting rotation to 0 when not moving
            if (!showMouse)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
            }
        }

        /*isGrounded = characterController.isGrounded; //is the player on the ground?
        if (!isJumping && Jump) //jump once (no double jump)
        {
            upVelocity = jumpHeight;
        }
        if (isGrounded && isJumping) //reset isJumping
        {
            isJumping = false;
        }
        if (isGrounded && upVelocity < 0) //reset up vector when landing after falliing
        {
            upVelocity = -1f; //set to -1 to make sure isGrounded works properly
        }
        if (!isGrounded) //handle gravity (falling)
        {
            upVelocity += gravity * fallRate * Time.deltaTime;
            isJumping = true;
        }*/

        movementDirection.y = upVelocity; //up vector
        movementDirection.x *= currentSpeed; //horizontal vector
        movementDirection.z *= currentSpeed; //vertical vector
        rigidBody.velocity = movementDirection * Time.deltaTime; //move player
    }
}
