﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMove : MonoBehaviour
{       


    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    [HideInInspector]public Vector3 moveDirection;

    Rigidbody rb;

    public bool canMove;

    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public CinemachineBasicMultiChannelPerlin a;
    PlayerAudio playerAudio;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerAudio = GetComponent<PlayerAudio>();
       
        readyToJump = true;
        canMove = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.01f, whatIsGround);

        MyInput();
        SpeedControl();
        JumpControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        if(canMove)
        {
            MovePlayer();
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            if(canMove)
            {
                Jump();
            }

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if(moveDirection != Vector3.zero)
        {
            playerAudio.stepSound.enabled = true;
            if(Input.GetKey(sprintKey))
            {
                moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, Time.deltaTime * .2f);
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, Time.deltaTime * 1f);
            }
       
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = moveSpeed / walkSpeed * moveSpeed / walkSpeed;
        }
        else
        {
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0.5f;
            StartCoroutine("makeAudioFalse");
        }

        
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = transform.forward * verticalInput + orientation.right * horizontalInput;
       
        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized;
             limitedVel = new Vector3(limitedVel.x * moveSpeed , 0f, limitedVel.z * moveSpeed );
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
    
    private void JumpControl()
    {
        Vector3 flatVel = new Vector3(0, rb.velocity.y, 0);

        // limit velocity if needed
        if(flatVel.magnitude > jumpForce)
        {
            Vector3 limitedVel = flatVel.normalized * jumpForce;
            rb.velocity = new Vector3(rb.velocity.x, limitedVel.y, rb.velocity.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
    IEnumerator makeAudioFalse() 
    {

        yield return new WaitForSeconds(0.1f);
        playerAudio.stepSound.enabled = false;
    }
}

