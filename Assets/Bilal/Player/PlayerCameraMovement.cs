using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class PlayerCameraMovement : MonoBehaviour
{
    [Header("References")]
    public Transform player; //reference player transform

    [Header("Camera Movement")]
    public float smoothTime = 0.2f; //smoothing value of camera follow
    public float rotationRate = 1f; //rate of camera rotation

    private Vector3 offset; //offset between camera and player
    private Vector3 smoothVelocity = Vector3.zero; //empty vector to use in SmoothDamp
    private float rotationSpeed = 90f; //camera rotation speed
    
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.position; //calculate offset between camera and player
    }

    // LateUpdate is called every frame, if the behaviour is enabled
    void LateUpdate()
    {
        //camera follows player from its original position to the target position (player.position + offset)
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref smoothVelocity, smoothTime); //for sharp follow, smoothTime=0

        //rotate camera around player with Q & E keys
        if (Input.GetKey(KeyCode.Q))
        {
            transform.RotateAround(player.position, Vector3.up, rotationSpeed * rotationRate * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.RotateAround(player.position, Vector3.up, -rotationSpeed * rotationRate * Time.deltaTime);
        }
    }
}
